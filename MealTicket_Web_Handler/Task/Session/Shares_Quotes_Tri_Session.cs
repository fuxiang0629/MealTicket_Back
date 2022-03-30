using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Quotes_Tri_Session
    {
        public static Dictionary<long, t_shares_quotes_tri> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_quotes_tri
                              select item).ToList();
                return result.ToDictionary(k => k.SharesKey.Value, v => v);
            }
        }

        public static Dictionary<long, t_shares_quotes_tri> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, t_shares_quotes_tri>;
            return new Dictionary<long, t_shares_quotes_tri>(data);
        }
    }
}
