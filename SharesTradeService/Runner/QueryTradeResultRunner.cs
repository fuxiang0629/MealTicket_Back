using FXCommon.Common;
using Newtonsoft.Json;
using NoticeHandler;
using SharesTradeService.Handler;
using SharesTradeService.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeAPI;

namespace SharesTradeService
{
    public class QueryTradeResultRunner : Runner
    {
        DateTime timeNow = DateTime.Now;
        DateTime startTime = DateTime.Now;
        DateTime endTime = DateTime.Now;

        //上一次委托数据
        List<LastEntrustInfo> lastEntrust = new List<LastEntrustInfo>();


        public QueryTradeResultRunner() 
        {
            Name = "QueryTradeResultRunner";
            SleepTime = Singleton.instance.QueryTradeResultSleepTime_TradeDate;
        }

        public override bool Check 
        {
            get 
            {
                timeNow = DateTime.Now;
                int start = Singleton.instance.QueryTradeResultStartTime;
                int end = Singleton.instance.QueryTradeResultEndTime;
                if (end < 180)
                {
                    end = 180;
                }
                startTime = timeNow.AddSeconds(-start);
                endTime = timeNow.AddSeconds(-end);
                try
                {
                    if (!Helper.CheckTradeDate())
                    {
                        SleepTime = Singleton.instance.QueryTradeResultSleepTime_NoTradeDate;
                        return false;
                    }

                    if (!Helper.CheckTradeTime(startTime) && !Helper.CheckTradeTime(endTime))
                    {
                        SleepTime = Singleton.instance.QueryTradeResultSleepTime_TradeDate;
                        return false;
                    }

                    SleepTime = Singleton.instance.QueryTradeResultSleepTime_TradeDate;
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public override void Execute()
        {
            DateTime timeNow = DateTime.Now;
            DateTime timeDate = timeNow.Date;

            lastEntrust = lastEntrust.Where(e => e.CurrTime > timeDate).ToList();
            try
            {
                Dictionary<string, List<SharesEntrustInfo>> entrustDic = new Dictionary<string, List<SharesEntrustInfo>>();
                Dictionary<string, List<SharesEntrustDealInfo>> entrustDealDic = new Dictionary<string, List<SharesEntrustDealInfo>>();
                using (var db = new meal_ticketEntities())
                {
                    var entrustManager = (from item in db.t_account_shares_entrust_manager
                                          where item.CreateTime>= timeDate
                                          let orderIndex=item.Status==3?0:1
                                          orderby orderIndex,item.EntrustTime,item.Id
                                          select item).ToList();

                    var sharesAccountEntrust = (from item in db.t_broker_account_shares_rel_entrust
                                                where item.Status != 3 && item.CreateTime >= timeDate
                                                select item).ToList();

                    if (entrustManager.Count() <= 0 && sharesAccountEntrust.Count() <= 0)
                    {
                        return;
                    }

                    //查询所有账号当天委托列表/成交列表
                    StringBuilder sResult = new StringBuilder(1024 * 1024);
                    StringBuilder pageMark = new StringBuilder(256);
                    foreach (var accountCode in Singleton.instance.AllTradeAccountCodeList)
                    {
                        var client = Singleton.instance.queryTradeClient.GetTradeClient(accountCode);
                        if (client == null)
                        {
                            continue;
                        }
                        try
                        {
                            List<SharesEntrustInfo> entrustList = new List<SharesEntrustInfo>();
                            List<SharesEntrustDealInfo> entrustDealList = new List<SharesEntrustDealInfo>();

                            pageMark = new StringBuilder(256);
                            do
                            {
                                sResult = new StringBuilder(1024 * 1024 * 4);
                                if (!SharesTradeBusiness.QueryTradeData(client.TradeClientId0, 2, 200, pageMark, sResult))
                                {
                                    break;
                                }
                                else
                                {
                                    //解析返回数据
                                    string result = sResult.ToString();
                                    string[] rows = result.Split('\n');
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

                                        string EntrustTime = column[0];//委托时间
                                        string SharesCode = column[1];//证券代码
                                        string SharesName = column[2];//股票名称
                                        int TradeType = int.Parse(column[3]);//交易类型0买入 1卖出
                                        string StatusDes = column[6];//状态描述
                                        long EntrustPrice = (long)(Math.Round(float.Parse(column[7]) * Singleton.instance.PriceFormat, 0));//委托价格
                                        int EntrustCount = (int)(float.Parse(column[8]));//委托数量
                                        string EntrustId = column[9];
                                        long DealPrice = (long)(Math.Round(float.Parse(column[10]) * Singleton.instance.PriceFormat, 0));//成交价格
                                        int DealCount = (int)(float.Parse(column[11]));//成交数量

                                        //判断上一次获取数据时间
                                        DateTime lastTime = timeNow;

                                        if(StatusDes== "已成" || StatusDes== "部撤")
                                        {
                                            var tempLastEntrust = lastEntrust.Where(e => e.AccountCode == accountCode && e.EntrustId == EntrustId).FirstOrDefault();
                                            if (tempLastEntrust == null)
                                            {
                                                lastTime = timeNow;
                                                lastEntrust.Add(new LastEntrustInfo
                                                {
                                                    AccountCode= accountCode,
                                                    EntrustId= EntrustId,
                                                    CurrTime=timeNow
                                                });
                                            }
                                            else
                                            {
                                                lastTime = tempLastEntrust.CurrTime;
                                            }
                                        }
                                        
                                        entrustList.Add(new SharesEntrustInfo
                                        { 
                                            EntrustTime = EntrustTime,
                                            SharesCode = SharesCode,
                                            SharesName = SharesName,
                                            TradeType = TradeType,
                                            StatusDes = StatusDes,
                                            EntrustPrice = EntrustPrice,
                                            EntrustCount = EntrustCount,
                                            EntrustId = EntrustId,
                                            DealPrice = DealPrice,
                                            DealCount = DealCount,
                                            TotalDealCount= DealCount,
                                            TradeAccountCode = accountCode,
                                            CurrTime=timeNow,
                                            LastTime= lastTime
                                        });
                                    }
                                }


                                if (string.IsNullOrEmpty(pageMark.ToString()))
                                {
                                    break;
                                }

                            } while (true);
                            entrustDic.Add(accountCode, entrustList);


                            pageMark = new StringBuilder(256);
                            do
                            {
                                sResult = new StringBuilder(1024 * 1024 * 4);
                                if (!SharesTradeBusiness.QueryTradeData(client.TradeClientId0, 3, 200, pageMark, sResult))
                                {
                                    break;
                                }
                                else
                                {
                                    //解析返回数据
                                    string result = sResult.ToString();
                                    string[] rows = result.Split('\n');
                                    for (int i = 0; i < rows.Length; i++)
                                    {
                                        if (i == 0)
                                        {
                                            continue;
                                        }
                                        string[] column = rows[i].Split('\t');
                                        if (column.Length < 10)
                                        {
                                            continue;
                                        }

                                        DateTime DealTime;
                                        if (!DateTime.TryParseExact(timeNow.ToString("yyyyMMdd") + column[0].Replace(":", ""), "yyyyMMddHHmmss", new CultureInfo("zh-CN", true), DateTimeStyles.None, out DealTime))//成交时间
                                        {
                                            DealTime = DateTime.Now;
                                        }
                                        long DealPrice = (long)(Math.Round(float.Parse(column[5]) * Singleton.instance.PriceFormat, 0));//成交价格
                                        int DealCount = (int)(float.Parse(column[6]));//成交数量
                                        long DealAmount = (long)(Math.Round(float.Parse(column[7]) * Singleton.instance.PriceFormat, 0));//成交金额
                                        string DealId = column[8];//成交Id
                                        string EntrustId = column[9];//委托Id

                                        entrustDealList.Add(new SharesEntrustDealInfo
                                        {
                                            DealAmount = DealAmount,
                                            DealCount = DealCount,
                                            DealId = DealId,
                                            DealPrice = DealPrice,
                                            DealTime = DealTime,
                                            EntrustId = EntrustId,
                                            TradeAccountCode= accountCode
                                        });
                                    }
                                }

                                if (string.IsNullOrEmpty(pageMark.ToString()))
                                {
                                    break;
                                }

                            } while (true);
                            entrustDealDic.Add(accountCode, entrustDealList);
                        }
                        finally
                        {
                            //归还链接
                            Singleton.instance.queryTradeClient.AddTradeClient(client);
                        }
                    }

                    foreach (var item in entrustManager)
                    {
                        List<SharesEntrustInfo> tempList;
                        if (!entrustDic.TryGetValue(item.TradeAccountCode, out tempList))
                        {
                            continue;
                        }
                        var tempInfo = tempList.Where(e => e.EntrustId == item.EntrustId && item.CreateTime > timeDate && item.TradeAccountCode == e.TradeAccountCode).FirstOrDefault();
                        if (tempInfo == null)
                        {
                            continue;
                        }

                        if (item.Status == 3)
                        {
                            tempInfo.DealCount = tempInfo.DealCount - item.DealCount;
                            continue;
                        }
                        if (tempInfo.DealCount < 0)
                        {
                            tempInfo.DealCount = 0;
                        }

                        List<SharesEntrustDealInfo> dealList = new List<SharesEntrustDealInfo>();
                        List<SharesEntrustDealInfo> relDealList = new List<SharesEntrustDealInfo>();
                        if (entrustDealDic.TryGetValue(item.TradeAccountCode, out relDealList))
                        {
                            relDealList = relDealList.Where(e => e.EntrustId == item.EntrustId && item.CreateTime > timeDate && item.TradeAccountCode == e.TradeAccountCode).ToList();
                        }

                        int dealCount = 0;
                        //判断是否需要分配
                        if (tempInfo.TotalDealCount > item.EntrustCount)
                        {
                            dealCount = item.EntrustCount;
                            if (tempInfo.DealCount < item.EntrustCount)
                            {
                                dealCount = tempInfo.DealCount;
                            }
                            dealList.Add(new SharesEntrustDealInfo
                            {
                                DealAmount = dealCount * tempInfo.DealPrice,
                                DealCount= dealCount,
                                DealId= "FP_"+Guid.NewGuid().ToString("N"),
                                DealPrice= tempInfo.DealPrice,
                                DealTime= DateTime.Now,
                                EntrustId= item.EntrustId,
                                TradeAccountCode= item.TradeAccountCode
                            });
                        }
                        else
                        {
                            dealCount = tempInfo.DealCount;
                            if (dealCount>0)
                            {
                                dealList = relDealList;
                            }
                        }
                        bool isSuccess = TradeFinish(item.Id, tempInfo.DealPrice, dealCount, tempInfo.Status, dealList, relDealList,db);
                        tempInfo.DealCount = tempInfo.DealCount - dealCount;

                        if (isSuccess)
                        {
                            if (tempInfo.Status == 2)
                            {
                                Logger.WriteFileLog("结果数据:" + JsonConvert.SerializeObject(tempInfo), "QueryResult", null);

                                //发送通知
                                try
                                {
                                    var entrust = (from x in db.t_account_shares_entrust
                                                   join x2 in db.t_shares_all on new { x.Market, x.SharesCode } equals new { x2.Market, x2.SharesCode }
                                                   where x.Id == item.BuyId
                                                   select new { x, x2 }).FirstOrDefault();
                                    if (entrust != null)
                                    {
                                        var noticeHold1 = (from x in db.t_notice_hold
                                                           where x.HoldId == entrust.x.HoldId && x.Type == 1
                                                           select x).FirstOrDefault();
                                        var noticeHold2 = (from x in db.t_notice_hold
                                                           where x.HoldId == entrust.x.HoldId && x.Type == 2
                                                           select x).FirstOrDefault();
                                        var tempPara = JsonConvert.SerializeObject(new
                                        {
                                            time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                            sharescode = entrust.x.SharesCode,
                                            sharesname = entrust.x2.SharesName,
                                            dealcount = entrust.x.DealCount,
                                            dealstatus = entrust.x.DealCount <= 0 ? "已撤" : entrust.x.DealCount >= entrust.x.EntrustCount ? "已成" : entrust.x.DealCount < entrust.x.EntrustCount ? "部撤" : ""
                                        });
                                        if (entrust.x.Type == 0)//客户提交
                                        {
                                            if (entrust.x.TradeType == 1)
                                            {
                                                NoticeSender.SendExecute("NoticeSend.BuySharesFinish", entrust.x.AccountId, tempPara);
                                                //警戒线通知清除
                                                try
                                                {
                                                    if (entrust.x.DealCount > 0)
                                                    {
                                                        if (noticeHold1 != null)
                                                        {
                                                            noticeHold1.LastNotifyTime = null;
                                                            noticeHold1.TimeId = -1;
                                                        }
                                                        if (noticeHold2 != null)
                                                        {
                                                            noticeHold2.LastNotifyTime = null;
                                                            noticeHold2.TimeId = -1;
                                                        }
                                                        db.SaveChanges();
                                                    }
                                                }
                                                catch (Exception ex) { }
                                            }
                                            else
                                            {
                                                NoticeSender.SendExecute("NoticeSend.SellSharesFinish", entrust.x.AccountId, tempPara);
                                            }
                                        }
                                        if (entrust.x.Type == 1)//自动平仓
                                        {
                                            NoticeSender.SendExecute("NoticeSend.ClosingAuto", entrust.x.AccountId, tempPara);
                                        }
                                        if (entrust.x.Type == 2)//强制平仓
                                        {
                                            NoticeSender.SendExecute("NoticeSend.ClosingForce", entrust.x.AccountId, tempPara);
                                            //警戒线通知清除
                                            try
                                            {
                                                if (entrust.x.DealCount > 0)
                                                {
                                                    if (noticeHold1 != null)
                                                    {
                                                        noticeHold1.LastNotifyTime = null;
                                                        noticeHold1.TimeId = -1;
                                                    }
                                                    if (noticeHold2 != null)
                                                    {
                                                        noticeHold2.LastNotifyTime = null;
                                                        noticeHold2.TimeId = -1;
                                                    }
                                                    db.SaveChanges();
                                                }
                                            }
                                            catch (Exception ex) { }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteFileLog("股票买卖通知异常", ex);
                                }
                            }
                            //结束交易时间外，结束交易
                            if (Helper.CheckTradeTime(endTime) && !Helper.CheckTradeTime(timeNow))
                            {
                                item.CanClear = true;
                                db.SaveChanges();
                            }
                        }
                    }

                    foreach (var item in sharesAccountEntrust)
                    {
                        using (var tran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var tradeCode = (from x in db.t_broker_account_shares_rel
                                                 where x.Id == item.RelId
                                                 select x).FirstOrDefault();
                                if (tradeCode == null)
                                {
                                    continue;
                                }
                                List<SharesEntrustInfo> tempList;
                                if (!entrustDic.TryGetValue(tradeCode.TradeAccountCode, out tempList))
                                {
                                    continue;
                                }
                                var tempInfo = tempList.Where(e => e.EntrustId == item.EntrustId && item.CreateTime > timeDate && tradeCode.TradeAccountCode == e.TradeAccountCode).FirstOrDefault();
                                if (tempInfo == null)
                                {
                                    continue;
                                }
                                if (tempInfo.Status != 2)//终结态
                                {
                                    continue;
                                }

                                item.DealCount = tempInfo.DealCount;
                                item.DealPrice = tempInfo.DealPrice;
                                item.DealAmount = tempInfo.DealCount * tempInfo.DealPrice;
                                item.CancelCount = item.EntrustCount - tempInfo.DealCount;
                                item.Status = 3;
                                db.SaveChanges();
                                tradeCode.TotalSharesCount = tradeCode.TotalSharesCount - tempInfo.DealCount;
                                tradeCode.SimulateSharesCount = tradeCode.SimulateSharesCount - tempInfo.DealCount;
                                tradeCode.SimulateCanSoldCount = tradeCode.SimulateCanSoldCount + item.EntrustCount - tempInfo.DealCount;
                                tradeCode.SimulateSellingCount = tradeCode.SimulateSellingCount - item.EntrustCount;
                                db.SaveChanges();

                                tran.Commit();
                            }
                            catch (Exception ex)
                            {
                                tran.Rollback();
                                Logger.WriteFileLog("t_broker_account_shares_rel更新数据库失败", ex);
                            }
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("QueryTradeResultRunner失败", ex);
            }
        }

        private bool TradeFinish(long Id, long DealPrice, int DealCount, int Status, List<SharesEntrustDealInfo> dealList, List<SharesEntrustDealInfo> realDealList, meal_ticketEntities db)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    if (Status == 2)//终结态
                    {
                        db.P_TradeFinish(Id, DealPrice, DealCount);
                    }
                    else
                    {
                        var entrustManager = (from item in db.t_account_shares_entrust_manager
                                              where item.Id == Id
                                              select item).FirstOrDefault();
                        if (entrustManager != null)
                        {
                            entrustManager.DealCount = DealCount;
                            entrustManager.DealPrice = DealPrice;
                            entrustManager.DealAmount = DealCount * DealPrice;

                            db.SaveChanges();
                        }
                    }
                    if (dealList != null)
                    {
                        var details = (from item in db.t_account_shares_entrust_manager_dealdetails
                                       where item.EntrustManagerId == Id
                                       select item).ToList();
                        if (details.Count() > 0)
                        {
                            db.t_account_shares_entrust_manager_dealdetails.RemoveRange(details);
                            db.SaveChanges();
                        }

                        foreach (var item in dealList)
                        {
                            db.t_account_shares_entrust_manager_dealdetails.Add(new t_account_shares_entrust_manager_dealdetails
                            {
                                DealAmount = item.DealAmount,
                                DealCount = item.DealCount,
                                DealId = item.DealId,
                                DealPrice = item.DealPrice,
                                DealTime = item.DealTime,
                                EntrustManagerId = Id
                            });
                        }
                        db.SaveChanges();
                    }
                    if (realDealList != null)
                    {
                        var details = (from item in db.t_account_shares_entrust_manager_dealdetails_real
                                       where item.EntrustManagerId == Id
                                       select item).ToList();
                        if (details.Count() > 0)
                        {
                            db.t_account_shares_entrust_manager_dealdetails_real.RemoveRange(details);
                            db.SaveChanges();
                        }

                        foreach (var item in realDealList)
                        {
                            db.t_account_shares_entrust_manager_dealdetails_real.Add(new t_account_shares_entrust_manager_dealdetails_real
                            {
                                DealAmount = item.DealAmount,
                                DealCount = item.DealCount,
                                DealId = item.DealId,
                                DealPrice = item.DealPrice,
                                DealTime = item.DealTime,
                                EntrustManagerId = Id
                            });
                        }
                        db.SaveChanges();
                    }

                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.WriteFileLog("查询结果数据库出错", ex);
                    return false;
                }
            }
        }
    }
}
