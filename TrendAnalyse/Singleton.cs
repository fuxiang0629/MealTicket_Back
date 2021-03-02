using FXCommon.Common;
using FXCommon.Database;
using Newtonsoft.Json;
using StockTrendMonitor.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrendAnalyse
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

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            Dispose();

            handlerThreadCount = 10;
            connString_meal_ticket = ConfigurationManager.ConnectionStrings["connString_meal_ticket"].ConnectionString;
            connString_transactiondata = ConfigurationManager.ConnectionStrings["connString_transactiondata"].ConnectionString;
            HeartSecond = 60;

            UpdatePar();

            m_stockMonitor = new StockMonitorCore();
            int error = m_stockMonitor.InitCore(connString_meal_ticket + "|" + connString_transactiondata, 0, "");
            if (error != 0)
            {
                Console.WriteLine("走势分析对象初始化出错,error:"+ error);
            }

            mqHandler = new MQHandler();

            UpdateWait.Init();
            StartSysparUpdateThread();
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

                    UpdatePar();

                } while (true);
            });
            SysparUpdateThread.Start();
        }

        private void UpdatePar() 
        {
            try
            {
                string sql = "select top 1 ParamValue from t_system_param with(nolock) where ParamName='TrendAnalyse'";
                SqlHelper sh = new SqlHelper(connString_meal_ticket);
                var result = sh.ExecuteScalar(sql);
                if (result != null)
                {
                    var sysValue = JsonConvert.DeserializeObject<dynamic>(result.ToString());
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
        }
    }
}
