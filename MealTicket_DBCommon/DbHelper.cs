using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_DBCommon
{
    public class DbHelper
    {
        /// <summary>
        /// 检查当天是否交易日
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
            if (!CheckTradeDate())
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
                    //解析time1
                    if (item.Time1 != null)
                    {
                        string[] timeArr = item.Time1.Split(',');
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
                    if (item.Time3 != null)
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
                    if (item.Time4 != null)
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
        /// 获取最近交易日日期
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLastTradeDate2(int hour = 0, int minute = 0, int second = 0, int day = 0,DateTime? date=null)
        {
            DateTime dateNow = date == null ? DateTime.Now.AddHours(hour).AddMinutes(minute).AddSeconds(second).Date : date.Value.AddHours(hour).AddMinutes(minute).AddSeconds(second).Date;
            int index = day > 0 ? 1 : -1;
            day = Math.Abs(day);
            int i = 0;
            while (true)
            {
                if (CheckTradeDate(dateNow))
                {
                    if (i >= day)
                    {
                        break;
                    }
                    i++;
                }
                dateNow = dateNow.AddDays(index);
            }
            return dateNow;
        }

        /// <summary>
        /// 获取平仓规则
        /// </summary>
        public static List<TradeRulesInfo> GetTraderules(long accountId) 
        {
            List<TradeRulesInfo> ResultList = new List<TradeRulesInfo>();

            using (var db = new meal_ticketEntities())
            {
                if (accountId > 0)
                {
                    var accountTraderulesOther = from item in db.t_shares_limit_traderules_other
                                                 where item.Status == 1 && item.Type == 1
                                                 select item;
                    var accountTraderules = (from item in db.t_account_par_setting
                                             join item2 in accountTraderulesOther on item.Id equals item2.RulesId into a from ai in a.DefaultIfEmpty()
                                             where item.Status == 1 && item.AccountId == accountId && item.ParamName == "ClosingRules"
                                             group new { item,ai} by item into g
                                             select g).ToList();
                    foreach (var item in accountTraderules)
                    {
                        try
                        {
                            var tempRules=JsonConvert.DeserializeObject<dynamic>(item.Key.ParamValue);
                            string _LimitKey = tempRules.LimitKey;
                            int _LimitType = tempRules.LimitType;
                            bool _LimitMarket0 = false;
                            bool _LimitMarket1 = false;
                            try
                            {
                                _LimitMarket0 = tempRules.LimitMarket0;
                            }
                            catch (Exception ex)
                            { }
                            try
                            {
                                _LimitMarket1 = tempRules.LimitMarket1;
                            }
                            catch (Exception ex)
                            { }
                            int _LimitMarket = 0;
                            if (_LimitMarket0 && _LimitMarket1) { _LimitMarket = -1; }
                            else if (_LimitMarket0) { _LimitMarket = 0; }
                            else if (_LimitMarket1) { _LimitMarket = 1; }
                            else { continue; }
                            int _Cordon = tempRules.Cordon;
                            int _ClosingLine = tempRules.ClosingLine;

                            ResultList.Add(new TradeRulesInfo
                            {
                                Status = item.Key.Status,
                                ClosingLine = _ClosingLine,
                                Cordon = _Cordon,
                                Id = item.Key.Id,
                                LimitKey = _LimitKey,
                                LimitMarket = _LimitMarket,
                                LimitType = _LimitType,
                                Type = 1,
                                OtherList = (from x in item
                                             where x.ai != null
                                             select new TradeRulesOtherInfo
                                             {
                                                 Status = x.ai.Status,
                                                 ClosingLine = x.ai.ClosingLine,
                                                 Cordon = x.ai.Cordon,
                                                 Id = x.ai.Id,
                                                 RulesId = x.ai.RulesId,
                                                 Times = x.ai.Times,
                                                 Type = 1
                                             }).ToList()
                            });
                        }
                        catch (Exception ex)
                        {
                            
                        }
                    }
                }

                var traderulesOther = from item in db.t_shares_limit_traderules_other
                                      where item.Status == 1 && item.Type == 0
                                      select item;
                var traderulesList = (from item in db.t_shares_limit_traderules
                                      join item2 in traderulesOther on item.Id equals item2.RulesId into a
                                      from ai in a.DefaultIfEmpty()
                                      where item.Status == 1
                                      group new { item, ai } by item into g
                                      select new TradeRulesInfo
                                      {
                                          Status = g.Key.Status,
                                          ClosingLine = g.Key.ClosingLine,
                                          Cordon = g.Key.Cordon,
                                          Id = g.Key.Id,
                                          LimitKey = g.Key.LimitKey,
                                          LimitMarket = g.Key.LimitMarket,
                                          LimitType = g.Key.LimitType,
                                          Type = 0,
                                          OtherList = (from x in g
                                                       where x.ai != null
                                                       select new TradeRulesOtherInfo
                                                       {
                                                           Status = x.ai.Status,
                                                           ClosingLine = x.ai.ClosingLine,
                                                           Cordon = x.ai.Cordon,
                                                           Id = x.ai.Id,
                                                           RulesId = x.ai.RulesId,
                                                           Times = x.ai.Times,
                                                           Type = 0
                                                       }).ToList()
                                      }).ToList();
                ResultList.AddRange(traderulesList);
                return ResultList;
            }
        }

        /// <summary>
        /// 获取某只股票整天的平仓规则情况
        /// </summary>
        public static List<TradeRulesInfo> GetSharesTotalRules(string sharesCode,string sharesName,int market,List<TradeRulesInfo> rules) 
        {
            if (rules == null)
            {
                return new List<TradeRulesInfo>();
            }
            var rulesValid = (from item in rules
                              where (item.LimitMarket == -1 || item.LimitMarket == market) && ((item.LimitType == 1 && sharesCode.StartsWith(item.LimitKey)) || (item.LimitType == 2 && sharesName.StartsWith(item.LimitKey))) && item.Status == 1
                              select item).ToList();
            rules = rulesValid.Where(e => e.Type == 1).ToList();
            if (rules.Count() <= 0)
            {
                rules = rulesValid.Where(e => e.Type == 0).ToList();
            }
            return rules;
        }

        /// <summary>
        /// 获取某只股票当前的平仓线/警戒线
        /// </summary>
        public static void GetSharesCurrRule(string sharesCode, string sharesName, int market, List<TradeRulesInfo> rulesPar,ref int cordon,ref int closingLine, ref TimeSpan? startTime, ref TimeSpan? endTime,ref long rulesId)
        {
            TimeSpan timeSpan = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
            cordon = -1;
            closingLine = -1;
            int cordon_common = -1;
            int closingLine_common = -1;
            rulesId = 0;
            var rules=GetSharesTotalRules(sharesCode, sharesName, market, rulesPar);
            foreach (var item in rules)
            {
                if (closingLine_common < item.ClosingLine)
                {
                    cordon_common = item.Cordon;
                    closingLine_common = item.ClosingLine;
                }
                foreach (var other in item.OtherList)
                {
                    try
                    {
                        TimeSpan tempStartTime = TimeSpan.Parse(other.Times.Split('-')[0]);
                        TimeSpan tempEndTime = TimeSpan.Parse(other.Times.Split('-')[1]);
                        if (timeSpan >= tempStartTime && timeSpan < tempEndTime)
                        {
                            if (closingLine < other.ClosingLine)
                            {
                                closingLine = other.ClosingLine;
                                cordon = other.Cordon;
                                startTime = tempStartTime;
                                endTime = tempEndTime;
                                rulesId = other.Id;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
            if (closingLine == -1)
            {
                cordon = cordon_common==-1?0: cordon_common;
                closingLine = closingLine_common == -1 ? 0 : closingLine_common;
            }
        }

        /// <summary>
        /// 获取某只股票当前的平仓线/警戒线
        /// </summary>
        public static void GetSharesNextRule(string sharesCode, string sharesName, int market, List<TradeRulesInfo> rulesPar, TimeSpan? currEndTime, ref int cordon, ref int closingLine, ref TimeSpan? startTime, ref TimeSpan? endTime, ref long rulesId)
        {
            TimeSpan timeNowSpan = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
            cordon = -1;
            closingLine = -1;
            int cordon_common = -1;
            int closingLine_common = -1;
            rulesId = 0;
            startTime = null;
            endTime = null;
            var rules=GetSharesTotalRules(sharesCode, sharesName, market, rulesPar);
            foreach (var item in rules)
            {
                if (closingLine_common < item.ClosingLine)
                {
                    cordon_common = item.Cordon;
                    closingLine_common = item.ClosingLine;
                }
                TimeSpan? minStartTime = null;
                foreach (var other in item.OtherList)
                {
                    try
                    {
                        TimeSpan tempStartTime = TimeSpan.Parse(other.Times.Split('-')[0]);
                        TimeSpan tempEndTime = TimeSpan.Parse(other.Times.Split('-')[1]);
                        if (timeNowSpan >= tempStartTime)
                        {
                            continue;
                        }
                        if (minStartTime == null || minStartTime > tempStartTime)
                        {
                            closingLine = other.ClosingLine;
                            cordon = other.Cordon;
                            startTime = tempStartTime;
                            endTime = tempEndTime;
                            rulesId = other.Id;
                        }
                        else if (minStartTime == tempStartTime && closingLine < other.ClosingLine)
                        {
                            closingLine = other.ClosingLine;
                            cordon = other.Cordon;
                            startTime = tempStartTime;
                            endTime = tempEndTime;
                            rulesId = other.Id;
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
            if (currEndTime != null && (startTime == null || startTime > currEndTime))
            {
                cordon = cordon_common;
                closingLine = closingLine_common;
                rulesId = 0;
                startTime = currEndTime;
                endTime = null;
            }
            else if (startTime==null)
            {
                cordon = cordon_common == -1 ? 0 : cordon_common;
                closingLine = closingLine_common == -1 ? 0 : closingLine_common;
                rulesId = 0;
                startTime = null;
                endTime = null;
            }
        }

        /// <summary>
        /// 计算平仓价格信息
        /// </summary>
        public static void CalculateClosingInfo(long CurrPrice, int RemainCount, long RemainDeposit, long RemainFundAmount, int Cordon, int ClosingLine, out long ClosingPrice, out long AddDepositAmount, out long CordonPrice)
        {
            //计算平仓信息
            //（市值+保证金-借钱）/借钱=价格线
            ClosingPrice = RemainCount <= 0 ? 0 : (long)(ClosingLine * 1.0 / 10000 * RemainFundAmount + RemainFundAmount - RemainDeposit) / RemainCount;
            CordonPrice = RemainCount <= 0 ? 0 : (long)(Cordon * 1.0 / 10000 * RemainFundAmount + RemainFundAmount - RemainDeposit) / RemainCount;
            long needDepositAmount = (long)(Cordon * 1.0 / 10000 * RemainFundAmount + RemainFundAmount - CurrPrice * RemainCount);
            AddDepositAmount = needDepositAmount - RemainDeposit < 0 ? 0 : needDepositAmount - RemainDeposit;
        }

        /// <summary>
        /// 计算其他成本
        /// </summary>
        /// <returns></returns>
        public static long CalculateOtherCost(int Type, ref List<OtherCostInfo> CostList)
        {
            long totalCost = 0;
            using (var db = new meal_ticketEntities())
            {
                //统计其他成本的项
                var par = (from item in db.t_system_param
                           where item.ParamName == "OtherCostRules"
                           select item).FirstOrDefault();
                if (par == null)
                {
                    return 0;
                }
                var parValue = JsonConvert.DeserializeObject<List<string>>(par.ParamValue);

                List<t_account_shares_entrust> result = new List<t_account_shares_entrust>();

                var IdList = CostList.Select(e => e.Id).ToList();
                if (Type == 1)
                {
                    if (parValue.Contains("HoldServiceRules"))
                    {
                        var hold = (from item in db.t_account_shares_hold
                                    where IdList.Contains(item.Id)
                                    select item).ToList();
                        CostList = (from item in CostList
                                    join item2 in hold on item.Id equals item2.Id
                                    select new OtherCostInfo
                                    {
                                        Id = item.Id,
                                        Cost = item2.ServiceFee
                                    }).ToList();
                        if (CostList.Count() <= 0)
                        {
                            return 0;
                        }
                        totalCost = totalCost + CostList.Sum(e => e.Cost);
                    }

                    result = (from item in db.t_account_shares_entrust
                              where IdList.Contains(item.HoldId) && item.DealCount > 0
                              select item).ToList();
                }
                else if (Type == 2)
                {
                    result = (from item in db.t_account_shares_entrust
                              where IdList.Contains(item.Id) && item.DealCount > 0
                              select item).ToList();
                }
                foreach (var item in CostList)
                {
                    long tempCost = 0;
                    List<t_account_shares_entrust> list = new List<t_account_shares_entrust>();
                    if (Type == 1)
                    {
                        list = result.Where(e => e.HoldId == item.Id).ToList();
                        if (list.Count() <= 0)
                        {
                            continue;
                        }
                    }
                    else if (Type == 2)
                    {
                        list = result.Where(e => e.Id == item.Id).ToList();
                        if (list.Count() <= 0)
                        {
                            continue;
                        }
                    }

                    if (parValue.Contains("HandlingFeeRules"))
                    {
                        tempCost = tempCost + list.Sum(e => e.HandlingFee);
                    }
                    if (parValue.Contains("BuyServiceRules"))
                    {
                        tempCost = tempCost + list.Sum(e => e.BuyService);
                    }
                    if (parValue.Contains("CommissionRules"))
                    {
                        tempCost = tempCost + list.Sum(e => e.Commission);
                    }
                    if (parValue.Contains("AdministrationFeeRules"))
                    {
                        tempCost = tempCost + list.Sum(e => e.AdministrationFee);
                    }
                    if (parValue.Contains("StampFeeRules"))
                    {
                        tempCost = tempCost + list.Sum(e => e.StampFee);
                    }
                    if (parValue.Contains("ProfitTaxRules"))
                    {
                        tempCost = tempCost + list.Sum(e => e.ProfitService);
                    }
                    if (parValue.Contains("TransferFeeRules"))
                    {
                        tempCost = tempCost + list.Sum(e => e.TransferFee);
                    }
                    if (parValue.Contains("SettlementFeeRules"))
                    {
                        tempCost = tempCost + list.Sum(e => e.SettlementFee);
                    }
                    item.Cost = item.Cost + tempCost;
                    totalCost = totalCost + tempCost;
                }
                return totalCost;
            }
        }

        /// <summary>
        /// 获取账户买入设置
        /// </summary>
        /// <returns></returns>
        public static List<t_account_shares_buy_setting> GetAccountBuySetting(long accountId,int type) 
        {
            using (var db = new meal_ticketEntities())
            {
                var setting = from item in db.t_account_shares_buy_setting
                              select item;
                if (accountId != 0)
                {
                    setting = from item in setting
                              where item.AccountId == accountId
                              select item;
                }
                if (type != 0)
                {
                    setting = from item in setting
                              where item.Type == type
                              select item;
                }
                return setting.ToList();
            }
        }
    }
}
