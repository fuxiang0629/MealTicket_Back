using MealTicket_DBCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Helper
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
        /// 计算其他成本
        /// </summary>
        /// <param name="IdList">持仓Id/委托Id</param>
        /// <param name="Type">1.持仓Id 2.委托Id</param>
        /// <returns></returns>
        public static long CalculateOtherCost(List<long> IdList, int Type)
        {
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

                long cost = 0;
                List<t_account_shares_entrust> result = new List<t_account_shares_entrust>();
                if (Type == 1)
                {
                    var hold = (from item in db.t_account_shares_hold
                                where IdList.Contains(item.Id)
                                select item).ToList();
                    if (hold.Count() <= 0)
                    {
                        return 0;
                    }
                    if (parValue.Contains("HoldServiceRules"))
                    {
                        cost = cost + hold.Sum(e => e.ServiceFee);
                    }
                    result = (from item in db.t_account_shares_entrust
                              where IdList.Contains(item.HoldId) && item.DealCount>0 
                              select item).ToList();
                }
                if (Type == 2)
                {
                    result = (from item in db.t_account_shares_entrust
                              where IdList.Contains(item.Id) && item.DealCount > 0
                              select item).ToList();
                }
                if (result.Count() > 0)
                {
                    if (parValue.Contains("HandlingFeeRules"))
                    {
                        cost = cost + result.Sum(e => e.HandlingFee);
                    }
                    if (parValue.Contains("BuyServiceRules"))
                    {
                        cost = cost + result.Sum(e => e.BuyService);
                    }
                    if (parValue.Contains("CommissionRules"))
                    {
                        cost = cost + result.Sum(e => e.Commission);
                    }
                    if (parValue.Contains("AdministrationFeeRules"))
                    {
                        cost = cost + result.Sum(e => e.AdministrationFee);
                    }
                    if (parValue.Contains("StampFeeRules"))
                    {
                        cost = cost + result.Sum(e => e.StampFee);
                    }
                    if (parValue.Contains("ProfitTaxRules"))
                    {
                        cost = cost + result.Sum(e => e.ProfitService);
                    }
                    if (parValue.Contains("TransferFeeRules"))
                    {
                        cost = cost + result.Sum(e => e.TransferFee);
                    }
                    if (parValue.Contains("SettlementFeeRules"))
                    {
                        cost = cost + result.Sum(e => e.SettlementFee);
                    }
                }
                return cost;
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
        /// 获取最近交易日日期
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLastTradeDate(int hour = 0,int minute=0,int second=0,int day=0) 
        {
            DateTime dateNow = DateTime.Now.AddHours(hour).AddMinutes(minute).AddSeconds(second).Date;
            int i = 0;
            while (true)
            {
                if (Helper.CheckTradeDate(dateNow))
                {
                    if (i >= day)
                    {
                        break;
                    }
                    i++;
                }
                dateNow = dateNow.AddDays(-1);
            }
            return dateNow;
        }

        /// <summary>
        /// 获取最近交易日日期
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLastTradeDate2(int hour = 0, int minute = 0, int second = 0, int day = 0)
        {
            DateTime dateNow = DateTime.Now.AddHours(hour).AddMinutes(minute).AddSeconds(second).Date;
            int index = day > 0 ? 1 : -1;
            day = Math.Abs(day);
            int i = 0;
            while (true)
            {
                if (Helper.CheckTradeDate(dateNow))
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
    }
}
