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

        public static Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>;
            var resultData = new Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>();
            foreach (var item in data)
            {
                if (!resultData.ContainsKey(item.Key))
                {
                    resultData.Add(item.Key, new Dictionary<DateTime, Plate_Quotes_Session_Info>());
                }
                foreach (var item2 in item.Value)
                {
                    if (!resultData[item.Key].ContainsKey(item2.Key))
                    {
                        resultData[item.Key].Add(item2.Key, new Plate_Quotes_Session_Info());
                    }
                    resultData[item.Key][item2.Key].ClosedPrice = item2.Value.ClosedPrice;
                    resultData[item.Key][item2.Key].Date = item2.Value.Date;
                    resultData[item.Key][item2.Key].PlateId = item2.Value.PlateId;
                    resultData[item.Key][item2.Key].RealDays = item2.Value.RealDays;
                    resultData[item.Key][item2.Key].YestodayClosedPrice = item2.Value.YestodayClosedPrice;
                }
            }
            return resultData;
        }
    }
}
