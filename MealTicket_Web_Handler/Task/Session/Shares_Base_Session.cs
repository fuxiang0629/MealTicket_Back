using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Base_Session
    {
        public static Dictionary<long, Shares_Base_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select Market,SharesCode,SharesName,SharesPyjc,TotalCapital,CirculatingCapital,MarketStatus from v_shares_baseinfo";
                var result = db.Database.SqlQuery<Shares_Base_Session_Info>(sql).ToList();
                return result.ToDictionary(k => long.Parse(k.SharesCode)*10+k.Market, v => v) ;
            }
        }
        public static Dictionary<long, Shares_Base_Session_Info> UpdateSessionPart(object newData)
        {
            throw new NotSupportedException();
        }

        public static Dictionary<long, Shares_Base_Session_Info> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Shares_Base_Session_Info>;
            var resultData = new Dictionary<long, Shares_Base_Session_Info>();
            foreach (var item in data)
            {
                if (!resultData.ContainsKey(item.Key))
                {
                    resultData.Add(item.Key, new Shares_Base_Session_Info());
                }
                resultData[item.Key].CirculatingCapital = item.Value.CirculatingCapital;
                resultData[item.Key].Market = item.Value.Market;
                resultData[item.Key].MarketStatus = item.Value.MarketStatus;
                resultData[item.Key].SharesCode = item.Value.SharesCode;
                resultData[item.Key].SharesName = item.Value.SharesName;
                resultData[item.Key].SharesPyjc = item.Value.SharesPyjc;
                resultData[item.Key].TotalCapital = item.Value.TotalCapital;
            }
            return resultData;
        }
    }
}
