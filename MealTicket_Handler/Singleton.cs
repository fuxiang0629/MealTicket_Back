﻿using FXCommon.Common;
using MealTicket_DBCommon;
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
        #endregion

        #region====强制平仓定时器参数====
        /// <summary>
        /// 强制平仓定时器间隔时间（交易日）
        /// </summary>
        public int ClosingForceSleepTime_TradeDate = 1000;
        #endregion

        #region====委托每日清除定时器参数====
        /// <summary>
        /// 委托每日清除定时器执行起始小时
        /// </summary>
        public TimeSpan TradeCleanStartHour = TimeSpan.Parse("20:00:00");

        /// <summary>
        /// 委托每日清除定时器执行截止小时
        /// </summary>
        public TimeSpan TradeCleanEndHour = TimeSpan.Parse("23:00:00");
        #endregion

        #region====服务费计算定时器参数====
        /// <summary>
        /// 服务费计算定时器执行起始小时
        /// </summary>
        public TimeSpan HoldServiceCalcuStartHour = TimeSpan.Parse("01:00:00");

        /// <summary>
        /// 服务费计算定时器执行截止小时
        /// </summary>
        public TimeSpan HoldServiceCalcuEndHour = TimeSpan.Parse("08:00:00");
        #endregion

        #region=====分笔数据更新参数====
        /// <summary>
        /// 分笔数据更新定时器执行起始小时
        /// </summary>
        public TimeSpan TransactiondataStartHour = TimeSpan.Parse("20:00:00");

        /// <summary>
        /// 分笔数据更新定时器执行截止小时
        /// </summary>
        public TimeSpan TransactiondataEndHour = TimeSpan.Parse("23:00:00");
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
                        int tempClosingAutoSleepTime_TradeDate= sysValue.ClosingAutoSleepTime_TradeDate;
                        if(tempClosingAutoSleepTime_TradeDate>0)
                        {
                            ClosingAutoSleepTime_TradeDate = tempClosingAutoSleepTime_TradeDate;
                        }
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
                        int tempClosingForceSleepTime_TradeDate = sysValue.ClosingForceSleepTime_TradeDate;
                        if (tempClosingForceSleepTime_TradeDate > 0)
                        {
                            ClosingForceSleepTime_TradeDate = tempClosingForceSleepTime_TradeDate;
                        }
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
                        string tempTradeCleanStartHour= sysValue.TradeCleanStartHour;
                        if (!TimeSpan.TryParse(tempTradeCleanStartHour, out TradeCleanStartHour))
                        {
                            TradeCleanStartHour = TimeSpan.Parse("20:00:00");
                        }
                        string tempTradeCleanEndHour = sysValue.TradeCleanEndHour;
                        if (!TimeSpan.TryParse(tempTradeCleanEndHour, out TradeCleanEndHour))
                        {
                            TradeCleanEndHour = TimeSpan.Parse("23:00:00");
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
                        string tempHoldServiceCalcuStartHour = sysValue.HoldServiceCalcuStartHour;
                        if (!TimeSpan.TryParse(tempHoldServiceCalcuStartHour, out HoldServiceCalcuStartHour))
                        {
                            HoldServiceCalcuStartHour = TimeSpan.Parse("01:00:00");
                        }
                        string tempHoldServiceCalcuEndHour = sysValue.HoldServiceCalcuEndHour;
                        if (!TimeSpan.TryParse(tempHoldServiceCalcuEndHour, out HoldServiceCalcuEndHour))
                        {
                            HoldServiceCalcuEndHour = TimeSpan.Parse("08:00:00");
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
                        SharesCodeMatch0 = sysPar8.ParamValue;
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
                        SharesCodeMatch1 = sysPar9.ParamValue;
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
                        string tempTransactiondataStartHour = sysValue.TransactiondataStartHour;
                        if (!TimeSpan.TryParse(tempTransactiondataStartHour, out TransactiondataStartHour))
                        {
                            TransactiondataStartHour= TimeSpan.Parse("20:00:00");
                        }
                        string tempTransactiondataEndHour = sysValue.TransactiondataEndHour;
                        if (!TimeSpan.TryParse(tempTransactiondataEndHour, out TransactiondataEndHour))
                        {
                            TransactiondataEndHour = TimeSpan.Parse("23:00:00");
                        }
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
                        ForceVersionOrder_Android = sysValue.ForceVersionOrder;
                        MaxVersionOrder_Android = sysValue.MaxVersionOrder;
                        MaxVersionName_Android = sysValue.MaxVersionName;
                        MaxVersionUrl_Android = sysValue.MaxVersionUrl;
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
                        ForceVersionOrder_Ios = sysValue.ForceVersionOrder;
                        MaxVersionOrder_Ios = sysValue.MaxVersionOrder;
                        MaxVersionName_Ios = sysValue.MaxVersionName;
                        MaxVersionUrl_Ios = sysValue.MaxVersionUrl;
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 队列对象
        /// </summary>
        public MQHandler mqHandler;
        /// <summary>
        /// 启动Mq队列
        /// </summary>
        public MQHandler StartMqHandler(string listenQueueName)
        {
            string hostName = ConfigurationManager.AppSettings["MQ_HostName"];
            int port = int.Parse(ConfigurationManager.AppSettings["MQ_Port"]);
            string userName = ConfigurationManager.AppSettings["MQ_UserName"];
            string password = ConfigurationManager.AppSettings["MQ_Password"];
            string virtualHost = ConfigurationManager.AppSettings["MQ_VirtualHost"];
            mqHandler = new MQHandler(hostName, port, userName, password, virtualHost);
            return mqHandler;
        }
    }
}
