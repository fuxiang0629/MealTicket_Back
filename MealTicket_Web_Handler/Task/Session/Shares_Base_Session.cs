using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Base_Session
    {
        public static Dictionary<long, Shares_Base_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select Market,SharesCode,SharesName,SharesPyjc,TotalCapital,CirculatingCapital,MarketStatus from v_shares_baseinfo";
                var result = db.Database.SqlQuery<Shares_Base_Session_Info>(sql).ToList();
                return result.ToDictionary(k => long.Parse(k.SharesCode)*10+k.Market, v => v) ;
            }
        }
        public static Dictionary<long, Shares_Base_Session_Info> UpdateSessionPart(object newData)
        {
            throw new NotSupportedException();
        }
    }
}
