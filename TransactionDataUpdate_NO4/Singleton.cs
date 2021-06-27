using FXCommon.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeAPI;

namespace TransactionDataUpdate_NO4
{
    public sealed class Singleton
    {
        /// <summary>
        /// 单例对象
        /// </summary>
        private static readonly Singleton instance = new Singleton();

        public static Singleton Instance
        {
            get
            {
                return instance;
            }
        }

        // 显式静态构造函数告诉C＃编译器
        // 不要将类型标记为BeforeFieldInit
        static Singleton()
        {

        }

        private Singleton()
        {
            Init();
        }

        private List<HostInfo> HostList;// 行情服务器
        public int hqClientCount = 32;// 行情连接数量

        Thread HqClientRetryThread;// 重连线程
        Thread HeartbeatThread;// 心跳线程
        Thread SysparUpdateThread;// 系统参数更新线程

        /// <summary>
        /// 临时表名
        /// </summary>
        public string TempTableName;

        #region====新分笔成交数据====
        public int NewTransactionDataCount = 50;// 获取分笔成交数据每批次数量
        public int NewTransactionDataSendPeriodTime = 3000;//更新间隔
        public int NewTransactionDataRunStartTime = -180;//运行时间比交易时间提前秒数
        public int NewTransactionDataRunEndTime = 180;//运行时间比交易时间滞后秒数
        public int NewTransactiondataSleepTime = 10000;//每天执行时的间隔时间
        public int NewTransactiondataStartHour = 16;//每天执行开始时间
        public int NewTransactiondataEndHour = 23;//每天执行结束时间
        public int NewTransactionDataTrendHandlerCount = 10;
        #endregion

        public string connString_transactiondata= string.Empty;

        private ThreadMsgTemplate<int> UpdateWait = new ThreadMsgTemplate<int>();// 系统参数更新线程队列
        public ThreadMsgTemplate<int> cltData = new ThreadMsgTemplate<int>();//行情链接队列
        public ThreadMsgTemplate<int> retryData = new ThreadMsgTemplate<int>();//行情重连队列
        private ThreadMsgTemplate<int> retryWait = new ThreadMsgTemplate<int>();//行情重连线程队列
        private ThreadMsgTemplate<int> heartbeatWait = new ThreadMsgTemplate<int>();//心跳线程队列

        /// <summary>
        /// 获取行情链接
        /// </summary>
        /// <returns></returns>
        public int GetHqClient()
        {

            int clientId = -1;
            cltData.WaitMessage(ref clientId);
            return clientId;
        }

        /// <summary>
        /// 添加行情链接
        /// </summary>
        /// <param name="id"></param>
        public void AddHqClient(int hqClient)
        {
            cltData.AddMessage(hqClient);
        }

        /// <summary>
        /// 添加行情重连服务器链接
        /// </summary>
        public void AddRetryClient(int client)
        {
            Console.WriteLine("链接" + client + "加入重连");
            retryData.AddMessage(client, false);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            connString_transactiondata = ConfigurationManager.ConnectionStrings["ConnectionString_data"].ConnectionString;
            TempTableName = ConfigurationManager.AppSettings["temp_table_name"];

            using (SqlConnection conn = new SqlConnection(connString_transactiondata))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                try
                {
                    string sql = string.Format(@"USE [meal_ticket_shares_transactiondata]
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U'))
begin
	IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'LastModified'))
	EXEC sys.sp_dropextendedproperty @name=N'MS_Description' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'LastModified'

	IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Type'))
	EXEC sys.sp_dropextendedproperty @name=N'MS_Description' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Type'

	IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Stock'))
	EXEC sys.sp_dropextendedproperty @name=N'MS_Description' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Stock'

	IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Volume'))
	EXEC sys.sp_dropextendedproperty @name=N'MS_Description' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Volume'

	IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Price'))
	EXEC sys.sp_dropextendedproperty @name=N'MS_Description' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Price'

	IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'TimeStr'))
	EXEC sys.sp_dropextendedproperty @name=N'MS_Description' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'TimeStr'

	IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Time'))
	EXEC sys.sp_dropextendedproperty @name=N'MS_Description' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Time'

	IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'SharesCode'))
	EXEC sys.sp_dropextendedproperty @name=N'MS_Description' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'SharesCode'

	IF  EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Market'))
	EXEC sys.sp_dropextendedproperty @name=N'MS_Description' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Market'

	/****** Object:  Index [index_time_orderindex]    Script Date: 2021/1/25 16:21:39 ******/
	IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_time_orderindex')
	DROP INDEX [index_time_orderindex] ON [dbo].{0}

	/****** Object:  Index [index_sharesinfonum_time]    Script Date: 2021/1/25 16:21:39 ******/
	IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_sharesinfonum_time')
	DROP INDEX [index_sharesinfonum_time] ON [dbo].[{0}]

	/****** Object:  Index [index_sharesinfo_time]    Script Date: 2021/1/25 16:21:39 ******/
	IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_sharesinfo_time')
	DROP INDEX [index_sharesinfo_time] ON [dbo].[{0}]

	/****** Object:  Index [index_orderindex]    Script Date: 2021/1/25 16:21:39 ******/
	IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_orderindex')
	DROP INDEX [index_orderindex] ON [dbo].[{0}]

	/****** Object:  Index [index_key]    Script Date: 2021/1/25 16:21:39 ******/
	IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_key')
	DROP INDEX [index_key] ON [dbo].[{0}]

	/****** Object:  Index [index_isTimeFirst]    Script Date: 2021/1/25 16:21:39 ******/
	IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_isTimeFirst')
	DROP INDEX [index_isTimeFirst] ON [dbo].[{0}]

	/****** Object:  Index [index_isTimeLast]    Script Date: 2021/1/25 16:21:39 ******/
	IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_isTimeLast')
	DROP INDEX [index_isTimeLast] ON [dbo].[{0}]

	/****** Object:  Index [index_price]    Script Date: 2021/1/25 16:21:39 ******/
	IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_price')
	DROP INDEX [index_price] ON [dbo].[{0}]

	/****** Object:  Table [dbo].[{0}]    Script Date: 2021/1/25 16:21:39 ******/
	IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U'))
	DROP TABLE [dbo].[{0}]

	/****** Object:  Table [dbo].[{0}]    Script Date: 2021/1/25 16:21:39 ******/
	SET ANSI_NULLS ON

	SET QUOTED_IDENTIFIER ON

	SET ANSI_PADDING ON

	IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [dbo].[{0}](
		[Id] [bigint] IDENTITY(1,1) NOT NULL,
		[Market] [int] NOT NULL,
		[SharesCode] [varchar](20) NOT NULL,
		[Time] [datetime] NOT NULL,
		[TimeStr] [varchar](50) NOT NULL,
		[Price] [bigint] NOT NULL,
		[Volume] [int] NOT NULL,
		[Stock] [int] NOT NULL,
		[Type] [int] NOT NULL,
		[OrderIndex] [int] NOT NULL CONSTRAINT [DF_{0}_OrderIndex]  DEFAULT ((0)),
		[LastModified] [datetime] NOT NULL,
		[SharesInfo] [varchar](20) NULL,
		[SharesInfoNum] [int] NOT NULL CONSTRAINT [DF_{0}_SharesInfoNum]  DEFAULT ((0)),
		[IsTimeFirst] [bit] NOT NULL CONSTRAINT [DF_{0}_IsFirst]  DEFAULT ((0)),
		[IsTimeLast] [bit] NOT NULL CONSTRAINT [DF_{0}_IsLast]  DEFAULT ((0)),
	 CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	END

	SET ANSI_PADDING OFF

	/****** Object:  Index [index_isTimeFirst]    Script Date: 2021/1/25 16:21:39 ******/
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_isTimeFirst')
	CREATE NONCLUSTERED INDEX [index_isTimeFirst] ON [dbo].[{0}]
	(
		[IsTimeFirst] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	SET ANSI_PADDING ON

	/****** Object:  Index [index_isTimeLast]    Script Date: 2021/1/25 16:21:39 ******/
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_isTimeLast')
	CREATE NONCLUSTERED INDEX [index_isTimeLast] ON [dbo].[{0}]
	(
		[IsTimeLast] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	SET ANSI_PADDING ON

	/****** Object:  Index [index_price]    Script Date: 2021/1/25 16:21:39 ******/
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_price')
	CREATE NONCLUSTERED INDEX [index_price] ON [dbo].[{0}]
	(
		[Price] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	SET ANSI_PADDING ON


	/****** Object:  Index [index_key]    Script Date: 2021/1/25 16:21:39 ******/
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_key')
	CREATE NONCLUSTERED INDEX [index_key] ON [dbo].[{0}]
	(
		[Market] ASC,
		[SharesCode] ASC,
		[Time] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	/****** Object:  Index [index_orderindex]    Script Date: 2021/1/25 16:21:39 ******/
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_orderindex')
	CREATE NONCLUSTERED INDEX [index_orderindex] ON [dbo].[{0}]
	(
		[OrderIndex] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	SET ANSI_PADDING ON


	/****** Object:  Index [index_sharesinfo_time]    Script Date: 2021/1/25 16:21:39 ******/
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_sharesinfo_time')
	CREATE NONCLUSTERED INDEX [index_sharesinfo_time] ON [dbo].[{0}]
	(
		[Time] ASC,
		[SharesInfo] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	/****** Object:  Index [index_sharesinfonum_time]    Script Date: 2021/1/25 16:21:39 ******/
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_sharesinfonum_time')
	CREATE NONCLUSTERED INDEX [index_sharesinfonum_time] ON [dbo].[{0}]
	(
		[Time] ASC,
		[SharesInfoNum] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	/****** Object:  Index [index_time_orderindex]    Script Date: 2021/1/25 16:21:39 ******/
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'index_time_orderindex')
	CREATE NONCLUSTERED INDEX [index_time_orderindex] ON [dbo].[{0}]
	(
		[Time] ASC,
		[OrderIndex] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Market'))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'市场代码0深圳 1上海' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Market'

	IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'SharesCode'))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'股票代码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'SharesCode'

	IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Time'))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Time'

	IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'TimeStr'))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'时间字符串' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'TimeStr'

	IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Price'))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'价格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Price'

	IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Volume'))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'现量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Volume'

	IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Stock'))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'笔数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Stock'

	IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'Type'))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'类型0买入 1卖出 2中性' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'Type'

	IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'{0}', N'COLUMN',N'LastModified'))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'数据更新时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{0}', @level2type=N'COLUMN',@level2name=N'LastModified'
end", TempTableName);
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("启动有误,请重新运行");
                }
                finally 
                {
                    conn.Close();
                }
            }


            SysparUpdate();
            GetSharesHqHost();

            retryWait.Init();
            retryData.Init();
            cltData.Init();
            heartbeatWait.Init();
            UpdateWait.Init();


            for (int i = 0; i < hqClientCount; i++)
            {
                StartHqClient(-1);
            }
            StartSysparUpdateThread();
            StartHqClientRetryThread();
            StartHeartbeatThread();
        }

        /// <summary>
        /// 链接行情服务器
        /// </summary>
        private void StartHqClient(int client)
        {
            int hqClient = client;
            if (hqClient >= 0)
            {
                TradeX_M.TdxHq_Disconnect(hqClient);
                hqClient = -1;
            }
            StringBuilder sResult = new StringBuilder(1024 * 1024);
            StringBuilder sErrInfo = new StringBuilder(256);

            Random rd = new Random();
            int index = rd.Next(HostList.Count);
            var hqHost = HostList[index];
            string hqIp = hqHost.Ip;
            int hqPort = hqHost.Port;
            try
            {
                hqClient = TradeX_M.TdxHq_Connect(hqIp, hqPort, sResult, sErrInfo);
            }
            catch (Exception)
            { }

            if (hqClient >= 0)
            {
                AddHqClient(hqClient);
                Console.WriteLine("链接成功,hqClient：" + hqClient);
            }
            else
            {
                AddRetryClient(-1);
                Console.WriteLine("链接失败，继续重连:原因：" + sErrInfo.ToString());
            }
        }

        /// <summary>
        /// 获取行情链接地址
        /// </summary>
        private void GetSharesHqHost()
        {
            using (var db = new meal_ticketEntities())
            {
                var host = (from item in db.t_shares_hq_host
                            where item.Status == 1
                            select new HostInfo
                            {
                                Ip = item.IpAddress,
                                Port = item.Port
                            }).ToList();
                HostList = host;
            }
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            if (HqClientRetryThread != null)
            {
                retryWait.AddMessage(0);
                HqClientRetryThread.Join();
                retryWait.Release();

                do
                {
                    int clientId = -1;
                    if (!cltData.GetMessage(ref clientId, true))
                    { break; }

                    TradeX_M.TdxHq_Disconnect(clientId);
                } while (true);
                cltData.Release();

                do
                {
                    int clientId = -1;
                    if (!retryData.GetMessage(ref clientId, true))
                    { break; }

                    if (clientId >= 0)
                    {
                        TradeX_M.TdxHq_Disconnect(clientId);
                    }
                } while (true);
                retryData.Release();
            }

            if (HeartbeatThread != null)
            {
                heartbeatWait.AddMessage(0);
                HeartbeatThread.Join();
                heartbeatWait.Release();
            }

            if (SysparUpdateThread != null)
            {
                UpdateWait.AddMessage(0);
                SysparUpdateThread.Join();
                UpdateWait.Release();
            }
        }

        /// <summary>
        /// 重连线程
        /// </summary>
        private void StartHqClientRetryThread()
        {
            HqClientRetryThread = new Thread(() =>
            {
                do
                {
                    int msgId = 0;
                    if (retryWait.WaitMessage(ref msgId, 2000))
                    {
                        break;
                    }

                    RetryMsgPump();
                } while (true);
            });
            HqClientRetryThread.Start();
        }

        private void RetryMsgPump()
        {
            do
            {
                int clientId = -1;
                if (!retryData.GetMessage(ref clientId, true))
                {
                    break;
                };

                StartHqClient(clientId);
            } while (true);
        }

        /// <summary>
        /// 心跳线程
        /// </summary>
        private void StartHeartbeatThread()
        {
            HeartbeatThread = new Thread(() =>
            {
                do
                {
                    int msgId = 0;
                    if (heartbeatWait.WaitMessage(ref msgId, 5000))
                    {
                        break;
                    }
                    HeartbeatMsgPump();
                } while (true);
            });
            HeartbeatThread.Start();
        }

        private void HeartbeatMsgPump()
        {
            List<int> clientFails = new List<int>(), clientSuccess = new List<int>();
            do
            {
                int clientId = -1;
                if (!cltData.GetMessage(ref clientId, true))
                {
                    break;
                };

                short nCount = 0;
                StringBuilder sErrInfo = new StringBuilder(256);
                if (TradeX_M.TdxHq_GetSecurityCount(clientId, 0, ref nCount, sErrInfo))
                {
                    clientSuccess.Add(clientId);
                }
                else
                {
                    clientFails.Add(clientId);
                }
            } while (true);

            string successStr = "";
            for (int idx = 0; idx < clientSuccess.Count(); idx++)
            {
                AddHqClient(clientSuccess[idx]);
                successStr = successStr + "," + clientSuccess[idx];
            }


            string failStr = "";
            for (int idx = 0; idx < clientFails.Count(); idx++)
            {
                AddRetryClient(clientFails[idx]);
                failStr = failStr + "," + clientFails[idx];
            }
        }

        /// <summary>
        /// 启动系统参数更新线程
        /// </summary>
        private void StartSysparUpdateThread()
        {
            SysparUpdateThread = new Thread(() =>
            {
                do
                {
                    int msgId = 0;
                    if (UpdateWait.WaitMessage(ref msgId, 600000))
                    {
                        break;
                    }
                } while (true);

                SysparUpdate();
            });
            SysparUpdateThread.Start();
        }

        private void SysparUpdate()
        {
            using (var db = new meal_ticketEntities())
            {
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "NewTransactiondataPar"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        if (sysValue.TransactionDataCount != null && sysValue.TransactionDataCount <= 2000 && sysValue.TransactionDataCount >= 1)
                        {
                            NewTransactionDataCount = sysValue.TransactionDataCount;
                        }
                        if (sysValue.SendPeriodTime != null)
                        {
                            NewTransactionDataSendPeriodTime = sysValue.SendPeriodTime;
                        }
                        if (sysValue.TransactiondataSleepTime != null && sysValue.TransactiondataSleepTime > 0)
                        {
                            NewTransactiondataSleepTime = sysValue.TransactiondataSleepTime;
                        }
                        if (sysValue.TransactiondataStartHour != null && sysValue.TransactiondataStartHour >= 16 && sysValue.TransactiondataStartHour <= 23)
                        {
                            NewTransactiondataStartHour = sysValue.TransactiondataStartHour;
                        }
                        if (sysValue.TransactiondataEndHour != null && sysValue.TransactiondataEndHour >= 16 && sysValue.TransactiondataEndHour <= 23)
                        {
                            NewTransactiondataEndHour = sysValue.TransactiondataEndHour;
                        }
                        if (sysValue.NewTransactionDataTrendHandlerCount != null && sysValue.NewTransactionDataTrendHandlerCount >= 0)
                        {
                            NewTransactionDataTrendHandlerCount = sysValue.NewTransactionDataTrendHandlerCount;
                        }
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "HqServerPar"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        NewTransactionDataRunStartTime = sysValue.RunStartTime;
                        NewTransactionDataRunEndTime = sysValue.RunEndTime;
                    }
                }
                catch { }
            }
        }
    }
}
