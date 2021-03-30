using FXCommon.Common;
using FXCommon.Database;
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
        /// 再触发设置
        /// </summary>
        public int MinPushTimeInterval = 180;
        public int MinPushRateInterval = 60;

        //清理分笔数据参数
        public int ClearTransactiondataSleepTime = 1800000;
        public int ClearTransactiondataStartHour = 3;
        public int ClearTransactiondataEndHour = 8;

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
                mqHandler.Dispose();
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
                            this.MinPushTimeInterval = sysValue.MinPushTimeInterval;
                        }
                        if (sysValue.MinPushRateInterval > 0 || sysValue.MinPushRateInterval == -1)
                        {
                            this.MinPushRateInterval = sysValue.MinPushRateInterval;
                        }
                        if (sysValue.HandlerThreadCount > 0 && sysValue.HandlerThreadCount <= 100)
                        {
                            this.handlerThreadCount = sysValue.HandlerThreadCount;
                        }
                        if (sysValue.HeartOverSecond > 3)
                        {
                            this.HeartSecond = sysValue.HeartOverSecond;
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
            }
        }
    }
}
