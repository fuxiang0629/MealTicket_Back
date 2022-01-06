using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Limit_Time_Session
    {
        public static List<t_shares_limit_time> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_limit_time
                              select item).ToList();
                return result;
            }
        }

        public static List<t_shares_limit_time> CopySessionData(object objData)
        {
            var data = objData as List<t_shares_limit_time>;
            return new List<t_shares_limit_time>(data);
        }
    }
}
