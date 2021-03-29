using FXCommon.Common;
using MealTicket_Web_Handler.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
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
                                 join item3 in db.t_shares_quotes on new { item2.Market, item2.SharesCode } equals new { item3.Market, item3.SharesCode }
                                 where item.TradeType == 2 && item.TriggerTime == null && item2.Status == 1 && SqlFunctions.DateAdd("MI", 1, item3.LastModified) > timeNow && item3.PresentPrice > 0 && item3.ClosedPrice>0 && item.Status == 1
                                 group new { item, item2, item3 } by new { item.HoldId, item.Type,item2,item3 } into g
                                 select g).ToList();
                foreach (var item in condition)
                {
                    //可卖数量为0不执行
                    if (item.Key.item2.RemainCount > 0 && item.Key.item2.CanSoldCount <= 0)
                    {
                        break;
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
                        break;
                    }
                    else
                    {
                        maxPrice = ((long)Math.Round((item.Key.item3.ClosedPrice + item.Key.item3.ClosedPrice * (rules.Range * 1.0 / 10000)) / 100)) * 100;
                        minPrice = ((long)Math.Round((item.Key.item3.ClosedPrice - item.Key.item3.ClosedPrice * (rules.Range * 1.0 / 10000)) / 100)) * 100;
                    }
                    var holdList = item.ToList();
                    if (item.Key.Type == 1)
                    {
                        holdList = holdList.Where(e => e.item.ConditionTime != null && e.item.ConditionTime < timeNow).OrderBy(e => e.item.ConditionTime).ToList();
                    }
                    else if (item.Key.Type == 2)
                    {
                        holdList = (from hold in holdList
                                    let disPrice = hold.item.ConditionType == 1 ? hold.item.ConditionPrice : (hold.item.ConditionRelativeType == 1 ? maxPrice : hold.item.ConditionRelativeType == 2 ? minPrice : (long)(Math.Round((hold.item.ConditionRelativeRate * 1.0 / 10000 * hold.item3.ClosedPrice + hold.item3.ClosedPrice) / 100) * 100))
                                    where hold.item3.PresentPrice >= disPrice
                                    orderby disPrice
                                    select hold).ToList();
                    }
                    else if (item.Key.Type == 3)
                    {
                        holdList = (from hold in holdList
                                    let disPrice = hold.item.ConditionType == 1 ? hold.item.ConditionPrice : (hold.item.ConditionRelativeType == 1 ? maxPrice : hold.item.ConditionRelativeType == 2 ? minPrice : (long)(Math.Round((hold.item.ConditionRelativeRate * 1.0 / 10000 * hold.item3.ClosedPrice + hold.item3.ClosedPrice) / 100) * 100))
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
                                tran.Rollback();
                                break;
                            }
                        }

                        int EntrustCount = hold.item.EntrustCount;
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

                        long sellId = 0;
                        using (var tran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                                ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                                ObjectParameter sellIdDb = new ObjectParameter("sellId", 0);
                                db.P_ApplyTradeSell(hold.item.AccountId, hold.item.HoldId, EntrustCount, hold.item.EntrustType, EntrustPrice, 0, hold.item.SourceFrom == 1 ? true : false, errorCodeDb, errorMessageDb, sellIdDb);
                                int errorCode = (int)errorCodeDb.Value;
                                string errorMessage = errorMessageDb.Value.ToString();
                                if (errorCode != 0)
                                {
                                    throw new Exception(errorMessage);
                                }
                                sellId = (long)sellIdDb.Value;

                                var entrustManager = (from x in db.t_account_shares_entrust_manager
                                                      join x2 in db.t_broker_account_info on x.TradeAccountCode equals x2.AccountCode
                                                      where x.BuyId == sellId && x.TradeType == 2 && x.Status == 1
                                                      select new { x, x2 }).ToList();
                                if (entrustManager.Count() <= 0)
                                {
                                    throw new Exception("内部错误");
                                }
                                foreach (var x in entrustManager)
                                {
                                    var sendData = new
                                    {
                                        SellManagerId = x.x.Id,
                                        SellTime = DateTime.Now.ToString("yyyy-MM-dd")
                                    };
                                    var server = (from y in db.t_server_broker_account_rel
                                                  join y2 in db.t_server on y.ServerId equals y2.ServerId
                                                  where y.BrokerAccountId == x.x2.Id
                                                  select y).FirstOrDefault();
                                    if (server == null)
                                    {
                                        throw new WebApiException(400, "服务器配置有误");
                                    }
                                    bool isSendSuccess = Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesSell", server.ServerId);
                                    if (!isSendSuccess)
                                    {
                                        throw new Exception("操作超时，请重新操作");
                                    }
                                }

                                tran.Commit();
                            }
                            catch (Exception ex)
                            {
                                sellId = 0;
                                tran.Rollback();
                                Logger.WriteFileLog("自动卖出提交失败", ex);
                            }
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
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                //查询价格条件达标的数据
                var disResult = (from item in db.t_account_shares_conditiontrade_buy_details
                                 join item2 in db.t_account_shares_conditiontrade_buy on item.ConditionId equals item2.Id
                                 join item3 in db.t_shares_quotes on new { item2.Market, item2.SharesCode } equals new { item3.Market, item3.SharesCode }
                                 where item.Status == 1 && item2.Status == 1 && item.BusinessStatus == 0
                                 select new { item, item2, item3 }).ToList();
                foreach (var item in disResult)
                {
                    long presentPrice = item.item3.PresentPrice;//当前价格
                    long closedPrice = item.item3.ClosedPrice;//昨日收盘价
                    if (closedPrice <= 0 || presentPrice <= 0)
                    {
                        continue;
                    }
                    long maxPrice = 0;//涨停价
                    long minPrice = 0;//跌停价
                    var rules = (from x in db.t_shares_limit_fundmultiple
                                 where (x.LimitMarket == item.item2.Market || x.LimitMarket == -1) && (item.item2.SharesCode.StartsWith(x.LimitKey))
                                 orderby x.Priority descending, x.FundMultiple
                                 select x).FirstOrDefault();
                    if (rules == null)
                    {
                        continue;
                    }
                    else
                    {
                        maxPrice = ((long)Math.Round((closedPrice + closedPrice * (rules.Range * 1.0 / 10000)) / 100)) * 100;
                        minPrice = ((long)Math.Round((closedPrice - closedPrice * (rules.Range * 1.0 / 10000)) / 100)) * 100;
                    }
                    //跌停价不买入
                    if (item.item.ForbidType == 1)
                    {
                        if (minPrice == item.item3.PresentPrice)
                        {
                            continue;
                        }
                    }

                    //判断价格条件
                    if (item.item.ConditionType == 1)//绝对价格
                    {
                        if ((presentPrice < item.item.ConditionPrice && item.item.IsGreater == true) || (presentPrice > item.item.ConditionPrice && item.item.IsGreater == false))
                        {
                            continue;
                        }
                    }
                    else if (item.item.ConditionType == 2)//相对价格
                    {
                        if ((item.item.ConditionRelativeType == 1 && presentPrice != maxPrice) || (item.item.ConditionRelativeType == 2 && presentPrice != minPrice) || (item.item.ConditionRelativeType == 3 && presentPrice < ((long)Math.Round((closedPrice + closedPrice * (item.item.ConditionRelativeRate * 1.0 / 10000)) / 100)) * 100))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    //判断额外条件
                    bool isTri = true;
                    var other = (from x in db.t_account_shares_conditiontrade_buy_details_other
                                 where x.DetailsId == item.item.Id && x.Status == 1
                                 select x).ToList();
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
                            if (tr.TrendId == 1)//快速拉升 
                            {
                                List<TREND_RESULT_RAPID_UP> resultInfo_Trend1 = new List<TREND_RESULT_RAPID_UP>();
                                int errorCode_Trend1 = DataHelper.Analysis_Trend1(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.item2.SharesCode,
                                        Market=item.item2.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend1);
                                if (errorCode_Trend1 == 0)
                                {
                                    var temp = resultInfo_Trend1.Where(e => e.strStockCode == (item.item2.SharesCode + "," + item.item2.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode) && temp.dicUpOrDownInfo.Count() > 0)
                                    {
                                        var tempModel = temp.dicUpOrDownInfo.FirstOrDefault();
                                        var pushTime = tempModel.Value.lastestInfo.dtTradeTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (tr.TrendId == 2)//多头向上 
                            {
                                List<TREND_RESULT_LINE_UP> resultInfo_Trend2 = new List<TREND_RESULT_LINE_UP>();
                                int errorCode_Trend2 = DataHelper.Analysis_Trend2(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.item2.SharesCode,
                                        Market=item.item2.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend2);
                                if (errorCode_Trend2 == 0)
                                {
                                    var temp = resultInfo_Trend2.Where(e => e.strStockCode == (item.item2.SharesCode + "," + item.item2.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode))
                                    {
                                        var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (tr.TrendId == 3)//箱体突破 
                            {
                                List<TREND_RESULT_BOX_BREACH> resultInfo_Trend3 = new List<TREND_RESULT_BOX_BREACH>();
                                int errorCode_Trend3 = DataHelper.Analysis_Trend3(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.item2.SharesCode,
                                        Market=item.item2.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend3);
                                if (errorCode_Trend3 == 0)
                                {
                                    var temp = resultInfo_Trend3.Where(e => e.strStockCode == (item.item2.SharesCode + "," + item.item2.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode))
                                    {
                                        var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (tr.TrendId == 4)//指定时间段 
                            {
                                var temp=JsonConvert.DeserializeObject<dynamic>(par[0]);
                                JArray timeList = temp.Times; 
                                if (timeNow >= DateTime.Parse(timeList[0].ToString()) && timeNow < DateTime.Parse(timeList[1].ToString()))
                                {
                                    tempTri = true;
                                    break;
                                }
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
                        continue;
                    }
                    //判断转自动条件
                    var auto = (from x in db.t_account_shares_conditiontrade_buy_details_auto
                                 where x.DetailsId == item.item.Id && x.Status == 1
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
                            if (tr.TrendId == 1)//快速拉升 
                            {
                                List<TREND_RESULT_RAPID_UP> resultInfo_Trend1 = new List<TREND_RESULT_RAPID_UP>();
                                int errorCode_Trend1 = DataHelper.Analysis_Trend1(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.item2.SharesCode,
                                        Market=item.item2.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend1);
                                if (errorCode_Trend1 == 0)
                                {
                                    var temp = resultInfo_Trend1.Where(e => e.strStockCode == (item.item2.SharesCode + "," + item.item2.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode) && temp.dicUpOrDownInfo.Count() > 0)
                                    {
                                        var tempModel = temp.dicUpOrDownInfo.FirstOrDefault();
                                        var pushTime = tempModel.Value.lastestInfo.dtTradeTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (tr.TrendId == 2)//多头向上 
                            {
                                List<TREND_RESULT_LINE_UP> resultInfo_Trend2 = new List<TREND_RESULT_LINE_UP>();
                                int errorCode_Trend2 = DataHelper.Analysis_Trend2(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.item2.SharesCode,
                                        Market=item.item2.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend2);
                                if (errorCode_Trend2 == 0)
                                {
                                    var temp = resultInfo_Trend2.Where(e => e.strStockCode == (item.item2.SharesCode + "," + item.item2.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode))
                                    {
                                        var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (tr.TrendId == 3)//箱体突破 
                            {
                                List<TREND_RESULT_BOX_BREACH> resultInfo_Trend3 = new List<TREND_RESULT_BOX_BREACH>();
                                int errorCode_Trend3 = DataHelper.Analysis_Trend3(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=item.item2.SharesCode,
                                        Market=item.item2.Market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend3);
                                if (errorCode_Trend3 == 0)
                                {
                                    var temp = resultInfo_Trend3.Where(e => e.strStockCode == (item.item2.SharesCode + "," + item.item2.Market)).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(temp.strStockCode))
                                    {
                                        var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                                        if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                                        {
                                            tempTri = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (tr.TrendId == 4)//指定时间段 
                            {
                                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                                JArray timeList = temp.Times;
                                if (timeNow >= DateTime.Parse(timeList[0].ToString()) && timeNow < DateTime.Parse(timeList[1].ToString()))
                                {
                                    tempTri = true;
                                    break;
                                }
                            }
                        }
                        if (!tempTri)
                        {
                            isTri = false;
                            break;
                        }
                    }

                    bool isAuto = item.item.BuyAuto;
                    if (!isAuto)
                    {
                        if (auto.Count() > 0 && isTri)//转自动交易成立
                        {
                            isAuto = true;
                        }
                        else
                        {
                            item.item.BusinessStatus = 1;
                            item.item.TriggerTime = DateTime.Now;
                            db.SaveChanges();
                        }
                    }

                    if (isAuto && isTri)//自动买入且条件满足
                    {
                        long EntrustPrice = 0;
                        if (item.item.EntrustType == 2)
                        {
                            switch (item.item.EntrustPriceGear)
                            {
                                case 1:
                                    EntrustPrice = item.item3.BuyPrice1 == 0 ? item.item3.PresentPrice : item.item3.BuyPrice1;
                                    break;
                                case 2:
                                    EntrustPrice = item.item3.BuyPrice2 == 0 ? item.item3.PresentPrice : item.item3.BuyPrice2;
                                    break;
                                case 3:
                                    EntrustPrice = item.item3.BuyPrice3 == 0 ? item.item3.PresentPrice : item.item3.BuyPrice3;
                                    break;
                                case 4:
                                    EntrustPrice = item.item3.BuyPrice4 == 0 ? item.item3.PresentPrice : item.item3.BuyPrice4;
                                    break;
                                case 5:
                                    EntrustPrice = item.item3.BuyPrice5 == 0 ? item.item3.PresentPrice : item.item3.BuyPrice5;
                                    break;
                                case 6:
                                    EntrustPrice = item.item3.SellPrice1 == 0 ? item.item3.PresentPrice : item.item3.SellPrice1;
                                    break;
                                case 7:
                                    EntrustPrice = item.item3.SellPrice2 == 0 ? item.item3.PresentPrice : item.item3.SellPrice2;
                                    break;
                                case 8:
                                    EntrustPrice = item.item3.SellPrice3 == 0 ? item.item3.PresentPrice : item.item3.SellPrice3;
                                    break;
                                case 9:
                                    EntrustPrice = item.item3.SellPrice4 == 0 ? item.item3.PresentPrice : item.item3.SellPrice4;
                                    break;
                                case 10:
                                    EntrustPrice = item.item3.SellPrice5 == 0 ? item.item3.PresentPrice : item.item3.SellPrice5;
                                    break;
                                case 11:
                                    EntrustPrice = maxPrice;
                                    break;
                                case 12:
                                    EntrustPrice = minPrice;
                                    break;
                            }
                        }

                        item.item.BusinessStatus = 3;
                        item.item.TriggerTime = DateTime.Now;
                        db.SaveChanges();

                        //查询跟投设置
                        var FollowList = (from x in db.t_account_shares_conditiontrade_buy_details_follow
                                          where x.DetailsId == item.item.Id
                                          select x.FollowAccountId).ToList();


                        //查询跟投人员
                        var followList = (from x in db.t_account_follow_rel
                                          join x2 in db.t_account_baseinfo on x.FollowAccountId equals x2.Id
                                          join x3 in db.t_account_wallet on x.FollowAccountId equals x3.AccountId
                                          where x.AccountId == item.item2.AccountId && x2.Status == 1
                                          select x3).ToList();

                        List<dynamic> buyList = new List<dynamic>();
                        buyList.Add(new
                        {
                            AccountId = item.item2.AccountId,
                            BuyAmount = item.item.EntrustAmount
                        });
                        //计算本人购买仓位比
                        var buyRateTemp = (from x in db.t_account_wallet
                                           where x.AccountId == item.item2.AccountId
                                           select x).FirstOrDefault();
                        if (buyRateTemp == null)
                        {
                            throw new WebApiException(400, "账户有误");
                        }
                        var buyRate = item.item.EntrustAmount * 1.0 / buyRateTemp.Deposit;//仓位占比
                        foreach (var account in FollowList)
                        {
                            var temp = followList.Where(e => e.AccountId == account).FirstOrDefault();
                            if (temp == null)
                            {
                                continue;
                            }
                            buyList.Add(new
                            {
                                AccountId = account,
                                BuyAmount = (long)(temp.Deposit * buyRate)
                            });
                        }

                        long mainEntrustId = 0;

                        List<dynamic> sendDataList = new List<dynamic>();
                        string error = "";
                        int i = 0;
                        foreach (var buy in buyList)
                        {
                            if (buy.BuyAmount <= 0)
                            {
                                continue;
                            }
                            long buyId = 0;
                            using (var tran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                                    ObjectParameter buyIdDb = new ObjectParameter("buyId", 0);
                                    db.P_ApplyTradeBuy(buy.AccountId, item.item2.Market, item.item2.SharesCode, buy.BuyAmount, 0, EntrustPrice, null, true, errorCodeDb, errorMessageDb, buyIdDb);
                                    int errorCode = (int)errorCodeDb.Value;
                                    string errorMessage = errorMessageDb.Value.ToString();
                                    if (errorCode != 0)
                                    {
                                        throw new WebApiException(errorCode, errorMessage);
                                    }
                                    buyId = (long)buyIdDb.Value;

                                    var entrust = (from x in db.t_account_shares_entrust
                                                   where x.Id == buyId
                                                   select x).FirstOrDefault();
                                    if (entrust == null)
                                    {
                                        throw new WebApiException(400, "未知错误");
                                    }
                                    if (buy.AccountId != item.item2.AccountId)//不是自己买，绑定跟买关系
                                    {
                                        db.t_account_shares_entrust_follow.Add(new t_account_shares_entrust_follow
                                        {
                                            CreateTime = DateTime.Now,
                                            MainAccountId = item.item2.AccountId,
                                            MainEntrustId = mainEntrustId,
                                            FollowAccountId = buy.AccountId,
                                            FollowEntrustId = entrust.Id,
                                        });
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        mainEntrustId = entrust.Id;
                                    }

                                    sendDataList.Add(new
                                    {
                                        BuyId = buyId,
                                        BuyCount = entrust.EntrustCount,
                                        BuyTime = DateTime.Now.ToString("yyyy-MM-dd")
                                    });

                                    tran.Commit();
                                    i++;
                                }
                                catch (Exception ex)
                                {
                                    error = ex.Message;
                                    Logger.WriteFileLog("购买失败", ex);
                                    tran.Rollback();
                                }
                            }

                            if (buy.AccountId == item.item2.AccountId)
                            {
                                item.item.EntrustId = buyId;
                                db.SaveChanges();
                            }
                        }
                        if (sendDataList.Count() <= 0)
                        {
                            throw new WebApiException(400, error);
                        }
                        bool isSendSuccess = Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new { type = 1, data = sendDataList })), "SharesBuy", "s1");
                        if (!isSendSuccess)
                        {
                            throw new WebApiException(400, "买入失败,请撤销重试");
                        }
                    }
                }
            }
        }
    }
}
