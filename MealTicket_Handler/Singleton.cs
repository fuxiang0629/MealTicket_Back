using FXCommon.Common;
using MealTicket_Handler.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeAPI;

namespace MealTicket_Handler
{
    public sealed class Singleton
    {
        /// <summary>
        /// 单例对象
        /// </summary>
        private static readonly Singleton instance = new Singleton();

        /// <summary>
        /// 系统参数更新线程
        /// </summary>
        Thread SysparUpdateThread;

        /// <summary>
        /// 系统参数更新线程等待队列
        /// </summary>
        private ThreadMsgTemplate<int> UpdateWait = new ThreadMsgTemplate<int>();

        #region====自动平仓定时器参数====
        /// <summary>
        /// 自动平仓定时器间隔时间（交易日）
        /// </summary>
        public int ClosingAutoSleepTime_TradeDate = 60000;

        /// <summary>
        /// 自动平仓定时器间隔时间（非交易日）
        /// </summary>
        public int ClosingAutoSleepTime_NoTradeDate = 3600000;
        #endregion

        #region====强制平仓定时器参数====
        /// <summary>
        /// 强制平仓定时器间隔时间（交易日）
        /// </summary>
        public int ClosingForceSleepTime_TradeDate = 1000;

        /// <summary>
        /// 强制平仓定时器间隔时间（非交易日）
        /// </summary>
        public int ClosingForceSleepTime_NoTradeDate = 3600000;
        #endregion

        #region====委托每日清除定时器参数====
        /// <summary>
        /// 委托每日清除定时器间隔时间（交易日）
        /// </summary>
        public int TradeCleanSleepTime = 3600000;

        /// <summary>
        /// 委托每日清除定时器执行起始小时
        /// </summary>
        public int TradeCleanStartHour = 20;

        /// <summary>
        /// 委托每日清除定时器执行截止小时
        /// </summary>
        public int TradeCleanEndHour = 24;
        #endregion

        #region====服务费计算定时器参数====
        /// <summary>
        /// 服务费计算定时器间隔时间（交易日）
        /// </summary>
        public int HoldServiceCalcuSleepTime = 3600000;

        /// <summary>
        /// 服务费计算定时器执行起始小时
        /// </summary>
        public int HoldServiceCalcuStartHour = 1;

        /// <summary>
        /// 服务费计算定时器执行截止小时
        /// </summary>
        public int HoldServiceCalcuEndHour = 8;
        #endregion

        #region=====分笔数据更新参数====
        /// <summary>
        /// 分笔数据更新定时器间隔时间
        /// </summary>
        public int TransactiondataSleepTime = 3600000;

        /// <summary>
        /// 分笔数据更新定时器执行起始小时
        /// </summary>
        public int TransactiondataStartHour = 20;

        /// <summary>
        /// 分笔数据更新定时器执行截止小时
        /// </summary>
        public int TransactiondataEndHour = 23;
        #endregion

        #region====新分笔成交数据====
        public int NewTransactionDataCount = 50;
        public int NewTransactionDataSendPeriodTime = 0;//更新间隔
        public int NewTransactionDataRunStartTime = -180;//运行时间比交易时间提前秒数
        public int NewTransactionDataRunEndTime = 180;//运行时间比交易时间滞后秒数
        public int NewTransactiondataSleepTime = 10000;//每天执行时的间隔时间
        public int NewTransactiondataStartHour = 0;//每天执行开始时间
        public int NewTransactiondataEndHour = 0;//每天执行结束时间
        public int NewTransactionDataTrendHandlerCount = 10;
        #endregion

        public int ForceVersionOrder_Android=0;

        public int MaxVersionOrder_Android = 0;

        public string MaxVersionUrl_Android = "";

        public string MaxVersionName_Android = "";

        public int ForceVersionOrder_Ios = 0;

        public int MaxVersionOrder_Ios = 0;

        public string MaxVersionUrl_Ios = "";

        public string MaxVersionName_Ios = "";

        /// <summary>
        /// 深圳股票匹配
        /// </summary>
        public string SharesCodeMatch0 = "(^00.*)|(^30.*)";

        /// <summary>
        /// 上海股票匹配
        /// </summary>
        public string SharesCodeMatch1 = "(^6.*)";

        /// <summary>
        /// 金额保存小数位数
        /// </summary>
        public long PriceFormat = 10000;

        // 显式静态构造函数告诉C＃编译器
        // 不要将类型标记为BeforeFieldInit
        static Singleton()
        {
            
        }

        private Singleton()
        {
            UpdateSysPar();
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

                    UpdateSysPar();
                } while (true);
            });
            SysparUpdateThread.Start();
        }

        private void UpdateSysPar() 
        {
            using (var db = new meal_ticketEntities())
            {
                try
                {
                    var sysPar1 = (from item in db.t_system_param
                                   where item.ParamName == "ClosingAutoPar"
                                   select item).FirstOrDefault();
                    if (sysPar1 != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar1.ParamValue);
                        this.ClosingAutoSleepTime_TradeDate = sysValue.ClosingAutoSleepTime_TradeDate;
                        this.ClosingAutoSleepTime_NoTradeDate = sysValue.ClosingAutoSleepTime_NoTradeDate;
                    }
                }
                catch { }
                try
                {
                    var sysPar2 = (from item in db.t_system_param
                                   where item.ParamName == "ClosingForcePar"
                                   select item).FirstOrDefault();
                    if (sysPar2 != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar2.ParamValue);
                        this.ClosingForceSleepTime_TradeDate = sysValue.ClosingForceSleepTime_TradeDate;
                        this.ClosingForceSleepTime_NoTradeDate = sysValue.ClosingForceSleepTime_NoTradeDate;
                    }
                }
                catch { }
                try
                {
                    var sysPar7 = (from item in db.t_system_param
                                   where item.ParamName == "TradeCleanPar"
                                   select item).FirstOrDefault();
                    if (sysPar7 != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar7.ParamValue);
                        this.TradeCleanSleepTime = sysValue.TradeCleanSleepTime;
                        if (sysValue.TradeCleanEndHour > sysValue.TradeCleanStartHour && sysValue.TradeCleanEndHour < 24 && sysValue.TradeCleanStartHour >= 20)
                        {
                            this.TradeCleanStartHour = sysValue.TradeCleanStartHour;
                            this.TradeCleanEndHour = sysValue.TradeCleanEndHour;
                        }
                    }
                }
                catch { }
                try
                {
                    var sysPar6 = (from item in db.t_system_param
                                   where item.ParamName == "HoldServiceCalcuPar"
                                   select item).FirstOrDefault();
                    if (sysPar6 != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar6.ParamValue);
                        this.HoldServiceCalcuSleepTime = sysValue.HoldServiceCalcuSleepTime;
                        if (sysValue.HoldServiceCalcuEndHour > sysValue.HoldServiceCalcuStartHour && sysValue.HoldServiceCalcuEndHour < 8 && sysValue.HoldServiceCalcuStartHour >= 1)
                        {
                            this.HoldServiceCalcuStartHour = sysValue.HoldServiceCalcuStartHour;
                            this.HoldServiceCalcuEndHour = sysValue.HoldServiceCalcuEndHour;
                        }
                    }
                }
                catch { }
                try
                {
                    var sysPar8 = (from item in db.t_system_param
                                   where item.ParamName == "SharesCodeMatch0"
                                   select item).FirstOrDefault();
                    if (sysPar8 != null)
                    {
                        this.SharesCodeMatch0 = sysPar8.ParamValue;
                    }
                }
                catch { }
                try
                {
                    var sysPar9 = (from item in db.t_system_param
                                   where item.ParamName == "SharesCodeMatch1"
                                   select item).FirstOrDefault();
                    if (sysPar9 != null)
                    {
                        this.SharesCodeMatch1 = sysPar9.ParamValue;
                    }
                }
                catch { }
                try
                {
                    var sysPar10 = (from item in db.t_system_param
                                    where item.ParamName == "TransactiondataPar"
                                    select item).FirstOrDefault();
                    if (sysPar10 != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar10.ParamValue);
                        if (sysValue.TransactiondataSleepTime != null)
                        {
                            this.TransactiondataSleepTime = sysValue.TransactiondataSleepTime;
                        }
                        if (sysValue.TransactiondataStartHour != null && sysValue.TransactiondataStartHour >= 16 && sysValue.TransactiondataStartHour < 23)
                        {
                            this.TransactiondataStartHour = sysValue.TransactiondataStartHour;
                        }
                        if (sysValue.TransactiondataEndHour != null && sysValue.TransactiondataEndHour >= 16 && sysValue.TransactiondataEndHour < 23)
                        {
                            this.TransactiondataEndHour = sysValue.TransactiondataEndHour;
                        }
                    }
                }
                catch { }
                try
                {
                    var sysPar1 = (from item in db.t_system_param
                                   where item.ParamName == "NewTransactiondataPar"
                                   select item).FirstOrDefault();
                    if (sysPar1 != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar1.ParamValue);
                        if (sysValue.TransactionDataCount != null && sysValue.TransactionDataCount <= 2000 && sysValue.TransactionDataCount >= 50)
                        {
                            this.NewTransactionDataCount = sysValue.TransactionDataCount;
                        }
                        if (sysValue.SendPeriodTime != null)
                        {
                            this.NewTransactionDataSendPeriodTime = sysValue.SendPeriodTime;
                        }
                        if (sysValue.TransactiondataSleepTime != null && sysValue.TransactiondataSleepTime > 0)
                        {
                            this.NewTransactiondataSleepTime = sysValue.TransactiondataSleepTime;
                        }
                        if (sysValue.TransactiondataStartHour != null && sysValue.TransactiondataStartHour >= 16 && sysValue.TransactiondataStartHour <= 23)
                        {
                            this.NewTransactiondataStartHour = sysValue.TransactiondataStartHour;
                        }
                        if (sysValue.TransactiondataEndHour != null && sysValue.TransactiondataEndHour >= 16 && sysValue.TransactiondataEndHour <= 23)
                        {
                            this.NewTransactiondataEndHour = sysValue.TransactiondataEndHour;
                        }
                        if (sysValue.NewTransactionDataTrendHandlerCount != null && sysValue.NewTransactionDataTrendHandlerCount >= 0)
                        {
                            this.NewTransactionDataTrendHandlerCount = sysValue.NewTransactionDataTrendHandlerCount;
                        }
                    }
                }
                catch { }
                try
                {
                    var sysPar1 = (from item in db.t_system_param
                                   where item.ParamName == "HqServerPar"
                                   select item).FirstOrDefault();
                    if (sysPar1 != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar1.ParamValue);
                        this.NewTransactionDataRunStartTime = sysValue.RunStartTime;
                        this.NewTransactionDataRunEndTime = sysValue.RunEndTime;
                    }
                }
                catch { }
                try
                {
                    var sysPar1 = (from item in db.t_system_param
                                   where item.ParamName == "VersionPar_Android"
                                   select item).FirstOrDefault();
                    if (sysPar1 != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar1.ParamValue);
                        this.ForceVersionOrder_Android = sysValue.ForceVersionOrder;
                        this.MaxVersionOrder_Android = sysValue.MaxVersionOrder;
                        this.MaxVersionName_Android = sysValue.MaxVersionName;
                        this.MaxVersionUrl_Android = sysValue.MaxVersionUrl;
                    }
                }
                catch { }
                try
                {
                    var sysPar1 = (from item in db.t_system_param
                                   where item.ParamName == "VersionPar_Ios"
                                   select item).FirstOrDefault();
                    if (sysPar1 != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar1.ParamValue);
                        this.ForceVersionOrder_Ios = sysValue.ForceVersionOrder;
                        this.MaxVersionOrder_Ios = sysValue.MaxVersionOrder;
                        this.MaxVersionName_Ios = sysValue.MaxVersionName;
                        this.MaxVersionUrl_Ios = sysValue.MaxVersionUrl;
                    }
                }
                catch { }
            }
        }
    }
}
