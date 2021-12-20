using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Quotes_Today_Session
    {
        public static Dictionary<long,Shares_Quotes_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = "exec P_GetShares_Quotes_Today_Session";
                var result = db.Database.SqlQuery<Shares_Quotes_Session_Info>(sql).ToList();
                return result.ToDictionary(k => long.Parse(k.SharesCode) * 10 + k.Market, v => v);
            }
        }
        public static Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> UpdateSessionPart(object newData)
        {
            throw new NotSupportedException();
        }

        public static Dictionary<long, Shares_Quotes_Session_Info> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Shares_Quotes_Session_Info>;
            var resultData = new Dictionary<long, Shares_Quotes_Session_Info>();
            foreach (var item in data)
            {
                if (!resultData.ContainsKey(item.Key))
                {
                    resultData.Add(item.Key, new Shares_Quotes_Session_Info());
                }
                resultData[item.Key].ClosedPrice = item.Value.ClosedPrice;
                resultData[item.Key].Date = item.Value.Date;
                resultData[item.Key].IsLimitUp = item.Value.IsLimitUp;
                resultData[item.Key].LimitUpTime = item.Value.LimitUpTime;
                resultData[item.Key].Market = item.Value.Market;
                resultData[item.Key].OpenedPrice = item.Value.OpenedPrice;
                resultData[item.Key].SharesCode = item.Value.SharesCode;
                resultData[item.Key].TotalAmount = item.Value.TotalAmount;
                resultData[item.Key].TotalCount = item.Value.TotalCount;
                resultData[item.Key].YestodayClosedPrice = item.Value.YestodayClosedPrice;
            }
            return resultData;
        }
    }
}
