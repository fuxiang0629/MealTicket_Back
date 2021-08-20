using FXCommon.Common;
using MealTicket_DBCommon;
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

namespace SecurityBarsDataUpdate
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

        private Thread HqClientRetryThread;// 重连线程
        private ThreadMsgTemplate<int> retryWait = new ThreadMsgTemplate<int>();//行情重连线程关闭队列
        private Thread HeartbeatThread;// 心跳线程
        private ThreadMsgTemplate<int> heartbeatWait = new ThreadMsgTemplate<int>();//心跳线程关闭队列
        private Thread SysparUpdateThread;// 系统参数更新线程
        private ThreadMsgTemplate<int> UpdateWait = new ThreadMsgTemplate<int>();// 系统参数更新线程关闭队列

        #region====K线参数====
        public int SecurityBarsGetCount = 50;// 获取K线数据每次数量
        #endregion

        public ThreadMsgTemplate<int> cltData = new ThreadMsgTemplate<int>();//行情链接队列
        public ThreadMsgTemplate<int> retryData = new ThreadMsgTemplate<int>();//行情重连队列

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
            StringBuilder sResult = new StringBuilder(1024);
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

            if (mqHandler != null)
            {
                mqHandler.Dispose();
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
                    SysparUpdate();
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
                                  where item.ParamName == "SecurityBarsPar"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        int tempSecurityBarsGetCount = sysValue.SecurityBarsGetCount;
                        if (tempSecurityBarsGetCount > 0)
                        {
                            SecurityBarsGetCount = tempSecurityBarsGetCount;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("k线配置",ex);
                }
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
            mqHandler.ListenQueueName = listenQueueName;//设置监听队列
            return mqHandler;
        }
    }
}
