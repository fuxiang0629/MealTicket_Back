using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Quotes_Date_Session
    {
        public static Shares_Quotes_Session_Info_Obj UpdateSession(object oct=null)
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
                string sql = string.Format("exec P_GetShares_Quotes_Date_Session {0}",days);
                var result = db.Database.SqlQuery<Shares_Quotes_Session_Info>(sql).ToList();
                return BuildDic(result, days);
            }
        }

        private static Shares_Quotes_Session_Info_Obj BuildDic(List<Shares_Quotes_Session_Info> dataList,int days)
        {
            Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> session_Dic = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
            foreach (var item in dataList)
            {
                long key = long.Parse(item.SharesCode) * 10 + item.Market;
                if (!session_Dic.ContainsKey(key))
                {
                    session_Dic.Add(key, new Dictionary<DateTime, Shares_Quotes_Session_Info>());
                }
                session_Dic[key][item.Date] = item;
            }
            return new Shares_Quotes_Session_Info_Obj 
            {
                Days=days,
                SessionDic= session_Dic
            };
        }

        public static Shares_Quotes_Session_Info_Obj CopySessionData(object objData)
        {
            var data = objData as Shares_Quotes_Session_Info_Obj;
            var resultData = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
            foreach (var item in data.SessionDic)
            {
                resultData[item.Key] = new Dictionary<DateTime, Shares_Quotes_Session_Info>(item.Value);
            }
            return new Shares_Quotes_Session_Info_Obj
            {
                Days = data.Days,
                SessionDic = resultData
            };
        }
    }
}
