using FXCommon.Common;
using Newtonsoft.Json;
using SharesTradeService.Handler;
using SharesTradeService.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradeAPI;

namespace SharesTradeService
{
    public sealed class Singleton
    {
        /// <summary>
        /// 单例对象
        /// </summary>
        public static readonly Singleton instance=new Singleton();

        private Singleton()
        {
            UpdateSysPar();
        }

        /// <summary>
        /// 系统参数更新线程
        /// </summary>
        Thread SysparUpdateThread;

        /// <summary>
        /// 系统参数更新线程等待队列
        /// </summary>
        private ThreadMsgTemplate<int> UpdateWait = new ThreadMsgTemplate<int>();

        /// <summary>
        /// 是否测试
        /// </summary>
        public bool IsTest = ConfigurationManager.AppSettings["test"] == "true" ? true : false;

        /// <summary>
        /// 购买交易链接key
        /// </summary>
        public string BuyKey = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 服务器Id
        /// </summary>
        public readonly string ServerId = ConfigurationManager.AppSettings["serverId"];

        /// <summary>
        /// 金额保存小数位数
        /// </summary>
        public readonly long PriceFormat = 10000;

        /// <summary>
        /// 终结延迟时间
        /// </summary>
        public long OverSecond = 2;

        /// <summary>
        /// 交易连接是否并行
        /// </summary>
        public string TradeClientParallel = "true";

        #region====查询交易结果参数====
        /// <summary>
        /// 查询交易结果间隔时间（交易日）
        /// </summary>
        public int QueryTradeResultSleepTime_TradeDate = 2000;

        /// <summary>
        /// 查询交易结果间隔时间（非交易日）
        /// </summary>
        public int QueryTradeResultSleepTime_NoTradeDate = 3600000;

        /// <summary>
        /// 查询交易结果起始时间(提前秒数)
        /// </summary>
        public int QueryTradeResultStartTime = -180;

        /// <summary>
        /// 查询交易结果截止时间(滞后秒数)
        /// </summary>
        public int QueryTradeResultEndTime = 180;
        #endregion

        /// <summary>
        /// 买入挂单超时时间
        /// </summary>
        public int CancelOverTimeSecond = 300;

        /// <summary>
        /// 交易服务器账号链接列表
        /// </summary>
        public List<string> AllTradeAccountCodeList;

        /// <summary>
        /// 买入连接对象
        /// </summary>
        public TradeClientManager buyTradeClient = new TradeClientManager();

        /// <summary>
        /// 卖出连接对象
        /// </summary>
        public TradeClientManager sellTradeClient = new TradeClientManager();

        /// <summary>
        /// 撤单连接对象
        /// </summary>
        public TradeClientManager cancelTradeClient = new TradeClientManager();

        /// <summary>
        /// 查询连接对象
        /// </summary>
        public TradeClientManager queryTradeClient = new TradeClientManager();

        public MQHandler mqHandler;

        /// <summary>
        /// 初始化交易服务器链接
        /// </summary>
        public void InitTradeClient()
        {
            Dispose();

            StringBuilder sErrInfo = new StringBuilder(256);
            //int openError = TradeX_M.OpenTdx(14, "6.40", (char)12, (char)0, sErrInfo);
            int openError = Trade_Helper.OpenTdx();
            if (openError == -1)
            {
                throw new Exception("初始化交易服务器链接出错：打开通达信出错,错误信息：" + sErrInfo);
            }
            var TradeLoginList = GetTradeLoginInfoList();
            if (TradeClientParallel == "false")
            {
                TradeLoginList = TradeLoginList.Take(1).ToList(); 
            }

            AllTradeAccountCodeList = TradeLoginList.Select(e => e.AccountCode).ToList();

            if (TradeClientParallel == "false")
            {
                BuyKey = TradeLoginList.Select(e=>e.AccountCode).FirstOrDefault();
            }

            buyTradeClient.Init(TradeLoginList, BuyKey);
            if (TradeClientParallel == "false")
            {
                sellTradeClient = buyTradeClient;
                cancelTradeClient = buyTradeClient;
                queryTradeClient = buyTradeClient;
            }
            else
            {
                sellTradeClient.Init(TradeLoginList);
                cancelTradeClient.Init(TradeLoginList);
                queryTradeClient.Init(TradeLoginList);
            }

            UpdateWait.Init();
            StartSysparUpdateThread();
        }

        /// <summary>
        /// 获取当前服务器可用账户列表
        /// </summary>
        /// <returns></returns>
        private List<TradeLoginInfo> GetTradeLoginInfoList()
        {
            using (var db = new meal_ticketEntities())
            {
                var tradeLoginInfo = (from item in db.t_broker_account_info
                                      join item2 in db.t_broker_department on new { item.BrokerCode, item.DepartmentCode } equals new { item2.BrokerCode, item2.DepartmentCode }
                                      join item3 in db.t_broker on item2.BrokerCode equals item3.BrokerCode
                                      join item4 in db.t_broker_host on item3.BrokerCode equals item4.BrokerCode
                                      join item5 in db.t_server_broker_account_rel on item.Id equals item5.BrokerAccountId
                                      where item.Status == 1 && item2.Status == 1 && item3.Status == 1 && item4.Status == 1 && item5.ServerId == ServerId
                                      group new { item, item2, item3, item4 } by new { item, item2, item3 } into g
                                      select new TradeLoginInfo
                                      {
                                          AccountCode = g.Key.item.AccountCode,
                                          AccountNo = g.Key.item.AccountNo,
                                          AccountType = g.Key.item.AccountType,
                                          JyPassword = g.Key.item.JyPassword,
                                          QsId = g.Key.item3.BrokerCode,
                                          TradeAccountNo0 = g.Key.item.TradeAccountNo0,
                                          TradeAccountNo1 = g.Key.item.TradeAccountNo1,
                                          Holder0=g.Key.item.Holder0,
                                          Holder1=g.Key.item.Holder1,
                                          TxPassword = g.Key.item.TxPassword,
                                          Version = g.Key.item3.Version,
                                          YybId = g.Key.item2.DepartmentCode,
                                          TradeClientId0=-1,
                                          TradeClientId1=-1,
                                          InitialFunding=g.Key.item.InitialFunding,
                                          TradeLoginHostList = (from x in g
                                                                select new TradeLoginHost
                                                                {
                                                                    QsHost = x.item4.IpAddress,
                                                                    QsPort = x.item4.Port
                                                                }).ToList()
                                      }).ToList();
                return tradeLoginInfo;
            }
        }

        /// <summary>
        /// 释放资源。
        /// 实现IDisposable接口，调用Dispose虚方法，取消终结操作。
        /// </summary>
        public void Dispose()
        {
            buyTradeClient.Dispose();
            sellTradeClient.Dispose();
            cancelTradeClient.Dispose();
            queryTradeClient.Dispose();
            if (mqHandler != null)
            {
                mqHandler.Dispose();
            }

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

        /// <summary>
        /// 更新系统参数
        /// </summary>
        private void UpdateSysPar() 
        {
            using (var db = new meal_ticketEntities())
            {
                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "QueryTradeResultPar"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        if (sysValue.QueryTradeResultSleepTime_TradeDate != null)
                        {
                            this.QueryTradeResultSleepTime_TradeDate = sysValue.QueryTradeResultSleepTime_TradeDate;
                        }
                        if (sysValue.QueryTradeResultSleepTime_NoTradeDate != null)
                        {
                            this.QueryTradeResultSleepTime_NoTradeDate = sysValue.QueryTradeResultSleepTime_NoTradeDate;
                        }
                        if (sysValue.QueryTradeResultStartTime != null)
                        {
                            this.QueryTradeResultStartTime = sysValue.QueryTradeResultStartTime;
                        }
                        if (sysValue.QueryTradeResultEndTime != null)
                        {
                            this.QueryTradeResultEndTime = sysValue.QueryTradeResultEndTime;
                        }
                        if (sysValue.CancelOverTimeSecond != null)
                        {
                            this.CancelOverTimeSecond = sysValue.CancelOverTimeSecond;
                        }
                        if (sysValue.OverSecond != null)
                        {
                            this.OverSecond = sysValue.OverSecond;
                            if (this.OverSecond > 60)
                            {
                                this.OverSecond = 60;
                            }
                            if (this.OverSecond < 1)
                            {
                                this.OverSecond = 1;
                            }
                        }
                    }
                }
                catch (Exception ex)
                { }

                try
                {
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "TradeClientParallel"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        this.TradeClientParallel = sysPar.ParamValue;
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
