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
    }
}
