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
        public static Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = "exec P_GetShares_Quotes_Date_Session";
                var result = db.Database.SqlQuery<Shares_Quotes_Session_Info>(sql).ToList();
                return BuildDic(result);
            }
        }

        private static Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> BuildDic(List<Shares_Quotes_Session_Info> dataList)
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
            return session_Dic;
        }

        public static Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> UpdateSessionPart(object newData)
        {
            throw new NotSupportedException();
        }

        public static Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>;
            var resultData = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
            foreach (var item in data)
            {
                if (!resultData.ContainsKey(item.Key))
                {
                    resultData.Add(item.Key, new Dictionary<DateTime, Shares_Quotes_Session_Info>());
                }
                foreach (var item2 in item.Value)
                {
                    if (!resultData[item.Key].ContainsKey(item2.Key))
                    {
                        resultData[item.Key].Add(item2.Key, new Shares_Quotes_Session_Info());
                    }
                    resultData[item.Key][item2.Key].ClosedPrice = item2.Value.ClosedPrice;
                    resultData[item.Key][item2.Key].Date = item2.Value.Date;
                    resultData[item.Key][item2.Key].IsLimitUp = item2.Value.IsLimitUp;
                    resultData[item.Key][item2.Key].LimitUpTime = item2.Value.LimitUpTime;
                    resultData[item.Key][item2.Key].OpenedPrice = item2.Value.OpenedPrice;
                    resultData[item.Key][item2.Key].Market = item2.Value.Market;
                    resultData[item.Key][item2.Key].TotalAmount = item2.Value.TotalAmount;
                    resultData[item.Key][item2.Key].TotalCount = item2.Value.TotalCount;
                    resultData[item.Key][item2.Key].YestodayClosedPrice = item2.Value.YestodayClosedPrice;
                    resultData[item.Key][item2.Key].SharesCode = item2.Value.SharesCode;
                }
            }
            return resultData;
        }
    }
}
