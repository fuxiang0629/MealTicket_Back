using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesTradeService
{
    public class Helper
    {
        /// <summary>
        /// 检查当天是否交易日
        /// </summary>
        /// <returns></returns>
        public static bool CheckTradeDate(DateTime? time = null) 
        {
            try
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
            catch (Exception ex)
            {
                Logger.WriteFileLog("检查当天是否交易日出错", ex);
                return false;
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
            try
            {
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
            catch (Exception ex)
            {
                Logger.WriteFileLog("检查当前是否交易时间出错", ex);
                return false;
            }
        }

        //检查两个数组是否相等

        public static bool CompareArray<T>(T[] bt1, T[] bt2)
        {
            var len1 = bt1.Length;
            var len2 = bt2.Length;
            if (len1 != len2)
            {
                return false;
            }
            for (var i = 0; i < len1; i++)
            {
                if (!bt1[i].Equals(bt2[i]))
                    return false;
            }
            return true;
        }
    }
}
