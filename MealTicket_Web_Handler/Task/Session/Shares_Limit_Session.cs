using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Limit_Session
    {
        public static List<Shares_Limit_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select LimitMarket,LimitKey,LimitType from t_shares_limit";
                var result = db.Database.SqlQuery<Shares_Limit_Session_Info>(sql).ToList();
                return result;
            }
        }

        public static List<Shares_Limit_Session_Info> CopySessionData(object objData)
        {
            var data = objData as List<Shares_Limit_Session_Info>;
            var resultData = new List<Shares_Limit_Session_Info>(data);
            return resultData;
        }
    }
}
