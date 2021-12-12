using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Quotes_Today_Session
    {
        public static Dictionary<long, Plate_Quotes_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = "exec P_GetPlate_Quotes_Today_Session";
                var result = db.Database.SqlQuery<Plate_Quotes_Session_Info>(sql).ToList();
                return result.ToDictionary(k => k.PlateId, v => v);
            }
        }
        public static Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> UpdateSessionPart(object newData)
        {
            throw new NotSupportedException();
        }
    }
}
