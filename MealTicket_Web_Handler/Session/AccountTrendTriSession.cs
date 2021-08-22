using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class AccountTrendTriSession:Session<List<AccountTrendTriInfo_Session>>
    {
        DateTime? lastDataTime;
        List<AccountTrendTriInfo_Session> lastList = new List<AccountTrendTriInfo_Session>();


        public AccountTrendTriSession()
        {
            Name = "AccountTrendTriSession";
        }
        public override List<AccountTrendTriInfo_Session> UpdateSession()
        {
            DateTime dateNow = Helper.GetLastTradeDate(-9, 0, 0);
            if (lastDataTime == null || lastDataTime.Value < dateNow)
            {
                lastList = new List<AccountTrendTriInfo_Session>();
                lastDataTime = dateNow;
            }

            using (var db = new meal_ticketEntities())
            {
                string sql = string.Format(@"select t.AccountId,t.Id OptionalId,t.Market,t.SharesCode,t.TriPushCount TriCountToday,t.LastPushTime PushTime,t.TrendId,t.RelId,t.LastPushDesc TriDesc
from
(
	select t.AccountId,t.Id,t.Market,t.SharesCode,t7.TriPushCount,t2.LastPushTime,t1.TrendId,
	t1.Id RelId,t2.LastPushDesc,
	ROW_NUMBER()OVER(partition by t.Market,t.SharesCode,t.AccountId order by t2.LastPushTime desc) num
	from t_account_shares_optional t with(nolock)
	inner join t_account_shares_optional_trend_rel t1 with(nolock) on t.Id=t1.OptionalId
	inner join t_account_shares_optional_trend_rel_tri t2 with(nolock) on t1.Id=t2.RelId
	inner join t_shares_monitor t6 with(nolock) on t.Market=t6.Market and t.SharesCode=t6.SharesCode 
	inner join t_account_shares_optional_trend_rel_tri_record_statistic t7 with(nolock) on t.Market=t7.Market and t.SharesCode=t7.SharesCode 
	and t.AccountId=t7.AccountId and t7.[Date]='{0}'
	where t1.[Status]=1 and convert(varchar(20),t2.LastPushTime,120)>'{1}' and t.IsTrendClose=0 
)t
where t.num=1", dateNow.ToString("yyyy-MM-dd"), lastDataTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                var list= db.Database.SqlQuery<AccountTrendTriInfo_Session>(sql).ToList();
                if (list.Count() > 0)
                {
                    lastDataTime = list.Max(e => e.PushTime);
                    foreach (var item in list)
                    {
                        lastList.RemoveAll(e => e.Market == item.Market && e.SharesCode == item.SharesCode);
                    }
                    lastList.AddRange(list);
                }
                return lastList;
            }
        }
    }
}
