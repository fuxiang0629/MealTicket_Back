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

        public static Dictionary<long, Plate_Quotes_Session_Info> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Plate_Quotes_Session_Info>;
            var resultData = new Dictionary<long, Plate_Quotes_Session_Info>();
            foreach (var item in data)
            {
                if (!resultData.ContainsKey(item.Key))
                {
                    resultData.Add(item.Key, new Plate_Quotes_Session_Info());
                }
                resultData[item.Key].ClosedPrice = item.Value.ClosedPrice;
                resultData[item.Key].Date = item.Value.Date;
                resultData[item.Key].PlateId = item.Value.PlateId;
                resultData[item.Key].RealDays = item.Value.RealDays;
                resultData[item.Key].YestodayClosedPrice = item.Value.YestodayClosedPrice;
            }
            return resultData;
        }
    }
}
