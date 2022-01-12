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
        public static Plate_Quotes_Session_Info_Obj UpdateSession(object oct = null)
        {
            int days = 0;
            if (oct != null)
            {
                days = (int)oct;
            }

            if (days < 15)
            {
                days = 15;
            }

            using (var db = new meal_ticketEntities())
            {
                string sql = string.Format("exec P_GetPlate_Quotes_Date_Session {0}", days);
                var result = db.Database.SqlQuery<Plate_Quotes_Session_Info>(sql).ToList();
                return BuildDic(result,days);
            }
        }

        private static Plate_Quotes_Session_Info_Obj BuildDic(List<Plate_Quotes_Session_Info> dataList, int days)
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
            return new Plate_Quotes_Session_Info_Obj
            {
                Days = days,
                SessionDic = session_Dic
            };
        }

        public static Plate_Quotes_Session_Info_Obj CopySessionData(object objData)
        {
            var data = objData as Plate_Quotes_Session_Info_Obj;
            var resultData = new Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>();
            foreach (var item in data.SessionDic)
            {
                resultData[item.Key] = new Dictionary<DateTime, Plate_Quotes_Session_Info>(item.Value);
            }
            return new Plate_Quotes_Session_Info_Obj
            {
                Days = data.Days,
                SessionDic = resultData
            };
        }
    }
}
