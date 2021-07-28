using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.session
{
    public class AccountRiseLimitTriSession:Session<List<AccountRiseLimitTriInfo>>
    {
        DateTime? lastDataTime;
        List<AccountRiseLimitTriInfo> lastList = new List<AccountRiseLimitTriInfo>();

        public AccountRiseLimitTriSession() 
        {
            Name = "AccountRiseLimitTriSession";
        }

        public override List<AccountRiseLimitTriInfo> UpdateSession()
        {
            DateTime dateNow = Helper.GetLastTradeDate(-9, 0, 0);
            if (lastDataTime == null || lastDataTime.Value < dateNow)
            {
                lastList = new List<AccountRiseLimitTriInfo>();
                lastDataTime = dateNow;
            }

            using (var db = new meal_ticketEntities())
            {
                string sql = string.Format(@"select t.SharesCode RelId, t.Market,t.SharesCode,t.ClosedPrice,t.PresentPrice,t.[Type],t2.PushTime,t2.TriCountToday
  from
  (
  select Market, SharesCode, ClosedPrice, PresentPrice, -1[Type]
  from t_shares_quotes_date with(nolock)
  where [Date] = '{0}' and PriceType = 1--涨停池
  union all
  select Market, SharesCode, ClosedPrice, PresentPrice, -2[Type]
  from t_shares_quotes_date with(nolock)
  where [Date] = '{0}' and PriceType = 2--跌停池
  union all
  select Market, SharesCode, ClosedPrice, PresentPrice, -3[Type]
  from t_shares_quotes_date with(nolock)
  where [Date] = '{0}' and PriceType = 0 and LimitUpCount > 0--炸板池
  union all
  select Market, SharesCode, ClosedPrice, PresentPrice, -4[Type]
  from t_shares_quotes_date with(nolock)
  where [Date] = '{0}' and PriceType = 0 and LimitDownCount > 0--翘板池
  union all
  select Market, SharesCode, ClosedPrice, PresentPrice, -5[Type]
  from t_shares_quotes_date with(nolock)
  where [Date] = '{0}' and TriNearLimitType = 1 and LimitUpCount = 0--即将涨停池
  union all
  select Market, SharesCode, ClosedPrice, PresentPrice, -6[Type]
  from t_shares_quotes_date with(nolock)
  where [Date] = '{0}' and TriNearLimitType = 2 and LimitDownCount = 0--即将跌停池
  )t
  left join
  (
    select Market, SharesCode,
	case when[Type] = 1 and BusinessType = 1 then - 1 when[Type] = 2 and BusinessType = 1 then - 2 when[Type] = 3 and BusinessType = 1 then - 3

    when[Type] = 4 and BusinessType = 1 then - 4 when[Type] = 5 and BusinessType = 2 then - 5

    when[Type] = 6 and BusinessType = 2 then - 6 else 0 end[Type],
	BusinessType,count(*) TriCountToday,Max(CreateTime) PushTime
    from t_shares_quotes_history with(nolock)
    where [Date]= '{0}'

    group by Market,SharesCode,[Type],BusinessType
  )t2 on t.Market = t2.Market and t.SharesCode = t2.SharesCode and t.[Type]= t2.[Type]
  where convert(varchar(20),t2.PushTime,120)> '{1}'
  order by t2.PushTime desc, t.SharesCode", dateNow.ToString("yyyy-MM-dd"), lastDataTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                var list = db.Database.SqlQuery<AccountRiseLimitTriInfo>(sql).ToList();
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
