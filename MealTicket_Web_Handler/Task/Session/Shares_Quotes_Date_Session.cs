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
            Dictionary<long, SortedSet<Shares_Quotes_Session_Info>> open_session_Dic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>();
            Dictionary<long, SortedSet<Shares_Quotes_Session_Info>> close_session_Dic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>();
            Dictionary<long, SortedSet<Shares_Quotes_Session_Info>> max_session_Dic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>();
            Dictionary<long, SortedSet<Shares_Quotes_Session_Info>> min_session_Dic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>();
            foreach (var item in dataList)
            {
                long key = long.Parse(item.SharesCode) * 10 + item.Market;
                if (!session_Dic.ContainsKey(key))
                {
                    session_Dic.Add(key, new Dictionary<DateTime, Shares_Quotes_Session_Info>());
                    open_session_Dic.Add(key,new SortedSet<Shares_Quotes_Session_Info>());
                    close_session_Dic.Add(key, new SortedSet<Shares_Quotes_Session_Info>());
                    max_session_Dic.Add(key, new SortedSet<Shares_Quotes_Session_Info>());
                    min_session_Dic.Add(key, new SortedSet<Shares_Quotes_Session_Info>());
                }
                session_Dic[key][item.Date] = item;
                item.SortedType = 1;
                open_session_Dic[key].Add(item);
                item.SortedType = 2;
                close_session_Dic[key].Add(item);
                item.SortedType = 3;
                max_session_Dic[key].Add(item);
                item.SortedType = 4;
                min_session_Dic[key].Add(item);
            }
            return new Shares_Quotes_Session_Info_Obj 
            {
                Days=days,
                SessionDic= session_Dic,
                CloseSessionDic= close_session_Dic,
                MaxSessionDic=max_session_Dic,
                OpenSessionDic=open_session_Dic,
                MinSessionDic=min_session_Dic
            };
        }

        public static Shares_Quotes_Session_Info_Obj CopySessionData(object objData)
        {
            var data = objData as Shares_Quotes_Session_Info_Obj;
            var resultData = new Shares_Quotes_Session_Info_Obj();
            resultData.Days = data.Days;
            resultData.SessionDic = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
            resultData.OpenSessionDic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>();
            resultData.CloseSessionDic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>();
            resultData.MaxSessionDic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>();
            resultData.MinSessionDic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>();
            foreach (var item in data.SessionDic)
            {
                resultData.SessionDic[item.Key] = new Dictionary<DateTime, Shares_Quotes_Session_Info>(item.Value);
            }
            foreach (var item in data.OpenSessionDic)
            {
                resultData.OpenSessionDic[item.Key] = item.Value;
            }
            foreach (var item in data.CloseSessionDic)
            {
                resultData.CloseSessionDic[item.Key] = item.Value;
            }
            foreach (var item in data.MaxSessionDic)
            {
                resultData.MaxSessionDic[item.Key] = item.Value;
            }
            foreach (var item in data.MinSessionDic)
            {
                resultData.MinSessionDic[item.Key] = item.Value;
            }
            return resultData;
        }
    }
}
