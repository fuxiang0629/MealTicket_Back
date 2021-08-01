﻿using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Enum;
using MealTicket_Web_Handler.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockTrendMonitor.Define.StockMonitorDefine;

namespace MealTicket_Web_Handler.Runner
{
    public class RunnerHelper
    {
        public static void ClearTransactiondata()
        {
            string sql = "";
            using (var db = new meal_ticketEntities(ConfigurationManager.ConnectionStrings["meal_ticketEntities2"].ConnectionString))
            {
                db.Database.CommandTimeout = 7200;
                try
                {
                    sql = @"declare @currTime datetime=getdate()
                    insert into t_shares_transactiondata_history
                    ([Market],[SharesCode],[Time],[TimeStr],[Price],[Volume],[Stock],[Type],[OrderIndex],[LastModified],[SharesInfo],[SharesInfoNum])
                    select [Market],[SharesCode],[Time],[TimeStr],[Price],[Volume],[Stock],[Type],[OrderIndex],[LastModified],[SharesInfo],[SharesInfoNum]
                    from t_shares_transactiondata with(xlock)
                    where [Time]<convert(varchar(10),@currTime,120);";
                    db.Database.ExecuteSqlCommand(sql);

                    sql = "truncate table t_shares_transactiondata";
                    db.Database.ExecuteSqlCommand(sql);
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("执行出错", ex);
                }
            }
        }

        /// <summary>
        /// 检查是否交易日期
        /// </summary>
        /// <returns></returns>
        public static bool CheckTradeDate(DateTime? time = null)
        {
            DateTime timeDate = DateTime.Now.Date;
            if (time != null)
            {
                timeDate = time.Value.Date;
            }
            int timeDateInt = int.Parse(timeDate.ToString("yyyyMMdd"));
            using (var db = new meal_ticketEntities())
            {
                //排除周末
                var timeWeek = (from item in db.t_dim_time
                                where item.the_date == timeDateInt
                                select item).FirstOrDefault();
                if (timeWeek == null || timeWeek.week_day == 7 || timeWeek.week_day == 1)
                {
                    return false;
                }
                //排除节假日
                var tradeDate = (from item in db.t_shares_limit_date_group
                                 join item2 in db.t_shares_limit_date on item.Id equals item2.GroupId
                                 where item.Status == 1 && item2.Status == 1 && item2.BeginDate <= timeDate && item2.EndDate >= timeDate
                                 select item2).FirstOrDefault();
                if (tradeDate != null)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// 检查当前是否交易真实时间（time2,time3,time4）
        /// </summary>
        /// <returns></returns>
        public static bool CheckTradeTime2(DateTime? time = null, bool time2 = true, bool time3 = true, bool time4 = true)
        {
            if (!CheckTradeDate(time))
            {
                return false;
            }
            DateTime timeDis = DateTime.Now;
            if (time != null)
            {
                timeDis = time.Value;
            }
            TimeSpan timeSpanNow = TimeSpan.Parse(timeDis.ToString("HH:mm:ss"));
            using (var db = new meal_ticketEntities())
            {
                var tradeTime = (from item in db.t_shares_limit_time
                                 select item).ToList();
                foreach (var item in tradeTime)
                {
                    //解析time2
                    if (item.Time2 != null && time2)
                    {
                        string[] timeArr = item.Time2.Split(',');
                        foreach (var times in timeArr)
                        {
                            var timeSpanArr = times.Split('-');
                            if (timeSpanArr.Length != 2)
                            {
                                continue;
                            }
                            TimeSpan timeStart = TimeSpan.Parse(timeSpanArr[0]);
                            TimeSpan timeEnd = TimeSpan.Parse(timeSpanArr[1]);
                            if (timeSpanNow >= timeStart && timeSpanNow < timeEnd)
                            {
                                return true;
                            }
                        }
                    }
                    //解析time3
                    if (item.Time3 != null && time3)
                    {
                        string[] timeArr = item.Time3.Split(',');
                        foreach (var times in timeArr)
                        {
                            var timeSpanArr = times.Split('-');
                            if (timeSpanArr.Length != 2)
                            {
                                continue;
                            }
                            TimeSpan timeStart = TimeSpan.Parse(timeSpanArr[0]);
                            TimeSpan timeEnd = TimeSpan.Parse(timeSpanArr[1]);
                            if (timeSpanNow >= timeStart && timeSpanNow < timeEnd)
                            {
                                return true;
                            }
                        }
                    }
                    //解析time4
                    if (item.Time4 != null && time4)
                    {
                        string[] timeArr = item.Time4.Split(',');
                        foreach (var times in timeArr)
                        {
                            var timeSpanArr = times.Split('-');
                            if (timeSpanArr.Length != 2)
                            {
                                continue;
                            }
                            TimeSpan timeStart = TimeSpan.Parse(timeSpanArr[0]);
                            TimeSpan timeEnd = TimeSpan.Parse(timeSpanArr[1]);
                            if (timeSpanNow >= timeStart && timeSpanNow < timeEnd)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 自动交易
        /// </summary>
        public static void TradeAuto()
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var condition = (from item in db.t_account_shares_hold_conditiontrade
                                 join item2 in db.t_account_shares_hold on item.HoldId equals item2.Id
                                 join item4 in db.t_shares_all on new { item2.Market, item2.SharesCode } equals new { item4.Market, item4.SharesCode }
                                 join item3 in db.v_shares_quotes_last on new { item2.Market, item2.SharesCode } equals new { item3.Market, item3.SharesCode }
                                 where item.TradeType == 2 && item.TriggerTime == null && item2.Status == 1 && SqlFunctions.DateAdd("MI", 1, item3.LastModified) > timeNow && item3.PresentPrice > 0 && item3.ClosedPrice > 0 && item.Status == 1
                                 group new { item, item2, item3, item4 } by new { item.HoldId, item.Type, item2, item3, item4 } into g
                                 select g).ToList();
                foreach (var item in condition)
                {
                    //可卖数量为0不执行
                    if (item.Key.item2.RemainCount > 0 && item.Key.item2.CanSoldCount <= 0)
                    {
                        continue;
                    }
                    //计算杠杆倍数
                    //计算涨停价格
                    long maxPrice;
                    //计算跌停价格
                    long minPrice;
                    var rules = (from x in db.t_shares_limit_fundmultiple
                                 where (x.LimitMarket == item.Key.item2.Market || x.LimitMarket == -1) && (item.Key.item2.SharesCode.StartsWith(x.LimitKey))
                                 orderby x.Priority descending, x.FundMultiple
                                 select x).FirstOrDefault();
                    if (rules == null)
                    {
                        continue;
                    }
                    else
                    {
                        int limitRange = rules.Range;
                        if (item.Key.item4.SharesName.Contains("ST"))
                        {
                            limitRange = limitRange / 2;
                        }
                        maxPrice = ((long)Math.Round((item.Key.item3.ClosedPrice + item.Key.item3.ClosedPrice * (limitRange * 1.0 / 10000)) / 100)) * 100;
                        minPrice = ((long)Math.Round((item.Key.item3.ClosedPrice - item.Key.item3.ClosedPrice * (limitRange * 1.0 / 10000)) / 100)) * 100;
                    }
                    var holdList = item.ToList();
                    if (item.Key.Type == 1)
                    {
                        holdList = holdList.Where(e => e.item.ConditionTime != null && e.item.ConditionTime < timeNow).OrderBy(e => e.item.ConditionTime).ToList();
                    }
                    else if (item.Key.Type == 2)
                    {
                        holdList = (from hold in holdList
                                    let disPrice = hold.item.ConditionType == 1 ? hold.item.ConditionPrice : (hold.item.ConditionRelativeType == 1 ? maxPrice + hold.item.ConditionRelativeRate * 100 : hold.item.ConditionRelativeType == 2 ? minPrice + hold.item.ConditionRelativeRate * 100 : (long)(Math.Round((hold.item.ConditionRelativeRate * 1.0 / 10000 * hold.item3.ClosedPrice + hold.item3.ClosedPrice) / 100) * 100))
                                    where hold.item3.PresentPrice >= disPrice
                                    orderby disPrice
                                    select hold).ToList();
                    }
                    else if (item.Key.Type == 3)
                    {
                        holdList = (from hold in holdList
                                    let disPrice = hold.item.ConditionType == 1 ? hold.item.ConditionPrice : (hold.item.ConditionRelativeType == 1 ? maxPrice + hold.item.ConditionRelativeRate * 100 : hold.item.ConditionRelativeType == 2 ? minPrice + hold.item.ConditionRelativeRate * 100 : (long)(Math.Round((hold.item.ConditionRelativeRate * 1.0 / 10000 * hold.item3.ClosedPrice + hold.item3.ClosedPrice) / 100) * 100))
                                    where hold.item3.PresentPrice <= disPrice
                                    orderby disPrice descending
                                    select hold).ToList();
                    }
                    else
                    {
                        continue;
                    }

                    foreach (var hold in holdList)
                    {
                        //涨停不卖出时不执行
                        if (hold.item.ForbidType == 1)
                        {
                            if (maxPrice == hold.item3.PresentPrice)
                            {
                                break;
                            }
                        }
                        //判断是否封板
                        if (hold.item.OtherConditionRelative == 1 && hold.item3.BuyPrice1 != hold.item3.LimitUpPrice)
                        {
                            continue;
                        }
                        else if (hold.item.OtherConditionRelative == 2 && hold.item3.SellPrice1 != hold.item3.LimitDownPrice)
                        {
                            continue;
                        }

                        using (var tran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                hold.item.TriggerTime = timeNow;
                                db.SaveChanges();

                                //查询需要更改的子条件
                                var childList = (from x in db.t_account_shares_hold_conditiontrade_child
                                                 where x.ConditionId == hold.item.Id
                                                 select x).ToList();
                                foreach (var child in childList)
                                {
                                    var childcondition = (from x in db.t_account_shares_hold_conditiontrade
                                                          where x.Id == child.ChildId
                                                          select x).FirstOrDefault();
                                    if (childcondition == null)
                                    {
                                        continue;
                                    }
                                    if (childcondition.TriggerTime != null)
                                    {
                                        continue;
                                    }
                                    childcondition.Status = child.Status;
                                }
                                db.SaveChanges();
                                tran.Commit();
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteFileLog("触发失败", ex);
                                tran.Rollback();
                                break;
                            }
                        }

                        int EntrustCount = hold.item.EntrustCount % 100 == 0 ? hold.item.EntrustCount : (hold.item.EntrustCount / 100 * 100 + 100);
                        if (hold.item2.CanSoldCount < EntrustCount)
                        {
                            EntrustCount = hold.item2.CanSoldCount;
                        }
                        long EntrustPrice = 0;
                        if (hold.item.EntrustType == 2)
                        {
                            switch (hold.item.EntrustPriceGear)
                            {
                                case 1:
                                    EntrustPrice = hold.item3.BuyPrice1 == 0 ? hold.item3.PresentPrice : hold.item3.BuyPrice1;
                                    break;
                                case 2:
                                    EntrustPrice = hold.item3.BuyPrice2 == 0 ? hold.item3.PresentPrice : hold.item3.BuyPrice2;
                                    break;
                                case 3:
                                    EntrustPrice = hold.item3.BuyPrice3 == 0 ? hold.item3.PresentPrice : hold.item3.BuyPrice3;
                                    break;
                                case 4:
                                    EntrustPrice = hold.item3.BuyPrice4 == 0 ? hold.item3.PresentPrice : hold.item3.BuyPrice4;
                                    break;
                                case 5:
                                    EntrustPrice = hold.item3.BuyPrice5 == 0 ? hold.item3.PresentPrice : hold.item3.BuyPrice5;
                                    break;
                                case 6:
                                    EntrustPrice = hold.item3.SellPrice1 == 0 ? hold.item3.PresentPrice : hold.item3.SellPrice1;
                                    break;
                                case 7:
                                    EntrustPrice = hold.item3.SellPrice2 == 0 ? hold.item3.PresentPrice : hold.item3.SellPrice2;
                                    break;
                                case 8:
                                    EntrustPrice = hold.item3.SellPrice3 == 0 ? hold.item3.PresentPrice : hold.item3.SellPrice3;
                                    break;
                                case 9:
                                    EntrustPrice = hold.item3.SellPrice4 == 0 ? hold.item3.PresentPrice : hold.item3.SellPrice4;
                                    break;
                                case 10:
                                    EntrustPrice = hold.item3.SellPrice5 == 0 ? hold.item3.PresentPrice : hold.item3.SellPrice5;
                                    break;
                                case 11:
                                    EntrustPrice = maxPrice;
                                    break;
                                case 12:
                                    EntrustPrice = minPrice;
                                    break;
                            }
                        }

                        //跟投用户
                        var followList = (from x in db.t_account_shares_hold_conditiontrade_follow
                                          where x.ConditiontradeId == hold.item.Id
                                          select x.FollowAccountId).ToList();

                        long sellId = 0;
                        TrendHandler trendHandler = new TrendHandler();
                        try
                        {
                            sellId = trendHandler.AccountApplyTradeSell(new AccountApplyTradeSellRequest
                            {
                                SellCount = EntrustCount,
                                SellPrice = EntrustPrice,
                                SellType = hold.item.EntrustType,
                                HoldId = hold.item2.Id,
                                FollowList = followList,
                                MainAccountId= hold.item.CreateAccountId
                            }, new HeadBase
                            {
                                AccountId = hold.item.AccountId
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("自动卖出失败", ex);
                            continue;
                        }
                        hold.item.EntrustId = sellId;
                        db.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// 自动买入交易
        /// </summary>
        public static void TradeAutoBuyBak()
        {
            ThreadMsgTemplate<TradeAutoBuyConditionGroup> conditionData = new ThreadMsgTemplate<TradeAutoBuyConditionGroup>();
            conditionData.Init();
            List<TradeAutoBuyConditionGroup> groupList = new List<TradeAutoBuyConditionGroup>();
            using (var db = new meal_ticketEntities())
            {
                List<TradeAutoBuyCondition> disResult = new List<TradeAutoBuyCondition>();
                //查询价格条件达标的数据
                string sql = @"select t.Id,t.CreateAccountId,t1.AccountId,t1.SharesCode,t1.Market,t2.PresentPrice,t2.ClosedPrice,t2.LastModified,t.LimitUp,t2.LimitUpPrice,t2.LimitDownPrice,
t.ForbidType,t.FirstExecTime,t2.BuyPrice1,t2.BuyPrice2,t2.BuyPrice3,t2.BuyPrice4,t2.BuyPrice5,t2.SellPrice1,t2.SellPrice2,t2.SellPrice3,
t2.SellPrice4,t2.SellPrice5,t.ConditionPrice,t.ConditionType,t.ConditionRelativeType,t.ConditionRelativeRate,t.IsGreater,t.OtherConditionRelative,
t.BusinessStatus,t.TriggerTime,t.ExecStatus,t.BuyAuto,t.EntrustPriceGear,t.EntrustType,t.EntrustAmount,t.IsHold,t.FollowType
from t_account_shares_conditiontrade_buy_details t with(nolock)
inner join t_account_shares_conditiontrade_buy t1 with(nolock) on t.ConditionId=t1.Id
inner join v_shares_quotes_last t2 with(nolock) on t1.Market=t2.Market and t1.SharesCode=t2.SharesCode
where t.[Status]=1 and t1.[Status]=1 and t.BusinessStatus=0";
                disResult = db.Database.SqlQuery<TradeAutoBuyCondition>(sql).ToList();

                if (disResult.Count() <= 0)
                {
                    conditionData.Release();
                    return;
                }
                foreach (var item in disResult)
                {
                    var groupInfo = groupList.Where(e => e.AccountId == item.AccountId && e.Market == item.Market && e.SharesCode == item.SharesCode).FirstOrDefault();
                    if (groupInfo == null)
                    {
                        groupList.Add(new TradeAutoBuyConditionGroup
                        {
                            SharesCode = item.SharesCode,
                            AccountId = item.AccountId,
                            Market = item.Market,
                            List = new List<TradeAutoBuyCondition>
                            {
                                JsonConvert.DeserializeObject<TradeAutoBuyCondition>(JsonConvert.SerializeObject(item))
                            }
                        });
                    }
                    else
                    {
                        groupInfo.List.Add(JsonConvert.DeserializeObject<TradeAutoBuyCondition>(JsonConvert.SerializeObject(item)));
                    }
                }
                foreach (var item in groupList)
                {
                    conditionData.AddMessage(JsonConvert.DeserializeObject<TradeAutoBuyConditionGroup>(JsonConvert.SerializeObject(item)), false);
                }
            }
            int taskCount = groupList.Count();
            if (Singleton.Instance.AutoBuyTaskMaxCount < taskCount)
            {
                taskCount = Singleton.Instance.AutoBuyTaskMaxCount;
            }
            Task[] tArr = new Task[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                tArr[i] = new Task(() =>
                {
                    while (true)
                    {
                        TradeAutoBuyConditionGroup temp = new TradeAutoBuyConditionGroup();
                        if (!conditionData.GetMessage(ref temp, true))
                        {
                            break;
                        }
                        foreach (var item in temp.List)
                        {
                            doAutoBuyTask(item);
                        }
                    }
                });
                tArr[i].Start();
            }
            Task.WaitAll(tArr);
            conditionData.Release();
        }

        public static void TradeAutoBuy()
        {
            List<TradeAutoBuyCondition> disResult = new List<TradeAutoBuyCondition>();
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select t.Id,t1.Id ConditionId,t1.AccountId,t1.SharesCode,t1.Market,t2.PresentPrice,t2.ClosedPrice,t2.LastModified,t.LimitUp,t2.LimitUpPrice,t2.LimitDownPrice,
t.ForbidType,t.FirstExecTime,t2.BuyPrice1,t2.BuyPrice2,t2.BuyPrice3,t2.BuyPrice4,t2.BuyPrice5,t2.SellPrice1,t2.SellPrice2,t2.SellPrice3,
t2.SellPrice4,t2.SellPrice5,t.ConditionPrice,t.ConditionType,t.ConditionRelativeType,t.ConditionRelativeRate,t.IsGreater,t.OtherConditionRelative,
t.BusinessStatus,t.TriggerTime,t.ExecStatus,t.BuyAuto,t.EntrustPriceGear,t.EntrustType,t.EntrustAmount,t.IsHold,t.FollowType
from t_account_shares_conditiontrade_buy_details t with(nolock)
inner join t_account_shares_conditiontrade_buy t1 with(nolock) on t.ConditionId=t1.Id
inner join v_shares_quotes_last t2 with(nolock) on t1.Market=t2.Market and t1.SharesCode=t2.SharesCode
where t.[Status]=1 and t1.[Status]=1 and t.BusinessStatus=0";
                disResult = db.Database.SqlQuery<TradeAutoBuyCondition>(sql).ToList();
            }

            TodoTradeAutoBuy(disResult);
        }

        private static void TodoTradeAutoBuy(List<TradeAutoBuyCondition> disResult, bool isSyncro = false)
        {
            ThreadMsgTemplate<TradeAutoBuyConditionGroup> conditionData = new ThreadMsgTemplate<TradeAutoBuyConditionGroup>();
            conditionData.Init();
            List<TradeAutoBuyConditionGroup> groupList = new List<TradeAutoBuyConditionGroup>();

            if (disResult.Count() <= 0)
            {
                conditionData.Release();
                return;
            }

            foreach (var item in disResult)
            {
                var groupInfo = groupList.Where(e => e.AccountId == item.AccountId && e.Market == item.Market && e.SharesCode == item.SharesCode).FirstOrDefault();
                if (groupInfo == null)
                {
                    groupList.Add(new TradeAutoBuyConditionGroup
                    {
                        SharesCode = item.SharesCode,
                        AccountId = item.AccountId,
                        Market = item.Market,
                        List = new List<TradeAutoBuyCondition>
                            {
                                JsonConvert.DeserializeObject<TradeAutoBuyCondition>(JsonConvert.SerializeObject(item))
                            }
                    });
                }
                else
                {
                    groupInfo.List.Add(JsonConvert.DeserializeObject<TradeAutoBuyCondition>(JsonConvert.SerializeObject(item)));
                }
            }
            foreach (var item in groupList)
            {
                conditionData.AddMessage(JsonConvert.DeserializeObject<TradeAutoBuyConditionGroup>(JsonConvert.SerializeObject(item)), false);
            }

            int taskCount = groupList.Count();
            if (Singleton.Instance.AutoBuyTaskMaxCount < taskCount)
            {
                taskCount = Singleton.Instance.AutoBuyTaskMaxCount;
            }
            Task[] tArr = new Task[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                tArr[i] = new Task(() =>
                {
                    while (true)
                    {
                        TradeAutoBuyConditionGroup temp = new TradeAutoBuyConditionGroup();
                        if (!conditionData.GetMessage(ref temp, true))
                        {
                            break;
                        }
                        foreach (var item in temp.List)
                        {
                            doAutoBuyTask(item, isSyncro);
                        }
                    }
                });
                tArr[i].Start();
            }
            Task.WaitAll(tArr);
            conditionData.Release();

            var list = Singleton.Instance.GetSyncroList();
            if (list.Count() > 0)
            {
                TodoTradeAutoBuy(list, true);
            }
        }

        private static void doAutoBuyTask(TradeAutoBuyCondition item, bool isSyncro = false)
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                string sql = "";
                StringBuilder logRecord = new StringBuilder();//判断日志
                logRecord.AppendLine("===" + item.AccountId + "开始执行条件" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                if (!isSyncro)
                {
                    logRecord.AppendLine("===开始判断价格条件" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                    if (item.LastModified < timeNow.AddSeconds(-30) || item.LimitUpPrice <= 0 || item.LimitDownPrice <= 0)
                    {
                        return;
                    }
                    long presentPrice = item.PresentPrice;//当前价格
                    long closedPrice = item.ClosedPrice;//昨日收盘价
                    if (closedPrice <= 0 || presentPrice <= 0)
                    {
                        return;
                    }
                    //跌停价不买入
                    if (item.ForbidType == 1)
                    {
                        if (item.LimitDownPrice == item.PresentPrice)
                        {
                            return;
                        }
                    }

                    //判断是否当天第一次跑
                    if (item.FirstExecTime == null || item.FirstExecTime < timeNow.Date)
                    {
                        sql = string.Format("update t_account_shares_conditiontrade_buy_details set FirstExecTime='{0}',ExecStatus=0 where Id={1}", timeNow.ToString("yyyy-MM-dd HH:mm:ss.fff"), item.Id);
                        db.Database.ExecuteSqlCommand(sql);
                    }

                    //判断当前是否涨停
                    int ExecStatus = 0;
                    if (item.BuyPrice1 == item.LimitUpPrice)//买一价为涨停价
                    {
                        if (item.LimitUp)
                        {
                            if (item.ExecStatus == 1)
                            {
                                return;
                            }
                            else if (item.ExecStatus == 0)
                            {
                                ExecStatus = 1;
                            }
                        }
                        else if (item.ExecStatus != 2)
                        {
                            ExecStatus = 2;
                        }
                    }
                    else
                    {
                        if (item.ExecStatus != 2)
                        {
                            ExecStatus = 2;
                        }
                    }
                    if (ExecStatus != 0)
                    {
                        sql = string.Format("update t_account_shares_conditiontrade_buy_details set ExecStatus={0} where Id={1}", ExecStatus, item.Id);
                        db.Database.ExecuteSqlCommand(sql);
                        if (ExecStatus == 1)
                        {
                            return;
                        }
                    }

                    //判断价格条件
                    if (item.ConditionType == 1)//绝对价格
                    {
                        if ((presentPrice < item.ConditionPrice && item.IsGreater == true) || (presentPrice > item.ConditionPrice && item.IsGreater == false))
                        {
                            return;
                        }
                    }
                    else if (item.ConditionType == 2)//相对价格
                    {
                        if (item.IsGreater)
                        {
                            if ((item.ConditionRelativeType == 1 && presentPrice < item.LimitUpPrice + item.ConditionRelativeRate * 100) || (item.ConditionRelativeType == 2 && presentPrice < item.LimitDownPrice + item.ConditionRelativeRate * 100) || (item.ConditionRelativeType == 3 && presentPrice < ((long)Math.Round((closedPrice + closedPrice * (item.ConditionRelativeRate * 1.0 / 10000)) / 100)) * 100))
                            {
                                return;
                            }
                        }
                        else
                        {
                            if ((item.ConditionRelativeType == 1 && presentPrice > item.LimitUpPrice + item.ConditionRelativeRate * 100) || (item.ConditionRelativeType == 2 && presentPrice > item.LimitDownPrice + item.ConditionRelativeRate * 100) || (item.ConditionRelativeType == 3 && presentPrice > ((long)Math.Round((closedPrice + closedPrice * (item.ConditionRelativeRate * 1.0 / 10000)) / 100)) * 100))
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                    //判断是否封板
                    if (item.OtherConditionRelative == 1 && item.BuyPrice1 != item.LimitUpPrice)
                    {
                        return;
                    }
                    else if (item.OtherConditionRelative == 2 && item.SellPrice1 != item.LimitDownPrice)
                    {
                        return;
                    }
                }

                //判断板块限制
                sql = string.Format("select top 1 ParValueJson from t_account_shares_buy_setting with(nolock) where AccountId={0} and [Type]=4", item.AccountId);
                string ParValueJson = db.Database.SqlQuery<string>(sql).FirstOrDefault();
                var tempSetting = JsonConvert.DeserializeObject<dynamic>(ParValueJson);
                long ParValue = tempSetting.Value;
                if (ParValue == 1)
                {
                    logRecord.AppendLine("===开始判断所属板块是否可买入" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                    sql = string.Format(@"select top 1 1  
from t_shares_plate_rel t with(nolock)
inner join t_account_shares_conditiontrade_buy_plate t1 with(nolock) on t.PlateId=t1.PlateId
inner join t_shares_plate t2 with(nolock) on t.PlateId=t2.Id
inner join t_shares_plate_type_business t3 with(nolock) on t2.[Type]=t3.Id
where t.Market={0} and t.SharesCode='{1}' and AccountId={2} and t2.[Status]=1 and (t2.ChooseStatus=1 or (t3.IsBasePlate=1 and t2.BaseStatus=1))", item.Market, item.SharesCode, item.AccountId);
                    int result = db.Database.SqlQuery<int>(sql).FirstOrDefault();
                    if (result != 1)
                    {
                        return;
                    }
                }

                //判断是否存在持仓
                if (item.IsHold)
                {
                    //查询当前是否存在该股票持仓
                    sql = @"select top 1 1
		                               from
		                               (
			                                select SharesCode,Market
			                                from t_account_shares_entrust with(nolock)
			                                where AccountId={0} and [Status]<>3  and Market={1} and SharesCode='{2}'
			                                union all
			                                select SharesCode,Market 
			                                from t_account_shares_hold with(nolock)
			                                where AccountId={0} and RemainCount>0 and Market={1} and SharesCode='{2}'
		                               )t
		                               group by SharesCode,Market";
                    int exc_result = db.Database.SqlQuery<int>(string.Format(sql, item.AccountId, item.Market, item.SharesCode)).FirstOrDefault();
                    if (exc_result == 1)
                    {
                        return;
                    }
                }

                bool isTri = true;
                bool isAuto = true;
                if (!isSyncro)
                {
                    //判断额外条件
                    var other = (from x in db.t_account_shares_conditiontrade_buy_details_other
                                 where x.DetailsId == item.Id && x.Status == 1
                                 select x).ToList();
                    logRecord.AppendLine("===开始判断额外/转自动条件" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                    foreach (var th in other)
                    {
                        var trend = (from x in db.t_account_shares_conditiontrade_buy_details_other_trend
                                     where x.OtherId == th.Id && x.Status == 1
                                     select x).ToList();
                        bool tempTri = false;
                        foreach (var tr in trend)
                        {
                            var par = (from x in db.t_account_shares_conditiontrade_buy_details_other_trend_par
                                       where x.OtherTrendId == tr.Id
                                       select x.ParamsInfo).ToList();
                            if (par.Count() <= 0)
                            {
                                tempTri = false;
                                break;
                            }
                            //时间段
                            if (tr.TrendId == 4)//指定时间段 
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "指定时间段判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                                JArray timeList = temp.Times;
                                string desMessage = "当前时间：" + timeNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + ",条件时段：" + timeList[0].ToString() + "-" + timeList[1].ToString();
                                if (timeNow >= DateTime.Parse(timeList[0].ToString()) && timeNow < DateTime.Parse(timeList[1].ToString()))
                                {
                                    logRecord.AppendLine("\t\t条件成立("+ desMessage + ")");
                                    tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                else
                                {
                                    logRecord.AppendLine("\t\t条件不成立(" + desMessage + ")");
                                }
                                logRecord.AppendLine("\t结果：未满足");
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //当前价格
                            if (tr.TrendId == 12)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "按当前价格判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend12 = DataHelper.Analysis_CurrentPrice(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend12 == 0)
                                {
                                    tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend12);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //历史涨跌幅
                            if (tr.TrendId == 5)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "历史涨跌幅判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend5 = DataHelper.Analysis_HisRiseRate(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend5 == 0)
                                {
                                    tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend5);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //当前涨跌幅
                            if (tr.TrendId == 6)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "当前涨跌幅判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend6 = DataHelper.Analysis_TodayRiseRate(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend6 == 0)
                                {
                                    tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足");
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //板块涨跌幅
                            if (tr.TrendId == 7)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "板块涨跌幅判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend7 = DataHelper.Analysis_PlateRiseRate(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend7 == 0)
                                {
                                    tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend7);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //判断买卖单占比
                            if (tr.TrendId == 8)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "买卖单占比判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend8 = DataHelper.Analysis_BuyOrSellCount(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend8 == 0)
                                {
                                    tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend8);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //判断按参照价格
                            if (tr.TrendId == 9)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "按参照价格判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend9 = DataHelper.Analysis_ReferPrice(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend9 == 0)
                                {
                                    tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend9);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //判断按均线价格
                            if (tr.TrendId == 10)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "按均线价格判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend10 = DataHelper.Analysis_ReferAverage(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend10 == 0)
                                {
                                    tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend10);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //买卖变化速度
                            if (tr.TrendId == 11)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "买卖变化速度判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend11 = DataHelper.Analysis_QuotesChangeRate(item.SharesCode, item.Market, item.PresentPrice, par, logRecord);
                                if (errorCode_Trend11 == 0)
                                {
                                    tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend11);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //快速拉升
                            if (tr.TrendId == 1)//快速拉升 
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "快速拉升判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                List<TREND_RESULT_RAPID_UP> resultInfo_Trend1 = new List<TREND_RESULT_RAPID_UP>();
                                int errorCode_Trend1 = DataHelper.Analysis_Trend1(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.SharesCode,
                                        Market=item.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend1);
                                DateTime? resultPushTime = null;
                                if (errorCode_Trend1 == 0)
                                {
                                    var temp = resultInfo_Trend1.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode) && temp.dicUpOrDownInfo.Count() > 0)
                                    {
                                        var tempModel = temp.dicUpOrDownInfo.FirstOrDefault();
                                        var pushTime = tempModel.Value.lastestInfo.dtTradeTime;
                                        resultPushTime = pushTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                            if (tempTri)
                                            {
                                                logRecord.AppendLine("\t结果：达到要求");
                                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                                break;
                                            }
                                        }
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足,返回code：" + errorCode_Trend1);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //多头向上
                            if (tr.TrendId == 2)//多头向上 
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "多头向上判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                List<TREND_RESULT_LINE_UP> resultInfo_Trend2 = new List<TREND_RESULT_LINE_UP>();
                                int errorCode_Trend2 = DataHelper.Analysis_Trend2(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.SharesCode,
                                        Market=item.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend2);
                                DateTime? resultPushTime = null;
                                if (errorCode_Trend2 == 0)
                                {
                                    var temp = resultInfo_Trend2.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode))
                                    {
                                        var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                                        resultPushTime = pushTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                            if (tempTri)
                                            {
                                                logRecord.AppendLine("\t结果：达到要求");
                                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                                break;
                                            }
                                        }
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend2);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //箱体上涨
                            if (tr.TrendId == 3)//箱体突破 
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "箱体突破判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                List<TREND_RESULT_BOX_BREACH> resultInfo_Trend3 = new List<TREND_RESULT_BOX_BREACH>();
                                int errorCode_Trend3 = DataHelper.Analysis_Trend3(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.SharesCode,
                                        Market=item.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend3);
                                DateTime? resultPushTime = null;
                                if (errorCode_Trend3 == 0)
                                {
                                    var temp = resultInfo_Trend3.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode))
                                    {
                                        var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                                        resultPushTime = pushTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                            if (tempTri)
                                            {
                                                logRecord.AppendLine("\t结果：达到要求");
                                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                                break;
                                            }
                                        }
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code" + errorCode_Trend3);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //五档变化速度
                            if (tr.TrendId == 13)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "五档变化速度判断-额外参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend13 = DataHelper.Analysis_QuotesTypeChangeRate(item.SharesCode, item.Market, item.PresentPrice, par, logRecord);
                                if (errorCode_Trend13 == 0)
                                {
                                    tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend13);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                        }
                        if (!tempTri)
                        {
                            isTri = false;
                            break;
                        }
                    }

                    if (!isTri)
                    {
                        logRecord.AppendLine("额外条件未满足");
                        Logger.WriteFileLog(logRecord.ToString(), "TrendAnaly/" + item.SharesCode, null);
                        return;
                    }


                    //判断转自动条件
                    var auto = (from x in db.t_account_shares_conditiontrade_buy_details_auto
                                where x.DetailsId == item.Id && x.Status == 1
                                select x).ToList();
                    foreach (var th in auto)
                    {
                        var trend = (from x in db.t_account_shares_conditiontrade_buy_details_auto_trend
                                     where x.AutoId == th.Id && x.Status == 1
                                     select x).ToList();
                        bool tempTri = false;
                        foreach (var tr in trend)
                        {
                            var par = (from x in db.t_account_shares_conditiontrade_buy_details_auto_trend_par
                                       where x.AutoTrendId == tr.Id
                                       select x.ParamsInfo).ToList();
                            if (par.Count() <= 0)
                            {
                                tempTri = false;
                                break;
                            }
                            //时间段
                            if (tr.TrendId == 4)//指定时间段 
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "指定时间段判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                                JArray timeList = temp.Times; 
                                string desMessage = "当前时间：" + timeNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + ",条件时段：" + timeList[0].ToString() + "-" + timeList[1].ToString();
                                if (timeNow >= DateTime.Parse(timeList[0].ToString()) && timeNow < DateTime.Parse(timeList[1].ToString()))
                                {
                                    logRecord.AppendLine("\t\t条件成立(" + desMessage + ")");
                                    tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                else
                                {
                                    logRecord.AppendLine("\t\t条件不成立(" + desMessage + ")");
                                }
                                logRecord.AppendLine("\t结果：未满足");
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //当前价格
                            if (tr.TrendId == 12)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "按当前价格判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend12 = DataHelper.Analysis_CurrentPrice(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend12 == 0)
                                {
                                    tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend12);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //历史涨跌幅
                            if (tr.TrendId == 5)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "历史涨跌幅判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend5 = DataHelper.Analysis_HisRiseRate(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend5 == 0)
                                {
                                    tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend5);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //当前涨跌幅
                            if (tr.TrendId == 6)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "当前涨跌幅判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend6 = DataHelper.Analysis_TodayRiseRate(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend6 == 0)
                                {
                                    tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足");
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //板块涨跌幅
                            if (tr.TrendId == 7)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "板块涨跌幅判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend7 = DataHelper.Analysis_PlateRiseRate(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend7 == 0)
                                {
                                    tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend7);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //判断买卖单占比
                            if (tr.TrendId == 8)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "买卖单占比判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend8 = DataHelper.Analysis_BuyOrSellCount(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend8 == 0)
                                {
                                    tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend8);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //判断按参照价格
                            if (tr.TrendId == 9)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "按参照价格判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend9 = DataHelper.Analysis_ReferPrice(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend9 == 0)
                                {
                                    tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend9);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //判断按均线价格
                            if (tr.TrendId == 10)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "按均线价格判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend10 = DataHelper.Analysis_ReferAverage(item.SharesCode, item.Market, par, logRecord);
                                if (errorCode_Trend10 == 0)
                                {
                                    tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend10);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //五档变化速度
                            if (tr.TrendId == 11)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "五档变化速度判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend11 = DataHelper.Analysis_QuotesChangeRate(item.SharesCode, item.Market, item.PresentPrice, par, logRecord);
                                if (errorCode_Trend11 == 0)
                                {
                                    tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend11);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //快速拉升
                            if (tr.TrendId == 1)//快速拉升 
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "快速拉升判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                List<TREND_RESULT_RAPID_UP> resultInfo_Trend1 = new List<TREND_RESULT_RAPID_UP>();
                                int errorCode_Trend1 = DataHelper.Analysis_Trend1(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.SharesCode,
                                        Market=item.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend1);
                                DateTime? resultPushTime = null;
                                if (errorCode_Trend1 == 0)
                                {
                                    var temp = resultInfo_Trend1.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode) && temp.dicUpOrDownInfo.Count() > 0)
                                    {
                                        var tempModel = temp.dicUpOrDownInfo.FirstOrDefault();
                                        var pushTime = tempModel.Value.lastestInfo.dtTradeTime;
                                        resultPushTime = pushTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                            if (tempTri)
                                            {
                                                logRecord.AppendLine("\t结果：达到要求");
                                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                                break;
                                            }
                                        }
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足,返回code：" + errorCode_Trend1);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //多头向上
                            if (tr.TrendId == 2)//多头向上 
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "多头向上判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                List<TREND_RESULT_LINE_UP> resultInfo_Trend2 = new List<TREND_RESULT_LINE_UP>();
                                int errorCode_Trend2 = DataHelper.Analysis_Trend2(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.SharesCode,
                                        Market=item.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend2);
                                DateTime? resultPushTime = null;
                                if (errorCode_Trend2 == 0)
                                {
                                    var temp = resultInfo_Trend2.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode))
                                    {
                                        var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                                        resultPushTime = pushTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                            if (tempTri)
                                            {
                                                logRecord.AppendLine("\t结果：达到要求");
                                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                                break;
                                            }
                                        }
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend2);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //箱体上涨
                            if (tr.TrendId == 3)//箱体突破 
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "箱体突破判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                List<TREND_RESULT_BOX_BREACH> resultInfo_Trend3 = new List<TREND_RESULT_BOX_BREACH>();
                                int errorCode_Trend3 = DataHelper.Analysis_Trend3(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.SharesCode,
                                        Market=item.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend3);
                                DateTime? resultPushTime = null;
                                if (errorCode_Trend3 == 0)
                                {
                                    var temp = resultInfo_Trend3.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode))
                                    {
                                        var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                                        resultPushTime = pushTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = IsGetAuto(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                            if (tempTri)
                                            {
                                                logRecord.AppendLine("\t结果：达到要求");
                                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                                break;
                                            }
                                        }
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code" + errorCode_Trend3);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                            //五档变化速度
                            if (tr.TrendId == 13)
                            {
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                logRecord.AppendLine("股票：" + item.SharesCode + "五档变化速度判断-转自动参数：");
                                logRecord.AppendLine("\t分组名称：" + th.Name);
                                logRecord.AppendLine("\t走势名称：" + tr.TrendDescription);
                                int errorCode_Trend13 = DataHelper.Analysis_QuotesTypeChangeRate(item.SharesCode, item.Market, item.PresentPrice, par, logRecord);
                                if (errorCode_Trend13 == 0)
                                {
                                    tempTri = IsGetOther(db, tr.Id, item.SharesCode, item.Market, item.PresentPrice, logRecord);
                                    if (tempTri)
                                    {
                                        logRecord.AppendLine("\t结果：达到要求");
                                        logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                                        break;
                                    }
                                }
                                logRecord.AppendLine("\t结果：未满足，返回code：" + errorCode_Trend13);
                                logRecord.AppendLine("===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
                            }
                        }
                        if (!tempTri)
                        {
                            isTri = false;
                            break;
                        }
                    }

                    logRecord.AppendLine("满足所有条件，可以触发");
                    Logger.WriteFileLog(logRecord.ToString(), "TrendAnaly/" + item.SharesCode, null);

                    isAuto = item.BuyAuto;
                    if (!isAuto)
                    {
                        if (auto.Count() > 0 && isTri)//转自动交易成立
                        {
                            isAuto = true;
                        }
                        else
                        {
                            sql = string.Format("update t_account_shares_conditiontrade_buy_details set BusinessStatus=1,TriggerTime='{0}' where Id={1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), item.Id);
                            db.Database.ExecuteSqlCommand(sql);
                        }
                    }

                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //查询需要更改的子条件
                            var childList = (from x in db.t_account_shares_conditiontrade_buy_details_child
                                             where x.ConditionId == item.Id
                                             select x).ToList();
                            foreach (var child in childList)
                            {
                                var childcondition = (from x in db.t_account_shares_conditiontrade_buy_details
                                                      where x.Id == child.ChildId
                                                      select x).FirstOrDefault();
                                if (childcondition == null)
                                {
                                    continue;
                                }
                                if (childcondition.TriggerTime != null)
                                {
                                    continue;
                                }
                                childcondition.Status = child.Status;
                            }
                            db.SaveChanges();
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            return;
                        }
                    }
                }

                if (isAuto && isTri)//自动买入且条件满足
                {
                    try
                    {
                        long EntrustPrice = 0;
                        if (item.EntrustType == 2)
                        {
                            switch (item.EntrustPriceGear)
                            {
                                case 1:
                                    EntrustPrice = item.BuyPrice1 == 0 ? item.PresentPrice : item.BuyPrice1;
                                    break;
                                case 2:
                                    EntrustPrice = item.BuyPrice2 == 0 ? item.PresentPrice : item.BuyPrice2;
                                    break;
                                case 3:
                                    EntrustPrice = item.BuyPrice3 == 0 ? item.PresentPrice : item.BuyPrice3;
                                    break;
                                case 4:
                                    EntrustPrice = item.BuyPrice4 == 0 ? item.PresentPrice : item.BuyPrice4;
                                    break;
                                case 5:
                                    EntrustPrice = item.BuyPrice5 == 0 ? item.PresentPrice : item.BuyPrice5;
                                    break;
                                case 6:
                                    EntrustPrice = item.SellPrice1 == 0 ? item.PresentPrice : item.SellPrice1;
                                    break;
                                case 7:
                                    EntrustPrice = item.SellPrice2 == 0 ? item.PresentPrice : item.SellPrice2;
                                    break;
                                case 8:
                                    EntrustPrice = item.SellPrice3 == 0 ? item.PresentPrice : item.SellPrice3;
                                    break;
                                case 9:
                                    EntrustPrice = item.SellPrice4 == 0 ? item.PresentPrice : item.SellPrice4;
                                    break;
                                case 10:
                                    EntrustPrice = item.SellPrice5 == 0 ? item.PresentPrice : item.SellPrice5;
                                    break;
                                case 11:
                                    EntrustPrice = item.LimitUpPrice;
                                    break;
                                case 12:
                                    EntrustPrice = item.LimitDownPrice;
                                    break;
                            }
                        }

                        sql = string.Format("update t_account_shares_conditiontrade_buy_details set BusinessStatus=3,TriggerTime='{0}' where Id={1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), item.Id);
                        db.Database.ExecuteSqlCommand(sql);

                        //查询跟投设置
                        List<FollowAccountInfo> FollowList = new List<FollowAccountInfo>();
                        if (item.FollowType == 1)
                        {
                            //查询当前轮询
                            using (var tran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    sql = "declare @parValueJson nvarchar(max),@id bigint;select @id=Id,@parValueJson=ParValueJson from t_account_shares_buy_setting with(xlock) where AccountId=" + item.AccountId + " and [Type]=3 if(@id is null) begin insert into t_account_shares_buy_setting(AccountId,[Type],Name,[Description],ParValueJson,CreateTime,LastModified) values(" + item.AccountId+",3,'跟投分组轮序位置','跟投分组轮序位置','{\"Value\":1}',getdate(),getdate());set @parValueJson = '{\"Value\":0}';end else begin declare @parValue bigint; set @parValue = dbo.f_parseJson(@parValueJson, 'Value'); set @parValue = @parValue + 1; update t_account_shares_buy_setting set ParValueJson = '{\"Value\":' + convert(varchar(10), @parValue) + '}' where Id = @id; end select @parValueJson";
                                    string turnJson = db.Database.SqlQuery<string>(sql).FirstOrDefault();
                                    var tempTurn=JsonConvert.DeserializeObject<dynamic>(turnJson);
                                    long turn = tempTurn.Value;

                                    //查询跟投分组信息
                                    sql = string.Format(@"select Id
  from t_account_follow_group with(nolock)
  where AccountId = {0} and[Status] = 1
  order by OrderIndex", item.AccountId);
                                    var groupList = db.Database.SqlQuery<long>(sql).ToList();
                                    int groupCount = groupList.Count();
                                    if (groupCount <= 0)
                                    {
                                        FollowList = new List<FollowAccountInfo>();
                                    }
                                    else
                                    {
                                        int thisIndex = (int)turn % groupCount;
                                        long thisId = groupList[thisIndex];

                                        sql = string.Format(@"select t2.FollowAccountId AccountId,t1.BuyAmount
  from t_account_follow_group t with(nolock)
  inner
  join t_account_follow_group_rel t1 with(nolock) on t.Id = t1.GroupId

inner
  join t_account_follow_rel t2 with(nolock) on t1.RelId = t2.Id
  where t.Id = {0} and t.AccountId = {1} and t.[Status]= 1 and t2.AccountId = {1}", thisId, item.AccountId);
                                        FollowList = db.Database.SqlQuery<FollowAccountInfo>(sql).ToList();
                                    }

                                    tran.Commit();
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteFileLog("更新跟投分组出错",ex);
                                    tran.Rollback();
                                }
                            }
                        }
                        else
                        {
                            FollowList = (from x in db.t_account_shares_conditiontrade_buy_details_follow
                                          where x.DetailsId == item.Id
                                          select new FollowAccountInfo
                                          {
                                              AccountId = x.FollowAccountId,
                                              BuyAmount = 0
                                          }).ToList();
                        }


                        //查询跟投人员
                        var followList = (from x in db.t_account_follow_rel
                                          join x2 in db.t_account_baseinfo on x.FollowAccountId equals x2.Id
                                          join x3 in db.t_account_wallet on x.FollowAccountId equals x3.AccountId
                                          where x.AccountId == item.AccountId && x2.Status == 1
                                          select x3).ToList();

                        long Deposit = 0;
                        long RemainDeposit = 0;
                        //计算本人购买仓位比
                        var wallet = (from x in db.t_account_wallet
                                      where x.AccountId == item.AccountId
                                      select x).FirstOrDefault();
                        if (wallet == null)
                        {
                            throw new WebApiException(400, "账户有误");
                        }
                        Deposit = wallet.Deposit;

                        var accountBuySetting = DbHelper.GetAccountBuySetting(item.AccountId, 1);
                        var buySetting = accountBuySetting.FirstOrDefault();
                        if (buySetting != null)
                        {
                            var valueObj = JsonConvert.DeserializeObject<dynamic>(buySetting.ParValueJson);
                            long parValue = valueObj.Value;
                            RemainDeposit = parValue / 100 * 100;
                        }

                        long EntrustAmount = item.EntrustAmount;
                        if (EntrustAmount > Deposit - RemainDeposit)
                        {
                            EntrustAmount = Deposit - RemainDeposit;
                        }

                        List<dynamic> buyList = new List<dynamic>();
                        buyList.Add(new
                        {
                            AccountId = item.AccountId,
                            BuyAmount = EntrustAmount,
                            IsFollow=false
                        });

                        var buyRate = EntrustAmount * 1.0 / (Deposit - RemainDeposit);//仓位占比
                        foreach (var account in FollowList)
                        {
                            var temp = followList.Where(e => e.AccountId == account.AccountId).FirstOrDefault();
                            if (temp == null)
                            {
                                continue;
                            }
                            long followRemainDeposit = 0;
                            var tempBuySetting = DbHelper.GetAccountBuySetting(account.AccountId, 1);
                            var followBuySetting = tempBuySetting.FirstOrDefault();
                            if (followBuySetting != null)
                            {
                                var valueObj = JsonConvert.DeserializeObject<dynamic>(followBuySetting.ParValueJson);
                                long parValue = valueObj.Value;
                                followRemainDeposit = parValue / 100 * 100;
                            }
                            var tempRate = account.BuyAmount == 0 ? buyRate : account.BuyAmount == -1 ? 1 : -1;
                            buyList.Add(new
                            {
                                AccountId = account.AccountId,
                                BuyAmount = tempRate == -1 ? account.BuyAmount : (long)((temp.Deposit - followRemainDeposit) * tempRate),
                                IsFollow = true
                            });
                        }

                        SqlParameter[] parameter = new SqlParameter[8];
                        //市场
                        var marketPar = new SqlParameter("@market", SqlDbType.Int);
                        marketPar.Value = item.Market;
                        parameter[0] = marketPar;
                        //股票代码
                        var sharesCodePar = new SqlParameter("@sharesCode", SqlDbType.VarChar,20);
                        sharesCodePar.Value = item.SharesCode;
                        parameter[1] = sharesCodePar;
                        //委托价格
                        var entrustPricePar = new SqlParameter("@entrustPrice", SqlDbType.BigInt);
                        entrustPricePar.Value = EntrustPrice;
                        parameter[2] = entrustPricePar;
                        //主账户Id
                        var mainAccountIdPar = new SqlParameter("@mainAccountId", SqlDbType.BigInt);
                        mainAccountIdPar.Value = item.CreateAccountId == 0 ? item.AccountId : item.CreateAccountId;
                        parameter[3] = mainAccountIdPar;
                        //自动买入条件Id
                        var autoBuyDetailsIdPar = new SqlParameter("@autoBuyDetailsId", SqlDbType.BigInt);
                        autoBuyDetailsIdPar.Value = item.Id;
                        parameter[4] = autoBuyDetailsIdPar;
                        //买入列表
                        var applyTradeBuyInfoPar = new SqlParameter("@applyTradeBuyInfo", SqlDbType.Structured);
                        applyTradeBuyInfoPar.TypeName = "dbo.ApplyTradeBuyInfo";
                        using (DataTable table = new DataTable())
                        {
                            #region====定义表字段数据类型====
                            table.Columns.Add("AccountId", typeof(long));
                            table.Columns.Add("BuyAmount", typeof(long));
                            table.Columns.Add("ClosingTime", typeof(DateTime));
                            table.Columns.Add("IsFollow", typeof(bool));
                            #endregion

                            #region====绑定数据====
                            foreach (var data in buyList)
                            {
                                long buyAccountId = data.AccountId;
                                long buyAmount = data.BuyAmount;
                                bool isFollow = data.IsFollow;
                                DataRow row = table.NewRow();
                                row["AccountId"] = buyAccountId;
                                row["BuyAmount"] = buyAmount;
                                row["ClosingTime"] = DBNull.Value;
                                row["IsFollow"] = isFollow;
                                table.Rows.Add(row);
                            }
                            #endregion

                            applyTradeBuyInfoPar.Value = table;
                        }
                        parameter[5] = applyTradeBuyInfoPar;
                        //errorcode
                        var errorCodePar = new SqlParameter("@errorCode", SqlDbType.Int);
                        errorCodePar.Direction = ParameterDirection.Output;
                        parameter[6] = errorCodePar;
                        //errormessage
                        var errorMessagePar = new SqlParameter("@errorMessage", SqlDbType.NVarChar,200);
                        errorMessagePar.Direction = ParameterDirection.Output;
                        parameter[7] = errorMessagePar;

                        var resultList=db.Database.SqlQuery<ApplyTradeBuyInfo>("exec P_ApplyTradeBuy_Batch @market,@sharesCode,@entrustPrice,@mainAccountId,@autoBuyDetailsId,@applyTradeBuyInfo,@errorCode out,@errorMessage out", parameter).ToList();

                        if ((int)parameter[6].Value != 0)
                        {
                            throw new WebApiException(400, parameter[7].Value.ToString());
                        }

                        List<dynamic> sendDataList = new List<dynamic>();
                        foreach (var x in resultList)
                        {
                            bool IsSuccess = x.IsSuccess;
                            if (IsSuccess)
                            {
                                sendDataList.Add(new
                                {
                                    BuyId = x.BuyId,
                                    BuyCount = x.BuyCount,
                                    BuyTime = x.BuyTime.ToString("yyyy-MM-dd 00:00:00"),
                                });
                            }
                            else 
                            {
                                string errorMessage = x.ErrorMessage;
                                Logger.WriteFileLog(errorMessage, null);
                            }
                        }
                        if (sendDataList.Count() > 0)
                        {
                            bool isSendSuccess = Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new { type = 1, data = sendDataList })), "SharesBuy", "s1");
                            if (!isSendSuccess)
                            {
                                throw new WebApiException(400, "买入失败,请撤销重试");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("自动买入这里出错", ex);
                    }

                    if (!isSyncro)
                    {
                        TradeAutoBuyCondition conditionInfo = new TradeAutoBuyCondition();
                        conditionInfo = JsonConvert.DeserializeObject<TradeAutoBuyCondition>(JsonConvert.SerializeObject(item));
                        var tempGroupList = (from x in db.t_account_shares_conditiontrade_buy_group
                                             join x2 in db.t_account_shares_conditiontrade_buy_group_rel on x.Id equals x2.GroupId
                                             where x.AccountId == item.AccountId && x2.Market == item.Market && x2.SharesCode == item.SharesCode
                                             select x).ToList();
                        List<long> authorized_group_id_list = new List<long>();
                        foreach (var groupInfo in tempGroupList)
                        {
                            var AuthorizedGroup = (from x in db.t_account_shares_conditiontrade_buy_group_syncro_account
                                                   join x2 in db.t_account_shares_buy_synchro_setting_authorized_group on x.AuthorizedGroupId equals x2.Id
                                                   where x.GroupId == groupInfo.Id && x2.AccountId == groupInfo.AccountId && x2.Status == 1 && x.Status == 1
                                                   orderby x.OrderIndex
                                                   select x2).ToList();
                            if (AuthorizedGroup.Count() <= 0)
                            {
                                continue;
                            }
                            int nexIndex = groupInfo.CurrSyncroIndex % AuthorizedGroup.Count();
                            authorized_group_id_list.Add(AuthorizedGroup[nexIndex].Id);

                            groupInfo.CurrSyncroIndex = groupInfo.CurrSyncroIndex + 1;
                            db.SaveChanges();
                        }

                        //导入条件数据
                        //1.查询股票所属自定义分组
                        //2.查询分组配置同步用户及参数
                        //3.循环导入参数并修改参数数据加入内存
                        var syncroList = (from x in db.t_account_shares_buy_synchro_setting_authorized_group_rel
                                          join x2 in db.t_account_shares_buy_synchro_setting on x.SettingId equals x2.Id
                                          where authorized_group_id_list.Contains(x.GroupId) && x2.Status == 1 && x2.MainAccountId == item.AccountId
                                          select new
                                          {
                                              disAccountId = x2.AccountId,
                                              followType = x2.FollowType,
                                              buyAmount = x2.BuyAmount,
                                              followList = (from y in db.t_account_shares_buy_synchro_setting_follow
                                                            where y.SettingId == x2.Id
                                                            select y.FollowAccountId).ToList(),
                                              groupList = (from y in db.t_account_shares_buy_synchro_setting_group
                                                           where y.SettingId == x2.Id
                                                           select y.GroupId).ToList()
                                          }).ToList();
                        foreach (var data in syncroList)
                        {
                            try
                            {
                                var condition_buy = (from x in db.t_account_shares_buy_setting_shares
                                                     where x.ConditiontradeBuyId == item.ConditionId && x.AccountId == data.disAccountId
                                                     select x).FirstOrDefault();
                                if (condition_buy == null)
                                {
                                    //判断默认是否买入
                                    var accountBuySetting = DbHelper.GetAccountBuySetting(data.disAccountId, 5);
                                    var buySetting = accountBuySetting.FirstOrDefault();
                                    if (buySetting != null)
                                    {
                                        var valueObj = JsonConvert.DeserializeObject<dynamic>(buySetting.ParValueJson);
                                        int parValue = valueObj.Value;
                                        if (parValue == 0)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (CommonEnum.DefaultBuyStatus == 0)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                else if (condition_buy.Status != 1)
                                {
                                    continue;
                                }
                                CopyAccountBuyCondition(item.Id, data.disAccountId, data.groupList, data.followList, data.followType, data.buyAmount, conditionInfo);
                                var tempInfo = JsonConvert.DeserializeObject<TradeAutoBuyCondition>(JsonConvert.SerializeObject(conditionInfo));
                                Singleton.Instance.AddSyncro(tempInfo);
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 同步股票参数
        /// </summary>
        private static void CopyAccountBuyCondition(long detailsId, long disAccountId, List<long> groupList, List<long> followList, int followType, long buyAmount, TradeAutoBuyCondition conditionInfo)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    string sql = "";
                    //查询条件
                    var condition = (from item in db.t_account_shares_conditiontrade_buy_details
                                     join item2 in db.t_account_shares_conditiontrade_buy on item.ConditionId equals item2.Id
                                     where item.Id == detailsId
                                     select new { item, item2 }).FirstOrDefault();
                    if (condition == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    if (condition.item.TriggerTime == null)
                    {
                        throw new WebApiException(400, "未触发无法复制");
                    }
                    //查询股票
                    var conditiontradeBuy = (from item in db.t_account_shares_conditiontrade_buy
                                             where item.AccountId == disAccountId && item.Market == condition.item2.Market && item.SharesCode == condition.item2.SharesCode
                                             select item).FirstOrDefault();
                    long conditiontradeId;
                    if (conditiontradeBuy == null)
                    {
                        t_account_shares_conditiontrade_buy conditiontrade_buy = new t_account_shares_conditiontrade_buy
                        {
                            SharesCode = condition.item2.SharesCode,
                            Status = 1,
                            AccountId = disAccountId,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            Market = condition.item2.Market
                        };
                        db.t_account_shares_conditiontrade_buy.Add(conditiontrade_buy);
                        db.SaveChanges();
                        conditiontradeId = conditiontrade_buy.Id;
                    }
                    else if (conditiontradeBuy.Status != 1)
                    {
                        throw new WebApiException(400, "原有股票状态无效");
                    }
                    else
                    {
                        conditiontradeId = conditiontradeBuy.Id;
                    }
                    //加入自定义分组
                    var thisGroupList = (from item in db.t_account_shares_conditiontrade_buy_group
                                         where item.AccountId == disAccountId && groupList.Contains(item.Id)
                                         select item.Id).ToList();
                    var tempGroupRel = (from item in db.t_account_shares_conditiontrade_buy_group_rel
                                        where thisGroupList.Contains(item.GroupId) && item.Market == condition.item2.Market && item.SharesCode == condition.item2.SharesCode
                                        select item.GroupId).ToList();
                    thisGroupList.RemoveAll(e => tempGroupRel.Contains(e));
                    foreach (var groupId in thisGroupList)
                    {
                        db.t_account_shares_conditiontrade_buy_group_rel.Add(new t_account_shares_conditiontrade_buy_group_rel
                        {
                            SharesCode = condition.item2.SharesCode,
                            Market = condition.item2.Market,
                            GroupId = groupId
                        });
                    }
                    db.SaveChanges();

                    t_account_shares_conditiontrade_buy_details details = new t_account_shares_conditiontrade_buy_details
                    {
                        SourceFrom = condition.item.SourceFrom,
                        Status = 1,
                        BusinessStatus = 2,
                        BuyAuto = condition.item.BuyAuto,
                        ConditionId = conditiontradeId,
                        ConditionPrice = condition.item.ConditionPrice,
                        ConditionRelativeRate = condition.item.ConditionRelativeRate,
                        ConditionRelativeType = condition.item.ConditionRelativeType,
                        ConditionType = condition.item.ConditionType,
                        CreateAccountId = disAccountId,
                        FollowType = followType,
                        CreateTime = DateTime.Now,
                        EntrustAmount = buyAmount,
                        EntrustId = 0,
                        EntrustPriceGear = condition.item.EntrustPriceGear,
                        OtherConditionRelative = condition.item.OtherConditionRelative,
                        EntrustType = condition.item.EntrustType,
                        ForbidType = condition.item.ForbidType,
                        IsGreater = condition.item.IsGreater,
                        LastModified = DateTime.Now,
                        Name = condition.item.Name,
                        FirstExecTime = null,
                        LimitUp = condition.item.LimitUp,
                        IsHold = condition.item.IsHold,
                        TriggerTime = DateTime.Now,
                        FatherId = detailsId
                    };
                    db.t_account_shares_conditiontrade_buy_details.Add(details);
                    db.SaveChanges();

                    //跟投复制
                    if (followType == 0)
                    {
                        followList = (from item in db.t_account_follow_rel
                                      where followList.Contains(item.FollowAccountId) && item.AccountId == disAccountId
                                      select item.FollowAccountId).ToList();
                        foreach (var followId in followList)
                        {
                            db.t_account_shares_conditiontrade_buy_details_follow.Add(new t_account_shares_conditiontrade_buy_details_follow
                            {
                                CreateTime = DateTime.Now,
                                DetailsId = details.Id,
                                FollowAccountId = followId
                            });
                        }
                        db.SaveChanges();
                    }

                    //目前不复制，使用应用参数
                    #region======参数复制=====
                    ////额外参数复制
                    //var otherList = (from item in db.t_account_shares_conditiontrade_buy_details_other
                    //                 where item.DetailsId == detailsId
                    //                 select item).ToList();
                    //foreach (var x in otherList)
                    //{
                    //    t_account_shares_conditiontrade_buy_details_other other = new t_account_shares_conditiontrade_buy_details_other
                    //    {
                    //        Status = x.Status,
                    //        CreateTime = DateTime.Now,
                    //        DetailsId = details.Id,
                    //        LastModified = DateTime.Now,
                    //        Name = x.Name
                    //    };
                    //    db.t_account_shares_conditiontrade_buy_details_other.Add(other);
                    //    db.SaveChanges();
                    //    var trendList = (from item in db.t_account_shares_conditiontrade_buy_details_other_trend
                    //                     where item.OtherId == x.Id
                    //                     select item).ToList();
                    //    var trendIdList = trendList.Select(e => e.Id).ToList();
                    //    var trend_other = (from item in db.t_account_shares_conditiontrade_buy_details_other_trend_other
                    //                       where trendIdList.Contains(item.OtherTrendId)
                    //                       select item).ToList();
                    //    var trend_otherIdList = trend_other.Select(e => e.Id).ToList();
                    //    var trend_other_par = (from item in db.t_account_shares_conditiontrade_buy_details_other_trend_other_par
                    //                           where trend_otherIdList.Contains(item.OtherTrendOtherId)
                    //                           select item).ToList();
                    //    foreach (var y in trendList)
                    //    {
                    //        t_account_shares_conditiontrade_buy_details_other_trend trend = new t_account_shares_conditiontrade_buy_details_other_trend
                    //        {
                    //            Status = y.Status,
                    //            CreateTime = DateTime.Now,
                    //            LastModified = DateTime.Now,
                    //            OtherId = other.Id,
                    //            TrendDescription = y.TrendDescription,
                    //            TrendId = y.TrendId,
                    //            TrendName = y.TrendName
                    //        };
                    //        db.t_account_shares_conditiontrade_buy_details_other_trend.Add(trend);
                    //        db.SaveChanges();

                    //        //参数复制
                    //        sql = string.Format("insert into t_account_shares_conditiontrade_buy_details_other_trend_par(OtherTrendId,ParamsInfo,CreateTime,LastModified) select {0},ParamsInfo,'{1}','{1}' from t_account_shares_conditiontrade_buy_details_other_trend_par where OtherTrendId={2}", trend.Id, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), y.Id);
                    //        db.Database.ExecuteSqlCommand(sql);
                    //        //额外关系复制
                    //        var temp_trend_otherList = trend_other.Where(e => e.OtherTrendId == y.Id).ToList();
                    //        foreach (var t2 in temp_trend_otherList)
                    //        {
                    //            t_account_shares_conditiontrade_buy_details_other_trend_other othertrendTemp = new t_account_shares_conditiontrade_buy_details_other_trend_other
                    //            {
                    //                Status = t2.Status,
                    //                CreateTime = DateTime.Now,
                    //                LastModified = DateTime.Now,
                    //                OtherTrendId = trend.Id,
                    //                TrendDescription = t2.TrendDescription,
                    //                TrendId = t2.TrendId,
                    //                TrendName = t2.TrendName
                    //            };
                    //            db.t_account_shares_conditiontrade_buy_details_other_trend_other.Add(othertrendTemp);
                    //            db.SaveChanges();
                    //            var other_parList = (from t3 in trend_other_par
                    //                                 where t3.OtherTrendOtherId == t2.Id
                    //                                 select new t_account_shares_conditiontrade_buy_details_other_trend_other_par
                    //                                 {
                    //                                     CreateTime = DateTime.Now,
                    //                                     OtherTrendOtherId = othertrendTemp.Id,
                    //                                     LastModified = DateTime.Now,
                    //                                     ParamsInfo = t3.ParamsInfo
                    //                                 }).ToList();
                    //            db.t_account_shares_conditiontrade_buy_details_other_trend_other_par.AddRange(other_parList);
                    //            db.SaveChanges();
                    //        }
                    //    }
                    //}
                    ////转自动参数复制
                    //var autoList = (from item in db.t_account_shares_conditiontrade_buy_details_auto
                    //                where item.DetailsId == detailsId
                    //                select item).ToList();
                    //foreach (var x in autoList)
                    //{
                    //    t_account_shares_conditiontrade_buy_details_auto auto = new t_account_shares_conditiontrade_buy_details_auto
                    //    {
                    //        Status = x.Status,
                    //        CreateTime = DateTime.Now,
                    //        DetailsId = details.Id,
                    //        LastModified = DateTime.Now,
                    //        Name = x.Name
                    //    };
                    //    db.t_account_shares_conditiontrade_buy_details_auto.Add(auto);
                    //    db.SaveChanges();
                    //    var trendList = (from item in db.t_account_shares_conditiontrade_buy_details_auto_trend
                    //                     where item.AutoId == x.Id
                    //                     select item).ToList();
                    //    var trendIdList = trendList.Select(e => e.Id).ToList();
                    //    var trend_auto = (from item in db.t_account_shares_conditiontrade_buy_details_auto_trend_other
                    //                      where trendIdList.Contains(item.AutoTrendId)
                    //                      select item).ToList();
                    //    var trend_autoIdList = trend_auto.Select(e => e.Id).ToList();
                    //    var trend_auto_par = (from item in db.t_account_shares_conditiontrade_buy_details_auto_trend_other_par
                    //                          where trend_autoIdList.Contains(item.AutoTrendOtherId)
                    //                          select item).ToList();
                    //    foreach (var y in trendList)
                    //    {
                    //        t_account_shares_conditiontrade_buy_details_auto_trend trend = new t_account_shares_conditiontrade_buy_details_auto_trend
                    //        {
                    //            Status = y.Status,
                    //            CreateTime = DateTime.Now,
                    //            LastModified = DateTime.Now,
                    //            AutoId = auto.Id,
                    //            TrendDescription = y.TrendDescription,
                    //            TrendId = y.TrendId,
                    //            TrendName = y.TrendName
                    //        };
                    //        db.t_account_shares_conditiontrade_buy_details_auto_trend.Add(trend);
                    //        db.SaveChanges();

                    //        //参数复制
                    //        sql = string.Format("insert into t_account_shares_conditiontrade_buy_details_auto_trend_par(AutoTrendId,ParamsInfo,CreateTime,LastModified) select {0},ParamsInfo,'{1}','{1}' from t_account_shares_conditiontrade_buy_details_auto_trend_par where AutoTrendId={2}", trend.Id, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), y.Id);
                    //        db.Database.ExecuteSqlCommand(sql);
                    //        //额外关系复制
                    //        var temp_trend_autoList = trend_auto.Where(e => e.AutoTrendId == y.Id).ToList();
                    //        foreach (var t2 in temp_trend_autoList)
                    //        {
                    //            t_account_shares_conditiontrade_buy_details_auto_trend_other autotrendTemp = new t_account_shares_conditiontrade_buy_details_auto_trend_other
                    //            {
                    //                Status = t2.Status,
                    //                CreateTime = DateTime.Now,
                    //                LastModified = DateTime.Now,
                    //                AutoTrendId = trend.Id,
                    //                TrendDescription = t2.TrendDescription,
                    //                TrendId = t2.TrendId,
                    //                TrendName = t2.TrendName
                    //            };
                    //            db.t_account_shares_conditiontrade_buy_details_auto_trend_other.Add(autotrendTemp);
                    //            db.SaveChanges();
                    //            var auto_parList = (from t3 in trend_auto_par
                    //                                where t3.AutoTrendOtherId == t2.Id
                    //                                select new t_account_shares_conditiontrade_buy_details_auto_trend_other_par
                    //                                {
                    //                                    CreateTime = DateTime.Now,
                    //                                    AutoTrendOtherId = autotrendTemp.Id,
                    //                                    LastModified = DateTime.Now,
                    //                                    ParamsInfo = t3.ParamsInfo
                    //                                }).ToList();
                    //            db.t_account_shares_conditiontrade_buy_details_auto_trend_other_par.AddRange(auto_parList);
                    //            db.SaveChanges();
                    //        }
                    //    }
                    //}
                    #endregion

                    tran.Commit();
                    conditionInfo.Id = details.Id;
                    conditionInfo.AccountId = disAccountId;
                    conditionInfo.EntrustAmount = buyAmount;
                    conditionInfo.FollowType = followType;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        private static bool IsGetOther(meal_ticketEntities db, long otherTrendId, string sharesCode, int market, long currPrice, StringBuilder logRecord)
        {
            var timeNow = DateTime.Now;
            var trend = (from x in db.t_account_shares_conditiontrade_buy_details_other_trend_other
                         where x.OtherTrendId == otherTrendId && x.Status == 1
                         select x).ToList();
            bool tempTri = true;
            string trendName = "";
            foreach (var tr in trend)
            {
                tempTri = false;
                var par = (from x in db.t_account_shares_conditiontrade_buy_details_other_trend_other_par
                           where x.OtherTrendOtherId == tr.Id
                           select x.ParamsInfo).ToList();
                if (par.Count() <= 0)
                {
                    tempTri = false;
                    break;
                }
                //时间段
                if (tr.TrendId == 4)//指定时间段 
                {
                    trendName = "指定时间段";
                    var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                    JArray timeList = temp.Times;
                    if (timeNow >= DateTime.Parse(timeList[0].ToString()) && timeNow < DateTime.Parse(timeList[1].ToString()))
                    {
                        tempTri = true;
                    }
                }
                //当前价格
                if (tr.TrendId == 12)
                {
                    trendName = "按当前价格";
                    int errorCode_Trend12 = DataHelper.Analysis_CurrentPrice(sharesCode, market, par, logRecord);
                    if (errorCode_Trend12 == 0)
                    {
                        tempTri = true;
                    }
                }
                //历史涨跌幅
                if (tr.TrendId == 5)
                {
                    trendName = "历史涨跌幅";
                    int errorCode_Trend5 = DataHelper.Analysis_HisRiseRate(sharesCode, market, par, logRecord);
                    if (errorCode_Trend5 == 0)
                    {
                        tempTri = true;
                    }
                }
                //当前涨跌幅
                if (tr.TrendId == 6)
                {
                    trendName = "当前涨跌幅";
                    int errorCode_Trend6 = DataHelper.Analysis_TodayRiseRate(sharesCode, market, par, logRecord);
                    if (errorCode_Trend6 == 0)
                    {
                        tempTri = true;
                    }
                }
                //板块涨跌幅
                if (tr.TrendId == 7)
                {
                    trendName = "板块涨跌幅";
                    int errorCode_Trend7 = DataHelper.Analysis_PlateRiseRate(sharesCode, market, par, logRecord);
                    if (errorCode_Trend7 == 0)
                    {
                        tempTri = true;
                    }
                }
                //判断买卖单占比
                if (tr.TrendId == 8)
                {
                    trendName = "买卖单占比";
                    int errorCode_Trend8 = DataHelper.Analysis_BuyOrSellCount(sharesCode, market, par, logRecord);
                    if (errorCode_Trend8 == 0)
                    {
                        tempTri = true;
                    }
                }
                //判断按参照价格
                if (tr.TrendId == 9)
                {
                    trendName = "按参照价格";
                    int errorCode_Trend9 = DataHelper.Analysis_ReferPrice(sharesCode, market, par, logRecord);
                    if (errorCode_Trend9 == 0)
                    {
                        tempTri = true;
                    }
                }
                //判断按均线价格
                if (tr.TrendId == 10)
                {
                    trendName = "按均线价格";
                    int errorCode_Trend10 = DataHelper.Analysis_ReferAverage(sharesCode, market, par, logRecord);
                    if (errorCode_Trend10 == 0)
                    {
                        tempTri = true;
                    }
                }
                //五档变化速度
                if (tr.TrendId == 11)
                {
                    trendName = "五档变化速度";
                    int errorCode_Trend11 = DataHelper.Analysis_QuotesChangeRate(sharesCode, market, currPrice, par, logRecord);
                    if (errorCode_Trend11 == 0)
                    {
                        tempTri = true;
                    }
                }
                //快速拉升
                if (tr.TrendId == 1)//快速拉升 
                {
                    trendName = "快速拉升";
                    List<TREND_RESULT_RAPID_UP> resultInfo_Trend1 = new List<TREND_RESULT_RAPID_UP>();
                    int errorCode_Trend1 = DataHelper.Analysis_Trend1(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=sharesCode,
                                        Market=market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend1);
                    DateTime? resultPushTime = null;
                    if (errorCode_Trend1 == 0)
                    {
                        var temp = resultInfo_Trend1.Where(e => e.strStockCode == (sharesCode + "," + market)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(temp.strStockCode) && temp.dicUpOrDownInfo.Count() > 0)
                        {
                            var tempModel = temp.dicUpOrDownInfo.FirstOrDefault();
                            var pushTime = tempModel.Value.lastestInfo.dtTradeTime;
                            resultPushTime = pushTime;
                            if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                            {
                                tempTri = true;
                            }
                        }
                    }
                }
                //多头向上
                if (tr.TrendId == 2)//多头向上 
                {
                    trendName = "多头向上";
                    List<TREND_RESULT_LINE_UP> resultInfo_Trend2 = new List<TREND_RESULT_LINE_UP>();
                    int errorCode_Trend2 = DataHelper.Analysis_Trend2(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=sharesCode,
                                        Market=market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend2);
                    DateTime? resultPushTime = null;
                    if (errorCode_Trend2 == 0)
                    {
                        var temp = resultInfo_Trend2.Where(e => e.strStockCode == (sharesCode + "," + market)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(temp.strStockCode))
                        {
                            var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                            resultPushTime = pushTime;
                            if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                            {
                                tempTri = true;
                            }
                        }
                    }
                }
                //箱体上涨
                if (tr.TrendId == 3)//箱体突破 
                {
                    trendName = "箱体突破";
                    List<TREND_RESULT_BOX_BREACH> resultInfo_Trend3 = new List<TREND_RESULT_BOX_BREACH>();
                    int errorCode_Trend3 = DataHelper.Analysis_Trend3(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=sharesCode,
                                        Market=market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend3);
                    DateTime? resultPushTime = null;
                    if (errorCode_Trend3 == 0)
                    {
                        var temp = resultInfo_Trend3.Where(e => e.strStockCode == (sharesCode + "," + market)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(temp.strStockCode))
                        {
                            var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                            resultPushTime = pushTime;
                            if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                            {
                                tempTri = true;
                            }
                        }
                    }
                }
                //五档变化速度
                if (tr.TrendId == 13)
                {
                    trendName = "五档变化速度";
                    int errorCode_Trend13 = DataHelper.Analysis_QuotesTypeChangeRate(sharesCode, market, currPrice, par, logRecord);
                    if (errorCode_Trend13 == 0)
                    {
                        tempTri = true;
                    }
                }

                if (!tempTri)
                {
                    logRecord.AppendLine("\t\t额外关系-" + trendName + "未满足条件");
                    break;
                }
            }
            return tempTri;
        }

        private static bool IsGetAuto(meal_ticketEntities db, long autoTrendId, string sharesCode, int market, long currPrice, StringBuilder logRecord)
        {
            var timeNow = DateTime.Now;
            var trend = (from x in db.t_account_shares_conditiontrade_buy_details_auto_trend_other
                         where x.AutoTrendId == autoTrendId && x.Status == 1
                         select x).ToList();
            bool tempTri = true;
            string trendName = "";
            foreach (var tr in trend)
            {
                tempTri = false;
                var par = (from x in db.t_account_shares_conditiontrade_buy_details_auto_trend_other_par
                           where x.AutoTrendOtherId == tr.Id
                           select x.ParamsInfo).ToList();
                if (par.Count() <= 0)
                {
                    tempTri = false;
                    break;
                }
                //时间段
                if (tr.TrendId == 4)//指定时间段 
                {
                    trendName = "指定时间段";
                    var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                    JArray timeList = temp.Times;
                    if (timeNow >= DateTime.Parse(timeList[0].ToString()) && timeNow < DateTime.Parse(timeList[1].ToString()))
                    {
                        tempTri = true;
                    }
                }
                //当前价格
                if (tr.TrendId == 12)
                {
                    trendName = "按当前价格";
                    int errorCode_Trend12 = DataHelper.Analysis_CurrentPrice(sharesCode, market, par, logRecord);
                    if (errorCode_Trend12 == 0)
                    {
                        tempTri = true;
                    }
                }
                //历史涨跌幅
                if (tr.TrendId == 5)
                {
                    trendName = "历史涨跌幅";
                    int errorCode_Trend5 = DataHelper.Analysis_HisRiseRate(sharesCode, market, par, logRecord);
                    if (errorCode_Trend5 == 0)
                    {
                        tempTri = true;
                    }
                }
                //当前涨跌幅
                if (tr.TrendId == 6)
                {
                    trendName = "当前涨跌幅";
                    int errorCode_Trend6 = DataHelper.Analysis_TodayRiseRate(sharesCode, market, par, logRecord);
                    if (errorCode_Trend6 == 0)
                    {
                        tempTri = true;
                    }
                }
                //板块涨跌幅
                if (tr.TrendId == 7)
                {
                    trendName = "板块涨跌幅";
                    int errorCode_Trend7 = DataHelper.Analysis_PlateRiseRate(sharesCode, market, par, logRecord);
                    if (errorCode_Trend7 == 0)
                    {
                        tempTri = true;
                    }
                }
                //判断买卖单占比
                if (tr.TrendId == 8)
                {
                    trendName = "买卖单占比";
                    int errorCode_Trend8 = DataHelper.Analysis_BuyOrSellCount(sharesCode, market, par, logRecord);
                    if (errorCode_Trend8 == 0)
                    {
                        tempTri = true;
                    }
                }
                //判断按参照价格
                if (tr.TrendId == 9)
                {
                    trendName = "按参照价格";
                    int errorCode_Trend9 = DataHelper.Analysis_ReferPrice(sharesCode, market, par, logRecord);
                    if (errorCode_Trend9 == 0)
                    {
                        tempTri = true;
                    }
                }
                //判断按均线价格
                if (tr.TrendId == 10)
                {
                    trendName = "按均线价格";
                    int errorCode_Trend10 = DataHelper.Analysis_ReferAverage(sharesCode, market, par, logRecord);
                    if (errorCode_Trend10 == 0)
                    {
                        tempTri = true;
                    }
                }
                //五档变化速度
                if (tr.TrendId == 11)
                {
                    trendName = "五档变化速度";
                    int errorCode_Trend11 = DataHelper.Analysis_QuotesChangeRate(sharesCode, market, currPrice, par, logRecord);
                    if (errorCode_Trend11 == 0)
                    {
                        tempTri = true;
                    }
                }
                //快速拉升
                if (tr.TrendId == 1)//快速拉升 
                {
                    trendName = "快速拉升";
                    List<TREND_RESULT_RAPID_UP> resultInfo_Trend1 = new List<TREND_RESULT_RAPID_UP>();
                    int errorCode_Trend1 = DataHelper.Analysis_Trend1(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=sharesCode,
                                        Market=market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend1);
                    DateTime? resultPushTime = null;
                    if (errorCode_Trend1 == 0)
                    {
                        var temp = resultInfo_Trend1.Where(e => e.strStockCode == (sharesCode + "," + market)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(temp.strStockCode) && temp.dicUpOrDownInfo.Count() > 0)
                        {
                            var tempModel = temp.dicUpOrDownInfo.FirstOrDefault();
                            var pushTime = tempModel.Value.lastestInfo.dtTradeTime;
                            resultPushTime = pushTime;
                            if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                            {
                                tempTri = true;
                            }
                        }
                    }
                }
                //多头向上
                if (tr.TrendId == 2)//多头向上 
                {
                    trendName = "多头向上";
                    List<TREND_RESULT_LINE_UP> resultInfo_Trend2 = new List<TREND_RESULT_LINE_UP>();
                    int errorCode_Trend2 = DataHelper.Analysis_Trend2(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=sharesCode,
                                        Market=market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend2);
                    DateTime? resultPushTime = null;
                    if (errorCode_Trend2 == 0)
                    {
                        var temp = resultInfo_Trend2.Where(e => e.strStockCode == (sharesCode + "," + market)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(temp.strStockCode))
                        {
                            var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                            resultPushTime = pushTime;
                            if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                            {
                                tempTri = true;
                            }
                        }
                    }
                }
                //箱体上涨
                if (tr.TrendId == 3)//箱体突破 
                {
                    trendName = "箱体突破";
                    List<TREND_RESULT_BOX_BREACH> resultInfo_Trend3 = new List<TREND_RESULT_BOX_BREACH>();
                    int errorCode_Trend3 = DataHelper.Analysis_Trend3(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=sharesCode,
                                        Market=market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend3);
                    DateTime? resultPushTime = null;
                    if (errorCode_Trend3 == 0)
                    {
                        var temp = resultInfo_Trend3.Where(e => e.strStockCode == (sharesCode + "," + market)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(temp.strStockCode))
                        {
                            var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                            resultPushTime = pushTime;
                            if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                            {
                                tempTri = true;
                            }
                        }
                    }
                }
                //五档变化速度
                if (tr.TrendId == 13)
                {
                    trendName = "五档变化速度";
                    int errorCode_Trend13 = DataHelper.Analysis_QuotesTypeChangeRate(sharesCode, market, currPrice, par, logRecord);
                    if (errorCode_Trend13 == 0)
                    {
                        tempTri = true;
                    }
                }

                if (!tempTri)
                {
                    logRecord.AppendLine("\t\t额外关系-" + trendName + "未满足条件");
                    break;
                }
            }
            return tempTri;
        }

        /// <summary>
        /// 推送股票分笔数据
        /// </summary>
        /// <returns></returns>
        public static int SendTransactionShares(string taskGuid,int dataType)
        {
            DateTime timeDate = DateTime.Now.Date;
            List<TradeSharesInfo> sharesList = new List<TradeSharesInfo>();
            using (var db = new meal_ticketEntities())
            {
                string sql = string.Empty;
                if (dataType == 0)
                {
                    sql = @"select Market,SharesCode
  from
  (
	  select Market, SharesCode
	  from t_shares_monitor
	  where [Status] = 1 and DataType=0
	  union all
	  select Market, SharesCode
	  from t_account_shares_conditiontrade_buy
	  where [Status] = 1
	  group by Market,SharesCode
	  having count(case when DataType=0 then 1 else null end)>0
  )t
  group by Market, SharesCode";
                }
                else
                {
                    sql = @"select Market,SharesCode
  from
  (
      select Market, SharesCode
      from t_shares_monitor
      where [Status] = 1 and DataType = 1
      union all
      select Market, SharesCode
      from t_account_shares_conditiontrade_buy
      where [Status] = 1
      group by Market, SharesCode
      having count(case when DataType = 0 then 1 else null end)=0
  )t
  group by Market, SharesCode";
                }
                sharesList = db.Database.SqlQuery<TradeSharesInfo>(sql).ToList();
            }
            int totalCount = sharesList.Count();
            //批次数
            int HandlerCount = dataType == 0 ? Singleton.Instance.NewTransactionDataTrendHandlerCount : Singleton.Instance.NewTransactionDataTrendHandlerCount2;
            int batchSize = totalCount / HandlerCount;
            if (totalCount % HandlerCount != 0)
            {
                batchSize = batchSize + 1;
            }
            if (dataType == 0)
            {
                Singleton.Instance.mqHandler.ClearQueueData("TransactionDataUpdateNew");
            }
            else
            {
                Singleton.Instance.mqHandler.ClearQueueData("TransactionDataUpdateNew2");
            }
            for (int size = 0; size < batchSize; size++)
            {
                var batchList = sharesList.Skip(size * HandlerCount).Take(HandlerCount).ToList();
                Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new
                {
                    TaskGuid = taskGuid,
                    DataType = dataType,
                    DataList = batchList
                })), "TransactionData", dataType == 0 ? "UpdateNew" : "UpdateNew2");
            }
            return batchSize;
        }
    }

    public class TradeAutoBuyCondition
    {
        public long Id { get; set; }

        public long ConditionId { get; set; }

        public long CreateAccountId { get; set; }

        public long AccountId { get; set; }

        public string SharesCode { get; set; }

        public int Market { get; set; }

        public long PresentPrice { get; set; }

        public long ClosedPrice { get; set; }

        public DateTime LastModified { get; set; }

        public bool LimitUp { get; set; }

        public long LimitUpPrice { get; set; }

        public long LimitDownPrice { get; set; }

        public int ForbidType { get; set; }

        public DateTime? FirstExecTime { get; set; }


        public long BuyPrice1 { get; set; }


        public long BuyPrice2 { get; set; }


        public long BuyPrice3 { get; set; }


        public long BuyPrice4 { get; set; }


        public long BuyPrice5 { get; set; }


        public long SellPrice1 { get; set; }


        public long SellPrice2 { get; set; }


        public long SellPrice3 { get; set; }


        public long SellPrice4 { get; set; }


        public long SellPrice5 { get; set; }

        public long ConditionPrice { get; set; }

        public int ConditionType { get; set; }

        public int ConditionRelativeType { get; set; }

        public int ConditionRelativeRate { get; set; }

        public bool IsGreater { get; set; }

        public int OtherConditionRelative { get; set; }

        public int BusinessStatus { get; set; }

        public DateTime? TriggerTime { get; set; }

        public int ExecStatus { get; set; }

        public bool BuyAuto { get; set; }

        public int EntrustType { get; set; }

        public int EntrustPriceGear { get; set; }

        public long EntrustAmount { get; set; }

        public bool IsHold { get; set; }

        public int FollowType { get; set; }
    }

    public class TradeAutoBuyConditionGroup
    {
        public long AccountId { get; set; }

        public string SharesCode { get; set; }

        public int Market { get; set; }

        public List<TradeAutoBuyCondition> List { get; set; }
    }

    public class FollowAccountInfo 
    {
        public long AccountId { get; set; }

        public long BuyAmount { get; set; }
    }
}
