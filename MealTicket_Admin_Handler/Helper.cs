using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler
{
    public class Helper
    {

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
    }
}
