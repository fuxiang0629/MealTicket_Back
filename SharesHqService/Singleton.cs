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

namespace SharesHqService
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

        object quotesLock = new object();

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
        /// 金额保存小数位数
        /// </summary>
        public readonly long PriceFormat = 10000;

        /// <summary>
        /// 股票基本信息列表
        /// </summary>
        public Dictionary<int, List<SharesBaseInfo>> SharesBaseInfoList=new Dictionary<int, List<SharesBaseInfo>>();

        /// <summary>
        /// 股票基本信息列表
        /// </summary>
        public Dictionary<int, Dictionary<string, SharesBaseInfo>> SharesBaseInfoDic=new Dictionary<int, Dictionary<string, SharesBaseInfo>>();

        /// <summary>
        /// 上一次五档行情数据
        /// </summary>
        public List<SharesQuotesInfo> LastSharesQuotesList;

        /// <summary>
        /// 深圳股票匹配
        /// </summary>
        public string SharesCodeMatch0 = "(^00.*)|(^30.*)";

        /// <summary>
        /// 上海股票匹配
        /// </summary>
        public string SharesCodeMatch1 = "(^6.*)";

        /// <summary>
        /// 重连线程
        /// </summary>
        Thread HqClientRetryThread;

        /// <summary>
        /// 行情服务器
        /// </summary>
        private List<HostInfo> HostList;

        /// <summary>
        /// 行情连接数量
        /// </summary>
        private readonly int hqClientCount = 32;

        /// <summary>
        /// 心跳线程
        /// </summary>
        Thread HeartbeatThread;

        /// <summary>
        /// 股票k线资源锁
        /// </summary>
        public object SecurityBarsLocked = new object();

        /// <summary>
        /// 指数k线资源锁
        /// </summary>
        public object SecurityIndexBarsLocked = new object();

        /// <summary>
        /// 分时行情数据资源锁
        /// </summary>
        public object MinuteTimeDataLocked = new object();

        /// <summary>
        /// 分笔成交数据资源锁
        /// </summary>
        public object TransactionDataLocked = new object();

        /// <summary>
        /// 实时行情更新频率（毫秒）
        /// </summary>
        public int SshqUpdateRate = 1000;

        /// <summary>
        /// 获取实时行情每批次数量（最大80）
        /// </summary>
        public int QuotesCount = 75;

        /// <summary>
        /// 股票基础数据起始时间
        /// </summary>
        public int AllSharesStartHour = 1;

        /// <summary>
        /// 股票基础数据截止时间
        /// </summary>
        public int AllSharesEndHour = 4;

        /// <summary>
        /// 股票涨幅列表
        /// </summary>
        public List<t_shares_limit_fundmultiple> RangeList = new List<t_shares_limit_fundmultiple>();

        //股票昨日收盘价缓存
        public List<SharesBaseInfo> SharesList = new List<SharesBaseInfo>();

        //是否可以进入行情更新
        public bool QuotesCanEnter = true;

        public bool TryQuotesCanEnter()
        {
            lock (quotesLock)
            {
                if (!QuotesCanEnter) { return false; }

                QuotesCanEnter = false;
            }

            return true;
        }

        public bool TryQuotesCanLeave()
        {
            lock (quotesLock)
            {
                if (QuotesCanEnter) { return false; }

                QuotesCanEnter = true;
            }

            return true;
        }

        #region====暂未使用====
        /// <summary>
        /// 获取证券k线数据每批次数量
        /// </summary>
        public readonly int BarsCount = 50;

        /// <summary>
        /// 获取指数k线数据每批次数量
        /// </summary>
        public readonly int IndexBarsCount = 50;

        /// <summary>
        /// 获取分时行情数据每批次数量
        /// </summary>
        public readonly int MinuteTimeDataCount = 50;

        /// <summary>
        /// 获取分笔成交数据每批次数量
        /// </summary>
        public readonly int TransactionDataCount = 1;
        #endregion

        /// <summary>
        /// 运行时间比交易时间提前秒数
        /// </summary>
        public int RunStartTime = -180;

        /// <summary>
        /// 运行时间比交易时间滞后秒数
        /// </summary>
        public int RunEndTime = 180;

        public ThreadMsgTemplate<int> cltData = new ThreadMsgTemplate<int>();
        public ThreadMsgTemplate<int> retryData = new ThreadMsgTemplate<int>();
        private ThreadMsgTemplate<int> retryWait = new ThreadMsgTemplate<int>();
        private ThreadMsgTemplate<int> heartbeatWait = new ThreadMsgTemplate<int>();

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
            Dispose();

            retryWait.Init();
            retryData.Init();
            cltData.Init();
            heartbeatWait.Init();
            StartHqClientRetryThread();
            StartHeartbeatThread();
            GetSysSettingInfo();

            GetSharesHqHost();
            GetSharesBaseInfoList();
            for (int i = 0; i < hqClientCount; i++)
            {
                StartHqClient(-1);
            }

            UpdateWait.Init();
        }

        public void StartSysPar()
        {
            SysparUpdate();
            StartSysparUpdateThread();
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
            StringBuilder sResult = new StringBuilder(1024 * 1024);
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
        /// 获取股票列表
        /// </summary>
        private void GetSharesBaseInfoList()
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_all
                              group item by item.Market into g
                              select new
                              {
                                  Key = g.Key,
                                  Value = (from x in g
                                           select new SharesBaseInfo
                                           {
                                               ShareCode = x.SharesCode,
                                               ShareHandCount = x.SharesHandCount,
                                               ShareName = x.SharesName,
                                               ShareClosedPrice = x.ShareClosedPrice,
                                               Market = x.Market
                                           }).ToList()
                              }).ToDictionary(e => e.Key, e => e.Value);
                SharesBaseInfoList = result;

                foreach (var item in result)
                {
                    Dictionary<string, SharesBaseInfo> dicInfo = new Dictionary<string, SharesBaseInfo>();
                    foreach (var k in item.Value)
                    {
                        dicInfo.Add(k.ShareCode, k);
                    }

                    SharesBaseInfoDic.Add(item.Key, dicInfo);
                }
            }
        }

        /// <summary>
        /// 获取股票五档数据列表
        /// </summary>
        public void GetLastSharesQuotesList()
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_quotes
                              where item.LastModified > dateNow
                              select new SharesQuotesInfo
                              {
                                  SellCount1=item.SellCount1,
                                  SellCount2=item.SellCount2,
                                  SellCount3=item.SellCount3,
                                  SellCount4=item.SellCount4,
                                  SellCount5=item.SellCount5,
                                  SellPrice1=item.SellPrice1,
                                  SellPrice2=item.SellPrice2,
                                  SellPrice3=item.SellPrice3,
                                  SellPrice4=item.SellPrice4,
                                  SellPrice5=item.SellPrice5,
                                  SharesCode=item.SharesCode,
                                  SpeedUp=item.SpeedUp,
                                  Activity=item.Activity,
                                  BuyCount1=item.BuyCount1,
                                  BuyCount2=item.BuyCount2,
                                  BuyCount3=item.BuyCount3,
                                  BuyCount4=item.BuyCount4,
                                  BuyCount5=item.BuyCount5,
                                  BuyPrice1=item.BuyPrice1,
                                  BuyPrice2=item.BuyPrice2,
                                  BuyPrice3=item.BuyPrice3,
                                  BuyPrice4=item.BuyPrice4,
                                  BuyPrice5=item.BuyPrice5,
                                  ClosedPrice=item.ClosedPrice,
                                  InvolCount=item.InvolCount,
                                  LimitDownPrice=item.LimitDownPrice,
                                  LimitUpPrice=item.LimitUpPrice,
                                  Market=item.Market,
                                  MaxPrice=item.MaxPrice,
                                  MinPrice=item.MinPrice,
                                  OuterCount=item.OuterCount,
                                  OpenedPrice=item.OpenedPrice,
                                  PresentCount=item.PresentCount,
                                  PriceType=item.PriceType,
                                  PresentPrice=item.PresentPrice,
                                  TotalAmount=item.TotalAmount,
                                  TotalCount=item.TotalCount,
                                  TriNearLimitType=item.TriNearLimitType,
                                  TriPriceType=item.TriPriceType,
                                  BackSharesCode=item.SharesCode
                              }).ToList();
                LastSharesQuotesList = result;
            }
        }

        /// <summary>
        /// 获取配资信息
        /// </summary>
        private void GetSysSettingInfo() 
        {
            using (var db = new meal_ticketEntities())
            {
                var sys = (from item in db.t_system_param
                           select item).ToList();
                var tempSharesCodeMatch0=sys.Where(e => e.ParamName == "SharesCodeMatch0").Select(e=>e.ParamValue).FirstOrDefault();
                var tempSharesCodeMatch1 = sys.Where(e => e.ParamName == "SharesCodeMatch1").Select(e => e.ParamValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(tempSharesCodeMatch0))
                {
                    SharesCodeMatch0 = tempSharesCodeMatch0;
                }
                if (!string.IsNullOrEmpty(tempSharesCodeMatch1))
                {
                    SharesCodeMatch1 = tempSharesCodeMatch1;
                }
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
                } while (true);
                SysparUpdate();
            });
            SysparUpdateThread.Start();
        }

        private void SysparUpdate()
        {
            try
            {
                using (var db = new meal_ticketEntities())
                {
                    try
                    {
                        var sysPar1 = (from item in db.t_system_param
                                       where item.ParamName == "HqServerPar"
                                       select item).FirstOrDefault();
                        if (sysPar1 != null)
                        {
                            var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar1.ParamValue);
                            this.RunStartTime = sysValue.RunStartTime;
                            this.RunEndTime = sysValue.RunEndTime;
                            this.SshqUpdateRate = sysValue.SshqUpdateRate;
                            this.QuotesCount = sysValue.QuotesCount;
                            this.AllSharesStartHour = sysValue.AllSharesStartHour;
                            this.AllSharesEndHour = sysValue.AllSharesEndHour;
                        }
                    }
                    catch { }
                    try
                    {
                        var sysPar2 = (from item in db.t_system_param
                                       where item.ParamName == "SharesCodeMatch0"
                                       select item).FirstOrDefault();
                        if (sysPar2 != null)
                        {
                            this.SharesCodeMatch0 = sysPar2.ParamValue;
                        }
                    }
                    catch { }
                    try
                    {
                        var sysPar3 = (from item in db.t_system_param
                                       where item.ParamName == "SharesCodeMatch1"
                                       select item).FirstOrDefault();
                        if (sysPar3 != null)
                        {
                            this.SharesCodeMatch1 = sysPar3.ParamValue;
                        }
                    }
                    catch { }
                    try
                    {
                        RangeList = (from item in db.t_shares_limit_fundmultiple
                                       select item).ToList();
                    }
                    catch { }
                    //try
                    //{
                    //    DateTime timeDate = DateTime.Now.Date;
                    //    SharesList = (from item in db.t_shares_quotes_date
                    //                  where item.LastModified < timeDate
                    //                  group item by new { item.Market, item.SharesCode } into g
                    //                  select new SharesBaseInfo
                    //                  {
                    //                      ShareCode=g.Key.SharesCode,
                    //                      Market=g.Key.Market,
                    //                      ShareClosedPrice=g.OrderByDescending(e=>e.Date).Select(e=>e.PresentPrice).FirstOrDefault()
                    //                  }).ToList();
                    //}
                    //catch { }
                }
            }
            catch (Exception ex)
            { }

        }
    }
}
