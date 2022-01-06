using FXCommon.Common;
using FXCommon.Database;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Runner;
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

        /// <summary>
        /// 板块排行展示
        /// </summary>
        public int PlateRankShow = 10;

        /// <summary>
        /// 配置web端展示前几日成交量/成交额/换手
        /// </summary>
        public int QuotesDaysShow = 20;


        //搜索线程数
        public int TempThreadCount = 4;
        public int ConditionThreadCount = 16;

        /// <summary>
        /// 监控搜索间隔
        /// </summary>
        public int SearchInterval = 15000;

        #region===再触发设置===
        public int MinPushTimeInterval = 180;
        public int MinPushRateInterval = 60;
        public int MinPushTodayPrice = -1; 
        #endregion

        #region===清理分笔数据参数===
        public TimeSpan ClearTransactiondataStartHour = TimeSpan.Parse("03:00:00");
        public TimeSpan ClearTransactiondataEndHour = TimeSpan.Parse("08:00:00");
        #endregion

        #region====新分笔成交数据====
        public int IsOpen = 1;
        public int NewTransactionDataSendPeriodTime = 0;//更新间隔
        public int NewTransactionDataRunStartTime = -180;//运行时间比交易时间提前秒数
        public int NewTransactionDataRunEndTime = 180;//运行时间比交易时间滞后秒数
        public int NewTransactiondataTaskTimeOut = 15000;//任务超时时间
        public int NewTransactiondataTaskTimeOut2 = 45000;//任务超时时间2
        public int NewTransactiondataTaskUpdateCountOnce = 5000;//单次写分时数据数量
        #endregion

        /// <summary>
        /// 板块指数加权类型
        /// </summary>
        public Dictionary<int, int> PlateIndexTypeDic = new Dictionary<int, int>();

        public double PlateIndexRatio = 4;

        public int SharesStockCal_MinMinute = 5;
        public int SharesStockCal_IntervalMinute = 15;
        public int SharesStockCal_Type = 1;

        /// <summary>
        ///分时叠加参数
        /// </summary>
        public int MaxCheckedCount = 5;
        public string[] MtLineColorList = new[] { "#1878dd", "#d628d9", "#abb511", "#d51123", "#21b548" };

        //板块监控涨跌幅排行颜色列表
        public string[] RiseRankBgColorList = new[] { "#ff0019", "#c3535e", "#d7a1a6" };
        public string[] DownRankBgColorList = new[] { "#52f900", "#5f8f48", "#accf9b" };
        public bool PlateRiseRateIsReal = false;
        public int[] EnergyIndexTypeList = new[] { 1, 2, 3, 4 };

        /// <summary>
        /// 自动买入最大任务数量
        /// </summary>
        public int AutoBuyTaskMaxCount = 50;

        /// <summary>
        /// 板块标记计算间隔时间（毫秒）
        /// </summary>
        public int PlateTagCalIntervalTime = 3000;

        /// <summary>
        /// 中军计算市值排名取前x/万
        /// </summary>
        public int MainArmyRankRate = 5000;

        /// <summary>
        /// 走势最像天数
        /// </summary>
        public int TrendLikeDays = 20;

        public int[] SharesLeaderType = new[] { 1, 2, 3 };
        public int[] SharesLeaderShowType = new[] { 1, 2, 3 };
        public int[] SharesLeaderDaysType = new [] { 1, 2, 3, 4 };

        /// <summary>
        /// adb.net操作对象
        /// </summary>
        public SqlHelper sqlHelper;

        /// <summary>
        /// 板块监控计算对象
        /// </summary>
        public PlateMonitorHelper plateMonitorHelper = null;

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

        #region===交易时间值====
        public TimeSpan Time1Start = TimeSpan.Parse("09:15:00");
        public int Time1StartInt = 915;
        public TimeSpan Time1End = TimeSpan.Parse("09:25:00");
        public int Time1EndInt = 925;
        public TimeSpan Time2Start = TimeSpan.Parse("09:25:00");
        public int Time2StartInt = 925;
        public TimeSpan Time2End = TimeSpan.Parse("09:30:00");
        public int Time2EndInt = 930;
        public TimeSpan Time3Start1 = TimeSpan.Parse("09:30:00");
        public int Time3Start1Int = 930;
        public TimeSpan Time3End1 = TimeSpan.Parse("11:30:00");
        public int Time3End1Int = 1130;
        public TimeSpan Time3Start2 = TimeSpan.Parse("13:00:00");
        public int Time3Start2Int = 1300;
        public TimeSpan Time3End2 = TimeSpan.Parse("15:00:00");
        public int Time3End2Int = 1500;
        public TimeSpan Time4Start = TimeSpan.Parse("14:57:00");
        public int Time4StartInt = 1457;
        public TimeSpan Time4End = TimeSpan.Parse("15:00:00");
        public int Time4EndInt = 1500;
        public TimeSpan Time5Start = TimeSpan.Parse("09:00:00");
        public int Time5StartInt = 900;
        public TimeSpan Time5End = TimeSpan.Parse("09:15:00");
        public int Time5EndInt = 915;
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

            PlateIndexTypeDic.Clear();
            PlateIndexTypeDic.Add(0, 2);//其他 1加权 2不加权
            PlateIndexTypeDic.Add(1, 2);//板块指数
            PlateIndexTypeDic.Add(2, 1);//市场指数
            PlateIndexTypeDic.Add(3, 1);//成分指数
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
            if (_SharesQuotesSession != null)
            {
                _SharesQuotesSession.Dispose();
            }
            if (_SharesBaseSession != null)
            {
                _SharesBaseSession.Dispose();
            }
            if (_SharesPlateSession != null)
            {
                _SharesPlateSession.Dispose();
            }
            if (_SharesPlateRelSession != null)
            {
                _SharesPlateRelSession.Dispose();
            }
            if (_SharesPlateQuotesSession != null)
            {
                _SharesPlateQuotesSession.Dispose();
            }
            if (_SharesPlateQuotesSession_New != null)
            {
                _SharesPlateQuotesSession_New.Dispose();
            }
            if (_BuyTipSession != null)
            {
                _BuyTipSession.Dispose();
            }
            if (_AccountRiseLimitTriSession != null)
            {
                _AccountRiseLimitTriSession.Dispose();
            }
            if (_AccountTrendTriSession != null)
            {
                _AccountTrendTriSession.Dispose();
            }
            if (_AccountSearchTriSession != null)
            {
                _AccountSearchTriSession.Dispose();
            }
            if (_SharesHisRiseRateSession != null)
            {
                _SharesHisRiseRateSession.Dispose();
            }
            if (quotesMQHandler != null)
            {
                quotesMQHandler.Dispose();
            }
            if (sharesPlateTagMQHandler != null)
            {
                sharesPlateTagMQHandler.Dispose();
            }
            if (sessionHandler != null)
            {
                sessionHandler.Dispose();
            }


            if (plateMonitorHelper != null)
            {
                plateMonitorHelper.Dispose();
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
                        int tempMinPushTodayPrice = sysValue.MinPushTodayPrice;
                        if (tempMinPushTodayPrice >= 0 || tempMinPushTodayPrice == -1)
                        {
                            MinPushTodayPrice = tempMinPushTodayPrice;
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
                        
                        int? tempIsOpen = sysValue.IsOpen;
                        if (tempIsOpen>0)
                        {
                            IsOpen = tempIsOpen.Value;
                        }
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
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "PlateRankShow"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        PlateRankShow = int.Parse(sysPar.ParamValue);
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "QuotesDaysShow"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        QuotesDaysShow = int.Parse(sysPar.ParamValue);
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "SearchMonitor"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        int tempSearchInterval = sysValue.SearchInterval;
                        if (tempSearchInterval > 0)
                        {
                            SearchInterval = tempSearchInterval;
                        }
                        int tempMaxTrendCheckTaskCount= sysValue.TempThreadCount;
                        if (tempMaxTrendCheckTaskCount > 0)
                        {
                            TempThreadCount = tempMaxTrendCheckTaskCount;
                        }
                        int tempConditionThreadCount = sysValue.ConditionThreadCount;
                        if (tempConditionThreadCount > 0)
                        {
                            ConditionThreadCount = tempConditionThreadCount;
                        }
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "IndexInitValue"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        int OtherIndex_KLineType = sysValue.OtherIndex_KLineType;
                        int PlateIndex_KLineType = sysValue.PlateIndex_KLineType;
                        int MarketIndex_KLineType = sysValue.MarketIndex_KLineType;
                        int UnitIndex_KLineType = sysValue.UnitIndex_KLineType;
                        PlateIndexRatio = sysValue.PlateIndexRatio;

                        PlateIndexTypeDic[0] = OtherIndex_KLineType;
                        PlateIndexTypeDic[1] = PlateIndex_KLineType;
                        PlateIndexTypeDic[2] = MarketIndex_KLineType;
                        PlateIndexTypeDic[3] = UnitIndex_KLineType;
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "PlateTagCalPar"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        int tempPlateTagCalIntervalTime = sysValue.PlateTagCalIntervalTime;
                        if (tempPlateTagCalIntervalTime > 0)
                        {
                            PlateTagCalIntervalTime = tempPlateTagCalIntervalTime;
                        }
                        int tempTrendLikeDays = sysValue.TrendLikeDays;
                        if (tempTrendLikeDays > 0)
                        {
                            TrendLikeDays = tempTrendLikeDays;
                        }
                        int tempMainArmyRankRate = sysValue.MainArmyRankRate;
                        if (tempMainArmyRankRate > 0)
                        {
                            MainArmyRankRate = tempMainArmyRankRate;
                        }
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "VolumeRatePar"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        int tempSharesStockCal_MinMinute = sysValue.SharesStockCal_MinMinute;
                        if (tempSharesStockCal_MinMinute > 0)
                        {
                            SharesStockCal_MinMinute = tempSharesStockCal_MinMinute;
                        }
                        int tempSharesStockCal_IntervalMinute = sysValue.SharesStockCal_IntervalMinute;
                        if (tempSharesStockCal_IntervalMinute > 0)
                        {
                            SharesStockCal_IntervalMinute = tempSharesStockCal_IntervalMinute;
                        }
                        int tempSharesStockCal_Type = sysValue.SharesStockCal_Type;
                        if (tempSharesStockCal_Type > 0)
                        {
                            SharesStockCal_Type = tempSharesStockCal_Type;
                        }
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "MtlineSuperpositionPar"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        int tempMaxCheckedCount = sysValue.MaxCheckedCount;
                        if (tempMaxCheckedCount >= 0)
                        {
                            MaxCheckedCount = tempMaxCheckedCount;
                        }
                        string mtLineColorListStr = sysValue.MtLineColorList;
                        MtLineColorList = mtLineColorListStr.Split(';');
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "PlateMonitorPar"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);

                        string riseRankBgColorListStr = sysValue.RiseRankBgColorList;
                        RiseRankBgColorList = riseRankBgColorListStr.Split(';');

                        string downRankBgColorListStr = sysValue.DownRankBgColorList;
                        DownRankBgColorList = downRankBgColorListStr.Split(';');

                        List<int> tempEnergyIndexTypeList = new List<int>();
                        if (sysValue.EnergyIndexTypeList != null)
                        {
                            foreach (int tem in sysValue.EnergyIndexTypeList)
                            {
                                tempEnergyIndexTypeList.Add(tem);
                            }
                        }
                        EnergyIndexTypeList = tempEnergyIndexTypeList.ToArray();

                        PlateRiseRateIsReal = sysValue.IsReal;
                    }
                }
                catch { }
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "SharesLeaderPar"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);

                        List<int> tempSharesLeaderType = new List<int>();
                        if (sysValue.TypeList != null)
                        {
                            foreach (int tem in sysValue.TypeList)
                            {
                                tempSharesLeaderType.Add(tem);
                            }
                        }
                        SharesLeaderType = tempSharesLeaderType.ToArray();

                        List<int> tempSharesLeaderShowType = new List<int>();
                        if (sysValue.ShowTypeList != null)
                        {
                            foreach (int tem in sysValue.ShowTypeList)
                            {
                                tempSharesLeaderShowType.Add(tem);
                            }
                        }
                        SharesLeaderShowType = tempSharesLeaderShowType.ToArray();
                        
                        List<int> tempSharesLeaderDaysType = new List<int>();
                        if (sysValue.DaysTypeList != null)
                        {
                            foreach (int tem in sysValue.DaysTypeList)
                            {
                                tempSharesLeaderDaysType.Add(tem);
                            }
                        }
                        SharesLeaderDaysType = tempSharesLeaderDaysType.ToArray();
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
        /// 队列对象
        /// </summary>
        public SharesQuotesMQHandler quotesMQHandler;
        /// <summary>
        /// 启动Mq队列
        /// </summary>
        public SharesQuotesMQHandler StartQuotesMQHandler()
        {
            string hostName = ConfigurationManager.AppSettings["MQ_HostName"];
            int port = int.Parse(ConfigurationManager.AppSettings["MQ_Port"]);
            string userName = ConfigurationManager.AppSettings["MQ_UserName"];
            string password = ConfigurationManager.AppSettings["MQ_Password"];
            string virtualHost = ConfigurationManager.AppSettings["MQ_VirtualHost"];
            quotesMQHandler = new SharesQuotesMQHandler(hostName, port, userName, password, virtualHost);
            quotesMQHandler.ListenQueueName = "TransactionDataQuotes";//设置监听队列
            return quotesMQHandler;
        }

        /// <summary>
        /// 队列对象
        /// </summary>
        public SharesPlateTagMQHandler sharesPlateTagMQHandler;
        /// <summary>
        /// 启动Mq队列
        /// </summary>
        public SharesPlateTagMQHandler StartSharesPlateTagMQHandler()
        {
            string hostName = ConfigurationManager.AppSettings["MQ_HostName"];
            int port = int.Parse(ConfigurationManager.AppSettings["MQ_Port"]);
            string userName = ConfigurationManager.AppSettings["MQ_UserName"];
            string password = ConfigurationManager.AppSettings["MQ_Password"];
            string virtualHost = ConfigurationManager.AppSettings["MQ_VirtualHost"];
            sharesPlateTagMQHandler = new SharesPlateTagMQHandler(hostName, port, userName, password, virtualHost);
            sharesPlateTagMQHandler.ListenQueueName = "SharesPlateTagNotice";//设置监听队列
            return sharesPlateTagMQHandler;
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

        public void Init() 
        {
            sessionHandler = new SessionHandler();
            sessionHandler.UpdateSessionManual();
            sessionHandler.StartUpdate();

            m_stockMonitor = new StockMonitorCore();
            int error = m_stockMonitor.InitCore(ConnectionString_meal_ticket + "|" + ConnectionString_meal_ticket_shares_transactiondata, -1, "");
            if (error != 0)
            {
                Logger.WriteFileLog("链接字符串错误", null);
            }

            UpdateWait.Init();
            StartSysparUpdateThread();

            var mqHandler = StartMqHandler();//生成Mq队列对象
            var transactionDataTask = StartTransactionDataTask();
            transactionDataTask.DoTask();
            mqHandler.StartListen();//启动队列监听


            var quotesMQHandler = StartQuotesMQHandler();//生成Mq队列对象
            quotesMQHandler.StartListen();
            var sharesPlateTagMQHandler = StartSharesPlateTagMQHandler();//生成Mq队列对象
            sharesPlateTagMQHandler.StartListen();

            _SharesQuotesSession.StartUpdate(3000);
            _SharesBaseSession.StartUpdate(600000);
            _SharesPlateSession.StartUpdate(600000);
            _SharesPlateRelSession.StartUpdate(600000);
            _SharesPlateQuotesSession.StartUpdate(3000);
            _SharesPlateQuotesSession_New.StartUpdate(3000);
            _BuyTipSession.StartUpdate(1000);
            _AccountRiseLimitTriSession.StartUpdate(3000);
            _AccountTrendTriSession.StartUpdate(3000);
            _AccountSearchTriSession.StartUpdate(15000);
            _SharesHisRiseRateSession.StartUpdate(600000);
        }

        #region===缓存===
        public SharesQuotesSession _SharesQuotesSession = new SharesQuotesSession();
        public SharesBaseSession _SharesBaseSession = new SharesBaseSession();
        public SharesPlateSession _SharesPlateSession = new SharesPlateSession();
        public SharesPlateRelSession _SharesPlateRelSession = new SharesPlateRelSession();
        public SharesPlateQuotesSession _SharesPlateQuotesSession = new SharesPlateQuotesSession();
        public SharesPlateQuotesSession_New _SharesPlateQuotesSession_New = new SharesPlateQuotesSession_New();
        public BuyTipSession _BuyTipSession = new BuyTipSession();
        public AccountRiseLimitTriSession _AccountRiseLimitTriSession = new AccountRiseLimitTriSession();
        public AccountTrendTriSession _AccountTrendTriSession = new AccountTrendTriSession();
        public AccountSearchTriSession _AccountSearchTriSession = new AccountSearchTriSession();
        public SharesHisRiseRateSession _SharesHisRiseRateSession = new SharesHisRiseRateSession();
        # endregion

        public SessionHandler sessionHandler;
    }
}
