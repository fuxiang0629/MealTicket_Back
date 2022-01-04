using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Minute_KLine_Session
	{
		public static Dictionary<long, List<Shares_Minute_KLine_Session_Info>> UpdateSession()
		{
			DateTime dateNow = DbHelper.GetLastTradeDate2();
			using (var db = new meal_ticketEntities())
			{
				string sql = string.Format(@"select Market,SharesCode,[Time],ClosedPrice,PreClosePrice
		from
		(
			select Market,SharesCode,[Time],ClosedPrice,PreClosePrice,ROW_NUMBER()OVER(partition by Market,SharesCode order by [Time] desc)num
			from t_shares_securitybarsdata_1min
			where [Time]>'{0}'
		)t
		where t.num<=15", dateNow.ToString("yyyy-MM-dd"));
				var result = db.Database.SqlQuery<Shares_Minute_KLine_Session_Info>(sql).ToList();
				return result.GroupBy(e => new { e.Market, e.SharesCode }).ToDictionary(k => long.Parse(k.Key.SharesCode) * 10 + k.Key.Market, v => v.OrderByDescending(e => e.Time).ToList());
			}
		}

		public static Dictionary<long, List<Shares_Minute_KLine_Session_Info>> CopySessionData(object objData)
		{
			var data = objData as Dictionary<long, List<Shares_Minute_KLine_Session_Info>>;
			Dictionary<long, List<Shares_Minute_KLine_Session_Info>> resultData = new Dictionary<long, List<Shares_Minute_KLine_Session_Info>>();
			foreach (var item in data)
			{
				resultData[item.Key] = new List<Shares_Minute_KLine_Session_Info>(item.Value);
			}
			return resultData;
		}
	}
}
