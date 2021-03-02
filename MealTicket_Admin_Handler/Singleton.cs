using FXCommon.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                    }
                }
                catch { }
            }
        }
    }
}
