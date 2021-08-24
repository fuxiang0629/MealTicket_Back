using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class SharesLimitTimeSession : Session<List<t_shares_limit_time>>
    {
        public SharesLimitTimeSession()
        {
            Name = "SharesLimitTimeSession";
        }
        public override List<t_shares_limit_time> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select * from t_shares_limit_time with(nolock)";
                var result = db.Database.SqlQuery<t_shares_limit_time>(sql).ToList();
                return result;
            }
        }
    }
}
