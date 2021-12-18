using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_TradeStock_Session
    {
        public static Dictionary<long, Shares_TradeStock_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = string.Format(@"declare @diffDays int;
  declare @dateNow datetime=convert(varchar(10),dateadd(MINUTE,-{1},dateadd(HOUR,-{0},getdate())),120); 
  declare @errorCode int;
  exec P_Check_TradeDate @dateNow,@errorCode output;
  if(@errorCode=0)
  begin
	set @diffDays=-1;
  end
  else
  begin
	set @diffDays=-2;
  end
  declare @disDate datetime;
  set @disDate=dbo.f_getTradeDate(@dateNow,@diffDays);
  declare @maxDate datetime=dateadd(DAY,1,@disDate)

  declare @dateInt bigint=convert(varchar(10),@disDate,112);
  declare @hour int=datepart(HOUR,getdate());
  declare @minute int=datepart(MINUTE,getdate());
  declare @timeSpan bigint=@hour*100+@minute;
  if(@timeSpan<{2} or @timeSpan>{3} or @errorCode<>0)
  begin
	set @timeSpan={3};
  end
  else if(@timeSpan<{4})
  begin
	set @timeSpan={4};
  end

  declare @GroupTimeKeyNow bigint=@dateInt*10000+@timeSpan;

  
  declare @dateMaxInt bigint=convert(varchar(10),@maxDate,112);
  declare @endGroupTimeKeyNow bigint=@dateMaxInt*10000+@timeSpan;
  declare @startGroupTimeKeyNow bigint=@endGroupTimeKeyNow-{5};
  if(@startGroupTimeKeyNow%100>=60)
  begin
	set @startGroupTimeKeyNow=@startGroupTimeKeyNow-40;
  end

  select t.Market,t.SharesCode,t.TradeStock_Yestoday,t.TradeStock_Now,t.GroupTimeKey,t1.TradeStock,t1.TradeStock_Interval,t1.TradeStock_Interval_Count
  from
  (
	  select Market,SharesCode,sum(TradeStock)TradeStock_Yestoday,sum(case when GroupTimeKey<=@GroupTimeKeyNow then TradeStock else 0 end)TradeStock_Now,@timeSpan GroupTimeKey
	  from t_shares_securitybarsdata_1min
	  where [Time]>@disDate and [Time]<@maxDate
	  group by Market,SharesCode
  )t
  left join 
  (  
	  select Market,SharesCode,sum(TradeStock)TradeStock,
	  sum(case when GroupTimeKey>@startGroupTimeKeyNow and GroupTimeKey<=@endGroupTimeKeyNow then TradeStock else 0 end)TradeStock_Interval,
	  count(case when GroupTimeKey>@startGroupTimeKeyNow and GroupTimeKey<=@endGroupTimeKeyNow then 1 else null end)TradeStock_Interval_Count
	  from t_shares_securitybarsdata_1min
	  where [Time]>@maxDate
	  group by Market,SharesCode
  )t1 on t.SharesCode=t1.SharesCode and t.Market=t1.Market", Singleton.Instance.Time1EndInt/100, Singleton.Instance.Time1EndInt%100, Singleton.Instance.Time1EndInt, Singleton.Instance.Time4EndInt,Singleton.Instance.Time3Start1Int,Singleton.Instance.SharesStockCal_IntervalMinute);
                var result = db.Database.SqlQuery<Shares_TradeStock_Session_Info>(sql).ToList();
                return result.ToDictionary(k => long.Parse(k.SharesCode) * 10 + k.Market, v => v);
            }
        }
        public static Dictionary<long, Shares_TradeStock_Session_Info> UpdateSessionPart(object newData)
        {
            throw new NotSupportedException();
        }

        public static long GetRemainMinute(long timeKey) 
        {
            if (timeKey < 930)
            {
                return 240;
            }
            else if (timeKey < 1000)
            {
                return (960 - timeKey) + 210;
            }
            else if (timeKey < 1100)
            {
                return (1060 - timeKey) + 150;
            }
            else if (timeKey <= 1130)
            {
                return (1130 - timeKey) + 120;
            }
            else if (timeKey < 1300)
            {
                return 120;
            }
            else if (timeKey < 1400)
            {
                return (1360 - timeKey) + 60;
            }
            else if (timeKey < 1500)
            {
                return 1460 - timeKey;
            }
            else
            {
                return 0;
            }
        }
    }
}
