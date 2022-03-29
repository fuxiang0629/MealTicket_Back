using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Search_Mark_Tri_Session
    {
        public static Dictionary<long, Dictionary<long, DateTime>> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_search_mark_tri
                              select item).ToList();
                return result.GroupBy(e => e.TemplateId).ToDictionary(k => k.Key, v => v.ToDictionary(xk => xk.SharesKey, xv => xv.CreateTime));
            }
        }

        public static Dictionary<long, Dictionary<long, DateTime>> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Dictionary<long, DateTime>>;
            return new Dictionary<long, Dictionary<long, DateTime>>(data);
        }
    }
}
