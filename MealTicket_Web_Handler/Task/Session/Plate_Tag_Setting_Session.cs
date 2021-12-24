using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Tag_Setting_Session
    { 
        public static Dictionary<long, bool> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var tag_setting = (from item in db.t_shares_plate_rel_tag_setting
                                   select item).ToList().ToDictionary(k => long.Parse(k.SharesCode) * 10 + k.Market, v => v.IsAuto);
                return tag_setting;
            }
        }

        public static bool UpdateSessionPart(object newData)
        {
            throw new NotSupportedException();
        }

        public static Dictionary<long, bool> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, bool>;
            var resultData = new Dictionary<long, bool>(data);
            return resultData;
        }
    }
}
