using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Quotes_Date_Session
    {
        public static Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = "exec P_GetPlate_Quotes_Date_Session";
                var result = db.Database.SqlQuery<Plate_Quotes_Session_Info>(sql).ToList();
                return BuildDic(result);
            }
        }

        private static Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> BuildDic(List<Plate_Quotes_Session_Info> dataList)
        {
            Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> session_Dic = new Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>();
            foreach (var item in dataList)
            {
                if (!session_Dic.ContainsKey(item.PlateId))
                {
                    session_Dic.Add(item.PlateId, new Dictionary<DateTime, Plate_Quotes_Session_Info>());
                }
                session_Dic[item.PlateId][item.Date] = item;
            }
            return session_Dic;
        }
        public static Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> UpdateSessionPart(object newData)
        {
            throw new NotSupportedException();
        }
    }
}
