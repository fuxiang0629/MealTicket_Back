using FXCommon.Common;
using FXCommon.Database;
using MealTicket_Web_Handler.Runner;
using Newtonsoft.Json;
using StockTrendMonitor.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public sealed class Singleton
    {
        /// <summary>
        /// 同步交易锁
        /// </summary>
        private object SyncroLockObj = new object();

        /// <summary>
        /// 同步数据列表
        /// </summary>
        private List<TradeAutoBuyCondition> SyncroList = new List<TradeAutoBuyCondition>();

        /// <summary>
        /// 单例对象
        /// </summary>
        private static readonly Singleton instance = new Singleton();

        /// <summary>
        /// meal_ticket数据库连接字符串
        /// </summary>
        public string ConnectionString_meal_ticket;

        /// <summary>
        /// meal_ticket_shares_transactiondata数据库连接字符串
        /// </summary>
        public string ConnectionString_meal_ticket_shares_transactiondata;

        /// <summary>
        /// 深圳股票匹配
        /// </summary>
        public string SharesCodeMatch0 = "(^00.*)|(^30.*)";

        /// <summary>
        /// 上海股票匹配
        /// </summary>
        public string SharesCodeMatch1 = "(^6.*)";

        #region===再触发设置===
        public int MinPushTimeInterval = 180;
        public int MinPushRateInterval = 60;
        #endregion

        #region===清理分笔数据参数===
        public int ClearTransactiondataSleepTime = 1800000;
        public int ClearTransactiondataStartHour = 3;
        public int ClearTransactiondataEndHour = 8;
        #endregion

        #region====新分笔成交数据====
        public int NewTransactionDataCount = 50;
        public int NewTransactionDataSendPeriodTime = 0;//更新间隔
        public int NewTransactionDataRunStartTime = -180;//运行时间比交易时间提前秒数
        public int NewTransactionDataRunEndTime = 180;//运行时间比交易时间滞后秒数
        public int NewTransactiondataSleepTime = 10000;//每天执行时的间隔时间
        public int NewTransactiondataStartHour = 0;//每天执行开始时间
        public int NewTransactiondataEndHour = 0;//每天执行结束时间
        #endregion



        /// <summary>
        /// 自动买入最大任务数量
        /// </summary>
        public int AutoBuyTaskMaxCount = 50;

        /// <summary>
        /// adb.net操作对象
        /// </summary>
        public SqlHelper sqlHelper;

        /// <summary>
        /// 系统参数更新线程
        /// </summary>
        Thread SysparUpdateThread;

        /// <summary>
        /// 系统参数更新线程等待队列
        /// </summary>
        private ThreadMsgTemplate<int> UpdateWait = new ThreadMsgTemplate<int>();

        #region====走势分析====
        /// <summary>
        /// 走势分析间隔时间
        /// </summary>
        public int TrendAnalyseSleepTime = 3000;

        /// <summary>
        /// 每批次分析股票数量
        /// </summary>
        public int NewTransactionDataTrendHandlerCount = 10;

        /// <summary>
        /// 处理线程数量
        /// </summary>
        public int handlerThreadCount;

        /// <summary>
        /// 数据库链接字符串1
        /// </summary>
        public string connString_meal_ticket;

        /// <summary>
        /// 数据库链接字符串2
        /// </summary>
        public string connString_transactiondata;

        /// <summary>
        /// 心跳时间（秒）
        /// </summary>
        public int HeartSecond;

        /// <summary>
        /// 走势分析对象
        /// </summary>
        public StockMonitorCore m_stockMonitor;

        /// <summary>
        /// 队列对象
        /// </summary>
        public MQHandler mqHandler;
        #endregion

        // 显式静态构造函数告诉C＃编译器
        // 不要将类型标记为BeforeFieldInit
        static Singleton()
        {
            
        }

        private Singleton()
        {
            handlerThreadCount = 10;
            connString_meal_ticket = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            connString_transactiondata = ConfigurationManager.ConnectionStrings["ConnectionString_data"].ConnectionString;
            HeartSecond = 60;

            ConnectionString_meal_ticket = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            ConnectionString_meal_ticket_shares_transactiondata = ConfigurationManager.ConnectionStrings["ConnectionString_data"].ConnectionString;
            sqlHelper = new SqlHelper(ConnectionString_meal_ticket);
            SysparUpdate();



            m_stockMonitor = new StockMonitorCore();
            int error=m_stockMonitor.InitCore(ConnectionString_meal_ticket + "|" + ConnectionString_meal_ticket_shares_transactiondata, 0,"");
            if (error != 0)
            {
                Logger.WriteFileLog("链接字符串错误",null);
            }
            mqHandler = new MQHandler();

            UpdateWait.Init();
            StartSysparUpdateThread();
        }

        public static Singleton Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose() 
        {
            if (SysparUpdateThread != null)
            {
                UpdateWait.AddMessage(0);
                SysparUpdateThread.Join();
                UpdateWait.Release();
            }
            if (m_stockMonitor != null)
            {
                m_stockMonitor.ReleaseCore();
            }
            if (mqHandler != null)
            {
                mqHandler.Dispose(true);
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

                    try
                    {
                        SysparUpdate();
                    }
                    catch (Exception) { }
                } while (true);
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
                                  where item.ParamName == "SharesCodeMatch0"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        this.SharesCodeMatch0 = sysPar.ParamValue;
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "SharesCodeMatch1"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        this.SharesCodeMatch1 = sysPar.ParamValue;
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "TrendAnalyse"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        if (sysValue.MinPushTimeInterval > 0 || sysValue.MinPushTimeInterval == -1)
                        {
                            MinPushTimeInterval = sysValue.MinPushTimeInterval;
                        }
                        if (sysValue.MinPushRateInterval > 0 || sysValue.MinPushRateInterval == -1)
                        {
                            MinPushRateInterval = sysValue.MinPushRateInterval;
                        }
                        if (sysValue.HandlerThreadCount > 0 && sysValue.HandlerThreadCount <= 100)
                        {
                            handlerThreadCount = sysValue.HandlerThreadCount;
                        }
                        if (sysValue.HeartOverSecond > 3)
                        {
                            HeartSecond = sysValue.HeartOverSecond;
                        }
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "ClearTransactiondataPar"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        if (sysValue.ClearTransactiondataSleepTime>=1800000)
                        {
                            this.ClearTransactiondataSleepTime = sysValue.ClearTransactiondataSleepTime;
                        }
                        if (sysValue.ClearTransactiondataStartHour > 0 && sysValue.ClearTransactiondataStartHour<6)
                        {
                            this.ClearTransactiondataStartHour = sysValue.ClearTransactiondataStartHour;
                        }
                        if (sysValue.ClearTransactiondataEndHour > 3 && sysValue.ClearTransactiondataEndHour < 9)
                        {
                            this.ClearTransactiondataEndHour = sysValue.ClearTransactiondataEndHour;
                        }
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "AutoBuyTaskMaxCount"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        this.AutoBuyTaskMaxCount = int.Parse(sysPar.ParamValue);
                    }
                }
                catch { }
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
                        if (sysValue.TrendAnalyseSleepTime != null && sysValue.TrendAnalyseSleepTime > 100)
                        {
                            TrendAnalyseSleepTime = sysValue.TrendAnalyseSleepTime;
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

        /// <summary>
        /// 获取同步交易数据
        /// </summary>
        /// <returns></returns>
        public List<TradeAutoBuyCondition> GetSyncroList()
        {
            lock (SyncroLockObj)
            {
                List<TradeAutoBuyCondition> temp = new List<TradeAutoBuyCondition>();
                temp.AddRange(SyncroList);
                SyncroList.Clear();
                return temp;
            }
        }

        /// <summary>
        /// 添加同步交易数据
        /// </summary>
        /// <returns></returns>
        public void AddSyncro(TradeAutoBuyCondition item)
        {
            lock (SyncroLockObj)
            {
                SyncroList.Add(item);
            }
        }
    }
}
