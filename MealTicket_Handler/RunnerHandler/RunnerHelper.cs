using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using FXCommon.Common;
using FXCommon.Database;
using Jiguang.JPush;
using MealTicket_Handler.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoticeHandler;
using SMS_SendHandler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using WxPayAPI;

namespace MealTicket_Handler.RunnerHandler
{
    public class RunnerHelper
    {
        /// <summary>
        /// 更新股票可卖出数量
        /// </summary>
        public static void JoinCanSold()
        {
            DateTime timeDate = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var entrustBuy = (from item in db.t_account_shares_entrust
                                  where item.CreateTime < timeDate && item.IsJoinCanSold == false && item.Status == 3 && item.TradeType == 1
                                  select item).ToList();
                foreach (var item in entrustBuy)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var hold = (from x in db.t_account_shares_hold
                                        where x.Id == item.HoldId
                                        select x).FirstOrDefault();
                            if (hold == null)
                            {
                                continue;
                            }
                            hold.CanSoldCount = hold.CanSoldCount + item.DealCount;
                            item.IsJoinCanSold = true;
                            db.SaveChanges();

                            var entrustManager = (from x in db.t_account_shares_entrust_manager
                                                  where x.BuyId == item.Id && x.TradeType == 1 && x.Status == 3
                                                  select x).ToList();
                            foreach (var item2 in entrustManager)
                            {
                                var holdManager = (from x in db.t_account_shares_hold_manager
                                                   where x.HoldId == item.HoldId && x.TradeAccountCode == item2.TradeAccountCode
                                                   select x).FirstOrDefault();
                                if (holdManager == null)
                                {
                                    continue;
                                }
                                holdManager.CanSoldCount = holdManager.CanSoldCount + item2.DealCount;

                                var accountShares = (from x in db.t_broker_account_shares_rel
                                                     where x.TradeAccountCode == item2.TradeAccountCode && x.Market == item.Market && x.SharesCode == item.SharesCode
                                                     select x).FirstOrDefault();
                                if (accountShares == null)
                                {
                                    continue;
                                }
                                accountShares.AccountCanSoldCount = accountShares.AccountCanSoldCount + item2.DealCount;
                                accountShares.SimulateCanSoldCount = accountShares.SimulateCanSoldCount + item2.SimulateDealCount;
                            }
                            db.SaveChanges();

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更新持仓计算服务费金额
        /// </summary>
        public static void JoinService()
        {
            DateTime timeDate = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                //服务费规则
                var holdServiceRules = (from item in db.t_system_param
                                        where item.ParamName == "HoldServiceRules"
                                        select item.ParamValue).FirstOrDefault();
                if (string.IsNullOrEmpty(holdServiceRules))
                {
                    return;
                }
                var rulesJson = JsonConvert.DeserializeObject<dynamic>(holdServiceRules);
                if (rulesJson == null)
                {
                    return;
                }
                if (rulesJson.Day == null)
                {
                    return;
                }
                int settingTradeDay = rulesJson.Day;//设置的收取服务费交易日天数

                //未交服务费购买委托
                var entrustBuy = (from item in db.t_account_shares_entrust
                                  where item.IsJoinService == false && item.Status == 3 && item.TradeType == 1
                                  select item).ToList();
                foreach (var item in entrustBuy)
                {
                    var accountPar = (from x in db.t_account_par_setting
                                      where x.AccountId == item.AccountId && x.ParamName == "HoldServiceRules" && x.Status == 1
                                      select x).FirstOrDefault();
                    if (accountPar != null)
                    {
                        var temp = JsonConvert.DeserializeObject<dynamic>(accountPar.ParamValue);
                        if (temp != null && temp.Day != null)
                        {
                            settingTradeDay = temp.Day;
                        }
                    }
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            DateTime tempDate = item.CreateTime.Date;
                            //交易日期天数
                            int tradeDays = 0;
                            do
                            {
                                if (CheckTradeDate(tempDate))
                                {
                                    tradeDays++;
                                }
                                tempDate = tempDate.AddDays(1);
                            } while (tempDate <= timeDate);
                            //需要交服务费
                            if (tradeDays > settingTradeDay)
                            {
                                var hold = (from x in db.t_account_shares_hold
                                            where x.Id == item.HoldId
                                            select x).FirstOrDefault();
                                if (hold == null)
                                {
                                    continue;
                                }
                                hold.ServiceAmount = hold.ServiceAmount + item.DealAmount;
                                item.IsJoinService = true;
                                db.SaveChanges();
                            }

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
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
        /// 检查当前是否交易时间
        /// </summary>
        /// <returns></returns>
        public static bool CheckTradeTime(DateTime? time = null)
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
            using (var db = new meal_ticketEntities())
            {
                var tradeTime = (from item in db.t_shares_limit_time
                                 select item).ToList();
                foreach (var item in tradeTime)
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    db.P_Check_TradeTime(item.LimitMarket, item.LimitKey, timeDis, errorCodeDb);
                    int errorCode = (int)errorCodeDb.Value;
                    if (errorCode == 0)
                    {
                        return true;
                    }
                }
                return false;
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
        /// 检查当前是否闭市时间
        /// </summary>
        /// <returns></returns>
        public static bool CheckTradeCloseTime(DateTime? time = null)
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
                    //解析time5
                    if (item.Time5 != null)
                    {
                        string[] timeArr = item.Time5.Split(',');
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
        /// 检查当前是否今日盈亏统计时间
        /// </summary>
        /// <returns></returns>
        public static bool CheckTodayProfitTime(DateTime timePar)
        {
            if (!CheckTradeDate(timePar.Date))
            {
                return false;
            }
            TimeSpan timeSpanNow = TimeSpan.Parse(timePar.ToString("HH:mm:ss"));
            using (var db = new meal_ticketEntities())
            {
                var times = (from item in db.t_system_param
                             where item.ParamName == "TodayProfitTimes"
                             select item).FirstOrDefault();
                if (times == null)
                {
                    return false;
                }
                string timesValue = times.ParamValue;
                string[] timeArr = timesValue.Split(',');
                foreach (var time in timeArr)
                {
                    var timeSpanArr = time.Split('-');
                    if (timeSpanArr.Length != 2)
                    {
                        continue;
                    }
                    TimeSpan timeStart = TimeSpan.Parse(timeSpanArr[0]);
                    TimeSpan timeEnd = TimeSpan.Parse(timeSpanArr[1]);
                    if (timeSpanNow >= timeStart && timeSpanNow <= timeEnd)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 收取服务费
        /// </summary>
        public static void ServiceFeeRecharge()
        {
            DateTime timeDate = DateTime.Now.Date;
            if (!RunnerHelper.CheckTradeDate())
            {
                return;
            }
            using (var db = new meal_ticketEntities())
            {
                //服务费规则
                var holdServiceRules = (from item in db.t_system_param
                                        where item.ParamName == "HoldServiceRules"
                                        select item.ParamValue).FirstOrDefault();
                if (string.IsNullOrEmpty(holdServiceRules))
                {
                    return;
                }
                var rulesJson = JsonConvert.DeserializeObject<dynamic>(holdServiceRules);
                if (rulesJson == null)
                {
                    return;
                }
                if (rulesJson.MinAmount == null || rulesJson.Rate == null)
                {
                    return;
                }
                long minAmount = rulesJson.MinAmount;
                int rate = rulesJson.Rate;

                var hold = (from item in db.t_account_shares_hold
                            where item.Status == 1 && item.ServiceAmount > 0 && item.LastServiceTime < timeDate
                            select item).ToList();
                foreach (var item in hold)
                {
                    var accountPar = (from x in db.t_account_par_setting
                                      where x.AccountId == item.AccountId && x.ParamName == "HoldServiceRules" && x.Status == 1
                                      select x).FirstOrDefault();
                    if (accountPar != null)
                    {
                        var temp = JsonConvert.DeserializeObject<dynamic>(accountPar.ParamValue);
                        if (temp != null && temp.MinAmount != null && temp.Rate != null)
                        {
                            minAmount = temp.MinAmount;
                            rate = temp.Rate;
                        }
                    }
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //计算服务费
                            long serviceFee = ((long)Math.Round(item.ServiceAmount * (rate * 1.0 / 100000), 0)) / 100 * 100;
                            item.LastServiceTime = timeDate;
                            item.ServiceFee = item.ServiceFee + serviceFee;
                            db.SaveChanges();
                            db.P_Calculate_DepositChange_Hold(item.Id, -serviceFee, "收取服务费");

                            ObjectParameter remainServiceFeeDb = new ObjectParameter("remainAmount", 0);
                            db.P_Calculate_Recommend_Prize(item.AccountId, 2, serviceFee, item.Id, 1062, remainServiceFeeDb);
                            var remainServiceFee = (long)remainServiceFeeDb.Value;
                            db.P_Calculate_WalletChange_Platform("serviceFee", serviceFee, remainServiceFee, item.Id);

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 自动平仓
        /// </summary>
        public static void ClosingAuto()
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                //查询需要自动平仓的持仓
                var hold = (from item in db.t_account_shares_hold
                            where item.Status == 1 && item.ClosingTime != null && item.ClosingTime <= timeNow && item.CanSoldCount > 0
                            select item).ToList();
                foreach (var item in hold)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                            ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                            ObjectParameter sellIdDb = new ObjectParameter("sellId", 0);
                            db.P_ApplyTradeSell(item.AccountId, item.Id, item.CanSoldCount, 1, 0, 1, false, errorCodeDb, errorMessageDb, sellIdDb);
                            int errorCode = (int)errorCodeDb.Value;
                            string errorMessage = errorMessageDb.Value.ToString();
                            if (errorCode != 0)
                            {
                                throw new Exception(errorMessage);
                            }
                            long sellId = (long)sellIdDb.Value;

                            var entrustManager = (from x in db.t_account_shares_entrust_manager
                                                  join x2 in db.t_broker_account_info on x.TradeAccountCode equals x2.AccountCode
                                                  where x.BuyId == sellId && x.TradeType == 2 && item.Status == 1
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
                                bool isSendSuccess = MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesSell", server.ServerId);
                                if (!isSendSuccess)
                                {
                                    throw new Exception("操作超时，请重新操作");
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
            }
        }

        /// <summary>
        /// 强制平仓
        /// </summary>
        public static void ClosingForce()
        {
            DateTime timeNow = DateTime.Now;
            TimeSpan timeSpanNow = TimeSpan.Parse(timeNow.ToString("HH:mm:ss"));
            using (var db = new meal_ticketEntities())
            {
                //查询需要强制平仓的持仓
                var hold = (from item in db.t_account_shares_hold
                            where item.Status == 1 && item.CanSoldCount > 0 && item.FundAmount > 0 && item.ClosingForceStatus != 2
                            select item).ToList();
                foreach (var item in hold)
                {
                    var shares = (from x in db.t_shares_all
                                  where x.SharesCode == item.SharesCode && item.Market == item.Market
                                  select x).FirstOrDefault();
                    if (shares == null)
                    {
                        continue;
                    }
                    if (string.IsNullOrEmpty(shares.SharesName))
                    {
                        continue;
                    }

                    long traderulesId = 0;
                    int closingLine = 0;
                    int cordon = 0;
                    int allDayClosingLine = 0;
                    int allDayCordon = 0;
                    TimeSpan? currStartTime = null;//当前起始时段
                    TimeSpan? currEndTime = null;//当前终止时段
                    try
                    {
                        GetCurrTimeClosingLine(item.Market, item.SharesCode, shares.SharesName, out traderulesId, out closingLine, out cordon, out currStartTime, out currEndTime, out allDayClosingLine, out allDayCordon);
                    }
                    catch (Exception) { }

                    long nextTraderulesId = 0;//下一时段额外Id
                    int nextClosingLine = 0;//下一时段额外平仓线
                    int nextCordon = 0;//下一时段额外警戒线
                    TimeSpan? nextStartTime = null;//下一时段额外起始时段
                    try
                    {
                        GetNextTimeClosingLine(item.Market, item.SharesCode, shares.SharesName, out nextTraderulesId, out nextClosingLine, out nextCordon, out nextStartTime);
                    }
                    catch (Exception) { }



                    if (traderulesId > 0)//当前属于额外时段
                    {
                        //判断下一时段是否存在额外时段
                        if (nextStartTime == null || nextStartTime > currEndTime)
                        {
                            nextTraderulesId = 0;
                            nextClosingLine = allDayClosingLine;
                            nextCordon = allDayCordon;
                            nextStartTime = currEndTime;
                        }
                    }


                    //警戒线低于平仓线，设置警戒线=平仓线加一个点
                    if (cordon <= closingLine)
                    {
                        cordon = closingLine + 100;
                    }

                    //计算当前市值
                    var quotes = (from x in db.t_shares_quotes
                                  where x.Market == item.Market && x.SharesCode == item.SharesCode && x.LastModified > SqlFunctions.DateAdd("MI", -1, timeNow)
                                  select x).FirstOrDefault();
                    if (quotes == null)
                    {
                        continue;
                    }
                    if (quotes.PresentPrice <= 0)
                    {
                        continue;
                    }
                    long marketValue = item.RemainCount * quotes.PresentPrice;

                    //判断是否需要强制平仓（(市值+保证金-借钱）/借钱*100>平仓线）
                    int currClosingLine = (int)((marketValue + item.RemainDeposit - item.FundAmount) * 1.0 / item.FundAmount * 10000);

                    //判断当前是否达到警戒线，达到判断发送通知
                    if (currClosingLine < cordon)
                    {
                        bool isSend = true;
                        try
                        {
                            //判断当前时段是否已经发送过通知
                            var noticeHold = (from x in db.t_notice_hold
                                              where x.HoldId == item.Id && x.Type == 1
                                              select x).FirstOrDefault();
                            if (noticeHold == null)
                            {
                                db.t_notice_hold.Add(new t_notice_hold
                                {
                                    HoldId = item.Id,
                                    Type = 1,
                                    TimeId = traderulesId,
                                    LastNotifyTime = timeNow
                                });
                                db.SaveChanges();
                            }
                            else if (noticeHold.TimeId != traderulesId)
                            {
                                noticeHold.TimeId = traderulesId;
                                noticeHold.LastNotifyTime = timeNow;
                                db.SaveChanges();
                            }
                            else if (noticeHold.LastNotifyTime.Value < timeNow.Date)
                            {
                                noticeHold.LastNotifyTime = timeNow;
                                db.SaveChanges();
                            }
                            else
                            {
                                isSend = false;
                            }

                            if (isSend)
                            {
                                //计算建议添加保证金
                                long needDepositAmount = (long)(cordon * 1.0 / 10000 * item.FundAmount + item.FundAmount - quotes.PresentPrice * item.RemainCount);
                                var tempPara = JsonConvert.SerializeObject(new
                                {
                                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    sharescode = item.SharesCode,
                                    sharesname = shares.SharesName,
                                    adddeposit = needDepositAmount - item.RemainDeposit <= 0 ? 0 : needDepositAmount - item.RemainDeposit
                                });
                                NoticeSender.SendExecute("NoticeSend.OverCordon", item.AccountId, tempPara);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("警戒线通知异常", ex);
                        }
                    }

                    //判断下一个时段是否达到平仓线
                    if (currClosingLine < nextClosingLine)
                    {
                        bool isSend = true;
                        try
                        {
                            //判断当前时段是否已经发送过通知
                            var noticeHold = (from x in db.t_notice_hold
                                              where x.HoldId == item.Id && x.Type == 2
                                              select x).FirstOrDefault();
                            if (noticeHold == null)
                            {
                                db.t_notice_hold.Add(new t_notice_hold
                                {
                                    HoldId = item.Id,
                                    Type = 2,
                                    TimeId = nextTraderulesId,
                                    LastNotifyTime = timeNow
                                });
                                db.SaveChanges();
                            }
                            else if (noticeHold.TimeId != nextTraderulesId)
                            {
                                noticeHold.TimeId = nextTraderulesId;
                                noticeHold.LastNotifyTime = timeNow;
                                db.SaveChanges();
                            }
                            else if (noticeHold.LastNotifyTime.Value < timeNow.Date)
                            {
                                noticeHold.LastNotifyTime = timeNow;
                                db.SaveChanges();
                            }
                            else
                            {
                                isSend = false;
                            }

                            if (isSend)
                            {
                                //计算建议添加保证金
                                long needDepositAmount = (long)(nextCordon * 1.0 / 10000 * item.FundAmount + item.FundAmount - quotes.PresentPrice * item.RemainCount);
                                long closingPrice = item.RemainCount <= 0 ? 0 : (long)(nextClosingLine * 1.0 / 10000 * item.FundAmount + item.FundAmount - item.RemainDeposit) / item.RemainCount;
                                var tempPara = JsonConvert.SerializeObject(new
                                {
                                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    sharescode = item.SharesCode,
                                    sharesname = shares.SharesName,
                                    times = nextStartTime.Value,
                                    closingprice = closingPrice,
                                    adddeposit = needDepositAmount - item.RemainDeposit <= 0 ? 0 : needDepositAmount - item.RemainDeposit
                                });
                                NoticeSender.SendExecute("NoticeSend.ClosingForceSoon", item.AccountId, tempPara);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("警戒线通知异常", ex);
                        }
                    }

                    //判断是否强制平仓
                    if (currClosingLine > closingLine)
                    {
                        if (item.ForceSellingCount <= 0 && item.ClosingForceStatus != 0)
                        {
                            item.ClosingForceStatus = 0;
                            item.ClosingForceTime = null;
                            db.SaveChanges();

                        }
                        continue;
                    }
                    if (!CheckTradeTime2(timeNow, false, true, true))//不在time3和time4中间
                    {
                        continue;
                    }


                    //设置需要强制平仓
                    if (item.ClosingForceStatus == 0)
                    {
                        item.ClosingForceStatus = 1;
                        item.ClosingForceTime = timeNow;
                        db.SaveChanges();
                    }
                    if (item.SellingCount > 0)//正在挂单数量大于0 ，先撤单
                    {
                        var entrustList = (from x in db.t_account_shares_entrust
                                           where x.HoldId == item.Id && x.TradeType == 2 && x.Status == 2
                                           select x).ToList();
                        foreach (var entrust in entrustList)
                        {
                            using (var tran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                                    db.P_ApplyTradeCancel(item.AccountId, entrust.Id, errorCodeDb, errorMessageDb);
                                    int errorCode = (int)errorCodeDb.Value;
                                    string errorMessage = errorMessageDb.Value.ToString();
                                    if (errorCode != 0)
                                    {
                                        throw new Exception(errorMessage);
                                    }

                                    var entrustManager = (from x in db.t_account_shares_entrust_manager
                                                          join x2 in db.t_broker_account_info on x.TradeAccountCode equals x2.AccountCode
                                                          where x.BuyId == item.Id
                                                          select new { x, x2 }).ToList();
                                    if (entrustManager.Count() <= 0)
                                    {
                                        throw new Exception("内部错误");
                                    }
                                    foreach (var x in entrustManager)
                                    {
                                        var sendData = new
                                        {
                                            TradeManagerId = x.x.Id,
                                            CancelTime = DateTime.Now.ToString("yyyy-MM-dd")
                                        };
                                        var server = (from y in db.t_server_broker_account_rel
                                                      join y2 in db.t_server on y.ServerId equals y2.ServerId
                                                      where y.BrokerAccountId == x.x2.Id
                                                      select y).FirstOrDefault();
                                        if (server == null)
                                        {
                                            throw new WebApiException(400, "服务器配置有误");
                                        }
                                        bool isSendSuccess = MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesCancel", server.ServerId);
                                        if (!isSendSuccess)
                                        {
                                            throw new Exception("队列操作失败");
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

                        if (item.ClosingForceTime != null && timeNow.AddSeconds(-45) < item.ClosingForceTime)
                        {
                            continue;
                        }
                    }


                    //计算需要平仓的股票数量
                    //1.卖出的最小百分比（sellx）
                    //2.剩余市值（marketValue-marketValue*sellx）
                    //3.剩余保证金（item.RemainDeposit-item.RemainDeposit*sellx）
                    //4.剩余配资金额（item.FundAmount-marketValue*sellx-item.RemainDeposit*sellx）
                    //5.警戒线(cordon)=(marketValue + item.RemainDeposit - item.FundAmount) * 1.0 / （item.FundAmount-marketValue*sellx-item.RemainDeposit*sellx
                    //6.sellx=(item.FundAmount-((marketValue + item.RemainDeposit - item.FundAmount)/(cordon*0.0001)))/(marketValue+item.RemainDeposit)
                    //7.最小卖出金额selly=marketValue*sellx
                    //8.最小卖出股票数量（sellz=selly/quotes.PresentCount）
                    //double sellx = (item.FundAmount - ((marketValue + item.RemainDeposit - item.FundAmount) * 0.0001 / cordon)) / (marketValue + item.RemainDeposit);
                    double selly = item.FundAmount - (marketValue + item.RemainDeposit - item.FundAmount) / (cordon * 0.0001);
                    if (selly > item.FundAmount)
                    {
                        selly = item.FundAmount;
                    }
                    int sellz = (int)(selly / quotes.PresentPrice);


                    sellz = sellz - item.ForceSellingCount;
                    if (sellz % shares.SharesHandCount != 0)
                    {
                        sellz = (sellz / shares.SharesHandCount + 1) * shares.SharesHandCount;
                    }
                    //剩余数量
                    int tempRemainCount = item.RemainCount - item.ForceSellingCount - item.SellingCount;
                    if (tempRemainCount == item.CanSoldCount && tempRemainCount <= sellz)
                    {
                        //清仓
                        sellz = item.CanSoldCount;
                    }
                    else
                    {
                        //判断剩余的是否大于1手
                        while (true)
                        {
                            if ((tempRemainCount - sellz >= shares.SharesHandCount || sellz <= 0) && sellz <= item.CanSoldCount)
                            {
                                break;
                            }
                            sellz = sellz - shares.SharesHandCount;
                        }
                    }
                    if (sellz > 0)
                    {
                        using (var tran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                                ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                                ObjectParameter sellIdDb = new ObjectParameter("sellId", 0);
                                db.P_ApplyTradeSell(item.AccountId, item.Id, sellz, quotes.BuyCount1 <= 0 ? 2 : 1, quotes.BuyCount1 <= 0 ? quotes.PresentPrice : 0, 2, false, errorCodeDb, errorMessageDb, sellIdDb);
                                int errorCode = (int)errorCodeDb.Value;
                                string errorMessage = errorMessageDb.Value.ToString();
                                if (errorCode != 0)
                                {
                                    throw new Exception(errorMessage);
                                }
                                long sellId = (long)sellIdDb.Value;

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
                                    bool isSendSuccess = MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesSell", server.ServerId);
                                    if (!isSendSuccess)
                                    {
                                        throw new Exception("操作超时，请重新操作");
                                    }
                                }

                                tran.Commit();
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteFileLog("强制平仓提交报错了", ex);
                                tran.Rollback();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前时段平仓线信息
        /// </summary>
        private static void GetCurrTimeClosingLine(int market, string sharesCode, string sharesName, out long currId, out int currClosingLine, out int currCordon, out TimeSpan? currStartTime, out TimeSpan? currEndTime, out int allDayClosingLine, out int allDayCordon)
        {
            TimeSpan timeSpan = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));

            currId = 0;
            currClosingLine = 0;
            currCordon = 0;
            currStartTime = null;
            currEndTime = null;
            allDayCordon = 0;
            allDayClosingLine = 0;

            using (var db = new meal_ticketEntities())
            {
                var traderulesList = (from x in db.t_shares_limit_traderules
                                      where (x.LimitMarket == -1 || x.LimitMarket == market) && ((x.LimitType == 1 && sharesCode.StartsWith(x.LimitKey)) || (x.LimitType == 2 && sharesName.StartsWith(x.LimitKey))) && x.Status == 1
                                      select x).ToList();
                foreach (var item in traderulesList)
                {
                    if (allDayClosingLine < item.ClosingLine)
                    {
                        allDayClosingLine = item.ClosingLine;
                        allDayCordon = item.Cordon;
                    }
                    //查询额外强制平仓线
                    var traderulesOther = (from x in db.t_shares_limit_traderules_other
                                           where x.RulesId == item.Id && x.Status == 1
                                           select x).ToList();
                    TimeSpan? tempCurrStartTime = null;
                    TimeSpan? tempCurrEndTime = null;
                    long tempCurrId = 0;
                    int tempCurrClosingLine = 0;
                    int tempCurrCordon = 0;
                    foreach (var other in traderulesOther)
                    {
                        try
                        {
                            TimeSpan startTime = TimeSpan.Parse(other.Times.Split('-')[0]);
                            TimeSpan endTime = TimeSpan.Parse(other.Times.Split('-')[1]);
                            if (timeSpan >= startTime && timeSpan < endTime)
                            {
                                if (tempCurrClosingLine < other.ClosingLine)
                                {
                                    tempCurrStartTime = startTime;
                                    tempCurrEndTime = endTime;
                                    tempCurrClosingLine = other.ClosingLine;
                                    tempCurrCordon = other.Cordon;
                                    tempCurrId = other.Id;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                    if (currClosingLine < tempCurrClosingLine)
                    {
                        currId = tempCurrId;
                        currClosingLine = tempCurrClosingLine;
                        currCordon = tempCurrCordon;
                        currStartTime = tempCurrStartTime;
                        currEndTime = tempCurrEndTime;
                    }
                }
                if (currId == 0)
                {
                    currClosingLine = allDayClosingLine;
                    currCordon = allDayCordon;
                }
            }
        }

        /// <summary>
        /// 获取下个时段额外平仓线信息
        /// </summary>
        private static void GetNextTimeClosingLine(int market, string sharesCode, string sharesName, out long nextId, out int nextClosingLine, out int nextCordon, out TimeSpan? nextStartTime)
        {
            TimeSpan timeSpan = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));

            nextId = 0;
            nextClosingLine = 0;
            nextCordon = 0;
            nextStartTime = null;

            using (var db = new meal_ticketEntities())
            {
                var traderulesList = (from x in db.t_shares_limit_traderules
                                      where (x.LimitMarket == -1 || x.LimitMarket == market) && ((x.LimitType == 1 && sharesCode.StartsWith(x.LimitKey)) || (x.LimitType == 2 && sharesName.StartsWith(x.LimitKey))) && x.Status == 1
                                      select x).ToList();
                foreach (var item in traderulesList)
                {
                    //查询额外强制平仓线
                    var traderulesOther = (from x in db.t_shares_limit_traderules_other
                                           where x.RulesId == item.Id && x.Status == 1
                                           select x).ToList();
                    TimeSpan? tempStartTime = null;
                    long tempNextId = 0;
                    int tempNextClosingLine = 0;
                    int tempNextCordon = 0;
                    foreach (var other in traderulesOther)
                    {
                        try
                        {
                            TimeSpan startTime = TimeSpan.Parse(other.Times.Split('-')[0]);
                            TimeSpan endTime = TimeSpan.Parse(other.Times.Split('-')[1]);
                            if (startTime <= timeSpan)
                            {
                                continue;
                            }
                            if (tempStartTime == null || tempStartTime > startTime)
                            {
                                tempStartTime = startTime;
                                tempNextId = other.Id;
                                tempNextClosingLine = other.ClosingLine;
                                tempNextCordon = other.Cordon;
                            }
                            else if (tempStartTime == startTime && tempNextClosingLine < other.ClosingLine)
                            {
                                tempStartTime = startTime;
                                tempNextId = other.Id;
                                tempNextClosingLine = other.ClosingLine;
                                tempNextCordon = other.Cordon;
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }

                    if (nextStartTime == null || nextStartTime > tempStartTime)
                    {
                        nextStartTime = tempStartTime;
                        nextId = tempNextId;
                        nextClosingLine = tempNextClosingLine;
                        nextCordon = tempNextCordon;
                    }
                    else if (nextStartTime == tempStartTime && nextClosingLine < tempNextClosingLine)
                    {
                        nextStartTime = tempStartTime;
                        nextId = tempNextId;
                        nextClosingLine = tempNextClosingLine;
                        nextCordon = tempNextCordon;
                    }
                }
            }
        }

        /// <summary>
        /// 关闭已清空持仓
        /// </summary>
        public static void HoldClose()
        {
            using (var db = new meal_ticketEntities())
            {
                var hold = (from item in db.t_account_shares_hold
                            where item.Status == 1 && item.RemainCount <= 0
                            select item).ToList();
                foreach (var item in hold)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            item.Status = 2;
                            db.SaveChanges();
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 每日清除未成交委托数据
        /// </summary>
        public static void TradeClean()
        {
            DateTime timeDate = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var entrustList = (from item in db.t_account_shares_entrust_manager
                                   where item.Status != 3 && item.CanClear == true
                                   select item).ToList();
                foreach (var item in entrustList)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            db.P_TradeFinish(item.Id, 0, 0);

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 充值订单关闭
        /// </summary>
        public static void RechargeClose()
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    db.P_Recharge_Close();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.WriteFileLog("充值订单关闭业务出错", ex);
                }
            }
        }

        /// <summary>
        /// 充值订单关闭导致退款
        /// </summary>
        public static void RechargeRefund()
        {
            using (var db = new meal_ticketEntities())
            {
                var recharge = (from item in db.t_recharge_record
                                where item.PayStatus == 5
                                select item).ToList();
                foreach (var item in recharge)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            item.PayStatus = 7;
                            db.SaveChanges();

                            string refundSn = Guid.NewGuid().ToString("N");
                            refundSn = "RAB" + refundSn;

                            bool isSuccess = false;
                            string failReason = "";
                            //支付账号信息
                            var paymentSetting = (from x in db.t_payment_account
                                                  join x2 in db.t_payment_account_settings on x.Id equals x2.PaymentAccountId
                                                  where x.Id == item.PaymentAccountId
                                                  select x2).ToList();
                            if (item.ChannelCode == "Alipay")//支付宝退款
                            {
                                string app_id = "";
                                string rsaprivatekey = "";
                                foreach (var x in paymentSetting)
                                {
                                    if (x.SettingKey == "app_id")
                                    {
                                        app_id = x.SettingValue;
                                    }
                                    if (x.SettingKey == "rsaprivatekey")
                                    {
                                        rsaprivatekey = x.SettingValue;
                                    }
                                }

                                IAopClient client = new DefaultAopClient("https://openapi.alipay.com/gateway.do", app_id, rsaprivatekey, "json", "1.0", "RSA2", "", "utf-8", false);
                                AlipayTradeRefundRequest request = new AlipayTradeRefundRequest();
                                AlipayTradeRefundModel model = new AlipayTradeRefundModel();
                                model.OutTradeNo = item.OrderSN;
                                model.RefundAmount = Math.Round((item.PayAmount / 100) * 1.0 / 100, 2).ToString();
                                model.OutRequestNo = refundSn;
                                request.SetBizModel(model);
                                AlipayTradeRefundResponse response = client.Execute(request);
                                var res = JsonConvert.DeserializeObject<ZFB_Refund_Res>(response.Body);
                                if (res != null && res.alipay_trade_refund_response != null && res.alipay_trade_refund_response.code == "10000" && res.alipay_trade_refund_response.sub_code == null)
                                {
                                    isSuccess = true;
                                    failReason = "";
                                }
                                else
                                {
                                    item.PayStatus = 5;
                                    db.SaveChanges();

                                    isSuccess = false;
                                    failReason = (res != null && res.alipay_trade_refund_response != null) ? (string.IsNullOrEmpty(res.alipay_trade_refund_response.sub_msg) ? res.alipay_trade_refund_response.msg : res.alipay_trade_refund_response.sub_msg) : "";
                                }
                            }
                            else if (item.ChannelCode == "WeChat")//微信退款
                            {
                                string appid = "";
                                string mch_id = "";
                                string key = "";
                                string SSlCertPath = "";
                                string SSlCertPassword = "";
                                foreach (var x in paymentSetting)
                                {
                                    if (x.SettingKey == "appid")
                                    {
                                        appid = x.SettingValue;
                                    }
                                    if (x.SettingKey == "mch_id")
                                    {
                                        mch_id = x.SettingValue;
                                    }
                                    if (x.SettingKey == "key")
                                    {
                                        key = x.SettingValue;
                                    }
                                    if (x.SettingKey == "SSlCertPath")
                                    {
                                        SSlCertPath = x.SettingValue;
                                    }
                                    if (x.SettingKey == "SSlCertPassword")
                                    {
                                        SSlCertPassword = x.SettingValue;
                                    }
                                }

                                //退钱
                                WxPayData payModel = new WxPayData();//传入数据
                                WxPayData outPayModel = new WxPayData();//微信返回数据
                                payModel.SetValue("appid", appid);//appid
                                payModel.SetValue("mch_id", mch_id);//mch_id
                                payModel.SetValue("out_trade_no", item.OrderSN);//订单号
                                payModel.SetValue("op_user_id", mch_id);//mch_id
                                payModel.SetValue("out_refund_no", refundSn);//退款单号
                                payModel.SetValue("total_fee", (item.PayAmount / 100).ToString());//订单总金额
                                payModel.SetValue("refund_fee", (item.PayAmount / 100).ToString());//退款金额

                                outPayModel = WxPayApi.Refund(payModel, key, SSlCertPath, SSlCertPassword);//调用微信接口获得返回数据
                                                                                                           //将微信的数据赋值给model
                                if (outPayModel != null)
                                {
                                    string ReturnCode = outPayModel.GetValue("return_code").ToString();//返回状态码 SUCCESS/FAIL
                                    string ReturnMsg = outPayModel.GetValue("return_msg").ToString();//返回信息 OK/异常信息
                                    if (!ReturnCode.Equals("SUCCESS"))
                                    {
                                        item.PayStatus = 5;
                                        db.SaveChanges();

                                        isSuccess = false;
                                        failReason = ReturnMsg;
                                    }
                                    else
                                    {
                                        string ResultCode = outPayModel.GetValue("result_code").ToString();//结果状态码 SUCCESS/FAIL
                                        if (!ResultCode.Equals("SUCCESS"))
                                        {
                                            item.PayStatus = 5;
                                            db.SaveChanges();

                                            isSuccess = false;
                                            failReason = ResultCode;
                                        }
                                        else
                                        {
                                            isSuccess = true;
                                            failReason = "";
                                        }
                                    }
                                }
                                else
                                {
                                    item.PayStatus = 5;
                                    db.SaveChanges();

                                    isSuccess = false;
                                    failReason = "";
                                }
                            }
                            else
                            {
                                throw new Exception("渠道有误");
                            }
                            db.t_recharge_abnormal_refund_record.Add(new t_recharge_abnormal_refund_record
                            {
                                OrderSN = item.OrderSN,
                                CreateTime = DateTime.Now,
                                LastModified = DateTime.Now,
                                RecordId = item.Id,
                                RefundAmount = item.PayAmount,
                                RefundStatus = isSuccess ? 1 : 2,
                                RefundFailReason = failReason,
                                RefundSN = refundSn
                            });
                            db.SaveChanges();
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            Logger.WriteFileLog("充值订单关闭退款业务出错", ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 充值订单关闭导致退款结果查询
        /// </summary>
        public static void RechargeRefundQuery()
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var record = (from item in db.t_recharge_abnormal_refund_record
                              where item.RefundStatus == 1
                              select item).ToList();
                foreach (var item in record)
                {
                    try
                    {
                        var recharge = (from x in db.t_recharge_record
                                        where x.Id == item.RecordId
                                        select x).FirstOrDefault();
                        if (recharge == null)
                        {
                            item.RefundStatus = 2;
                            item.RefundFailReason = "找不到对应充值订单";
                            item.LastModified = timeNow;
                            db.SaveChanges();
                            continue;
                        }
                        if (recharge.PayStatus != 7)
                        {
                            item.RefundStatus = 2;
                            item.RefundFailReason = "退款状态不正确";
                            item.LastModified = timeNow;
                            db.SaveChanges();
                            continue;
                        }
                        //支付账号信息
                        var paymentSetting = (from x in db.t_payment_account
                                              join x2 in db.t_payment_account_settings on x.Id equals x2.PaymentAccountId
                                              where x.Id == recharge.PaymentAccountId
                                              select x2).ToList();
                        if (recharge.ChannelCode == "Alipay")//支付宝支付
                        {
                            string app_id = "";
                            string rsaprivatekey = "";
                            foreach (var x in paymentSetting)
                            {
                                if (x.SettingKey == "app_id")
                                {
                                    app_id = x.SettingValue;
                                }
                                if (x.SettingKey == "rsaprivatekey")
                                {
                                    rsaprivatekey = x.SettingValue;
                                }
                            }

                            IAopClient client = new DefaultAopClient("https://openapi.alipay.com/gateway.do", app_id, rsaprivatekey, "json", "1.0", "RSA2", "", "utf-8", false);
                            AlipayTradeFastpayRefundQueryRequest request = new AlipayTradeFastpayRefundQueryRequest();
                            AlipayTradeFastpayRefundQueryModel model = new AlipayTradeFastpayRefundQueryModel();
                            model.OutTradeNo = recharge.OrderSN;
                            model.OutRequestNo = item.RefundSN;
                            request.SetBizModel(model);
                            AlipayTradeFastpayRefundQueryResponse response = client.Execute(request);
                            var res = JsonConvert.DeserializeObject<ZFB_Refund_Res>(response.Body);
                            if (res.alipay_trade_fastpay_refund_query_response.code == "10000" && res.alipay_trade_fastpay_refund_query_response.sub_code == null && res.alipay_trade_fastpay_refund_query_response.refund_amount != null)
                            {
                                //修改退款状态为退款成功
                                item.RefundStatus = 3;
                                item.LastModified = timeNow;

                                recharge.PayStatus = 8;
                                db.SaveChanges();
                            }

                        }
                        else if (recharge.ChannelCode == "WeChat")//微信支付
                        {
                            string appid = "";
                            string mch_id = "";
                            string key = "";
                            string SSlCertPath = "";
                            string SSlCertPassword = "";
                            foreach (var x in paymentSetting)
                            {
                                if (x.SettingKey == "appid")
                                {
                                    appid = x.SettingValue;
                                }
                                if (x.SettingKey == "mch_id")
                                {
                                    mch_id = x.SettingValue;
                                }
                                if (x.SettingKey == "key")
                                {
                                    key = x.SettingValue;
                                }
                                if (x.SettingKey == "SSlCertPath")
                                {
                                    SSlCertPath = x.SettingValue;
                                }
                                if (x.SettingKey == "SSlCertPassword")
                                {
                                    SSlCertPassword = x.SettingValue;
                                }
                            }
                            WxPayData payModel = new WxPayData();//传入数据
                            WxPayData outPayModel = new WxPayData();//微信返回数据
                            payModel.SetValue("appid", appid);//appid
                            payModel.SetValue("mch_id", mch_id);//mch_id
                            payModel.SetValue("out_trade_no", recharge.OrderSN);//订单号
                            outPayModel = WxPayApi.RefundQuery(payModel, key);//调用微信接口获得返回数据

                            //将微信的数据赋值给model
                            if (outPayModel != null)
                            {
                                string ReturnCode = outPayModel.GetValue("return_code").ToString();//返回状态码 SUCCESS/FAIL
                                string ReturnMsg = outPayModel.GetValue("return_msg").ToString();//返回信息 OK/异常信息
                                string ResultCode = outPayModel.GetValue("result_code").ToString();//结果状态码 SUCCESS/FAIL
                                if (ReturnCode.Equals("SUCCESS") && ResultCode.Equals("SUCCESS"))
                                {
                                    string refund_status = outPayModel.GetValue("refund_status_0").ToString();//退款状态
                                    if (refund_status.Equals("SUCCESS"))
                                    {
                                        //修改退款状态为退款成功
                                        item.RefundStatus = 3;
                                        item.LastModified = timeNow;

                                        recharge.PayStatus = 8;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        item.RefundStatus = 2;
                                        item.RefundFailReason = refund_status;
                                        item.LastModified = timeNow;

                                        recharge.PayStatus = 5;
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        else
                        {
                            item.RefundStatus = 2;
                            item.RefundFailReason = "不支持的渠道";
                            item.LastModified = timeNow;

                            recharge.PayStatus = 5;
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("退款查询出错", ex);
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// 通知短信发送
        /// </summary>
        public static void SmsSend()
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var notice = (from item in db.t_notice_send_record
                              where item.IsSend == false && item.MaxSendTime >= timeNow
                              select item).ToList();
                var notice_sms = (from item in notice
                                  where item.SendType == 1
                                  select item).ToList();
                var notice_push = (from item in notice
                                   where item.SendType == 2
                                   select item).ToList();
                foreach (var item in notice_sms)
                {
                    item.IsSend = true;
                    item.SendTime = timeNow;
                    if (string.IsNullOrEmpty(item.SmsThirdSignId))
                    {
                        item.ErrorMessage = "短信签名为空";
                        continue;
                    }
                    if (string.IsNullOrEmpty(item.SmsThirdTempId))
                    {
                        item.ErrorMessage = "短信模板为空";
                        continue;
                    }
                    //分析para
                    Dictionary<string, string> tempParaDic = new Dictionary<string, string>();
                    var tempPara = string.IsNullOrEmpty(item.SmsTempPara) ? null : JsonConvert.DeserializeObject<JObject>(item.SmsTempPara);
                    if (tempPara != null)
                    {
                        foreach (var p in tempPara.Properties().ToArray())
                        {
                            tempParaDic.Add(p.Name, tempPara[p.Name].Value<string>());
                        }
                    }
                    Dictionary<string, string> paraDic = new Dictionary<string, string>();
                    Regex regex = new Regex(@"{{\w*}}"); //匹配所有参数
                    var matchGroup = regex.Match(item.SendContent).Groups;
                    bool paraSuccess = true;
                    for (int i = 0; i < matchGroup.Count; i++)
                    {
                        string key = matchGroup[i].Value;
                        if (string.IsNullOrEmpty(key))
                        {
                            continue;
                        }
                        key = key.Replace("{", "");
                        key = key.Replace("}", "");
                        string value = "";
                        if (!tempParaDic.TryGetValue(key, out value))
                        {
                            paraSuccess = false;
                            break;
                        }
                        if (!paraDic.ContainsKey(key))
                        {
                            paraDic.Add(key, value);
                        }
                    }
                    if (paraSuccess == false)
                    {
                        item.ErrorMessage = "模板参数对应不上";
                        continue;
                    }
                    db.SaveChanges();
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //分析mobile
                            List<SmsSendInfo> sendInfo = new List<SmsSendInfo>();
                            var mobileArr = item.SmsMobile.Split(',');
                            foreach (var mobile in mobileArr)
                            {
                                if (sendInfo.Where(e => e.Mobile == mobile).FirstOrDefault() != null)
                                {
                                    continue;
                                }
                                sendInfo.Add(new SmsSendInfo
                                {
                                    SignId = item.SmsThirdSignId,
                                    TemplateId = item.SmsThirdTempId,
                                    Type = item.SmsTempType,
                                    TemplateParameters = paraDic,
                                    Mobile = mobile
                                });

                                db.t_notice_send_record_details.Add(new t_notice_send_record_details
                                {
                                    SmsAppKey = item.SmsAppKey,
                                    SmsChannelCode = item.SmsChannelCode,
                                    CreateTime = DateTime.Now,
                                    ReceiveTime = null,
                                    Mobile = mobile,
                                    RecordId = item.Id,
                                    ThirdMsgId = "",
                                    Status = 4000,
                                    StatusDes = "发送中"
                                });
                            }
                            db.SaveChanges();

                            ThirdSmsBase smsObj = new ThirdSmsBase(item.SmsChannelCode, item.SmsAppKey, item.SmsAppSecret);
                            var smsSend = smsObj.GetThirdSmsObj();
                            var res = smsSend.SendSmsBatch(sendInfo);
                            var resObj = JsonConvert.DeserializeObject<dynamic>(res.Content);
                            if (resObj.error != null)
                            {
                                string errorMessage = resObj.error.code;
                                throw new Exception(errorMessage);
                            }

                            try
                            {
                                foreach (var x in resObj.recipients)
                                {
                                    string msg_id = x.msg_id;
                                    string mobile = x.mobile;
                                    string error_code = x.error_code;

                                    var details = (from d in db.t_notice_send_record_details
                                                   where d.Mobile == mobile && d.RecordId == item.Id
                                                   select d).FirstOrDefault();
                                    if (details == null)
                                    {
                                        continue;
                                    }
                                    if (error_code != null)
                                    {
                                        details.Status = 5000;
                                        details.StatusDes = "提交时出错，原因：" + error_code;
                                    }
                                    details.ThirdMsgId = msg_id;
                                }
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            { }

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            Logger.WriteFileLog("发送短信异常,Id=" + item.Id, ex);

                            item.ErrorMessage = "业务处理异常";
                            db.SaveChanges();
                        }
                    }
                }


                foreach (var item in notice_push)
                {
                    item.IsSend = true;
                    item.SendTime = timeNow;
                    //分析para
                    Dictionary<string, string> tempParaDic = new Dictionary<string, string>();
                    var tempPara = string.IsNullOrEmpty(item.SmsTempPara) ? null : JsonConvert.DeserializeObject<JObject>(item.SmsTempPara);
                    if (tempPara != null)
                    {
                        foreach (var p in tempPara.Properties().ToArray())
                        {
                            tempParaDic.Add(p.Name, tempPara[p.Name].Value<string>());
                        }
                    }
                    Dictionary<string, string> paraDic = new Dictionary<string, string>();
                    Regex regex = new Regex(@"{{\w*}}"); //匹配所有参数
                    var matchGroup = regex.Match(item.SendContent).Groups;
                    bool paraSuccess = true;
                    for (int i = 0; i < matchGroup.Count; i++)
                    {
                        string key = matchGroup[i].Value;
                        if (string.IsNullOrEmpty(key))
                        {
                            continue;
                        }
                        key = key.Replace("{", "");
                        key = key.Replace("}", "");
                        string value = "";
                        if (!tempParaDic.TryGetValue(key, out value))
                        {
                            paraSuccess = false;
                            break;
                        }
                        if (!paraDic.ContainsKey(key))
                        {
                            paraDic.Add(key, value);
                        }
                        item.SendContent = item.SendContent.Replace("{{" + key + "}}", value);
                    }
                    if (paraSuccess == false)
                    {
                        item.ErrorMessage = "模板参数对应不上";
                        continue;
                    }
                    db.SaveChanges();
                    JPushClient pushObj = new JPushClient(item.SmsAppKey, item.SmsAppSecret);
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var mobileArr = item.SmsMobile.Split(',');
                            foreach (var mobile in mobileArr)
                            {
                                long accountId = long.Parse(mobile);
                                //查询token
                                var accountToken = (from x in db.t_account_login_token
                                                    where x.AccountId == accountId
                                                    select x.Login_Uuid).FirstOrDefault();
                                if (string.IsNullOrEmpty(accountToken))
                                {
                                    continue;
                                }

                                db.t_notice_send_record_details.Add(new t_notice_send_record_details
                                {
                                    SmsAppKey = item.SmsAppKey,
                                    SmsChannelCode = item.SmsChannelCode,
                                    CreateTime = DateTime.Now,
                                    ReceiveTime = null,
                                    Mobile = mobile,
                                    RecordId = item.Id,
                                    ThirdMsgId = "",
                                    Status = 4000,
                                    StatusDes = "发送中"
                                });

                                string audi = "{\"alias\":[\"" + accountToken.Replace("-", "") + "\"]}";

                                Dictionary<string, string> dic = new Dictionary<string, string>();
                                string actionPath = item.JumpUrl;
                                string actionType = "0";
                                if (!string.IsNullOrEmpty(actionPath))
                                {
                                    actionType = "1";
                                }
                                dic.Add("actionType", actionType);
                                dic.Add("actionPath", actionPath);

                                var rrr = pushObj.SendPush(new Jiguang.JPush.Model.PushPayload
                                {
                                    Platform = "all",
                                    Audience = JsonConvert.DeserializeObject(audi),
                                    Notification = new Jiguang.JPush.Model.Notification
                                    {
                                        Alert = item.SendContent
                                    },
                                    Message = new Jiguang.JPush.Model.Message
                                    {
                                        Content = item.SendContent,
                                        Extras = dic
                                    },
                                    Options = new Jiguang.JPush.Model.Options
                                    {
                                        IsApnsProduction = true
                                    }
                                });
                                Logger.WriteFileLog(rrr.Content, null);
                            }
                            db.SaveChanges();

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            Logger.WriteFileLog("发送推送异常,Id=" + item.Id, ex);

                            item.ErrorMessage = "业务处理异常";
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 派股派息
        /// </summary>
        public static void SharesAllot()
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var allotList = (from item in db.t_shares_allot
                                 where item.HandleStatus == 1 && timeNow > item.AllotDate
                                 select item).ToList();
                foreach (var item in allotList)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            db.P_AccountSharesAllot(item.Id);
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            Logger.WriteFileLog("派股派息出错，Id=" + item.Id, ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更新股票分笔数据
        /// </summary>
        public static void UpdateTransactiondata()
        {
            DateTime timeDate = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var shares = (from item in db.t_shares_quotes
                              where item.LastModified > timeDate
                              select item).ToList();
                foreach (var item in shares)
                {
                    try
                    {
                        var sendData = new
                        {
                            Market = item.Market,
                            SharesCode = item.SharesCode,
                            Date = timeDate.ToString("yyyy-MM-dd"),
                            Type = 1
                        };
                        MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "TransactionData", "Update");
                    }
                    catch (Exception)
                    { }
                }
            }
        }

        /// <summary>
        /// 推送股票分笔数据
        /// </summary>
        /// <returns></returns>
        public static void SendTransactionShares()
        {
            using (var db = new meal_ticketEntities())
            {
                DateTime timeDate = DateTime.Now.Date;
                string sql = string.Format("select t.Market,t.SharesCode,t1.SharesName from t_shares_quotes t with(xlock) inner join t_shares_all t1 with(nolock) on t.Market=t1.Market and t.SharesCode=t1.SharesCode inner join t_shares_monitor t2 with(nolock) on t.Market=t2.Market and t.SharesCode=t2.SharesCode where t.LastModified>'{0}' and t2.[Status]=1", timeDate.ToString("yyyy-MM-dd"));
                var shares = db.Database.SqlQuery<TradeSharesInfo>(sql);

                DateTime tempTime = DateTime.Now.AddMinutes(-10);
                var monitorDetails = db.t_shares_monitor_details.Where(e => e.SendTime > tempTime).ToList();
                List<t_shares_monitor_details> list = new List<t_shares_monitor_details>();

                List<dynamic> pushList = new List<dynamic>();
                foreach (var item in shares)
                {
                    if (monitorDetails.Where(e => e.Market == item.Market && e.SharesCode == item.SharesCode).FirstOrDefault() != null)
                    {
                        continue;
                    }
                    list.Add(new t_shares_monitor_details
                    {
                        SendTime = DateTime.Now,
                        SharesCode = item.SharesCode,
                        Market = item.Market
                    });
                }

                var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                using (DataTable table = new DataTable())
                {
                    table.Columns.Add("Id", typeof(long));
                    table.Columns.Add("Market", typeof(int));
                    table.Columns.Add("SharesCode", typeof(string));
                    table.Columns.Add("SendTime", typeof(DateTime));

                    foreach (var item in list)
                    {
                        DataRow row = table.NewRow();
                        row["Id"] = 0;
                        row["Market"] = item.Market;
                        row["SharesCode"] = item.SharesCode;
                        row["SendTime"] = item.SendTime;
                        table.Rows.Add(row);
                    }
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("Id", "Id");
                    dic.Add("Market", "Market");
                    dic.Add("SharesCode", "SharesCode");
                    dic.Add("SendTime", "SendTime");
                    bulk.ColumnMappings = dic;
                    bulk.BatchSize = 10000;


                    bulk.BulkWriteToServer(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString, table, "t_shares_monitor_details");

                }

                int i = 0;
                foreach (var item in list)
                {
                    i++;
                    var sendData = new
                    {
                        Market = item.Market,
                        SharesCode = item.SharesCode,
                        Date = timeDate.ToString("yyyy-MM-dd"),
                        Type = 0
                    };
                    MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "TransactionData", "UpdateNew");

                    pushList.Add(new
                    {
                        Market = item.Market,
                        SharesCode = item.SharesCode
                    });

                    try
                    {
                        if (pushList.Count() > 0 && (pushList.Count() % Singleton.Instance.NewTransactionDataTrendHandlerCount == 0 || i >= shares.Count()))
                        {
                            var sendData2 = pushList;
                            MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData2)), "TransactionData", "TrendAnalyse");
                            pushList = new List<dynamic>();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }
    }
}
