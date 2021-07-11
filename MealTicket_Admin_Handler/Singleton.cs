using FXCommon.Common;
using MealTicket_DBCommon;
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

namespace MealTicket_Admin_Handler
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

        /// <summary>
        /// 金额保存小数位数
        /// </summary>
        public long PriceFormat = 10000;

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

        // 显式静态构造函数告诉C＃编译器
        // 不要将类型标记为BeforeFieldInit
        static Singleton()
        {
            
        }

        private Singleton()
        {
            SysparUpdate();
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


                    try
                    {
                        SysparUpdate();
                    }
                    catch (Exception ex)
                    { }
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
