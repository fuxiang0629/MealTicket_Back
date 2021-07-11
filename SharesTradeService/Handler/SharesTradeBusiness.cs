using FXCommon.Common;
using MealTicket_DBCommon;
using Newtonsoft.Json;
using SharesTradeService.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading;
using System.Transactions;
using TradeAPI;

namespace SharesTradeService.Handler
{
    /// <summary>
    /// 股票交易业务
    /// </summary>
    public class SharesTradeBusiness
    {
        /// <summary>
        /// 买入交易
        /// </summary>
        /// <returns></returns>
        public static void BuyTrade(List<dynamic> buyList)
        {
            BuyInfo buyInfo = null;
            string[] accounrCodeList=null;
            using (var db = new meal_ticketEntities())
            {
                foreach (var buy in buyList)
                {
                    try
                    {
                        long entrustId = buy.entrustId;
                        int buyCount = buy.buyCount;

                        bool error = false;
                        using (var tran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                //查询委托数据
                                var entrust = (from item in db.t_account_shares_entrust
                                               where item.Id == entrustId && item.TradeType == 1 && (item.Status == 1 || item.Status == 2)
                                               select item).FirstOrDefault();
                                if (entrust == null)
                                {
                                    throw new Exception();
                                }

                                //判断当前账户可使用的券商账户
                                string sql = string.Format(@"if(not exists(select BrokerAccountId
  from t_broker_account_info_frontaccount_rel t with(nolock)
  inner join t_broker_account_info t1 with(nolock) on t.BrokerAccountId=t1.Id
  where t.AccountId={0} and t1.[Status]=1 and t1.IsPrivate=1))
  begin
	select AccountCode from t_broker_account_info t with(nolock) where t.[Status]=1 and t.IsPrivate=0
  end
  else
  begin
	select t.AccountCode from t_broker_account_info t with(nolock)
	inner join t_broker_account_info_frontaccount_rel t1 with(nolock) on t.Id=t1.BrokerAccountId
	where t.[Status]=1 and t1.AccountId={0} and t1.[Status]=1 and t.IsPrivate=1
  end", entrust.AccountId);

                                var tempCodeList=db.Database.SqlQuery<string>(sql).ToArray();
                                if (accounrCodeList == null)
                                {
                                    accounrCodeList = tempCodeList;
                                }
                                else
                                {
                                    if (!Helper.CompareArray<string>(tempCodeList, accounrCodeList))
                                    {
                                        BuyTradeFinish(new List<BuyDetails>
                                        {
                                            new BuyDetails
                                            {
                                                EntrustId=entrustId
                                            }
                                        }, db);
                                        continue;
                                    }
                                }

                                //查询委托账户已买入数量
                                int managerCount = 0;
                                var manager = (from item in db.t_account_shares_entrust_manager
                                               where item.BuyId == entrustId && item.TradeType == 1
                                               select item).ToList();
                                if (manager.Count() > 0)
                                {
                                    managerCount = manager.Sum(e => e.EntrustCount);
                                }
                                if (entrust.EntrustCount - managerCount < buyCount)
                                {
                                    BuyTradeFinish(new List<BuyDetails> 
                                    {
                                        new BuyDetails
                                        {
                                            EntrustId=entrustId
                                        }
                                    }, db);
                                    continue;
                                }

                                int remainEntrustCount = buyCount;//当前需要委托数量
                                if (remainEntrustCount <= 0)
                                {
                                    BuyTradeFinish(new List<BuyDetails>
                                    {
                                        new BuyDetails
                                        {
                                            EntrustId=entrustId
                                        }
                                    }, db);
                                    continue;
                                }
                                if (remainEntrustCount % entrust.SharesHandCount != 0)
                                {
                                    BuyTradeFinish(new List<BuyDetails>
                                    {
                                        new BuyDetails
                                        {
                                            EntrustId=entrustId
                                        }
                                    }, db);
                                    continue;
                                }
                                if (entrust.Status == 1)
                                {
                                    entrust.Status = 2;
                                    db.SaveChanges();
                                }

                                if (buyInfo == null)
                                {
                                    buyInfo = new BuyInfo 
                                    {
                                        SharesCode=entrust.SharesCode,
                                        Market= entrust.Market,
                                        BuyCount= remainEntrustCount,
                                        EntrustPrice= entrust.EntrustPrice,
                                        SharesHandCount=entrust.SharesHandCount,
                                        Type=entrust.Type,
                                        BuyList=new List<BuyDetails> 
                                        {
                                            new BuyDetails
                                            {
                                                AccountId=entrust.AccountId,
                                                BuyCount=remainEntrustCount,
                                                EntrustId=entrust.Id
                                            }
                                        }
                                    };
                                }
                                else
                                {
                                    buyInfo.BuyCount = buyInfo.BuyCount + remainEntrustCount;
                                    buyInfo.BuyList.Add(new BuyDetails
                                    {
                                        AccountId = entrust.AccountId,
                                        BuyCount = remainEntrustCount,
                                        EntrustId = entrust.Id
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                error = true;
                                Logger.WriteFileLog("买入操作出错", ex);
                                tran.Rollback();
                            }
                            finally
                            {
                                if (!error)
                                {
                                    tran.Commit();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("数据库事务创建出错", ex);
                    }
                }

                if (buyInfo == null)
                {
                    return;
                }

                //没有可用券商账户
                if (accounrCodeList == null || accounrCodeList.Length <= 0)
                {
                    BuyTradeFinish(buyInfo.BuyList, db);
                    return;
                }

                //支持券商账户随机排序
                accounrCodeList = accounrCodeList.OrderBy(c => Guid.NewGuid()).ToArray();

                //取过的连接
                List<string> accountList = new List<string>();
                //查询当前股票当前账户最大可交易额度规则
                var quotaList = (from item in db.t_shares_limit_tradequota
                                 where (item.LimitMarket == -1 || item.LimitMarket == buyInfo.Market) && item.Status == 1 && (buyInfo.SharesCode.StartsWith(item.LimitKey) || item.LimitKey == "9999999")
                                 select item).ToList();

                int takeCount = 0;
                do
                {
                    //当前委托数量
                    int currEntrustCount = buyInfo.BuyCount;
                    //所有帐户都操作过了，直接退出
                    if (accounrCodeList.Length <= takeCount)
                    {
                        break;
                    }
                    //取不到连接，直接退出
                    var client = Singleton.instance.buyTradeClient.GetTradeClient(accounrCodeList[takeCount]);
                    if (client == null)
                    {
                        break;
                    }
                    takeCount++;
                    //连接已被取过
                    if (accountList.Contains(client.AccountCode))
                    {
                        //归还连接
                        Singleton.instance.buyTradeClient.AddTradeClient(client);
                        break;
                    }
                    accountList.Add(client.AccountCode);//加入取过的连接

                    try
                    {
                        //查询账户可交易股票数量
                        int MaxBuyCount = 100000000;

                        //计算该账户该类股票最多可交易数量
                        foreach (var item in quotaList)
                        {
                            //可买比例
                            int quotaRate = item.Rate;
                            //判断有没有额外限制
                            var quoteOther = (from x in db.t_shares_limit_tradequota_other
                                              where x.QuotaId == item.Id && x.AccountCode == client.AccountCode && x.Status == 1
                                              orderby x.Rate
                                              select x).FirstOrDefault();
                            if (quoteOther != null)
                            {
                                quotaRate = quoteOther.Rate;
                            }
                            //当前账户总共可买金额
                            long totalValue = (long)Math.Floor(quotaRate * 1.0 / 10000 * client.InitialFunding);
                            //当前账户已借金额
                            long holdValue = 0;
                            var hold = (from x in db.t_account_shares_hold
                                        join x2 in db.t_account_shares_hold_manager on x.Id equals x2.HoldId
                                        where x.Status == 1 && x.RemainCount > 0 && (x.Market == item.LimitMarket || item.LimitMarket == -1) && (x.SharesCode.StartsWith(item.LimitKey) || (item.LimitKey == "9999999" && x.Market == buyInfo.Market && x.SharesCode == buyInfo.SharesCode)) && x2.TradeAccountCode == client.AccountCode
                                        select (x2.TotalCount * 1.0 / x.RemainCount) * x.FundAmount).ToList();
                            if (hold.Count() > 0)
                            {
                                holdValue = (long)Math.Ceiling(hold.Sum());
                            }
                            int tempQuotaMaxCount = (int)(((totalValue - holdValue) / buyInfo.EntrustPrice) / buyInfo.SharesHandCount) * buyInfo.SharesHandCount;
                            if (tempQuotaMaxCount <= MaxBuyCount)
                            {
                                MaxBuyCount = tempQuotaMaxCount;
                            }
                        }

                        //计算当前购买股票数量
                        if (currEntrustCount > MaxBuyCount)
                        {
                            currEntrustCount = MaxBuyCount;
                        }

                        if (currEntrustCount > 0)
                        {
                            StringBuilder sErrInfo = new StringBuilder(256);
                            StringBuilder sResult = new StringBuilder(1024 * 1024);

                            if (!Singleton.instance.IsTest)
                            {
                                Trade_Helper.SendOrder(buyInfo.Market == 0 ? client.TradeClientId0 : client.TradeClientId1, 0, 0, buyInfo.Market == 0 ? client.Holder0 : client.Holder1, buyInfo.SharesCode + "," + buyInfo.Market, (float)Math.Round(buyInfo.EntrustPrice * 1.0 / 10000, 2), currEntrustCount, sResult, sErrInfo);
                            }
                            else
                            {
                                int dealCount = currEntrustCount;
                                //dealCount = 99;//系统回收
                                TestHelper.SendBuyOrder(buyInfo.SharesCode, Math.Round(buyInfo.EntrustPrice * 1.0 / 10000, 2).ToString(), dealCount.ToString(), sResult, sErrInfo);
                            }
                            if (!string.IsNullOrEmpty(sResult.ToString()))
                            {
                                //提交成功后剩余委托股票数量
                                buyInfo.BuyCount = buyInfo.BuyCount - currEntrustCount <= 0 ? 0 : buyInfo.BuyCount - currEntrustCount;

                                string tradeEntrustId = "";

                                //解析返回数据
                                string result = sResult.ToString();

                                string[] rows = result.Split('\n');
                                for (int i = 0; i < rows.Length; i++)
                                {
                                    if (i == 1)
                                    {
                                        string[] column = rows[i].Split('\t');
                                        tradeEntrustId = column[0];//委托编号
                                    }
                                }
                                BuySuccess(client.AccountCode, currEntrustCount, buyInfo, tradeEntrustId, db);
                            }
                            else
                            {
                                var EntrustList = string.Join(",", buyInfo.BuyList.Select(e => e.EntrustId).ToArray()); 
                                string sql = string.Format("update t_account_shares_entrust set StatusDes='{0}' where Id in {1}", sErrInfo.ToString(), EntrustList);
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                    finally 
                    {
                        //归还连接
                        Singleton.instance.buyTradeClient.AddTradeClient(client);
                    }

                } while (buyInfo.BuyCount > 0);

                using (var tran = db.Database.BeginTransaction())
                {
                    bool error = false;
                    try
                    {
                        //委托数量未消耗完，继续扔回下一个队列
                        if (buyInfo.BuyCount < buyInfo.SharesHandCount)
                        {
                            BuyTradeFinish(buyInfo.BuyList,db);
                            return;
                        }
                        //查询下一个服务器
                        var serverIndex = (from x in db.t_server
                                           where x.ServerId == Singleton.instance.ServerId
                                           select x.OrderIndex).FirstOrDefault();
                        var nextServer = (from x in db.t_server
                                          where x.OrderIndex > serverIndex
                                          select x).FirstOrDefault();
                        if (nextServer == null)
                        {
                            BuyTradeFinish(buyInfo.BuyList, db);
                            return;
                        }

                        List<dynamic> sendDataList = new List<dynamic>();
                        foreach (var item in buyInfo.BuyList)
                        {
                            if (item.BuyCount <= 0)
                            {
                                continue;
                            }
                            sendDataList.Add(new
                            {
                                BuyId = item.EntrustId,
                                BuyCount = item.BuyCount,
                                BuyTime = DateTime.Now.ToString("yyyy-MM-dd")
                            });
                        }

                        if (!Singleton.instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new { type = 1, data = sendDataList })), "SharesBuy", nextServer.ServerId))
                        {
                            BuyTradeFinish(buyInfo.BuyList, db);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        error = true;
                        Logger.WriteFileLog("这里发生了错误",ex);
                        tran.Rollback();
                    }
                    finally 
                    {
                        if (!error)
                        {
                            tran.Commit();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 卖出交易
        /// </summary>
        /// <returns></returns>
        public static void SellTrade(List<long> entrustManagerIdList)
        {
            Dictionary<string, SellInfo> sellDic = new Dictionary<string, SellInfo>();
            using (var db = new meal_ticketEntities())
            {
                //委托账户分组
                foreach (var entrustManagerId in entrustManagerIdList)
                {
                    var entrustManager = (from item in db.t_account_shares_entrust_manager
                                          where item.Id == entrustManagerId && item.TradeType == 2 && item.Status == 1
                                          select item).FirstOrDefault();
                    if (entrustManager == null)
                    {
                        continue;
                    }
                    if (!sellDic.ContainsKey(entrustManager.TradeAccountCode))
                    {
                        sellDic.Add(entrustManager.TradeAccountCode, new SellInfo
                        {
                            SellCount = 0,
                            EntrustManagerList = new List<SellDetails>
                            {
                                new SellDetails
                                {
                                     EntrustManagerId = entrustManagerId,
                                     EntrustCount=0
                                }
                            }
                        });
                    }
                    else
                    {
                        sellDic[entrustManager.TradeAccountCode].EntrustManagerList.Add(new SellDetails
                        {
                            EntrustManagerId = entrustManagerId,
                            EntrustCount = 0
                        });
                    }
                }
                //计算每组可卖，提交卖出
                foreach (var tradeInfo in sellDic)
                {
                    string tradeEntrustId = "";//委托编号
                    bool success = false;
                    using (var tran = db.Database.BeginTransaction())
                    {
                        bool error = false;
                        try
                        {
                            string managerIdListStr="("+string.Join(",", tradeInfo.Value.EntrustManagerList.Select(e => e.EntrustManagerId).ToList())+")";
                            string sql = @"select Id,EntrustCount,BuyId from t_account_shares_entrust_manager with(xlock) where Id in {0} and TradeType=2 and [Status]=1";
                            var entrustManagerList = db.Database.SqlQuery<SellEntrustManagerSelect>(string.Format(sql, managerIdListStr)).ToList();
                            //判断卖出账户，生成sellDic
                            foreach (var manager in tradeInfo.Value.EntrustManagerList)
                            {
                                var entrustManager = entrustManagerList.Where(e => e.Id == manager.EntrustManagerId).FirstOrDefault();
                                if (entrustManager == null)
                                {
                                    throw new Exception("委托manager不存在");
                                }
                                long BuyId = entrustManager.BuyId;//委托Id
                                int EntrustCount = entrustManager.EntrustCount;//当前需要委托数量
                                var entrust = (from item in db.t_account_shares_entrust
                                               where item.Id == BuyId
                                               select item).FirstOrDefault();
                                if (entrust == null)
                                {
                                    SellTradeFinish(new List<SellDetails>
                                    {
                                        new SellDetails
                                        {
                                            EntrustManagerId=manager.EntrustManagerId
                                        }
                                    }, db);
                                    continue;
                                }
                                if (EntrustCount <= 0)
                                {
                                    SellTradeFinish(new List<SellDetails>
                                    {
                                        new SellDetails
                                        {
                                            EntrustManagerId=manager.EntrustManagerId
                                        }
                                    }, db);
                                    continue;
                                }
                                tradeInfo.Value.Market = entrust.Market;
                                tradeInfo.Value.SharesCode = entrust.SharesCode;
                                tradeInfo.Value.EntrustType = entrust.EntrustType;
                                tradeInfo.Value.EntrustPrice = entrust.EntrustPrice;

                                //查询账户持仓情况
                                sql = @"select top 1 AccountCanSoldCount,TotalSharesCount from t_broker_account_shares_rel with(xlock) where TradeAccountCode='{0}' and Market={1} and SharesCode='{2}'";
                                var accountShares = db.Database.SqlQuery<AccountSharesRelSelect>(string.Format(sql, tradeInfo.Key, tradeInfo.Value.Market, tradeInfo.Value.SharesCode)).FirstOrDefault();
                                if (accountShares == null)
                                {
                                    SellTradeFinish(new List<SellDetails>
                                    {
                                        new SellDetails
                                        {
                                            EntrustManagerId=manager.EntrustManagerId
                                        }
                                    }, db);
                                    continue;
                                }

                                int AccountCanSoldCount = accountShares.AccountCanSoldCount;
                                int TotalSharesCount = accountShares.TotalSharesCount;
                                int realSoldCount = AccountCanSoldCount;//可提交到证券市场数量
                                if (TotalSharesCount == EntrustCount && realSoldCount == EntrustCount)//清仓
                                {
                                    realSoldCount = EntrustCount;
                                }
                                else if (realSoldCount >= EntrustCount)//只能卖一手的整数倍，且留下的股票必须大于等于1手
                                {
                                    int tempCount = EntrustCount / entrust.SharesHandCount * entrust.SharesHandCount;
                                    if (accountShares.TotalSharesCount - tempCount >= entrust.SharesHandCount)//剩余股票必须大于1手
                                    {
                                        realSoldCount = tempCount;
                                    }
                                    else if (tempCount - entrust.SharesHandCount >= entrust.SharesHandCount)//剩余小于1手的，判断减掉一手是否可卖
                                    {
                                        realSoldCount = tempCount - entrust.SharesHandCount;
                                    }
                                    else//减掉一手不可卖
                                    {
                                        SellTradeFinish(new List<SellDetails>
                                        {
                                            new SellDetails
                                            {
                                                EntrustManagerId=manager.EntrustManagerId
                                            }
                                        }, db);
                                        continue;
                                    }
                                }
                                else//可卖数量不足
                                {
                                    SellTradeFinish(new List<SellDetails>
                                    {
                                        new SellDetails
                                        {
                                            EntrustManagerId=manager.EntrustManagerId
                                        }
                                    }, db);
                                    continue;
                                }

                                if (realSoldCount <= 0)
                                {
                                    SellTradeFinish(new List<SellDetails>
                                    {
                                        new SellDetails
                                        {
                                            EntrustManagerId=manager.EntrustManagerId
                                        }
                                    }, db);
                                    continue;
                                }

                                manager.EntrustCount = realSoldCount;
                                tradeInfo.Value.SellCount = tradeInfo.Value.SellCount + realSoldCount;
                                //更新账户状态为交易中
                                sql = @"update t_account_shares_entrust_manager set [Status]=2 where Id={0}";
                                db.Database.ExecuteSqlCommand(string.Format(sql, manager.EntrustManagerId));
                            }

                            if (tradeInfo.Value.SellCount <= 0)
                            {
                                continue;
                            }

                            StringBuilder sErrInfo = new StringBuilder(256);
                            StringBuilder sResult = new StringBuilder(1024 * 1024);
                            var client = Singleton.instance.sellTradeClient.GetTradeClient(tradeInfo.Key);
                            if (client == null)
                            {
                                SellTradeFinish(tradeInfo.Value.EntrustManagerList, db);
                                continue;
                            }
                            try
                            {
                                if (!Singleton.instance.IsTest)
                                {
                                    Trade_Helper.SendOrder(tradeInfo.Value.Market == 0 ? client.TradeClientId0 : client.TradeClientId1, 1, tradeInfo.Value.EntrustType == 1 ? 4 : 0, tradeInfo.Value.Market == 0 ? client.Holder0 : client.Holder1, tradeInfo.Value.SharesCode + "," + tradeInfo.Value.Market, (float)Math.Round(tradeInfo.Value.EntrustPrice * 1.0 / Singleton.instance.PriceFormat, 2), tradeInfo.Value.SellCount, sResult, sErrInfo);
                                }
                                else
                                {
                                    int dealCount = tradeInfo.Value.SellCount;
                                    //dealCount = dealCount - 99;//系统回收
                                    TestHelper.SendSellOrder(tradeInfo.Value.SharesCode, Math.Round(tradeInfo.Value.EntrustPrice * 1.0 / Singleton.instance.PriceFormat, 2).ToString(), dealCount.ToString(), sResult, sErrInfo);
                                    //sResult = new StringBuilder();
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteFileLog("卖出提交出错", ex);
                            }
                            finally
                            {
                                //归还链接
                                Singleton.instance.sellTradeClient.AddTradeClient(client);
                            }
                            if (!string.IsNullOrEmpty(sResult.ToString()))
                            {
                                //解析返回数据
                                string result = sResult.ToString();

                                string[] rows = result.Split('\n');
                                for (int i = 0; i < rows.Length; i++)
                                {
                                    if (i == 1)
                                    {
                                        string[] column = rows[i].Split('\t');
                                        if (!string.IsNullOrEmpty(column[0]))
                                        {
                                            tradeEntrustId = column[0];//委托编号
                                        }
                                    }
                                }
                                success = true;
                            }
                            else
                            {
                                SellTradeFinish(tradeInfo.Value.EntrustManagerList, db);
                            }
                        }
                        catch (Exception ex)
                        {
                            error = true;
                            Logger.WriteFileLog("卖出提交出错", ex);
                            tran.Rollback();
                        }
                        finally
                        {
                            if (!error)
                            {
                                tran.Commit();
                            }
                        }
                    }
                    if (success)
                    {
                        SellSuccess(tradeInfo.Key, tradeInfo.Value, tradeEntrustId, db);
                    }
                    //触发判断回购股票是否需要卖出
                    try
                    {
                        SimulateSellTradeDelegate d1 = SimulateSellTrade;
                        d1(tradeInfo.Key, tradeInfo.Value.Market, tradeInfo.Value.SharesCode);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 结束卖出交易
        /// </summary>
        /// <param name="entrustManagerIdList"></param>
        /// <param name="db"></param>
        private static void SellTradeFinish(List<SellDetails> entrustManagerList, meal_ticketEntities db)
        {
            foreach (var item in entrustManagerList)
            {
                db.P_TradeFinish(item.EntrustManagerId, 0, 0);
            }
        }

        /// <summary>
        /// 卖出提交成功
        /// </summary>
        private static void SellSuccess(string tradeAccountCode, SellInfo sellInfo, string tradeEntrustId,meal_ticketEntities db)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    string managerIdListStr = "(" + string.Join(",", sellInfo.EntrustManagerList.Select(e => e.EntrustManagerId).ToList()) + ")";
                    string sql = @"select Id from t_account_shares_entrust_manager with(xlock) where Id in {0} and TradeType=2 and [Status]=2";
                    var entrustManagerList = db.Database.SqlQuery<long>(string.Format(sql, managerIdListStr)).ToList();
                    foreach (var manager in sellInfo.EntrustManagerList)
                    {
                        sql = "update t_account_shares_entrust_manager set RealEntrustCount={0},EntrustId='{1}',EntrustTime='{2}',LastModified='{2}' where Id={3}";
                        db.Database.ExecuteSqlCommand(string.Format(sql, manager.EntrustCount, tradeEntrustId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), manager.EntrustManagerId));
                    }

                    sql = string.Format("select top 1 Id from t_broker_account_shares_rel with(xlock) where TradeAccountCode='{0}' and Market={1} and SharesCode='{2}'", tradeAccountCode, sellInfo.Market, sellInfo.SharesCode);
                    long brokerId=db.Database.SqlQuery<long>(sql).FirstOrDefault();

                    sql = string.Format("update t_broker_account_shares_rel set AccountCanSoldCount=AccountCanSoldCount-{0},AccountSellingCount=AccountSellingCount+{0} where Id={1}", sellInfo.SellCount, brokerId);
                    db.Database.ExecuteSqlCommand(sql);

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 结束买入交易
        /// </summary>
        /// <param name="entrustList"></param>
        /// <param name="db"></param>
        private static void BuyTradeFinish(List<BuyDetails> entrustList, meal_ticketEntities db)
        {
            foreach (var x in entrustList)
            {
                var entrust = (from item in db.t_account_shares_entrust
                               where item.Id == x.EntrustId && item.TradeType == 1 && (item.Status == 1 || item.Status == 2)
                               select item).FirstOrDefault();
                entrust.Status = 5;
                db.SaveChanges();

                //交易完成，尝试撤单
                db.P_TryToCancelEntrust(x.EntrustId);
            }
        }

        /// <summary>
        /// 买入提交成功
        /// </summary>
        private static void BuySuccess(string tradeAccountCode,int buyCount, BuyInfo buyInfo, string tradeEntrustId, meal_ticketEntities db)
        {
            DateTime overTime;
            if (Singleton.instance.CancelOverTimeSecond == -1 || buyInfo.Type!=1)
            {
                overTime = DateTime.Parse("9999-01-01 00:00:00");
            }
            else
            {
                overTime = DateTime.Now.AddSeconds(Singleton.instance.CancelOverTimeSecond);
            }
            foreach (var buy in buyInfo.BuyList)
            {
                int currEntrustCount;
                if (buyCount > buy.BuyCount)
                {
                    currEntrustCount = buy.BuyCount;
                }
                else
                {
                    currEntrustCount = buyCount;
                }
                if (currEntrustCount <= 0)
                {
                    break;
                }
                buyCount = buyCount - currEntrustCount;
                buy.BuyCount = buy.BuyCount - currEntrustCount;
                db.t_account_shares_entrust_manager.Add(new t_account_shares_entrust_manager
                {
                    Status = 2,
                    BuyId = buy.EntrustId,
                    CancelCount = 0,
                    CreateTime = DateTime.Now,
                    DealAmount = 0,
                    DealCount = 0,
                    DealPrice = 0,
                    RealEntrustCount = currEntrustCount,
                    EntrustCount = currEntrustCount,
                    EntrustId = tradeEntrustId,
                    EntrustPrice = buyInfo.EntrustPrice,
                    EntrustTime = DateTime.Now,
                    EntrustType = 2,
                    TradeType = 1,
                    LastModified = DateTime.Now,
                    TradeAccountCode = tradeAccountCode,
                    OverTime=overTime
                });
            }
            db.SaveChanges();
        }

        /// <summary>
        /// 撤单交易
        /// </summary>
        /// <returns></returns>
        public static void CancelTrade(long tradeManagerId,long entrustId)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    if (tradeManagerId == -1)
                    {
                        db.P_TryToCancelEntrust(entrustId);
                    }
                    else
                    {
                        //查询委托数据
                        var entrustManager = (from item in db.t_account_shares_entrust_manager
                                              where item.Id == tradeManagerId
                                              select item).FirstOrDefault();
                        if (entrustManager == null)
                        {
                            throw new Exception();
                        }
                        var entrust = (from item in db.t_account_shares_entrust
                                       where item.Id == entrustManager.BuyId
                                       select item).FirstOrDefault();
                        if (entrust == null)
                        {
                            throw new Exception();
                        }
                        if (string.IsNullOrEmpty(entrustManager.EntrustId))
                        {
                            db.P_TradeFinish(tradeManagerId, 0, 0);
                        }
                        else
                        {
                            StringBuilder sErrInfo;
                            StringBuilder sResult;
                            var client = Singleton.instance.cancelTradeClient.GetTradeClient(entrustManager.TradeAccountCode);
                            if (client == null)
                            {
                                throw new Exception();
                            }
                            try
                            {
                                sErrInfo = new StringBuilder(256);
                                sResult = new StringBuilder(1024 * 1024);
                                if (!Singleton.instance.IsTest)
                                {
                                    //TradeX_M.CancelOrder(entrust.Market == 0 ? client.TradeClientId0 : client.TradeClientId1, (byte)entrust.Market, entrustManager.EntrustId, sResult, sErrInfo);
                                    Trade_Helper.CancelOrder(entrust.Market == 0 ? client.TradeClientId0 : client.TradeClientId1, entrust.Market.ToString(), entrustManager.EntrustId, sResult, sErrInfo);
                                }
                            }
                            finally
                            {
                                Singleton.instance.cancelTradeClient.AddTradeClient(client);
                            }
                        }
                    }
                    tran.Commit();
                }
               
                catch (Exception ex)
                {
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// 同步资金/股份数据
        /// </summary>
        /// <param name="tradeAccountCode"></param>
        public static void QueryTrade(string tradeAccountCode)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                var tradeAccount = (from item in db.t_broker_account_info
                                    where item.AccountCode == tradeAccountCode
                                    select item).FirstOrDefault();
                if (tradeAccount == null)
                {
                    tran.Rollback();
                    return;
                }
                try
                {
                    StringBuilder sResult;
                    StringBuilder pageMark;
                    var client = Singleton.instance.queryTradeClient.GetTradeClient(tradeAccountCode);
                    if (client == null)
                    {
                        throw new Exception();
                    }
                    try
                    {
                        //获取资金
                        sResult = new StringBuilder(1024 * 1024);
                        pageMark = new StringBuilder(256);
                        if (!QueryTradeData(client.TradeClientId0, 0, 200, pageMark, sResult))
                        {
                            throw new Exception();
                        }
                        //解析返回数据
                        string result = sResult.ToString();
                        string[] rows = result.Split('\n');
                        for (int i = 0; i < rows.Length; i++)
                        {
                            if (i == 1)
                            {
                                string[] column = rows[i].Split('\t');
                                if (column.Length < 7)
                                {
                                    continue;
                                }
                                long TotalBalance = (long)Math.Round((float.Parse(column[1]) * Singleton.instance.PriceFormat) / 100, 0) * 100;
                                long AvailableBalance = (long)Math.Round((float.Parse(column[2]) * Singleton.instance.PriceFormat) / 100, 0) * 100;
                                long WithdrawBalance = (long)Math.Round((float.Parse(column[4]) * Singleton.instance.PriceFormat) / 100, 0) * 100;
                                long FreezeBalance = (long)Math.Round((float.Parse(column[3]) * Singleton.instance.PriceFormat) / 100, 0) * 100;
                                long TotalValue = (long)Math.Round((float.Parse(column[5]) * Singleton.instance.PriceFormat) / 100, 0) * 100;
                                long MarketValue = (long)Math.Round((float.Parse(column[6]) * Singleton.instance.PriceFormat) / 100, 0) * 100;

                                var capital = (from item in db.t_broker_account_info_capital
                                               where item.AccountCode == tradeAccountCode
                                               select item).FirstOrDefault();
                                if (capital == null)
                                {
                                    db.t_broker_account_info_capital.Add(new t_broker_account_info_capital
                                    {
                                        AccountCode = tradeAccountCode,
                                        AvailableBalance = AvailableBalance,
                                        FreezeBalance = FreezeBalance,
                                        LastModified = DateTime.Now,
                                        MarketValue = MarketValue,
                                        TotalBalance = TotalBalance,
                                        TotalValue = TotalValue,
                                        WithdrawBalance = WithdrawBalance
                                    });
                                }
                                else
                                {
                                    capital.AvailableBalance = AvailableBalance;
                                    capital.FreezeBalance = FreezeBalance;
                                    capital.LastModified = DateTime.Now;
                                    capital.MarketValue = MarketValue;
                                    capital.TotalBalance = TotalBalance;
                                    capital.TotalValue = TotalValue;
                                    capital.WithdrawBalance = WithdrawBalance;
                                }
                                db.SaveChanges();
                            }
                        }

                        //获取账户股份
                        var position = (from item in db.t_broker_account_info_position
                                        where item.AccountCode == tradeAccountCode
                                        select item).ToList();
                        db.t_broker_account_info_position.RemoveRange(position);
                        db.SaveChanges();

                        pageMark = new StringBuilder(256);
                        sResult = new StringBuilder(1024 * 1024);
                        if (!QueryTradeData(client.TradeClientId0, 1, 200, pageMark, sResult))
                        {
                            throw new Exception();
                        }
                        //解析返回数据
                        result = sResult.ToString();

                        rows = result.Split('\n');
                        for (int i = 0; i < rows.Length; i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            string[] column = rows[i].Split('\t');
                            if (column.Length < 12)
                            {
                                continue;
                            }
                            int Market = column[10] == tradeAccount.Holder0 ? 0 : 1;
                            string SharesCode = column[0];
                            string SharesName = column[1];
                            int TotalSharesCount = int.Parse(column[2]);
                            int CanSoldSharesCount = int.Parse(column[3]);
                            long CostPrice = (long)Math.Round((float.Parse(column[4]) * Singleton.instance.PriceFormat) / 100, 0) * 100;
                            long CurrPrice = (long)Math.Round((float.Parse(column[5]) * Singleton.instance.PriceFormat) / 100, 0) * 100;
                            //int BuyCountToday = (int)float.Parse(column[6]);
                            //int SellCountToday = (int)float.Parse(column[7]);
                            long MarketValue = (long)Math.Round((float.Parse(column[6]) * Singleton.instance.PriceFormat) / 100, 0) * 100;
                            long ProfitValue = (long)Math.Round((float.Parse(column[7]) * Singleton.instance.PriceFormat) / 100, 0) * 100;
                            int ProfitRate = (int)Math.Round((float.Parse(column[9]) * 1000), 0);

                            db.t_broker_account_info_position.Add(new t_broker_account_info_position
                            {
                                SellCountToday = 0,
                                SharesCode = SharesCode,
                                SharesName = SharesName,
                                TotalSharesCount = TotalSharesCount,
                                AccountCode = tradeAccountCode,
                                BuyCountToday = 0,
                                CostPrice = CostPrice,
                                CurrPrice = CurrPrice,
                                CanSoldSharesCount = CanSoldSharesCount,
                                LastModified = DateTime.Now,
                                Market = Market,
                                MarketValue = MarketValue,
                                ProfitRate = ProfitRate,
                                ProfitValue = ProfitValue
                            });
                        }
                        db.SaveChanges();
                    }
                    finally
                    {
                        //归还链接
                        Singleton.instance.queryTradeClient.AddTradeClient(client);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.WriteFileLog("券商资金同步出错", ex);
                }

            }

            using (var db = new meal_ticketEntities())
            {
                var tradeAccount = (from item in db.t_broker_account_info
                                    where item.AccountCode == tradeAccountCode
                                    select item).FirstOrDefault();
                if (tradeAccount != null)
                {
                    tradeAccount.SynchronizationStatus = 0;
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 回购股票卖出交易委托
        /// </summary>
        /// <param name="tradeAccountCode"></param>
        /// <param name="sharesMarket"></param>
        /// <param name="sharesCode"></param>
        private delegate void SimulateSellTradeDelegate(string tradeAccountCode, int sharesMarket, string sharesCode);

        /// <summary>
        /// 回购股票卖出交易
        /// </summary>
        private static void SimulateSellTrade(string tradeAccountCode,int sharesMarket, string sharesCode)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var accountShares = (from item in db.t_broker_account_shares_rel
                                         where item.TradeAccountCode == tradeAccountCode && item.Market == sharesMarket && item.SharesCode == sharesCode
                                         select item).FirstOrDefault();
                    if (accountShares == null)
                    {
                        throw new Exception();
                    }
                    var shares = (from item in db.t_shares_all
                                  where item.Market == sharesMarket && item.SharesCode == sharesCode
                                  select item).FirstOrDefault();
                    if (shares == null)
                    {
                        throw new Exception();
                    }

                    string tradeEntrustId = "";//委托编号
                    StringBuilder sErrInfo;
                    StringBuilder sResult;
                    var client = Singleton.instance.sellTradeClient.GetTradeClient(tradeAccountCode);
                    if (client == null)
                    {
                        throw new Exception();
                    }
                    try
                    {
                        int sellCount = accountShares.SimulateCanSoldCount;
                        int tempi = 0;
                        do
                        {
                            if (sellCount <= 0)
                            {
                                break;
                            }
                            tempi++;
                            sErrInfo = new StringBuilder(256);
                            sResult = new StringBuilder(1024 * 1024);

                            if (!Singleton.instance.IsTest)
                            {
                                //TradeX_M.SendOrder(sharesMarket == 0 ? client.TradeClientId0 : client.TradeClientId1, 1, 4, sharesMarket == 0 ? client.Holder0 : client.Holder1, sharesCode + "," + sharesMarket, 0, sellCount, sResult, sErrInfo);
                                Trade_Helper.SendOrder(sharesMarket == 0 ? client.TradeClientId0 : client.TradeClientId1, 1, 4, sharesMarket == 0 ? client.Holder0 : client.Holder1, sharesCode + "," + sharesMarket, 0, sellCount, sResult, sErrInfo);
                            }
                            else
                            {
                                TestHelper.SendSellOrder(sharesCode, "0", sellCount.ToString(), sResult, sErrInfo);
                            }

                            if (!string.IsNullOrEmpty(sResult.ToString()))
                            {
                                accountShares.SimulateCanSoldCount = accountShares.SimulateCanSoldCount - sellCount;
                                accountShares.SimulateSellingCount = accountShares.SimulateSellingCount + sellCount;
                                db.SaveChanges();

                                //解析返回数据
                                string result = sResult.ToString();

                                string[] rows = result.Split('\n');
                                for (int i = 0; i < rows.Length; i++)
                                {
                                    if (i == 1)
                                    {
                                        string[] column = rows[i].Split('\t');
                                        if (!string.IsNullOrEmpty(column[0]))
                                        {
                                            tradeEntrustId = column[0];//委托编号
                                        }
                                    }
                                }

                                db.t_broker_account_shares_rel_entrust.Add(new t_broker_account_shares_rel_entrust
                                {
                                    Status = 1,
                                    StatusDes = "",
                                    CancelCount = 0,
                                    CreateTime = DateTime.Now,
                                    DealAmount = 0,
                                    DealCount = 0,
                                    DealPrice = 0,
                                    EntrustCount = sellCount,
                                    EntrustId = tradeEntrustId,
                                    LastModified = DateTime.Now,
                                    RelId = accountShares.Id
                                });
                                db.SaveChanges();

                                break;
                            }
                            else
                            {
                                //卖出数量
                                sellCount = (sellCount - shares.SharesHandCount) / shares.SharesHandCount * shares.SharesHandCount;
                            }
                        } while (tempi <= 1);
                    }
                    finally
                    {
                        //归还链接
                        Singleton.instance.sellTradeClient.AddTradeClient(client);
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// 查询交易分页数据
        /// </summary>
        /// <param name="clientId">链接Id</param>
        /// <param name="nCategory"> 0资金 1股份 2当日委托 3当日成交 4可撤单 5股东代码 6融资余额 7融券余额 8可融证券 12可申购新股查询 13新股申购额度查询 14配号查询 15中签查询</param>
        public static bool QueryTradeData(int clientId,int nCategory,int bathNumber, StringBuilder pageMark, StringBuilder sResult) 
        {
            StringBuilder sErrInfo = new StringBuilder(256);
            if ((nCategory == 2 || nCategory == 3) && Singleton.instance.IsTest)
            {
                if (nCategory == 2)
                {
                    TestHelper.GetEntrustRecord(sResult, sErrInfo);
                }
                if (nCategory == 3)
                {
                    TestHelper.GetDealRecord(sResult, sErrInfo);
                }
            }
            else
            {

                Trade_Helper.QueryData(clientId, nCategory, sResult, sErrInfo);
                //if (nCategory == 0 || nCategory == 1)
                //{
                //    //TradeX_M.QueryData(clientId, nCategory, sResult, sErrInfo);
                //    Trade_Helper.QueryData(clientId, nCategory, sResult, sErrInfo);
                //}
                //else
                //{
                //    //TradeX_M.QueryDataEx(clientId, nCategory, bathNumber, pageMark, sResult, sErrInfo);
                //    Trade_Helper.QueryDataByePage(clientId, nCategory, bathNumber, pageMark, sResult, sErrInfo);
                //}
            }
            if (string.IsNullOrEmpty(sResult.ToString()))
            {
                return false;
            }
            return true;
        }
    }
}
