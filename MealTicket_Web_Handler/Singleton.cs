using FXCommon.Common;
using FXCommon.Database;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Runner;
using MealTicket_Web_Handler.session;
using MealTicket_Web_Handler.Transactiondata;
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
    /// <summary>
    /// 交易时间结构体
    /// </summary>
    public struct TradeTime 
    {
        /// <summary>
        /// 最小时间
        /// </summary>
        public TimeSpan MinTime { get; set; }

        /// <summary>
        /// 最大时间
        /// </summary>
        public TimeSpan MaxTime { get; set; }
    }

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
        public TimeSpan ClearTransactiondataStartHour = TimeSpan.Parse("03:00:00");
        public TimeSpan ClearTransactiondataEndHour = TimeSpan.Parse("08:00:00");
        #endregion

        #region====新分笔成交数据====
        public int NewTransactionDataSendPeriodTime = 0;//更新间隔
        public int NewTransactionDataRunStartTime = -180;//运行时间比交易时间提前秒数
        public int NewTransactionDataRunEndTime = 180;//运行时间比交易时间滞后秒数
        public int NewTransactiondataTaskTimeOut = 15000;//任务超时时间
        public int NewTransactiondataTaskTimeOut2 = 45000;//任务超时时间2
        public int NewTransactiondataTaskUpdateCountOnce = 4000;//单次写分时数据数量
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
        public int NewTransactionDataTrendHandlerCount = 200;

        /// <summary>
        /// 每批次分析股票数量
        /// </summary>
        public int NewTransactionDataTrendHandlerCount2 = 50;

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
            int error=m_stockMonitor.InitCore(ConnectionString_meal_ticket + "|" + ConnectionString_meal_ticket_shares_transactiondata, -1, "");
            if (error != 0)
            {
                Logger.WriteFileLog("链接字符串错误",null);
            }

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
                mqHandler.Dispose();
            }
            if (_transactionDataTask != null)
            {
                _transactionDataTask.Dispose();
            }
            if (_BuyTipSession != null)
            {
                _BuyTipSession.Dispose();
            }
            if (_SharesQuotesSession != null)
            {
                _SharesQuotesSession.Dispose();
            }
            if (_PlateRateSession != null)
            {
                _PlateRateSession.Dispose();
            }
            if (_AccountTrendTriSession != null)
            {
                _AccountTrendTriSession.Dispose();
            }
            if (_SharesBaseSession != null)
            {
                _SharesBaseSession.Dispose();
            }
            if (_AccountRiseLimitTriSession != null)
            {
                _AccountRiseLimitTriSession.Dispose();
            }
            if (_SharesQuotesDateSession != null)
            {
                _SharesQuotesDateSession.Dispose();
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
                        SharesCodeMatch0 = sysPar.ParamValue;
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
                        SharesCodeMatch1 = sysPar.ParamValue;
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
                        int tempMinPushTimeInterval = sysValue.MinPushTimeInterval;
                        if (tempMinPushTimeInterval > 0 || tempMinPushTimeInterval == -1)
                        {
                            MinPushTimeInterval = tempMinPushTimeInterval;
                        }
                        int tempMinPushRateInterval = sysValue.MinPushRateInterval;
                        if (tempMinPushRateInterval > 0 || tempMinPushRateInterval == -1)
                        {
                            MinPushRateInterval = tempMinPushRateInterval;
                        }
                        int tempHandlerThreadCount = sysValue.HandlerThreadCount;
                        if (tempHandlerThreadCount > 0 && tempHandlerThreadCount <= 100)
                        {
                            handlerThreadCount = tempHandlerThreadCount;
                        }
                        int tempHeartSecond = sysValue.HeartOverSecond;
                        if (tempHeartSecond > 3)
                        {
                            HeartSecond = tempHeartSecond;
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
                        string tempClearTransactiondataStartHour = sysValue.ClearTransactiondataStartHour;
                        if (!TimeSpan.TryParse(tempClearTransactiondataStartHour,out ClearTransactiondataStartHour))
                        {
                            ClearTransactiondataStartHour = TimeSpan.Parse("03:00:00"); ;
                        }
                        string tempClearTransactiondataEndHour = sysValue.ClearTransactiondataEndHour;
                        if (!TimeSpan.TryParse(tempClearTransactiondataEndHour, out ClearTransactiondataEndHour))
                        {
                            ClearTransactiondataEndHour = TimeSpan.Parse("08:00:00"); ;
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
                        AutoBuyTaskMaxCount = int.Parse(sysPar.ParamValue);
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
                        int tempSendPeriodTime = sysValue.SendPeriodTime;
                        if (tempSendPeriodTime != null)
                        {
                            NewTransactionDataSendPeriodTime = tempSendPeriodTime;
                        }
                        int tempNewTransactionDataTrendHandlerCount = sysValue.NewTransactionDataTrendHandlerCount;
                        if (tempNewTransactionDataTrendHandlerCount != null && tempNewTransactionDataTrendHandlerCount >= 0)
                        {
                            NewTransactionDataTrendHandlerCount = tempNewTransactionDataTrendHandlerCount;
                        }
                        int tempNewTransactionDataTrendHandlerCount2 = sysValue.NewTransactionDataTrendHandlerCount2;
                        if (tempNewTransactionDataTrendHandlerCount2 != null && tempNewTransactionDataTrendHandlerCount2 >= 0)
                        {
                            NewTransactionDataTrendHandlerCount2 = tempNewTransactionDataTrendHandlerCount2;
                        }
                        int tempTrendAnalyseSleepTime = sysValue.TrendAnalyseSleepTime;
                        if (tempTrendAnalyseSleepTime != null && tempTrendAnalyseSleepTime > 100)
                        {
                            TrendAnalyseSleepTime = tempTrendAnalyseSleepTime;
                        }
                        int tempNewTransactiondataTaskTimeOut = sysValue.NewTransactiondataTaskTimeOut;
                        if (tempNewTransactiondataTaskTimeOut != null && tempNewTransactiondataTaskTimeOut > 0 && tempNewTransactiondataTaskTimeOut < 300000)
                        {
                            NewTransactiondataTaskTimeOut = tempNewTransactiondataTaskTimeOut;
                        }
                        int tempNewTransactiondataTaskTimeOut2 = sysValue.tempNewTransactiondataTaskTimeOut2;
                        if (tempNewTransactiondataTaskTimeOut2 != null && tempNewTransactiondataTaskTimeOut2 > 0 && tempNewTransactiondataTaskTimeOut2 < 600000)
                        {
                            NewTransactiondataTaskTimeOut2 = tempNewTransactiondataTaskTimeOut2;
                        }
                        int tempNewTransactiondataTaskUpdateCountOnce = sysValue.tempNewTransactiondataTaskUpdateCountOnce;
                        if (tempNewTransactiondataTaskUpdateCountOnce != null && tempNewTransactiondataTaskUpdateCountOnce > 0)
                        {
                            NewTransactiondataTaskUpdateCountOnce = tempNewTransactiondataTaskUpdateCountOnce;
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

            _tradeTime = GetTradeCloseTime();
            SharesTrendParSession = GetSharesTrendParSession();
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



        public TradeTime _tradeTime;//闭市时间

        /// <summary>
        /// 获取交易闭市时间
        /// </summary>
        private TradeTime GetTradeCloseTime() 
        {
            using (var db = new meal_ticketEntities())
            {
                var tradeTime = (from item in db.t_shares_limit_time
                                 select item).FirstOrDefault();
                if (tradeTime == null)
                {
                    return GetDefaultTradeCloseTime();
                }
                string[] timeArr=tradeTime.Time2.Split(',');
                TimeSpan? result_maxTime = null;
                TimeSpan? result_minTime = null;
                for (int i = 0; i < timeArr.Length; i++)
                {
                    string[] spanArr=timeArr[i].Split('-');
                    TimeSpan _minTime = TimeSpan.Parse(spanArr[0]);
                    TimeSpan _maxTime = TimeSpan.Parse(spanArr[1]);
                    if (result_maxTime == null || result_maxTime.Value < _maxTime)
                    {
                        result_maxTime = _maxTime;
                    }
                    if (result_minTime == null || result_minTime.Value > _minTime)
                    {
                        result_minTime = _minTime;
                    }
                }
                if (result_maxTime == null || result_minTime == null)
                {
                    return GetDefaultTradeCloseTime();
                }
                return new TradeTime
                {
                    MaxTime = result_maxTime.Value,
                    MinTime = result_minTime.Value
                };
            }
        }

        private TradeTime GetDefaultTradeCloseTime()
        {
            return new TradeTime
            {
                MaxTime = TimeSpan.Parse("09:30:00"),
                MinTime = TimeSpan.Parse("09:25:00")
            };
        }


        public List<SharesTrendPar> SharesTrendParSession;
        private List<SharesTrendPar> GetSharesTrendParSession()
        {
            string sql = @"select Market,SharesCode,TrendId,ParamsInfo
  from t_shares_monitor t with(nolock)
  inner join t_shares_monitor_trend_rel t1 with(nolock) on t.Id=t1.MonitorId
  inner join t_shares_monitor_trend_rel_par t2 with(nolock) on t1.Id=t2.RelId
  where t.[Status]=1 and t1.[Status]=1";
            using (var db = new meal_ticketEntities())
            {
                var sqlResult = db.Database.SqlQuery<SharesTrendPar>(sql).ToList();
                var result = (from item in sqlResult
                              group item by new { item.Market, item.SharesCode, item.TrendId } into g
                              select new SharesTrendPar
                              {
                                  SharesCode = g.Key.SharesCode,
                                  Market = g.Key.Market,
                                  TrendId = g.Key.TrendId,
                                  ParamsList = g.Select(e => e.ParamsInfo).ToList()
                              }).ToList();
                return result;
            }
        }



        /// <summary>
        /// 队列对象
        /// </summary>
        public MQHandler mqHandler;
        /// <summary>
        /// 启动Mq队列
        /// </summary>
        public MQHandler StartMqHandler()
        {
            string hostName = ConfigurationManager.AppSettings["MQ_HostName"];
            int port = int.Parse(ConfigurationManager.AppSettings["MQ_Port"]);
            string userName = ConfigurationManager.AppSettings["MQ_UserName"];
            string password = ConfigurationManager.AppSettings["MQ_Password"];
            string virtualHost = ConfigurationManager.AppSettings["MQ_VirtualHost"];
            mqHandler = new MQHandler(hostName, port, userName, password, virtualHost);
            mqHandler.ListenQueueName = "TransactionDataTrendAnalyse";//设置监听队列
            return mqHandler;
        }

        /// <summary>
        /// 分笔数据处理任务
        /// </summary>
        public TransactionDataTask _transactionDataTask;

        public TransactionDataTask StartTransactionDataTask() 
        {
            _transactionDataTask = new TransactionDataTask();
            _transactionDataTask.Init();
            return _transactionDataTask;
        }

        #region=========session缓存========
        public BuyTipSession _BuyTipSession;
        public SharesQuotesSession _SharesQuotesSession;
        public SharesBaseSession _SharesBaseSession;
        public PlateRateSession _PlateRateSession;
        public AccountTrendTriSession _AccountTrendTriSession;
        public AccountRiseLimitTriSession _AccountRiseLimitTriSession;
        public SharesQuotesDateSession _SharesQuotesDateSession;
        #endregion
    }
}
