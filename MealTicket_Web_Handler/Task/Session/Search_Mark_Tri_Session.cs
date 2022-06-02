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
        public static Dictionary<long, Dictionary<long, t_sys_conditiontrade_template>> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_search_mark_tri
                              join item2 in db.t_sys_conditiontrade_template on item.TemplateId equals item2.Id
                              where item2.Type==6 && item2.Status==1
                              select new { item,item2}).ToList();
                return result.GroupBy(e => e.item2.Id).ToDictionary(k => k.Key, v => v.ToDictionary(xk => xk.item.SharesKey, xv => xv.item2));
            }
        }

        public static Dictionary<long, Dictionary<long, t_sys_conditiontrade_template>> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Dictionary<long, t_sys_conditiontrade_template>>;
            return new Dictionary<long, Dictionary<long, t_sys_conditiontrade_template>>(data);
        }
    }
}
