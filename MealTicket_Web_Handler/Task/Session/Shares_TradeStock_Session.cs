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
        public static Dictionary<long, Shares_TradeStock_Session_Info> UpdateSession2()
        {
            using (var db = new meal_ticketEntities())
            {
                db.Database.CommandTimeout = 600;
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
  declare @maxDate datetime=dbo.f_getTradeDate(@disDate,1)

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
  else if(@timeSpan>{5} and @timeSpan<{6})
  begin
	set @timeSpan={5};
  end

  declare @GroupTimeKeyNow bigint=@dateInt*10000+@timeSpan;

  
  declare @dateMaxInt bigint=convert(varchar(10),@maxDate,112);
  declare @endGroupTimeKeyNow bigint=@dateMaxInt*10000+@timeSpan;
  declare @startGroupTimeKeyNow bigint=@endGroupTimeKeyNow-{7};

  declare @endTimeSpan bigint=@startGroupTimeKeyNow%10000;
  if(@endTimeSpan<{4})
  begin
	set @startGroupTimeKeyNow=(@startGroupTimeKeyNow/10000*10000)+{4};
  end
  else if(@endTimeSpan>{5} and @endTimeSpan<{6})
  begin
	set @startGroupTimeKeyNow=(@startGroupTimeKeyNow/10000*10000)+{5};
  end

  if(@startGroupTimeKeyNow%100>=60)
  begin
	set @startGroupTimeKeyNow=@startGroupTimeKeyNow-40;
  end

  select t.Market,t.SharesCode,t1.TradeStock_Now,t.TradeStock_Yes,isnull(t1.TradeStock_Avg,0)TradeStock_Avg,t.TimeSpan
  from
  (
	  select Market,SharesCode,sum(case when GroupTimeKey<=@GroupTimeKeyNow then TradeStock else 0 end)TradeStock_Yes,@timeSpan TimeSpan
	  from t_shares_securitybarsdata_1min
	  where [Time]>@disDate and [Time]<@maxDate
	  group by Market,SharesCode
  )t
  left join 
  (  
	  select Market,SharesCode,sum(TradeStock)TradeStock_Now,
	  AVG(case when GroupTimeKey>@startGroupTimeKeyNow and GroupTimeKey<=@endGroupTimeKeyNow then TradeStock else null end)TradeStock_Avg
	  from t_shares_securitybarsdata_1min
	  where [Time]>@maxDate and [Time]<@maxDate+1
	  group by Market,SharesCode
  )t1 on t.SharesCode=t1.SharesCode and t.Market=t1.Market", Singleton.Instance.Time1EndInt / 100, Singleton.Instance.Time1EndInt % 100, Singleton.Instance.Time1EndInt, Singleton.Instance.Time4EndInt, Singleton.Instance.Time3Start1Int, Singleton.Instance.Time3End1Int, Singleton.Instance.Time3Start2Int, Singleton.Instance.SharesStockCal_IntervalMinute);

                var result = db.Database.SqlQuery<Shares_TradeStock_Session_Info>(sql).ToList();
                return result.ToDictionary(k => long.Parse(k.SharesCode) * 10 + k.Market, v => v);
            }
        }
        public static Dictionary<long, Shares_TradeStock_Session_Info> UpdateSession3()
        {
            DateTime timeNow = DateTime.Now;
            DateTime dateNow = DbHelper.GetLastTradeDate2(0, 0, 0, 0, timeNow.AddHours(-9));
            DateTime datePre= DbHelper.GetLastTradeDate2(0, 0, 0, -1, dateNow);

            DateTime time1 = dateNow.AddHours(9).AddMinutes(30);
            DateTime time2 = dateNow.AddHours(11).AddMinutes(30);
            DateTime time3 = dateNow.AddHours(13);
            if (timeNow.Date != dateNow.Date || timeNow.Hour >= 15)
            {
                timeNow = dateNow.AddHours(15);
            }
            else if (timeNow < time1)
            {
                timeNow = time1;
            }
            else if (timeNow > time2 && timeNow < time3)
            {
                timeNow = time2;
            }
            DateTime dateAvg = timeNow.AddMinutes(-Singleton.Instance.SharesStockCal_MinMinute*2);
            if (dateAvg < time1)
            {
                dateAvg = time1;
            }
            else if (dateAvg > time2 && dateAvg < time3)
            {
                dateAvg = dateAvg.AddMinutes(-90);
            }
            using (var db = new meal_ticketEntities())
            {
                db.Database.CommandTimeout = 600;
                string sql = string.Format(@"select t.Market,t.SharesCode,t.LastTradeStock TradeStock_Now,t1.LastTradeStock TradeStock_Yes,t2.TradeStock TradeStock_Avg,t.timeSpan
from
(
    select Market,SharesCode,LastTradeStock,GroupTimeKey,timeSpan
	from
	(
		select Market,SharesCode,LastTradeStock+TradeStock LastTradeStock,GroupTimeKey,GroupTimeKey%10000 timeSpan,ROW_NUMBER()OVER(partition by Market,SharesCode order by GroupTimeKey desc)num
		from t_shares_securitybarsdata_1min WITH (INDEX(index_time_key))
		where [Time]>='{0}' and [Time]<'{1}' and IsLast=1
	)t
	where t.num=1
)t 
inner join
(
	select Market,SharesCode,LastTradeStock+TradeStock LastTradeStock,GroupTimeKey,GroupTimeKey%10000 timeSpan
	from t_shares_securitybarsdata_1min WITH (INDEX(index_time_key))
	where [Time]>='{2}' and [Time]<'{3}'
)t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode and t.timeSpan=t1.timeSpan
inner join
(
	select Market,SharesCode,AVG(TradeStock)TradeStock
	from
	(
		select Market,SharesCode,TradeStock,ROW_NUMBER()OVER(partition by Market,SharesCode order by GroupTimeKey desc) num
		from t_shares_securitybarsdata_1min WITH (INDEX(index_time_key))
		where [Time]>'{4}' and [Time]<'{1}'
	)t
	where num<={5}
	group by Market,SharesCode
)t2 on t.Market=t2.Market and t.SharesCode=t2.SharesCode", dateNow.ToString("yyyy-MM-dd 09:30:00"), dateNow.AddDays(1).ToString("yyyy-MM-dd"), datePre.ToString("yyyy-MM-dd 09:30:00"), datePre.AddDays(1).ToString("yyyy-MM-dd"), dateAvg.ToString("yyyy-MM-dd HH:mm:00"), Singleton.Instance.SharesStockCal_MinMinute);
                
                var result = db.Database.SqlQuery<Shares_TradeStock_Session_Info>(sql).ToList();
                return result.ToDictionary(k => long.Parse(k.SharesCode) * 10 + k.Market, v => v);
            }
        }
        public static Dictionary<long, Shares_TradeStock_Session_Info> UpdateSession()
        {
            string sql = @"select t.SharesKey,t.Market,t.SharesCode,t.TradeStockNow TradeStock_Now,t.TradeStockAvg TradeStock_Avg,t1.TradeStock+t1.LastTradeStock TradeStock_Yes,t.TimeSpan
  from t_shares_tradestock_statistic t
  inner join t_shares_securitybarsdata_1min t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode and t.YesTime=t1.[Time]";
            using (var db = new meal_ticketEntities())
            {
                var result = db.Database.SqlQuery<Shares_TradeStock_Session_Info>(sql).ToDictionary(k=>k.SharesKey,v=>v);
                return result;
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

        public static Dictionary<long, Shares_TradeStock_Session_Info> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Shares_TradeStock_Session_Info>;
            var resultData = new Dictionary<long, Shares_TradeStock_Session_Info>(data);
            return resultData;
        }
    }
}
