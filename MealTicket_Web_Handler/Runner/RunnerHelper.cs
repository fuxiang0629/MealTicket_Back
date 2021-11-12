using FXCommon.Common;
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
using System.Transactions;
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
                                MainAccountId = hold.item.CreateAccountId
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
        public static void TradeAutoBuy()
        {
            StringBuilder sb = new StringBuilder();
            List<TradeAutoBuyCondition> disResult = new List<TradeAutoBuyCondition>();
            using (var db = new meal_ticketEntities())
            {
                sb.AppendLine("===查询需要自动买入的数据===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                string sql = @"select t.Id,t1.Id ConditionId,t1.AccountId,t1.SharesCode,t1.Market,t2.PresentPrice,t2.ClosedPrice,t2.LastModified,t.LimitUp,t2.LimitUpPrice,t2.LimitDownPrice,
t.ForbidType,t.FirstExecTime,t2.BuyPrice1,t2.BuyPrice2,t2.BuyPrice3,t2.BuyPrice4,t2.BuyPrice5,t2.SellPrice1,t2.SellPrice2,t2.SellPrice3,
t2.SellPrice4,t2.SellPrice5,t.ConditionPrice,t.ConditionType,t.ConditionRelativeType,t.ConditionRelativeRate,t.IsGreater,t.OtherConditionRelative,
t.BusinessStatus,t.TriggerTime,t.ExecStatus,t.BuyAuto,t.EntrustPriceGear,t.EntrustType,t.EntrustAmount,t.IsHold,t.FollowType
from t_account_shares_conditiontrade_buy_details t with(nolock)
inner join t_account_shares_conditiontrade_buy t1 with(nolock) on t.ConditionId=t1.Id
inner join v_shares_quotes_last t2 with(nolock) on t1.Market=t2.Market and t1.SharesCode=t2.SharesCode
where t.[Status]=1 and t1.[Status]=1 and t.BusinessStatus=0";
                disResult = db.Database.SqlQuery<TradeAutoBuyCondition>(sql).ToList();
                sb.AppendLine("===查询结束===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
            if (disResult.Count() <= 0)
            {
                return;
            }
            //Logger.WriteFileLog(sb.ToString(), null);
            //Logger.WriteFileLog("===开始自动买入判断（总）===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            TodoTradeAutoBuy(disResult);
            //Logger.WriteFileLog("===结束自动买入判断（总）===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            //Logger.WriteFileLog("==================================", null);
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
                        List = new List<TradeAutoBuyCondition>() { item }

                    });
                }
                else
                {
                    groupInfo.List.Add(item);
                }
            }
            foreach (var item in groupList)
            {
                conditionData.AddMessage(item);
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
                //Logger.WriteFileLog("===开始同步买入判断===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                TodoTradeAutoBuy(list, true);
                //Logger.WriteFileLog("===结束同步买入判断===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            }
        }

        private static void doAutoBuyTask(TradeAutoBuyCondition item, bool isSyncro = false)
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                //Logger.WriteFileLog("1"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),null);
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

                //Logger.WriteFileLog("2" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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

                //Logger.WriteFileLog("3" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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
                                    logRecord.AppendLine("\t\t条件成立(" + desMessage + ")");
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

                //Logger.WriteFileLog("4" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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
                                    sql = "declare @parValueJson nvarchar(max),@id bigint;select @id=Id,@parValueJson=ParValueJson from t_account_shares_buy_setting with(xlock) where AccountId=" + item.AccountId + " and [Type]=3 if(@id is null) begin insert into t_account_shares_buy_setting(AccountId,[Type],Name,[Description],ParValueJson,CreateTime,LastModified) values(" + item.AccountId + ",3,'跟投分组轮序位置','跟投分组轮序位置','{\"Value\":1}',getdate(),getdate());set @parValueJson = '{\"Value\":0}';end else begin declare @parValue bigint; set @parValue = dbo.f_parseJson(@parValueJson, 'Value'); set @parValue = @parValue + 1; update t_account_shares_buy_setting set ParValueJson = '{\"Value\":' + convert(varchar(10), @parValue) + '}' where Id = @id; end select @parValueJson";
                                    string turnJson = db.Database.SqlQuery<string>(sql).FirstOrDefault();
                                    var tempTurn = JsonConvert.DeserializeObject<dynamic>(turnJson);
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
                                    Logger.WriteFileLog("更新跟投分组出错", ex);
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


                        //Logger.WriteFileLog("5" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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
                            IsFollow = false
                        });

                        //Logger.WriteFileLog("6" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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

                        //Logger.WriteFileLog("7" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                        SqlParameter[] parameter = new SqlParameter[8];
                        //市场
                        var marketPar = new SqlParameter("@market", SqlDbType.Int);
                        marketPar.Value = item.Market;
                        parameter[0] = marketPar;
                        //股票代码
                        var sharesCodePar = new SqlParameter("@sharesCode", SqlDbType.VarChar, 20);
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
                        var errorMessagePar = new SqlParameter("@errorMessage", SqlDbType.NVarChar, 200);
                        errorMessagePar.Direction = ParameterDirection.Output;
                        parameter[7] = errorMessagePar;

                        var resultList = db.Database.SqlQuery<ApplyTradeBuyInfo>("exec P_ApplyTradeBuy_Batch @market,@sharesCode,@entrustPrice,@mainAccountId,@autoBuyDetailsId,@applyTradeBuyInfo,@errorCode out,@errorMessage out", parameter).ToList();

                        //Logger.WriteFileLog("8" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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
                            string serverId = "s1";
                            var server = (from x in db.t_server_broker_account_rel
                                          join x2 in db.t_server on x.ServerId equals x2.ServerId
                                          orderby x2.OrderIndex
                                          select x).FirstOrDefault();
                            if (server == null)
                            {
                                throw new WebApiException(400, "服务器配置有误");
                            }
                            serverId = server.ServerId;
                            bool isSendSuccess = Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new { type = 1, data = sendDataList })), "SharesBuy", serverId);
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

                    //Logger.WriteFileLog("9" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                    if (!isSyncro)
                    {
                        TradeAutoBuyCondition conditionInfo = new TradeAutoBuyCondition();
                        conditionInfo = JsonConvert.DeserializeObject<TradeAutoBuyCondition>(JsonConvert.SerializeObject(item));
                        var tempGroupList = (from x in db.t_account_shares_conditiontrade_buy_group
                                             join x2 in db.t_account_shares_conditiontrade_buy_group_rel on x.Id equals x2.GroupId
                                             where x.AccountId == item.AccountId && x2.Market == item.Market && x2.SharesCode == item.SharesCode
                                             select x).ToList();
                        Logger.WriteFileLog("10" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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

                        Logger.WriteFileLog("11" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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
                        Logger.WriteFileLog("12" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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
                            catch (Exception ex)
                            {
                                Logger.WriteFileLog("同步出错", ex);
                            }
                        }
                        Logger.WriteFileLog("13" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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
        public static int SendTransactionShares(string taskGuid, int dataType)
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

        /// <summary>
        /// 自动加入
        /// </summary>
        public static void SharesAutoJoin()
        {
            //查询需要自动加入的股票
            List<SharesAutoJoinInfo> disResult = ToQuerySharesAutoJoin();
            if (disResult.Count() <= 0)
            {
                return;
            }
            //查询每只股票判断参数
            List<SharesAutoJoinPar> parList = ToQuerySharesAutoJoinPar(disResult);
            if (parList.Count() <= 0)
            {
                return;
            }
            //判断是否达到加入条件
            List<SharesAutoJoinInfo> exportList = ToQueryNeedToExportList(parList);
            if (exportList.Count() <= 0)
            {
                return;
            }
            //构建导入数据
            List<SharesAutoJoinExportInfo> toExportList = QueryExportSharesInfo(exportList);
            if (toExportList.Count() <= 0)
            {
                return;
            }
            //导入
            var successList = ToExportShares(toExportList);
            //记录日志
            WriteToLogRecord(successList);
        }

        /// <summary>
        /// 查询需要判断的股票
        /// </summary>
        /// <returns></returns>
        private static List<SharesAutoJoinInfo> ToQuerySharesAutoJoin()
        {
            List<SharesAutoJoinInfo> disResult = new List<SharesAutoJoinInfo>();
            using (var db = new meal_ticketEntities())
            {
                string sql = @"declare @currTime datetime;
set @currTime=getdate();
select distinct m.Market,m.SharesCode,m1.ToBuyId,m1.ToBuyName
from
(
	select distinct t2.AccountId,t2.Market,t2.SharesCode,isnull(t6.GroupId,0) GroupId
	from t_account_shares_optional_trend_rel_tri t with(nolock)
	inner join t_account_shares_optional_trend_rel t1 with(nolock) on t.RelId=t1.Id
	inner join t_account_shares_optional t2 with(nolock) on t1.OptionalId=t2.Id
	inner join t_shares_monitor t3 with(nolock) on t2.Market=t3.Market and t2.SharesCode=t3.SharesCode
	left join 
	(
		select * from t_account_shares_optional_group_rel t6 with(nolock) where GroupIsContinue=1 or (ValidStartTime<=@currTime and ValidEndTime>@currTime) 
	)t6 on t2.Id=t6.OptionalId
	where t.LastPushTime>convert(varchar(10),@currTime,120) and t1.[Status]=1 and t2.IsTrendClose=0
	union all 
	select distinct -1 AccountId, Market, SharesCode,case when PriceType = 1 then -1 when PriceType = 2 then -2 when PriceType = 0 and LimitUpCount > 0 then -3 
	when PriceType = 0 and LimitDownCount > 0 then -4 when TriNearLimitType = 1 and LimitUpCount = 0 then -5 else -6 end GroupId
	from t_shares_quotes_date t with(nolock)
	where t.[Date]=convert(varchar(10),@currTime,120) and (t.PriceType = 1 or t.PriceType = 2 or (t.PriceType = 0 and t.LimitUpCount > 0) 
	or (t.PriceType = 0 and t.LimitDownCount > 0) or (t.TriNearLimitType = 1 and t.LimitUpCount = 0) or (t.TriNearLimitType = 2 and t.LimitDownCount = 0))
)m
inner join 
(
  select t.AccountId,t.Id ToBuyId,t1.GroupId,t.Name ToBuyName,t.TimeCycleType,t.TimeCycle
  from t_account_shares_auto_join_tobuy t with(nolock)
  inner join t_account_shares_auto_join_tobuy_usegroup t1 with(nolock) on t.Id=t1.ToBuyId
  where t.[Status]=1
)m1 on (m.AccountId=m1.AccountId or m.AccountId=-1) and m.GroupId=m1.GroupId
inner join t_shares_all m2 with(nolock) on m.Market=m2.Market and m.SharesCode=m2.SharesCode
left join t_shares_limit m3 with(nolock) on (m.Market=m3.LimitMarket or m3.LimitMarket=-1) 
	and ((m3.LimitType=1 and m.SharesCode like m3.LimitKey+'%') or (m3.LimitType=2 and m2.SharesName like m3.LimitKey+'%'))
left join 
(
	select ToBuyId,AccountId,Market,SharesCode,Max(CreateTime) LastTime
	from t_account_shares_auto_join_tobuy_record with(nolock)
	group by ToBuyId,AccountId,Market,SharesCode
) m4 on m1.ToBuyId=m4.ToBuyId and m1.AccountId=m4.AccountId and m.Market=m4.Market and m.SharesCode=m4.SharesCode 
and ((m1.TimeCycleType=1 and dateadd(HOUR,m1.TimeCycle,LastTime)>=@currTime) 
or (m1.TimeCycleType=2 and dateadd(DAY,m1.TimeCycle,LastTime)>=@currTime) 
or (m1.TimeCycleType=3 and dateadd(WEEK,m1.TimeCycle,LastTime)>=@currTime)
or (m1.TimeCycleType=4 and dateadd(MONTH,m1.TimeCycle,LastTime)>=@currTime)
or (m1.TimeCycleType=5 and dateadd(YEAR,m1.TimeCycle,LastTime)>=@currTime))
where m3.Id is null and m4.ToBuyId is null";
                disResult = db.Database.SqlQuery<SharesAutoJoinInfo>(sql).ToList();
            }
            return disResult;
        }

        /// <summary>
        /// 查询股票判断参数
        /// </summary>
        /// <param name="disResult"></param>
        /// <returns></returns>
        private static List<SharesAutoJoinPar> ToQuerySharesAutoJoinPar(List<SharesAutoJoinInfo> disResult)
        {
            //toBuyId分组
            var dicResult = disResult.GroupBy(e => e.ToBuyId).ToDictionary(e => e.Key, v => v.ToList());

            ThreadMsgTemplate<KeyValuePair<long, List<SharesAutoJoinInfo>>> conditionData = new ThreadMsgTemplate<KeyValuePair<long, List<SharesAutoJoinInfo>>>();
            conditionData.Init();

            foreach (var item in dicResult)
            {
                conditionData.AddMessage(item);
            }

            int maxTastCount = 50;
            int taskCount = conditionData.GetCount();
            if (taskCount > maxTastCount)
            {
                taskCount = maxTastCount;
            }

            List<SharesAutoJoinPar> parList = new List<SharesAutoJoinPar>();
            object parLock = new object();
            Task[] tArr = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                tArr[i] = new Task(() =>
                {
                    while (true)
                    {
                        KeyValuePair<long, List<SharesAutoJoinInfo>> temp = new KeyValuePair<long, List<SharesAutoJoinInfo>>();
                        if (!conditionData.GetMessage(ref temp, true))
                        {
                            break;
                        }
                        _ToQuerySharesAutoJoinPar(temp, parLock, parList);
                    }
                }, TaskCreationOptions.LongRunning);
                tArr[i].Start();
            }
            Task.WaitAll(tArr);
            conditionData.Release();

            return parList;
        }

        private static void _ToQuerySharesAutoJoinPar(KeyValuePair<long, List<SharesAutoJoinInfo>> toBuyInfo, object parLock, List<SharesAutoJoinPar> parList)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var other = (from item in db.t_account_shares_auto_join_tobuy_other
                             where item.ToBuyId == toBuyInfo.Key && item.Status == 1
                             select item).ToList();
                List<long> otherIdList = other.Select(e => e.Id).ToList();

                var other_trend = (from item in db.t_account_shares_auto_join_tobuy_other_trend
                                   where otherIdList.Contains(item.OtherId) && item.Status == 1
                                   select item).ToList();
                List<long> otherTrendIdList = other_trend.Select(e => e.Id).ToList();

                var other_trend_par = (from item in db.t_account_shares_auto_join_tobuy_other_trend_par
                                       where otherTrendIdList.Contains(item.OtherTrendId)
                                       select item).ToList();

                var other_trend_other = (from item in db.t_account_shares_auto_join_tobuy_other_trend_other
                                         where otherTrendIdList.Contains(item.OtherTrendId) && item.Status == 1
                                         select item).ToList();
                List<long> otherTrendOtherIdList = other_trend_other.Select(e => e.Id).ToList();

                var other_trend_other_par = (from item in db.t_account_shares_auto_join_tobuy_other_trend_other_par
                                             where otherTrendOtherIdList.Contains(item.OtherTrendOtherId)
                                             select item).ToList();
                List<SharesAutoJoinPar> tempList = new List<SharesAutoJoinPar>();
                foreach (var item in toBuyInfo.Value)
                {
                    tempList.Add(new SharesAutoJoinPar
                    {
                        Market = item.Market,
                        SharesCode = item.SharesCode,
                        ToBuyId = item.ToBuyId,
                        ToBuyName = item.ToBuyName,
                        OtherList = (from o in other
                                     select new SharesAutoJoinOther
                                     {
                                         OtherId = o.Id,
                                         OtherName = o.Name,
                                         TrendList = (from t in other_trend
                                                      where t.OtherId == o.Id
                                                      select new SharesAutoJoinOtherTrend
                                                      {
                                                          TrendId = t.TrendId,
                                                          TrendName = t.TrendName,
                                                          TrendDescription = t.TrendDescription,
                                                          ParList = (from p in other_trend_par
                                                                     where p.OtherTrendId == t.Id
                                                                     select p.ParamsInfo).ToList(),
                                                          OtherTrendList = (from ot in other_trend_other
                                                                            where ot.OtherTrendId == t.Id
                                                                            select new SharesAutoJoinOtherTrend
                                                                            {
                                                                                TrendId = ot.TrendId,
                                                                                TrendName = ot.TrendName,
                                                                                TrendDescription = ot.TrendDescription,
                                                                                ParList = (from op in other_trend_other_par
                                                                                           where op.OtherTrendOtherId == ot.Id
                                                                                           select op.ParamsInfo).ToList()
                                                                            }).ToList()
                                                      }).ToList()
                                     }).ToList()
                    });
                }

                lock (parLock)
                {
                    parList.AddRange(tempList);
                }

                scope.Complete();
            }
        }

        /// <summary>
        /// 查询符合条件的股票
        /// </summary>
        /// <param name="parList"></param>
        /// <returns></returns>
        private static List<SharesAutoJoinInfo> ToQueryNeedToExportList(List<SharesAutoJoinPar> parList)
        {
            ThreadMsgTemplate<SharesAutoJoinPar> conditionData = new ThreadMsgTemplate<SharesAutoJoinPar>();
            conditionData.Init();
            foreach (var item in parList)
            {
                conditionData.AddMessage(item);
            }

            int maxTastCount = 50;
            int taskCount = conditionData.GetCount();
            if (taskCount > maxTastCount)
            {
                taskCount = maxTastCount;
            }

            List<SharesAutoJoinInfo> exportList = new List<SharesAutoJoinInfo>();
            object exportLock = new object();
            Task[] tArr = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                tArr[i] = new Task(() =>
                {
                    while (true)
                    {
                        SharesAutoJoinPar temp = new SharesAutoJoinPar();
                        if (!conditionData.GetMessage(ref temp, true))
                        {
                            break;
                        }
                        _ToQueryNeedToExportList(temp, exportLock, exportList);
                    }
                }, TaskCreationOptions.LongRunning);
                tArr[i].Start();
            }
            Task.WaitAll(tArr);
            conditionData.Release();

            return exportList;
        }

        private static void _ToQueryNeedToExportList(SharesAutoJoinPar parInfo, object exportLock, List<SharesAutoJoinInfo> exportList)
        {
            StringBuilder logRecord = new StringBuilder();
            logRecord.AppendLine("===开始判断条件(" + parInfo.ToBuyName + ");" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
            bool success = true;
            foreach (var other in parInfo.OtherList)
            {
                var trendSuccess = _CheckTrendConditions(false, parInfo.Market, parInfo.SharesCode, other.OtherName, other.TrendList, logRecord);
                if (!trendSuccess)
                {
                    success = false;
                    break;
                }
            }
            if (success)
            {
                lock (exportLock)
                {
                    exportList.Add(new SharesAutoJoinInfo
                    {
                        SharesCode = parInfo.SharesCode,
                        Market = parInfo.Market,
                        ToBuyId = parInfo.ToBuyId
                    });
                }
            }
            logRecord.AppendLine("===条件(" + parInfo.ToBuyName + ")判断结束;" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
            Logger.WriteFileLog(logRecord.ToString(), "AutoJoinRecord/" + parInfo.SharesCode, null);
        }

        /// <summary>
        /// 判断走势条件是否符合
        /// </summary>
        /// <returns></returns>
        private static bool _CheckTrendConditions(bool isother, int market, string sharesCode, string groupName, List<SharesAutoJoinOtherTrend> trendList, StringBuilder logRecord = null)
        {
            StringBuilder tempString = new StringBuilder();
            string tab = "\t";
            if (isother)
            {
                tab = tab + "\t";
            }
            tempString.AppendLine(tab + "分组名称:" + groupName);

            bool trendSuccess = true;
            foreach (var trend in trendList)
            {
                trendSuccess = false;
                int errorCode = _ToJudgeCondition(isother, trend.TrendId, market, sharesCode, trend.TrendDescription, trend.ParList, tempString);
                if (errorCode == 0)
                {
                    trendSuccess = true;
                    if (!isother && trend.OtherTrendList.Count() > 0)//判断额外参数
                    {
                        trendSuccess = _CheckTrendConditions(true, market, sharesCode, "额外参数", trend.OtherTrendList, tempString);
                    }
                    if (trendSuccess)
                    {
                        break;
                    }
                }
            }
            tempString.AppendLine(tab + "分组结果" + (trendSuccess ? "条件成立" : "条件不成立"));
            if (logRecord != null)
            {
                logRecord.AppendLine(tempString.ToString());
            }
            return trendSuccess;
        }

        private static int _ToJudgeCondition(bool isother, long trendId, int market, string sharesCode, string trendDes, List<string> parList, StringBuilder logRecord = null)
        {
            StringBuilder tempString = new StringBuilder();
            DateTime timeNow = DateTime.Now;
            string tab = "\t\t";
            if (isother)
            {
                tab = tab + "\t";
            }
            tempString.Append(tab + "走势名称:" + trendDes + "");

            int result = -1;
            switch (trendId)
            {
                case 1:
                    result = DataHelper.Analysis_Trend1_New(sharesCode, market, parList, tempString);
                    break;
                case 2:
                    result = DataHelper.Analysis_Trend2_New(sharesCode, market, parList, tempString);
                    break;
                case 3:
                    result = DataHelper.Analysis_Trend3_New(sharesCode, market, parList, tempString);
                    break;
                case 4:
                    result = DataHelper.Analysis_TimeSlot_New(sharesCode, market, parList, tempString);
                    break;
                case 5:
                    result = DataHelper.Analysis_HisRiseRate_New(sharesCode, market, parList, tempString);
                    break;
                case 6:
                    result = DataHelper.Analysis_TodayRiseRate_New(sharesCode, market, parList, tempString);
                    break;
                case 7:
                    result = DataHelper.Analysis_PlateRiseRate_New(sharesCode, market, parList, tempString);
                    break;
                case 8:
                    result = DataHelper.Analysis_BuyOrSellCount_New(sharesCode, market, parList, tempString);
                    break;
                case 9:
                    result = DataHelper.Analysis_ReferPrice_New(sharesCode, market, parList, tempString);
                    break;
                case 10:
                    result = DataHelper.Analysis_ReferAverage_New(sharesCode, market, parList, tempString);
                    break;
                case 11:
                    result = DataHelper.Analysis_QuotesChangeRate_New(sharesCode, market, parList, tempString);
                    break;
                case 12:
                    result = DataHelper.Analysis_CurrentPrice_New(sharesCode, market, parList, tempString);
                    break;
                case 13:
                    result = DataHelper.Analysis_QuotesTypeChangeRate_New(sharesCode, market, parList, tempString);
                    break;
                default:
                    result = -1;
                    break;

            }

            if (logRecord != null)
            {
                logRecord.AppendLine(tempString.ToString());
            }
            return result;
        }

        /// <summary>
        /// 构建导入数据
        /// </summary>
        private static List<SharesAutoJoinExportInfo> QueryExportSharesInfo(List<SharesAutoJoinInfo> exportList)
        {
            Dictionary<long, List<SharesAutoJoinInfo>> exportDic = exportList.GroupBy(e => e.ToBuyId).ToDictionary(e => e.Key, v => v.ToList());
            ThreadMsgTemplate<KeyValuePair<long, List<SharesAutoJoinInfo>>> conditionData = new ThreadMsgTemplate<KeyValuePair<long, List<SharesAutoJoinInfo>>>();
            conditionData.Init();

            foreach (var item in exportDic)
            {
                conditionData.AddMessage(item);
            }

            int maxTastCount = 50;
            int taskCount = conditionData.GetCount();
            if (taskCount > maxTastCount)
            {
                taskCount = maxTastCount;
            }

            List<SharesAutoJoinExportInfo> toExportList = new List<SharesAutoJoinExportInfo>();
            object exportLock = new object();

            Task[] tArr = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                tArr[i] = new Task(() =>
                {
                    while (true)
                    {
                        KeyValuePair<long, List<SharesAutoJoinInfo>> temp = new KeyValuePair<long, List<SharesAutoJoinInfo>>();
                        if (!conditionData.GetMessage(ref temp, true))
                        {
                            break;
                        }
                        _ToQueryExportSharesInfo(temp, exportLock, toExportList);
                    }
                }, TaskCreationOptions.LongRunning);
                tArr[i].Start();
            }
            Task.WaitAll(tArr);
            conditionData.Release();

            return toExportList;
        }

        private static void _ToQueryExportSharesInfo(KeyValuePair<long, List<SharesAutoJoinInfo>> temp, object exportLock, List<SharesAutoJoinExportInfo> toExportList)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                //查询基础信息
                var toBuy = (from item in db.t_account_shares_auto_join_tobuy
                             where item.Id == temp.Key
                             select new SharesAutoJoinExportInfo
                             {
                                 ToBuyId = temp.Key,
                                 AccountId = item.AccountId,
                                 IsClear = item.IsClearOriginal,
                                 Type = item.TemplateType,
                                 TemplateId = item.TemplateId,
                                 FollowType = item.FollowType
                             }).FirstOrDefault();
                if (toBuy == null)
                {
                    return;
                }
                //查询跟投人员
                var follow = (from item in db.t_account_shares_auto_join_tobuy_follow
                              where item.ToBuyId == temp.Key
                              select item.FollowAccountId).ToList();
                toBuy.FollowList = follow;

                //查询自定义分组
                var groupInfo = (from item in db.t_account_shares_auto_join_tobuy_group
                                 where item.ToBuyId == temp.Key
                                 select item.GroupId).ToList();
                toBuy.GroupList = groupInfo;

                //查询股票
                toBuy.SharesList = (from item in temp.Value
                                    select new SharesBase
                                    {
                                        Market = item.Market,
                                        SharesCode = item.SharesCode
                                    }).ToList();

                lock (exportLock)
                {
                    toExportList.Add(toBuy);
                }
                scope.Complete();
            }
        }

        //导入股票
        private static List<SharesAutoJoinExportInfo> ToExportShares(List<SharesAutoJoinExportInfo> toExportList)
        {
            List<SharesAutoJoinExportInfo> successList = new List<SharesAutoJoinExportInfo>();
            object dataLock = new object();

            ThreadMsgTemplate<SharesAutoJoinExportInfo> conditionData = new ThreadMsgTemplate<SharesAutoJoinExportInfo>();
            conditionData.Init();

            foreach (var item in toExportList)
            {
                conditionData.AddMessage(item);
            }

            int maxTastCount = 50;
            int taskCount = conditionData.GetCount();
            if (taskCount > maxTastCount)
            {
                taskCount = maxTastCount;
            }

            Task[] tArr = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                tArr[i] = new Task(() =>
                {
                    while (true)
                    {
                        SharesAutoJoinExportInfo temp = new SharesAutoJoinExportInfo();
                        if (!conditionData.GetMessage(ref temp, true))
                        {
                            break;
                        }
                        List<SharesBase> successShares = _ToExportShares(temp);
                        lock (dataLock)
                        {
                            successList.Add(new SharesAutoJoinExportInfo
                            {
                                SharesList = successShares,
                                ToBuyId = temp.ToBuyId,
                                AccountId = temp.AccountId
                            });
                        }
                    }
                }, TaskCreationOptions.LongRunning);
                tArr[i].Start();
            }
            Task.WaitAll(tArr);
            conditionData.Release();

            return successList;
        }

        public static List<SharesBase> _ToExportShares(SharesAutoJoinExportInfo temp)
        {
            List<SharesBase> successList = new List<SharesBase>();
            object dataLock = new object();
            //查询模板数据
            List<template_buy> template_buy = new List<template_buy>();
            List<buy_child> template_buy_child = new List<buy_child>();
            List<buy_other> template_buy_other = new List<buy_other>();
            List<buy_other_trend> template_buy_other_trend = new List<buy_other_trend>();
            List<buy_other_trend_par> template_buy_other_trend_par = new List<buy_other_trend_par>();
            List<buy_other_trend_other> template_buy_other_trend_other = new List<buy_other_trend_other>();
            List<buy_other_trend_other_par> template_buy_other_trend_other_par = new List<buy_other_trend_other_par>();
            List<buy_auto> template_buy_auto = new List<buy_auto>();
            List<buy_auto_trend> template_buy_auto_trend = new List<buy_auto_trend>();
            List<buy_auto_trend_par> template_buy_auto_trend_par = new List<buy_auto_trend_par>();
            List<buy_auto_trend_other> template_buy_auto_trend_other = new List<buy_auto_trend_other>();
            List<buy_auto_trend_other_par> template_buy_auto_trend_other_par = new List<buy_auto_trend_other_par>();
            long totalDeposit = 0;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                if (temp.Type == 1)
                {
                    var template = (from item in db.t_account_shares_conditiontrade_template
                                    where item.Type == 1 && item.Id == temp.TemplateId
                                    select item).FirstOrDefault();
                    if (template == null)
                    {
                        throw new WebApiException(400, "模板不存在");
                    }
                    template_buy = (from item in db.t_account_shares_conditiontrade_template_buy
                                    where item.TemplateId == temp.TemplateId
                                    select new template_buy
                                    {
                                        Status = item.Status,
                                        BuyAuto = item.BuyAuto,
                                        ConditionPriceBase = item.ConditionPriceBase,
                                        ConditionPriceRate = item.ConditionPriceRate,
                                        ConditionPriceType = item.ConditionPriceType,
                                        ConditionType = item.ConditionType,
                                        CreateTime = item.CreateTime,
                                        EntrustAmount = item.EntrustAmount,
                                        EntrustAmountType = item.EntrustAmountType,
                                        EntrustPriceGear = item.EntrustPriceGear,
                                        EntrustType = item.EntrustType,
                                        ForbidType = item.ForbidType,
                                        Id = item.Id,
                                        IsGreater = item.IsGreater,
                                        IsHold = item.IsHold,
                                        LastModified = item.LastModified,
                                        LimitUp = item.LimitUp,
                                        Name = item.Name,
                                        OtherConditionRelative = item.OtherConditionRelative,
                                        TemplateId = item.TemplateId
                                    }).ToList();
                    var template_buy_id_list = template_buy.Select(e => e.Id).ToList();

                    template_buy_child = (from item in db.t_account_shares_conditiontrade_template_buy_child
                                          where template_buy_id_list.Contains(item.FatherId)
                                          select new buy_child
                                          {
                                              ChildId = item.ChildId,
                                              FatherId = item.FatherId,
                                              Id = item.Id,
                                              Status = item.Status
                                          }).ToList();

                    template_buy_other = (from item in db.t_account_shares_conditiontrade_template_buy_other
                                          where template_buy_id_list.Contains(item.TemplateBuyId)
                                          select new buy_other
                                          {
                                              CreateTime = item.CreateTime,
                                              Status = item.Status,
                                              Id = item.Id,
                                              LastModified = item.LastModified,
                                              Name = item.Name,
                                              TemplateBuyId = item.TemplateBuyId
                                          }).ToList();
                    var template_buy_other_id_list = template_buy_other.Select(e => e.Id).ToList();

                    template_buy_other_trend = (from item in db.t_account_shares_conditiontrade_template_buy_other_trend
                                                where template_buy_other_id_list.Contains(item.OtherId)
                                                select new buy_other_trend
                                                {
                                                    Status = item.Status,
                                                    CreateTime = item.CreateTime,
                                                    Id = item.Id,
                                                    LastModified = item.LastModified,
                                                    OtherId = item.OtherId,
                                                    TrendDescription = item.TrendDescription,
                                                    TrendId = item.TrendId,
                                                    TrendName = item.TrendName
                                                }).ToList();
                    var template_buy_other_trend_id_list = template_buy_other_trend.Select(e => e.Id).ToList();

                    template_buy_other_trend_par = (from item in db.t_account_shares_conditiontrade_template_buy_other_trend_par
                                                    where template_buy_other_trend_id_list.Contains(item.OtherTrendId)
                                                    select new buy_other_trend_par
                                                    {
                                                        CreateTime = item.CreateTime,
                                                        Id = item.Id,
                                                        LastModified = item.LastModified,
                                                        OtherTrendId = item.OtherTrendId,
                                                        ParamsInfo = item.ParamsInfo
                                                    }).ToList();

                    template_buy_other_trend_other = (from item in db.t_account_shares_conditiontrade_template_buy_other_trend_other
                                                      where template_buy_other_trend_id_list.Contains(item.OtherTrendId)
                                                      select new buy_other_trend_other
                                                      {
                                                          CreateTime = item.CreateTime,
                                                          Id = item.Id,
                                                          Status = item.Status,
                                                          LastModified = item.LastModified,
                                                          OtherTrendId = item.OtherTrendId,
                                                          TrendDescription = item.TrendDescription,
                                                          TrendId = item.TrendId,
                                                          TrendName = item.TrendName
                                                      }).ToList();
                    var template_buy_other_trend_other_id_list = template_buy_other_trend_other.Select(e => e.Id).ToList();

                    template_buy_other_trend_other_par = (from item in db.t_account_shares_conditiontrade_template_buy_other_trend_other_par
                                                          where template_buy_other_trend_other_id_list.Contains(item.OtherTrendOtherId)
                                                          select new buy_other_trend_other_par
                                                          {
                                                              CreateTime = item.CreateTime,
                                                              Id = item.Id,
                                                              LastModified = item.LastModified,
                                                              OtherTrendOtherId = item.OtherTrendOtherId,
                                                              ParamsInfo = item.ParamsInfo
                                                          }).ToList();

                    template_buy_auto = (from item in db.t_account_shares_conditiontrade_template_buy_auto
                                         where template_buy_id_list.Contains(item.TemplateBuyId)
                                         select new buy_auto
                                         {
                                             CreateTime = item.CreateTime,
                                             Status = item.Status,
                                             Id = item.Id,
                                             LastModified = item.LastModified,
                                             Name = item.Name,
                                             TemplateBuyId = item.TemplateBuyId
                                         }).ToList();
                    var template_buy_auto_id_list = template_buy_auto.Select(e => e.Id).ToList();

                    template_buy_auto_trend = (from item in db.t_account_shares_conditiontrade_template_buy_auto_trend
                                               where template_buy_auto_id_list.Contains(item.AutoId)
                                               select new buy_auto_trend
                                               {
                                                   Status = item.Status,
                                                   CreateTime = item.CreateTime,
                                                   Id = item.Id,
                                                   LastModified = item.LastModified,
                                                   AutoId = item.AutoId,
                                                   TrendDescription = item.TrendDescription,
                                                   TrendId = item.TrendId,
                                                   TrendName = item.TrendName
                                               }).ToList();
                    var template_buy_auto_trend_id_list = template_buy_auto_trend.Select(e => e.Id).ToList();

                    template_buy_auto_trend_par = (from item in db.t_account_shares_conditiontrade_template_buy_auto_trend_par
                                                   where template_buy_auto_trend_id_list.Contains(item.AutoTrendId)
                                                   select new buy_auto_trend_par
                                                   {
                                                       CreateTime = item.CreateTime,
                                                       Id = item.Id,
                                                       LastModified = item.LastModified,
                                                       AutoTrendId = item.AutoTrendId,
                                                       ParamsInfo = item.ParamsInfo
                                                   }).ToList();

                    template_buy_auto_trend_other = (from item in db.t_account_shares_conditiontrade_template_buy_auto_trend_other
                                                     where template_buy_auto_trend_id_list.Contains(item.AutoTrendId)
                                                     select new buy_auto_trend_other
                                                     {
                                                         CreateTime = item.CreateTime,
                                                         Id = item.Id,
                                                         Status = item.Status,
                                                         LastModified = item.LastModified,
                                                         AutoTrendId = item.AutoTrendId,
                                                         TrendDescription = item.TrendDescription,
                                                         TrendId = item.TrendId,
                                                         TrendName = item.TrendName
                                                     }).ToList();
                    var template_buy_auto_trend_other_id_list = template_buy_auto_trend_other.Select(e => e.Id).ToList();

                    template_buy_auto_trend_other_par = (from item in db.t_account_shares_conditiontrade_template_buy_auto_trend_other_par
                                                         where template_buy_auto_trend_other_id_list.Contains(item.AutoTrendOtherId)
                                                         select new buy_auto_trend_other_par
                                                         {
                                                             CreateTime = item.CreateTime,
                                                             Id = item.Id,
                                                             LastModified = item.LastModified,
                                                             AutoTrendOtherId = item.AutoTrendOtherId,
                                                             ParamsInfo = item.ParamsInfo
                                                         }).ToList();
                }
                else if (temp.Type == 2)
                {
                    var template = (from item in db.t_sys_conditiontrade_template
                                    where item.Type == 1 && item.Id == temp.TemplateId
                                    select item).FirstOrDefault();
                    if (template == null)
                    {
                        throw new WebApiException(400, "模板不存在");
                    }
                    template_buy = (from item in db.t_sys_conditiontrade_template_buy
                                    where item.TemplateId == temp.TemplateId
                                    select new template_buy
                                    {
                                        Status = item.Status,
                                        BuyAuto = item.BuyAuto,
                                        ConditionPriceBase = item.ConditionPriceBase,
                                        ConditionPriceRate = item.ConditionPriceRate,
                                        ConditionPriceType = item.ConditionPriceType,
                                        ConditionType = item.ConditionType,
                                        CreateTime = item.CreateTime,
                                        EntrustAmount = item.EntrustAmount,
                                        EntrustAmountType = item.EntrustAmountType,
                                        EntrustPriceGear = item.EntrustPriceGear,
                                        EntrustType = item.EntrustType,
                                        ForbidType = item.ForbidType,
                                        Id = item.Id,
                                        IsGreater = item.IsGreater,
                                        IsHold = item.IsHold,
                                        LastModified = item.LastModified,
                                        LimitUp = item.LimitUp,
                                        Name = item.Name,
                                        OtherConditionRelative = item.OtherConditionRelative,
                                        TemplateId = item.TemplateId
                                    }).ToList();
                    var template_buy_id_list = template_buy.Select(e => e.Id).ToList();

                    template_buy_child = (from item in db.t_sys_conditiontrade_template_buy_child
                                          where template_buy_id_list.Contains(item.FatherId)
                                          select new buy_child
                                          {
                                              ChildId = item.ChildId,
                                              FatherId = item.ChildId,
                                              Id = item.Id,
                                              Status = item.Status
                                          }).ToList();

                    template_buy_other = (from item in db.t_sys_conditiontrade_template_buy_other
                                          where template_buy_id_list.Contains(item.TemplateBuyId)
                                          select new buy_other
                                          {
                                              CreateTime = item.CreateTime,
                                              Status = item.Status,
                                              Id = item.Id,
                                              LastModified = item.LastModified,
                                              Name = item.Name,
                                              TemplateBuyId = item.TemplateBuyId
                                          }).ToList();
                    var template_buy_other_id_list = template_buy_other.Select(e => e.Id).ToList();

                    template_buy_other_trend = (from item in db.t_sys_conditiontrade_template_buy_other_trend
                                                where template_buy_other_id_list.Contains(item.OtherId)
                                                select new buy_other_trend
                                                {
                                                    Status = item.Status,
                                                    CreateTime = item.CreateTime,
                                                    Id = item.Id,
                                                    LastModified = item.LastModified,
                                                    OtherId = item.OtherId,
                                                    TrendDescription = item.TrendDescription,
                                                    TrendId = item.TrendId,
                                                    TrendName = item.TrendName
                                                }).ToList();
                    var template_buy_other_trend_id_list = template_buy_other_trend.Select(e => e.Id).ToList();

                    template_buy_other_trend_par = (from item in db.t_sys_conditiontrade_template_buy_other_trend_par
                                                    where template_buy_other_trend_id_list.Contains(item.OtherTrendId)
                                                    select new buy_other_trend_par
                                                    {
                                                        CreateTime = item.CreateTime,
                                                        Id = item.Id,
                                                        LastModified = item.LastModified,
                                                        OtherTrendId = item.OtherTrendId,
                                                        ParamsInfo = item.ParamsInfo
                                                    }).ToList();

                    template_buy_other_trend_other = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other
                                                      where template_buy_other_trend_id_list.Contains(item.OtherTrendId)
                                                      select new buy_other_trend_other
                                                      {
                                                          CreateTime = item.CreateTime,
                                                          Id = item.Id,
                                                          Status = item.Status,
                                                          LastModified = item.LastModified,
                                                          OtherTrendId = item.OtherTrendId,
                                                          TrendDescription = item.TrendDescription,
                                                          TrendId = item.TrendId,
                                                          TrendName = item.TrendName
                                                      }).ToList();
                    var template_buy_other_trend_other_id_list = template_buy_other_trend_other.Select(e => e.Id).ToList();

                    template_buy_other_trend_other_par = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other_par
                                                          where template_buy_other_trend_other_id_list.Contains(item.OtherTrendOtherId)
                                                          select new buy_other_trend_other_par
                                                          {
                                                              CreateTime = item.CreateTime,
                                                              Id = item.Id,
                                                              LastModified = item.LastModified,
                                                              OtherTrendOtherId = item.OtherTrendOtherId,
                                                              ParamsInfo = item.ParamsInfo
                                                          }).ToList();

                    template_buy_auto = (from item in db.t_sys_conditiontrade_template_buy_auto
                                         where template_buy_id_list.Contains(item.TemplateBuyId)
                                         select new buy_auto
                                         {
                                             CreateTime = item.CreateTime,
                                             Status = item.Status,
                                             Id = item.Id,
                                             LastModified = item.LastModified,
                                             Name = item.Name,
                                             TemplateBuyId = item.TemplateBuyId
                                         }).ToList();
                    var template_buy_auto_id_list = template_buy_auto.Select(e => e.Id).ToList();

                    template_buy_auto_trend = (from item in db.t_sys_conditiontrade_template_buy_auto_trend
                                               where template_buy_other_id_list.Contains(item.AutoId)
                                               select new buy_auto_trend
                                               {
                                                   Status = item.Status,
                                                   CreateTime = item.CreateTime,
                                                   Id = item.Id,
                                                   LastModified = item.LastModified,
                                                   AutoId = item.AutoId,
                                                   TrendDescription = item.TrendDescription,
                                                   TrendId = item.TrendId,
                                                   TrendName = item.TrendName
                                               }).ToList();
                    var template_buy_auto_trend_id_list = template_buy_auto_trend.Select(e => e.Id).ToList();

                    template_buy_auto_trend_par = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_par
                                                   where template_buy_auto_trend_id_list.Contains(item.AutoTrendId)
                                                   select new buy_auto_trend_par
                                                   {
                                                       CreateTime = item.CreateTime,
                                                       Id = item.Id,
                                                       LastModified = item.LastModified,
                                                       AutoTrendId = item.AutoTrendId,
                                                       ParamsInfo = item.ParamsInfo
                                                   }).ToList();

                    template_buy_auto_trend_other = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other
                                                     where template_buy_auto_trend_id_list.Contains(item.AutoTrendId)
                                                     select new buy_auto_trend_other
                                                     {
                                                         CreateTime = item.CreateTime,
                                                         Id = item.Id,
                                                         Status = item.Status,
                                                         LastModified = item.LastModified,
                                                         AutoTrendId = item.AutoTrendId,
                                                         TrendDescription = item.TrendDescription,
                                                         TrendId = item.TrendId,
                                                         TrendName = item.TrendName
                                                     }).ToList();
                    var template_buy_auto_trend_other_id_list = template_buy_auto_trend_other.Select(e => e.Id).ToList();

                    template_buy_auto_trend_other_par = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other_par
                                                         where template_buy_auto_trend_other_id_list.Contains(item.AutoTrendOtherId)
                                                         select new buy_auto_trend_other_par
                                                         {
                                                             CreateTime = item.CreateTime,
                                                             Id = item.Id,
                                                             LastModified = item.LastModified,
                                                             AutoTrendOtherId = item.AutoTrendOtherId,
                                                             ParamsInfo = item.ParamsInfo
                                                         }).ToList();
                }
                else
                {
                    throw new WebApiException(400, "参数错误");
                }

                totalDeposit = (from item in db.t_account_wallet
                                where item.AccountId == temp.AccountId
                                select item.Deposit).FirstOrDefault();
                var accountBuySetting = DbHelper.GetAccountBuySetting(temp.AccountId, 1);
                var buysetting = accountBuySetting.FirstOrDefault();
                if (buysetting != null)
                {
                    var valueObj = JsonConvert.DeserializeObject<dynamic>(buysetting.ParValueJson);
                    long parValue = valueObj.Value;
                    totalDeposit = totalDeposit - parValue / 100 * 100;
                }
                scope.Complete();
            }

            int sharesCount = temp.SharesList.Count();
            int taskCount = sharesCount;
            Task[] tArr = new Task[taskCount];

            for (int i = 0; i < sharesCount; i++)
            {
                int index = i;
                tArr[index] = new Task(() =>
                {
                    using (var db = new meal_ticketEntities())
                    {
                        int market = temp.SharesList[index].Market;
                        string sharesCode = temp.SharesList[index].SharesCode;
                        string sharesName = (from item in db.t_shares_all
                                             where item.Market == market && item.SharesCode == sharesCode
                                             select item.SharesName).FirstOrDefault();
                        using (var tran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                //判断目标是否存在这只股票
                                string sql = @"declare @timeNow datetime=getdate(),@buyId bigint;
select top 1 @buyId=Id from t_account_shares_conditiontrade_buy with(xlock) where AccountId={0} and Market={1} and SharesCode='{2}';
if(@buyId is null)
begin
	insert into t_account_shares_conditiontrade_buy
	(AccountId,Market,SharesCode,[Status],CreateTime,LastModified,DataType)
	values({0},{1},'{2}',1,@timeNow,@timeNow,1);
	set @buyId=@@IDENTITY;
end
select @buyId;";
                                long shares_buy_id = db.Database.SqlQuery<long>(string.Format(sql, temp.AccountId, market, sharesCode)).FirstOrDefault();

                                //添加自定义分组
                                var groupInfo = (from item in db.t_account_shares_conditiontrade_buy_group_rel
                                                 where temp.GroupList.Contains(item.GroupId) && item.Market == market && item.SharesCode == sharesCode
                                                 select item).ToList();
                                if (groupInfo.Count() > 0)
                                {
                                    db.t_account_shares_conditiontrade_buy_group_rel.RemoveRange(groupInfo);
                                    db.SaveChanges();
                                }
                                foreach (var groupId in temp.GroupList)
                                {
                                    db.t_account_shares_conditiontrade_buy_group_rel.Add(new t_account_shares_conditiontrade_buy_group_rel
                                    {
                                        SharesCode = sharesCode,
                                        Market = market,
                                        GroupId = groupId
                                    });
                                }
                                db.SaveChanges();

                                //是否清除原来数据
                                if (temp.IsClear)
                                {
                                    sql = @"delete t_account_shares_conditiontrade_buy_details where ConditionId={0} and TriggerTime is null";
                                    db.Database.ExecuteSqlCommand(string.Format(sql, shares_buy_id));
                                }
                                else
                                {
                                    sql = @"delete t_account_shares_conditiontrade_buy_details where ToBuyId={0} and TriggerTime is null";
                                    db.Database.ExecuteSqlCommand(string.Format(sql, temp.ToBuyId));
                                }

                                //当前价
                                long currPrice = 0;
                                //昨日收盘价
                                long closedPrice = 0;
                                //计算杠杆倍数
                                int range = 0;

                                //当前行情信息
                                var quote = (from item in db.v_shares_quotes_last
                                             where item.Market == market && item.SharesCode == sharesCode
                                             select item).FirstOrDefault();
                                if (quote == null)
                                {
                                    throw new WebApiException(400, "暂无行情");
                                }
                                //当前价
                                currPrice = quote.PresentPrice;
                                //昨日收盘价
                                closedPrice = quote.ClosedPrice;
                                //计算杠杆倍数
                                range = 1000;
                                var rules = (from item in db.t_shares_limit_fundmultiple
                                             where (item.LimitMarket == market || item.LimitMarket == -1) && (sharesCode.StartsWith(item.LimitKey))
                                             orderby item.Priority descending, item.FundMultiple
                                             select item).FirstOrDefault();
                                if (rules == null)
                                {
                                    throw new WebApiException(400, "后台配置有误");
                                }
                                else
                                {
                                    range = rules.Range;
                                    if (sharesName.ToUpper().Contains("ST"))
                                    {
                                        range = range / 2;
                                    }
                                }

                                List<RelIdInfo> relIdList = new List<RelIdInfo>();
                                List<t_account_shares_conditiontrade_buy_details_child> tempList = new List<t_account_shares_conditiontrade_buy_details_child>();
                                foreach (var buy in template_buy)
                                {
                                    long conditionPrice = 0;
                                    long EntrustAmount = buy.EntrustAmountType == 0 ? (long)(totalDeposit * 1.0 / 10000 * buy.EntrustAmount) : buy.EntrustAmount;
                                    int ConditionRelativeRate = buy.ConditionPriceRate ?? 0;
                                    int ConditionRelativeType = buy.ConditionPriceType ?? 0;
                                    int ConditionPriceBase = buy.ConditionPriceBase ?? 0;
                                    int ConditionType = buy.ConditionType;
                                    if (ConditionType == 1)
                                    {
                                        long basePrice = ConditionPriceBase == 1 ? closedPrice : currPrice;
                                        conditionPrice = ConditionRelativeType == 1 ? ((long)Math.Round(basePrice / 100 + basePrice / 100 * 1.0 / 10000 * range + ConditionRelativeRate)) * 100 : ConditionRelativeType == 2 ? ((long)Math.Round(basePrice / 100 - basePrice / 100 * 1.0 / 10000 * range + ConditionRelativeRate)) * 100 : ((long)Math.Round(basePrice / 100 + basePrice / 100 * 1.0 / 10000 * ConditionRelativeRate)) * 100;
                                    }
                                    t_account_shares_conditiontrade_buy_details buy_details = new t_account_shares_conditiontrade_buy_details
                                    {
                                        ToBuyId = temp.ToBuyId,
                                        Status = buy.Status,
                                        CreateTime = DateTime.Now,
                                        EntrustId = 0,
                                        LastModified = DateTime.Now,
                                        EntrustPriceGear = buy.EntrustPriceGear,
                                        ConditionType = ConditionType,
                                        EntrustType = buy.EntrustType,
                                        ForbidType = buy.ForbidType,
                                        FollowType = temp.FollowType,
                                        Name = buy.Name,
                                        TriggerTime = null,
                                        ConditionRelativeType = ConditionRelativeType,
                                        ConditionRelativeRate = ConditionRelativeRate,
                                        SourceFrom = 1,
                                        CreateAccountId = temp.AccountId,
                                        BuyAuto = buy.BuyAuto,
                                        IsGreater = buy.IsGreater,
                                        ConditionId = shares_buy_id,
                                        BusinessStatus = 0,
                                        ExecStatus = 0,
                                        LimitUp = buy.LimitUp,
                                        IsHold = buy.LimitUp,
                                        EntrustAmount = EntrustAmount,
                                        ConditionPrice = conditionPrice,
                                        OtherConditionRelative = buy.OtherConditionRelative
                                    };
                                    db.t_account_shares_conditiontrade_buy_details.Add(buy_details);
                                    db.SaveChanges();
                                    relIdList.Add(new RelIdInfo
                                    {
                                        TemplateId = buy.Id,
                                        RelId = buy_details.Id
                                    });
                                    //child
                                    var temp_child = template_buy_child.Where(e => e.FatherId == buy.Id).ToList();
                                    foreach (var x in temp_child)
                                    {
                                        tempList.Add(new t_account_shares_conditiontrade_buy_details_child
                                        {
                                            Status = x.Status,
                                            ConditionId = buy_details.Id,
                                            ChildId = x.ChildId,
                                        });
                                    }
                                    //添加跟投
                                    foreach (var followId in temp.FollowList)
                                    {
                                        db.t_account_shares_conditiontrade_buy_details_follow.Add(new t_account_shares_conditiontrade_buy_details_follow
                                        {
                                            CreateTime = DateTime.Now,
                                            FollowAccountId = followId,
                                            DetailsId = buy_details.Id
                                        });
                                    }
                                    db.SaveChanges();

                                    //额外参数
                                    var other = template_buy_other.Where(e => e.TemplateBuyId == buy.Id).ToList();
                                    var otherIdList = other.Select(e => e.Id).ToList();
                                    var trend = template_buy_other_trend.Where(e => otherIdList.Contains(e.OtherId)).ToList();
                                    var trendIdList = trend.Select(e => e.Id).ToList();
                                    var par = template_buy_other_trend_par.Where(e => trendIdList.Contains(e.OtherTrendId)).ToList();
                                    var trend_other = template_buy_other_trend_other.Where(e => trendIdList.Contains(e.OtherTrendId)).ToList();
                                    var trend_otherIdList = trend_other.Select(e => e.Id).ToList();
                                    var trend_other_par = template_buy_other_trend_other_par.Where(e => trend_otherIdList.Contains(e.OtherTrendOtherId)).ToList();
                                    foreach (var o in other)
                                    {
                                        t_account_shares_conditiontrade_buy_details_other otherTemp = new t_account_shares_conditiontrade_buy_details_other
                                        {
                                            Status = o.Status,
                                            CreateTime = DateTime.Now,
                                            DetailsId = buy_details.Id,
                                            LastModified = DateTime.Now,
                                            Name = o.Name
                                        };
                                        db.t_account_shares_conditiontrade_buy_details_other.Add(otherTemp);
                                        db.SaveChanges();
                                        var trendList = trend.Where(e => e.OtherId == o.Id).ToList();
                                        foreach (var t in trendList)
                                        {
                                            t_account_shares_conditiontrade_buy_details_other_trend trendTemp = new t_account_shares_conditiontrade_buy_details_other_trend
                                            {
                                                OtherId = otherTemp.Id,
                                                LastModified = DateTime.Now,
                                                CreateTime = DateTime.Now,
                                                Status = t.Status,
                                                TrendDescription = t.TrendDescription,
                                                TrendId = t.TrendId,
                                                TrendName = t.TrendName
                                            };
                                            db.t_account_shares_conditiontrade_buy_details_other_trend.Add(trendTemp);
                                            db.SaveChanges();
                                            var parList = (from x in par
                                                           where x.OtherTrendId == t.Id
                                                           select new t_account_shares_conditiontrade_buy_details_other_trend_par
                                                           {
                                                               CreateTime = DateTime.Now,
                                                               OtherTrendId = trendTemp.Id,
                                                               LastModified = DateTime.Now,
                                                               ParamsInfo = x.ParamsInfo
                                                           }).ToList();
                                            db.t_account_shares_conditiontrade_buy_details_other_trend_par.AddRange(parList);
                                            db.SaveChanges();
                                            var temp_trend_otherList = trend_other.Where(e => e.OtherTrendId == t.Id).ToList();
                                            foreach (var t2 in temp_trend_otherList)
                                            {
                                                t_account_shares_conditiontrade_buy_details_other_trend_other othertrendTemp = new t_account_shares_conditiontrade_buy_details_other_trend_other
                                                {
                                                    Status = t2.Status,
                                                    CreateTime = DateTime.Now,
                                                    LastModified = DateTime.Now,
                                                    OtherTrendId = trendTemp.Id,
                                                    TrendDescription = t2.TrendDescription,
                                                    TrendId = t2.TrendId,
                                                    TrendName = t2.TrendName
                                                };
                                                db.t_account_shares_conditiontrade_buy_details_other_trend_other.Add(othertrendTemp);
                                                db.SaveChanges();
                                                var other_parList = (from x in trend_other_par
                                                                     where x.OtherTrendOtherId == t2.Id
                                                                     select new t_account_shares_conditiontrade_buy_details_other_trend_other_par
                                                                     {
                                                                         CreateTime = DateTime.Now,
                                                                         OtherTrendOtherId = othertrendTemp.Id,
                                                                         LastModified = DateTime.Now,
                                                                         ParamsInfo = x.ParamsInfo
                                                                     }).ToList();
                                                db.t_account_shares_conditiontrade_buy_details_other_trend_other_par.AddRange(other_parList);
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                    //转自动参数
                                    var auto = template_buy_auto.Where(e => e.TemplateBuyId == buy.Id).ToList();
                                    var autoIdList = auto.Select(e => e.Id).ToList();
                                    var autotrend = template_buy_auto_trend.Where(e => autoIdList.Contains(e.AutoId)).ToList();
                                    var autotrendIdList = autotrend.Select(e => e.Id).ToList();
                                    var autopar = template_buy_auto_trend_par.Where(e => autotrendIdList.Contains(e.AutoTrendId)).ToList();
                                    var autotrend_other = template_buy_auto_trend_other.Where(e => autotrendIdList.Contains(e.AutoTrendId)).ToList();
                                    var autotrend_otherIdList = autotrend_other.Select(e => e.Id).ToList();
                                    var autotrend_other_par = template_buy_auto_trend_other_par.Where(e => autotrend_otherIdList.Contains(e.AutoTrendOtherId)).ToList();
                                    foreach (var o in auto)
                                    {
                                        t_account_shares_conditiontrade_buy_details_auto autoTemp = new t_account_shares_conditiontrade_buy_details_auto
                                        {
                                            Status = o.Status,
                                            CreateTime = DateTime.Now,
                                            DetailsId = buy_details.Id,
                                            LastModified = DateTime.Now,
                                            Name = o.Name
                                        };
                                        db.t_account_shares_conditiontrade_buy_details_auto.Add(autoTemp);
                                        db.SaveChanges();
                                        var autotrendList = autotrend.Where(e => e.AutoId == o.Id).ToList();
                                        foreach (var t in autotrendList)
                                        {
                                            t_account_shares_conditiontrade_buy_details_auto_trend autotrendTemp = new t_account_shares_conditiontrade_buy_details_auto_trend
                                            {
                                                AutoId = autoTemp.Id,
                                                LastModified = DateTime.Now,
                                                CreateTime = DateTime.Now,
                                                Status = t.Status,
                                                TrendDescription = t.TrendDescription,
                                                TrendId = t.TrendId,
                                                TrendName = t.TrendName
                                            };
                                            db.t_account_shares_conditiontrade_buy_details_auto_trend.Add(autotrendTemp);
                                            db.SaveChanges();
                                            var autoparList = (from x in autopar
                                                               where x.AutoTrendId == t.Id
                                                               select new t_account_shares_conditiontrade_buy_details_auto_trend_par
                                                               {
                                                                   CreateTime = DateTime.Now,
                                                                   AutoTrendId = autotrendTemp.Id,
                                                                   LastModified = DateTime.Now,
                                                                   ParamsInfo = x.ParamsInfo
                                                               }).ToList();
                                            db.t_account_shares_conditiontrade_buy_details_auto_trend_par.AddRange(autoparList);
                                            db.SaveChanges();
                                            var temp_autotrend_otherList = autotrend_other.Where(e => e.AutoTrendId == t.Id).ToList();
                                            foreach (var t2 in temp_autotrend_otherList)
                                            {
                                                t_account_shares_conditiontrade_buy_details_auto_trend_other othertrendTemp = new t_account_shares_conditiontrade_buy_details_auto_trend_other
                                                {
                                                    Status = t2.Status,
                                                    CreateTime = DateTime.Now,
                                                    LastModified = DateTime.Now,
                                                    AutoTrendId = autotrendTemp.Id,
                                                    TrendDescription = t2.TrendDescription,
                                                    TrendId = t2.TrendId,
                                                    TrendName = t2.TrendName
                                                };
                                                db.t_account_shares_conditiontrade_buy_details_auto_trend_other.Add(othertrendTemp);
                                                db.SaveChanges();
                                                var other_parList = (from x in autotrend_other_par
                                                                     where x.AutoTrendOtherId == t2.Id
                                                                     select new t_account_shares_conditiontrade_buy_details_auto_trend_other_par
                                                                     {
                                                                         CreateTime = DateTime.Now,
                                                                         AutoTrendOtherId = othertrendTemp.Id,
                                                                         LastModified = DateTime.Now,
                                                                         ParamsInfo = x.ParamsInfo
                                                                     }).ToList();
                                                db.t_account_shares_conditiontrade_buy_details_auto_trend_other_par.AddRange(other_parList);
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                                foreach (var x in tempList)
                                {
                                    x.ChildId = relIdList.Where(e => e.TemplateId == x.ChildId).Select(e => e.RelId).FirstOrDefault();
                                    db.t_account_shares_conditiontrade_buy_details_child.Add(x);
                                }
                                db.SaveChanges();
                                tran.Commit();
                                lock (dataLock)
                                {
                                    successList.Add(new SharesBase
                                    {
                                        Market = market,
                                        SharesCode = sharesCode
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteFileLog("导入买入模板出错", ex);
                                tran.Rollback();
                            }
                        }
                    }
                });
                tArr[i].Start();
            }
            Task.WaitAll(tArr);

            return successList;
        }

        //记录日志
        public static void WriteToLogRecord(List<SharesAutoJoinExportInfo> list)
        {
            using (var db = new meal_ticketEntities())
            {
                foreach (var item in list)
                {
                    foreach (var item2 in item.SharesList)
                    {
                        db.t_account_shares_auto_join_tobuy_record.Add(new t_account_shares_auto_join_tobuy_record
                        {
                            SharesCode = item2.SharesCode,
                            Market = item2.Market,
                            AccountId = item.AccountId,
                            ToBuyId = item.ToBuyId,
                            CreateTime = DateTime.Now
                        });
                    }
                }
                db.SaveChanges();
            }
        }

        public static void SearchMonitor()
        {
            TrendHandler handler = new TrendHandler();
            using (var db = new meal_ticketEntities())
            {
                var searchTemplate = (from item in db.t_sys_conditiontrade_template
                                      join item2 in db.t_sys_conditiontrade_template_search on item.Id equals item2.TemplateId
                                      where item.Status == 1 && item.Type == 5
                                      select item2).ToList();
                Dictionary<long, List<SharesBase_Session>> totalDic = new Dictionary<long, List<SharesBase_Session>>();
                foreach (var item in searchTemplate)
                {
                    List<dynamic> searchList = JsonConvert.DeserializeObject<List<dynamic>>(item.TemplateContent);
                    List<searchInfo> searchPar = new List<searchInfo>();
                    foreach (var x in searchList)
                    {
                        int connect = x.connect;
                        int leftbracket = string.IsNullOrEmpty(Convert.ToString(x.leftbracket))?0:Convert.ToInt32(x.leftbracket);
                        int rightbracket = string.IsNullOrEmpty(Convert.ToString(x.rightbracket)) ? 0 : Convert.ToInt32(x.rightbracket);
                        int type = x.type;
                        searchPar.Add(new searchInfo
                        {
                            connect = connect,
                            content = JsonConvert.SerializeObject(x.content),
                            leftbracket= leftbracket,
                            rightbracket= rightbracket,
                            type=type
                        });
                    }
                    var result = handler.GetEligibleSharesBatch(searchPar);
                    totalDic.Add(item.TemplateId, result.Distinct().ToList());
                }

                DateTime dateNow = Helper.GetLastTradeDate(-9, 0, 0);
                foreach (var dic in totalDic)
                {
                    var accountSetting = (from item in db.t_search_tri_setting
                                          where item.TemplateId == dic.Key && item.Type == -13
                                          select item.AccountId).ToList();
                    var tri = from item in db.t_search_tri
                              where item.LastPushTime > dateNow
                              select item;
                    var search_tri = (from item in db.t_account_baseinfo
                                      join item2 in tri on item.Id equals item2.AccountId
                                      where item.Status == 1 && item.MonitorStatus == 1 && accountSetting.Contains(item.Id)
                                      select new
                                      {
                                          AccountId = item.Id,
                                          Market = item2.Market,
                                          SharesCode = item2.SharesCode,
                                          LastPushTime = item2.LastPushTime,
                                          LastPushMaxRate = item2.LastPushMaxRate,
                                          LastPushMinRate = item2.LastPushMinRate,
                                          LastPushRate=item2.LastPushRate,
                                          MinPushTimeInterval = item2.MinPushTimeInterval,
                                          MinPushMaxRateInterval = item2.MinPushMaxRateInterval,
                                          MinPushMinRateInterval = item2.MinPushMinRateInterval,
                                          MinPushRiseRateInterval=item2.MinPushRiseRateInterval,
                                          MinPushDownRateInterval=item2.MinPushDownRateInterval,
                                          TriCount = item2.TriCount
                                      }).ToList();
                    var sharesList = (from item in dic.Value
                                      join item2 in Singleton.Instance._SharesQuotesSession.GetSessionData() on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                      where item2.ClosedPrice > 0 && item2.PresentPrice > 0
                                      select new
                                      {
                                          Market = item.Market,
                                          SharesCode = item.SharesCode,
                                          Rate = (int)((item2.PresentPrice - item2.ClosedPrice) * 10000.0 / item2.ClosedPrice)
                                      }).ToList();
                    var search_tri_dic = search_tri.ToDictionary(k => k.AccountId * 10000000 + int.Parse(k.SharesCode) * 10 + k.Market, v => v);
                    var resultList = new List<dynamic>();
                    foreach (var item in sharesList)
                    {
                        foreach (var item2 in accountSetting)
                        {
                            long key = item2 * 10000000 + int.Parse(item.SharesCode) * 10 + item.Market;
                            if (!search_tri_dic.ContainsKey(key))
                            {
                                resultList.Add(new
                                {
                                    AccountId = item2,
                                    Market = item.Market,
                                    SharesCode = item.SharesCode,
                                    LastPushTime = DateTime.Now,
                                    LastPushMaxRate = item.Rate,
                                    LastPushMinRate = item.Rate,
                                    LastPushRate= item.Rate,
                                    LastPushType = 0,
                                    TriCount = 1
                                });
                                continue;
                            }

                            var temp = search_tri_dic[key];
                            if (item.Rate >= temp.LastPushMaxRate + temp.MinPushMaxRateInterval)
                            {
                                resultList.Add(new
                                {
                                    AccountId = item2,
                                    Market = item.Market,
                                    SharesCode = item.SharesCode,
                                    LastPushTime = DateTime.Now,
                                    LastPushMaxRate = item.Rate,
                                    LastPushMinRate = temp.LastPushMinRate,
                                    LastPushRate = item.Rate,
                                    LastPushType = 1,
                                    TriCount = temp.TriCount + 1
                                });
                            }
                            else if (item.Rate <= temp.LastPushMinRate + temp.MinPushMinRateInterval)
                            {
                                resultList.Add(new
                                {
                                    AccountId = item2,
                                    Market = item.Market,
                                    SharesCode = item.SharesCode,
                                    LastPushTime = DateTime.Now,
                                    LastPushMaxRate = temp.LastPushMaxRate,
                                    LastPushMinRate = item.Rate,
                                    LastPushRate = item.Rate,
                                    LastPushType = 2,
                                    TriCount = temp.TriCount + 1
                                });
                            }
                            else if (item.Rate >= temp.LastPushRate + temp.MinPushRiseRateInterval)
                            {
                                resultList.Add(new
                                {
                                    AccountId = item2,
                                    Market = item.Market,
                                    SharesCode = item.SharesCode,
                                    LastPushTime = DateTime.Now,
                                    LastPushMaxRate = temp.LastPushMaxRate,
                                    LastPushMinRate = temp.LastPushMinRate,
                                    LastPushRate = item.Rate,
                                    LastPushType = 3,
                                    TriCount = temp.TriCount + 1
                                });
                            }
                            else if (item.Rate <= temp.LastPushRate + temp.MinPushDownRateInterval)
                            {
                                resultList.Add(new
                                {
                                    AccountId = item2,
                                    Market = item.Market,
                                    SharesCode = item.SharesCode,
                                    LastPushTime = DateTime.Now,
                                    LastPushMaxRate = temp.LastPushMaxRate,
                                    LastPushMinRate = temp.LastPushMinRate,
                                    LastPushRate = item.Rate,
                                    LastPushType = 4,
                                    TriCount = temp.TriCount + 1
                                });
                            }
                        }
                    }
                    SearchMonitor_ToDataBase(resultList);
                }
            }
        }

        private static void SearchMonitor_ToDataBase(List<dynamic> list)
        {
            DataTable table = new DataTable();
            #region====定义表字段数据类型====
            table.Columns.Add("AccountId", typeof(long));
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("LastPushTime", typeof(DateTime));
            table.Columns.Add("LastPushMaxRate", typeof(int));
            table.Columns.Add("LastPushMinRate", typeof(int));
            table.Columns.Add("LastPushRate", typeof(int));
            table.Columns.Add("LastPushType", typeof(int));
            table.Columns.Add("TriCount", typeof(int)); 
            #endregion

            #region====绑定数据====
            foreach (var item in list)
            {
                DataRow row = table.NewRow();
                row["AccountId"] = item.AccountId;
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["LastPushTime"] = item.LastPushTime;
                row["LastPushMaxRate"] = item.LastPushMaxRate;
                row["LastPushMinRate"] = item.LastPushMinRate;
                row["LastPushRate"] = item.LastPushRate;
                row["LastPushType"] = item.LastPushType;
                row["TriCount"] = item.TriCount;
                table.Rows.Add(row);
            }
            #endregion

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@searchMonitorList", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SearchMonitor";
                    //赋值
                    parameter.Value = table;
                    db.Database.ExecuteSqlCommand("exec P_SearchMonitor_Update @searchMonitorList", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("搜索结果入库失败了", ex);
                    tran.Rollback();
                }
            }
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

    public class SharesAutoJoinInfo
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public long ToBuyId { get; set; }

        public string ToBuyName { get; set; }
    }

    public class SharesAutoJoinPar : SharesAutoJoinInfo
    {
        public List<SharesAutoJoinOther> OtherList { get; set; }
    }

    public class SharesAutoJoinOther 
    {
        public long OtherId { get; set; }

        public string OtherName { get; set; }

        public List<SharesAutoJoinOtherTrend> TrendList { get; set; }
    }

    public class SharesAutoJoinOtherTrend
    {
        public long TrendId { get; set; }

        public string TrendName { get; set; }

        public string TrendDescription { get; set; }

        public List<string> ParList { get; set; }

        public List<SharesAutoJoinOtherTrend> OtherTrendList { get; set; }
    }

    public class SharesAutoJoinExportInfo
    {
        public long ToBuyId { get; set; }
        public long AccountId { get; set; }
        public long TemplateId { get; set; }
        public int Type { get; set; }
        public bool IsClear { get; set; }
        public List<SharesBase> SharesList { get; set; }
        public int FollowType { get; set; }
        public List<long> FollowList { get; set; }
        public List<long> GroupList { get; set; }
    }

    public class SharesBase 
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public List<bool> IsSuccess { get; set; }
    }
}
