using FXCommon.Common;
using FXCommon.MqQueue;
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
using static SharesHqService.SessionHandler;

namespace SharesHqService
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

        public SessionHandler session = null;

        private Singleton()
        {
            session = new SessionHandler();
            session.UpdateSessionManual();
            ClientInit();
        }

        /// <summary>
        /// 金额保存小数位数
        /// </summary>
        public readonly long PriceFormat = 10000;

        /// <summary>
        /// 行情连接数量
        /// </summary>
        public readonly int hqClientCount = 32;

        /// <summary>
        /// 行情内存空间
        /// </summary>
        public TdxHq_Result[] tdxHq_Result;

        //行情是否在运行
        object isRunLock = new object();
        public bool IsRun = false;

        public bool TryToSetIsRun(bool isRun)
        {
            lock (isRunLock)
            {
                if (IsRun == isRun)
                {
                    return false;
                }
                IsRun = isRun;
            }
            return true;
        }

        public bool GetIsRun()
        {
            lock (isRunLock)
            {
                return IsRun;
            }
        }

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
        /// 链接初始化
        /// </summary>
        private void ClientInit()
        {
            QueueInit();//初始化各个队列
            ApplyTdxMemory();
            for (int i = 0; i < hqClientCount; i++)
            {
                StartHqClient(-1);
            }
        }

        /// <summary>
        /// 申请行情获取内存空间
        /// </summary>
        private void ApplyTdxMemory()
        {
            int QuotesCount = session.GetQuotesCount();
            //申请行情内存空间数组
            tdxHq_Result = new TdxHq_Result[hqClientCount];
            for (int i = 0; i < hqClientCount; i++)
            {
                tdxHq_Result[i] = new TdxHq_Result
                {
                    sErrInfo = new StringBuilder(512),
                    sResult = new StringBuilder(2048 * QuotesCount)
                };
            }
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

            var HostList = session.GetHostList();
            Random rd = new Random();
            int index = rd.Next(HostList.Count);
            var hqHost = HostList[index];
            string hqIp = hqHost.Ip;
            int hqPort = hqHost.Port;
            try
            {
                hqClient = TradeX_M.TdxHq_Connect(hqIp, hqPort, sResult, sErrInfo);
            }
            catch (Exception ex)
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
        /// 初始化
        /// </summary>
        public void Init()
        {
            session.SetTimerStatus("", 0);
            session.UpdateSessionManual();
            session.StartUpdate();
            StartHqClientRetryThread();

            StartHeartbeatThread();
        }

        /// <summary>
        /// 队列初始化
        /// </summary>
        public ThreadMsgTemplate<int> cltData = new ThreadMsgTemplate<int>();
        public ThreadMsgTemplate<int> retryData = new ThreadMsgTemplate<int>();
        private ThreadMsgTemplate<int> retryWait = new ThreadMsgTemplate<int>();
        private ThreadMsgTemplate<int> heartbeatWait = new ThreadMsgTemplate<int>();
        private ThreadMsgTemplate<int> UpdateWait = new ThreadMsgTemplate<int>();

        private void QueueInit()
        {
            retryWait.Init();
            retryData.Init();
            cltData.Init();
            heartbeatWait.Init();
            UpdateWait.Init();
        }

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
            return mqHandler;
        }

        /// <summary>
        /// 重连线程
        /// </summary>
        Thread HqClientRetryThread;

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
        Thread HeartbeatThread;

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
                    if (!Helper.CheckTradeTime())
                    {
                        HeartbeatMsgPump();
                    }
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
                StringBuilder sErrInfo = new StringBuilder(256);
                short nCount = 0;
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


            if (session != null)
            {
                session.Dispose();
            }
        }
    }
}
