using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Quotes_Today_Session
    {
        public static Dictionary<long,Shares_Quotes_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = "exec P_GetShares_Quotes_Today_Session";
                var result = db.Database.SqlQuery<Shares_Quotes_Session_Info>(sql).ToList();
                return result.ToDictionary(k => long.Parse(k.SharesCode) * 10 + k.Market, v => v);
            }
        }
        public static Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> UpdateSessionPart(object newData)
        {
            throw new NotSupportedException();
        }
    }
}
