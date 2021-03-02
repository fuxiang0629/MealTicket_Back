using FXCommon.Common;
using SharesTradeService.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeAPI;

namespace SharesTradeService.Handler
{
    public class TradeClientManager
    {
        /// <summary>
        /// 连接字典
        /// </summary>
        private Dictionary<string, ThreadMsgTemplate<TradeLoginInfo>> ClientDic = new Dictionary<string, ThreadMsgTemplate<TradeLoginInfo>>();

        /// <summary>
        /// 重连连接队列
        /// </summary>
        private ThreadMsgTemplate<TradeLoginInfo> ReClient = new ThreadMsgTemplate<TradeLoginInfo>();

        /// <summary>
        /// 重连等待队列
        /// </summary>
        private ThreadMsgTemplate<int> RetryWait = new ThreadMsgTemplate<int>();
        
        /// <summary>
        /// 心跳等待队列
        /// </summary>
        private ThreadMsgTemplate<int> heartbeatWait = new ThreadMsgTemplate<int>();

        /// <summary>
        /// 获取交易链接
        /// </summary>
        /// <returns></returns>
        public TradeLoginInfo GetTradeClient(string accountCode)
        {
            if (!ClientDic.ContainsKey(accountCode))
            {
                return null;
            }

            TradeLoginInfo client = new TradeLoginInfo();
            return (ClientDic[accountCode].WaitMessage(ref client) ? client : null);
        }

        /// <summary>
        /// 添加交易链接
        /// </summary>
        /// <param name="id"></param>
        public void AddTradeClient(TradeLoginInfo client, string accountCode="")
        {
            if (string.IsNullOrEmpty(accountCode))
            {
                accountCode = client.AccountCode;
            }

            if (!ClientDic.ContainsKey(accountCode))
            {
                return;
            }

            ClientDic[accountCode].AddMessage(client);
        }

        /// <summary>
        /// 添加交易重连
        /// </summary>
        public void AddRetryClient(TradeLoginInfo client)
        {
            Console.WriteLine("链接" + client.AccountCode + "加入重连");
            ReClient.AddMessage(client, false);
        }

        /// <summary>
        /// 重连线程
        /// </summary>
        Thread TradeClientRetryThread;

        /// <summary>
        /// 心跳线程
        /// </summary>
        Thread HeartbeatThread;

        /// <summary>
        /// 开始交易链接
        /// </summary>
        private void StartTradeClient(TradeLoginInfo tradeLoginInfo, string accountCode)
        {
            if (tradeLoginInfo.TradeClientId0 >= 0)
            {
                //TradeX_M.Logoff(tradeLoginInfo.TradeClientId0);
                Trade_Helper.Logoff(tradeLoginInfo.TradeClientId0);
                tradeLoginInfo.TradeClientId0 = -1;
            }
            if (tradeLoginInfo.TradeClientId1 >= 0)
            {
                //TradeX_M.Logoff(tradeLoginInfo.TradeClientId1);
                Trade_Helper.Logoff(tradeLoginInfo.TradeClientId1);
                tradeLoginInfo.TradeClientId1 = -1;
            }

            var tempClient = new TradeLoginInfo
            {
                AccountCode = tradeLoginInfo.AccountCode,
                AccountNo = tradeLoginInfo.AccountNo,
                AccountType = tradeLoginInfo.AccountType,
                Holder0 = tradeLoginInfo.Holder0,
                Holder1 = tradeLoginInfo.Holder1,
                JyPassword = tradeLoginInfo.JyPassword,
                QsId = tradeLoginInfo.QsId,
                TradeAccountNo0 = tradeLoginInfo.TradeAccountNo0,
                TradeAccountNo1 = tradeLoginInfo.TradeAccountNo1,
                TradeClientId0 = tradeLoginInfo.TradeClientId0,
                TradeClientId1 = tradeLoginInfo.TradeClientId1,
                TxPassword = tradeLoginInfo.TxPassword,
                Version = tradeLoginInfo.Version,
                YybId = tradeLoginInfo.YybId,
                TradeLoginHostList = tradeLoginInfo.TradeLoginHostList,
                InitialFunding = tradeLoginInfo.InitialFunding
            };
            StringBuilder sErrInfo = new StringBuilder(256);
            Random rd = new Random();
            int index = rd.Next(tempClient.TradeLoginHostList.Count);
            var tradeLoginHost = tempClient.TradeLoginHostList[index];
            string HostIp = tradeLoginHost.QsHost;
            int HostPort = tradeLoginHost.QsPort;

            //int clientId0 = TradeX_M.Logon(tempClient.QsId, HostIp, (short)HostPort,
            //        tempClient.Version, (short)tempClient.YybId, (char)tempClient.AccountType, tempClient.AccountNo, tempClient.TradeAccountNo0, tempClient.JyPassword, tempClient.TxPassword, sErrInfo); 
            int clientId0 = Trade_Helper.Logon(HostIp, (short)HostPort,
                    tempClient.Version, (short)tempClient.YybId, tempClient.AccountNo, tempClient.TradeAccountNo0, tempClient.JyPassword, tempClient.TxPassword, sErrInfo);
            if (clientId0 < 0)
            {
                AddRetryClient(tempClient);
                Console.WriteLine("重连");
                return;
            }
            int clientId1 = -1;

            if (Singleton.instance.TradeClientParallel == "false")
            {
                clientId1 = clientId0;
            }
            else
            {
                // clientId1 = TradeX_M.Logon(tempClient.QsId, HostIp, (short)HostPort,
                //tempClient.Version, (short)tempClient.YybId, (char)tempClient.AccountType, tempClient.AccountNo, tempClient.TradeAccountNo1, tempClient.JyPassword, tempClient.TxPassword, sErrInfo);

                clientId1 = Trade_Helper.Logon(HostIp, (short)HostPort,
                        tempClient.Version, (short)tempClient.YybId, tempClient.AccountNo, tempClient.TradeAccountNo1, tempClient.JyPassword, tempClient.TxPassword, sErrInfo);
            }
            if (clientId1 < 0)
            {
                //TradeX_M.Logoff(clientId0);
                Trade_Helper.Logoff(clientId0);

                AddRetryClient(tempClient);
                Console.WriteLine("重连");
                return;
            }

            tempClient.TradeClientId0 = clientId0;
            tempClient.TradeClientId1 = clientId1;

            AddTradeClient(tempClient, accountCode);
            Console.WriteLine("连接成功");
        }

        /// <summary>
        /// 启动重连线程
        /// </summary>
        private void StartTradeClientRetryThread(string accountCode)
        {
            TradeClientRetryThread = new Thread(() =>
            {
                do
                {
                    int msgId = 0;
                    if (RetryWait.WaitMessage(ref msgId, 2000))
                    {
                        break;
                    }

                    do
                    {
                        TradeLoginInfo client = null;
                        if (!ReClient.GetMessage(ref client, true))                        {
                            break;
                        };

                        StartTradeClient(client, accountCode);
                    } while (true);
                } while (true);
            });
            TradeClientRetryThread.Start();
        }

        /// <summary>
        /// 启动心跳线程
        /// </summary>
        private void StartHeartbeatThread(string accountCode)
        {
            HeartbeatThread = new Thread(() =>
            {
                do
                {
                    int msgId = 0;
                    if (heartbeatWait.WaitMessage(ref msgId, 15000))
                    {
                        break;
                    }

                    if (Helper.CheckTradeDate())
                    {
                        StringBuilder sErrInfo = new StringBuilder(256);
                        StringBuilder sResult = new StringBuilder(1024 * 1024);
                        foreach (var item in ClientDic)
                        {
                            List<TradeLoginInfo> clientFails = new List<TradeLoginInfo>();
                            List<TradeLoginInfo>  clientSuccess = new List<TradeLoginInfo>();
                            do
                            {
                                TradeLoginInfo client = null;
                                if (!item.Value.GetMessage(ref client, true))
                                {
                                    break;
                                };

                                //int isSuccess = 0;
                                sErrInfo = new StringBuilder(256); 
                                sResult = new StringBuilder(1024 * 1024);
                                //isSuccess=TradeX_M.QueryData(client.TradeClientId0,0, sResult, sErrInfo);
                                Trade_Helper.QueryData(client.TradeClientId0, 0, sResult, sErrInfo);
                                if (sErrInfo.Length > 0) 
                                {
                                    clientFails.Add(client);
                                    continue;
                                }
                                //if (isSuccess < 0)
                                //{
                                //    clientFails.Add(client);
                                //    continue;
                                //}
                                sErrInfo = new StringBuilder(256);
                                sResult = new StringBuilder(1024 * 1024);
                                //isSuccess=TradeX_M.QueryData(client.TradeClientId1, 0, sResult, sErrInfo);
                                Trade_Helper.QueryData(client.TradeClientId1, 0, sResult, sErrInfo);
                                if (sErrInfo.Length > 0)
                                {
                                    clientFails.Add(client);
                                    continue;
                                }
                                //if (isSuccess < 0)
                                //{
                                //    clientFails.Add(client);
                                //    continue;
                                //}
                                clientSuccess.Add(client);
                            } while (true);

                            foreach(var s in clientSuccess)
                            {
                                AddTradeClient(s, accountCode);
                            }
                            foreach (var f in clientFails)
                            {
                                AddRetryClient(f);
                            }
                        }
                    }
                } while (true);
            });
            HeartbeatThread.Start();
        }

        /// <summary>
        /// 初始化交易服务器链接
        /// </summary>
        public void Init(List<TradeLoginInfo> TradeLoginList,string accountCode="")
        {
            Dispose();

            ReClient.Init();
            RetryWait.Init();
            heartbeatWait.Init();
            StartTradeClientRetryThread(accountCode);
            StartHeartbeatThread(accountCode);

            foreach (var item in TradeLoginList)
            {
                string tempCode = accountCode;
                if (string.IsNullOrEmpty(tempCode))
                {
                    tempCode = item.AccountCode;
                }
                if (!ClientDic.ContainsKey(tempCode))
                {
                    ClientDic.Add(tempCode, new ThreadMsgTemplate<TradeLoginInfo>());
                    ClientDic[tempCode].Init();
                }

                StartTradeClient(item, tempCode);
            }
        }
            
        /// <summary>
        /// 释放资源。
        /// 实现IDisposable接口，调用Dispose虚方法，取消终结操作。
        /// </summary>
        public void Dispose()
        {
            if (TradeClientRetryThread != null)
            {
                RetryWait.AddMessage(0);
                TradeClientRetryThread.Join();
                RetryWait.Release();

                foreach (var item in ClientDic)
                {
                    do
                    {
                        TradeLoginInfo client = null;
                        if (!item.Value.GetMessage(ref client, true))
                        { break; }

                        //TradeX_M.Logoff(client.TradeClientId0);
                        //TradeX_M.Logoff(client.TradeClientId1);
                        Trade_Helper.Logoff(client.TradeClientId0);
                        Trade_Helper.Logoff(client.TradeClientId1);
                    } while (true);
                    item.Value.Release();
                }

                do
                {
                    TradeLoginInfo client = null;
                    if (!ReClient.GetMessage(ref client, true))
                    { break; }

                    if (client.TradeClientId0 >= 0)
                    {
                        //TradeX_M.Logoff(client.TradeClientId0);
                        Trade_Helper.Logoff(client.TradeClientId0);
                    }
                    if (client.TradeClientId1 >= 0)
                    {
                        //TradeX_M.Logoff(client.TradeClientId1);
                        Trade_Helper.Logoff(client.TradeClientId1);
                    }
                } while (true);
                ReClient.Release();
            }

            if (HeartbeatThread != null)
            {
                heartbeatWait.AddMessage(0);
                HeartbeatThread.Join();
                heartbeatWait.Release();
            }
        }
    }
}
