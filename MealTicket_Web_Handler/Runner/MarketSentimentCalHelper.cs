using FXCommon.Common;
using MealTicket_DBCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Runner
{
    public class SharesMtInfo
    {
        public long SharesKey { get; set; }

        public DateTime Time { get; set; }

        public long GroupTimeKey { get; set; }

        public long ClosedPrice { get; set; }

        public long YestodayClosedPrice { get; set; }

        public int RiseRate 
        {
            get 
            {
                if (YestodayClosedPrice == 0)
                {
                    return 0;
                }
                return (int)Math.Round((ClosedPrice - YestodayClosedPrice) * 1.0 / YestodayClosedPrice*10000, 0);
            }
        }

        public int PriceType { get; set; }

        public bool IsLimitUpBomb { get; set; }

        public bool IsLimitDownBomb { get; set; }
    }

    public class MarketsentimentBaseinfo
    {
        public DateTime Date { get; set; }

        public DateTime Time { get; set; }

        public long GroupTimeKey { get; set; }

        public int LimitUpCount { get; set; }

        public int LimitDownCount { get; set; }

        public int RiseUpCount { get; set; }

        public int RiseDownCount { get; set; }

        public int RiseFlatCount { get; set; }

        public int RiseUpYesMinCount { get; set; }

        public int LimitUpYesCount { get; set; }

        public int LimitUpYesRiseRate 
        {
            get 
            {
                if (LimitUpYesCount == 0)
                {
                    return 0;
                }
                return TotalLimitUpYesRiseRate / LimitUpYesCount;
            }
        }

        public int TotalLimitUpYesRiseRate { get; set; }

        public int LimitUpBombCount { get; set; }

        public int LimitDownBombCount { get; set; }

        public int HighMarkYesCount { get; set; }

        public int HighMarkYesRiseRate 
        {
            get 
            {
                if (HighMarkYesCount == 0)
                {
                    return 0;
                }
                return TotalHighMarkYesRiseRate / HighMarkYesCount;
            } 
        }

        public int TotalHighMarkYesRiseRate { get; set; }

        public int TotalSharesCount { get; set; }
    }

    public class MarketSentimentCalHelper
    {
        public static void Cal_MarketSentiment(DateTime date,bool IsReal) 
        {
            DateTime endDate = date.AddDays(1);
            DateTime startTime = date;
            if (IsReal)
            {
                using (var db = new meal_ticketEntities())
                {
                    var marketsentiment_baseinfo = (from item in db.t_shares_marketsentiment_baseinfo
                                                    where item.Date == date
                                                    orderby item.GroupTimeKey descending
                                                    select item).FirstOrDefault();
                    if (marketsentiment_baseinfo != null)
                    {
                        startTime = marketsentiment_baseinfo.Time;
                    }
                }
            }
            var result = _cal_MarketSentiment_oneday(date, startTime, endDate);
            ToWriteToDataBase(result);
        }

        public static List<MarketsentimentBaseinfo> _cal_MarketSentiment_oneday(DateTime date,DateTime startTime,DateTime endTime)
        {
            DateTime preDate = DbHelper.GetLastTradeDate2(0, 0, 0, -1, date);
            var shares_quotes_yes = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(preDate, false);//昨日行情
            var shares_limit = Singleton.Instance.sessionHandler.GetShares_Limit_Session(false);
            var shares_base = Singleton.Instance.sessionHandler.GetShares_Base_Session(false);
            Dictionary<long, long> limitUpYesSession = new Dictionary<long, long>();//昨日涨停股票
            foreach (var item in shares_quotes_yes)
            {
                if (shares_limit.Contains(item.Key))
                {
                    continue;
                }
                if (item.Value.PriceType == 1)
                {
                    limitUpYesSession.Add(item.Key, item.Key);
                }
            }

            Dictionary<long, long> highMarkYesSession = new Dictionary<long, long>();//昨日高标股票
            Dictionary<long, int> riseUpYesCount = new Dictionary<long, int>();//昨日上涨家数

            long highMarkAccountId = Singleton.Instance.HighMarkAccountId;
            List<SharesMtInfo> mtLineList = new List<SharesMtInfo>();
            using (var db = new meal_ticketEntities())
            {
                highMarkYesSession = (from item in db.t_shares_hotspot_highmark
                                      where item.Status == 1 && item.IsFit == true && item.Date == preDate && item.AccountId == highMarkAccountId
                                      select item.SharesKey).ToDictionary(k => k, v => v);
                long ContextId = (from item in db.t_shares_hotspot
                                  where item.DataType == 1 && item.AccountId == highMarkAccountId
                                  select item.Id).FirstOrDefault();
                riseUpYesCount = (from item in db.t_shares_group_statistic_mtline_all
                                  where item.Date == preDate && item.ContextId == ContextId
                                  select item).ToList().ToDictionary(k => k.GroupTimeKey%10000, v => v.RiseUpCount);

                string sql = string.Format(@"select convert(bigint,SharesNumber) SharesKey,[Time],GroupTimeKey,ClosedPrice,YestodayClosedPrice,PriceType,IsLimitUpBomb,IsLimitDownBomb
  from t_shares_securitybarsdata_1min
  where [Time]>='{0}' and [Time]<'{1}'
  order by [Time]", startTime.ToString("yyyy-MM-dd HH:mm:00"), endTime.ToString("yyyy-MM-dd HH:mm:00"));
                mtLineList = db.Database.SqlQuery<SharesMtInfo>(sql).ToList();
            }

            List<MarketsentimentBaseinfo> result = new List<MarketsentimentBaseinfo>();
            MarketsentimentBaseinfo tempInfo = null;
            int baseSharesCount = (int)((shares_base.Count() - shares_limit.Count()) * 0.90);
            int HighMarkYesCount=highMarkYesSession.Count();
            int LimitUpYesCount = limitUpYesSession.Count();
            foreach (var item in mtLineList)
            {
                if (shares_limit.Contains(item.SharesKey))
                {
                    continue;
                }
                if (tempInfo == null || tempInfo.GroupTimeKey != item.GroupTimeKey)
                {
                    if (tempInfo != null && tempInfo.TotalSharesCount> baseSharesCount)
                    {
                        result.Add(tempInfo);
                    }
                    tempInfo = new MarketsentimentBaseinfo
                    {
                        Date = date,
                        GroupTimeKey = item.GroupTimeKey,
                        Time = item.Time,
                        LimitDownBombCount = 0,
                        LimitDownCount = 0,
                        LimitUpBombCount = 0,
                        LimitUpCount = 0,
                        RiseDownCount = 0,
                        RiseFlatCount = 0,
                        RiseUpCount = 0,
                        RiseUpYesMinCount = 0,
                        TotalSharesCount=0,
                        TotalHighMarkYesRiseRate=0,
                        TotalLimitUpYesRiseRate=0,
                        HighMarkYesCount= HighMarkYesCount,
                        LimitUpYesCount= LimitUpYesCount
                    };
                    if (riseUpYesCount.ContainsKey(tempInfo.GroupTimeKey%10000))
                    {
                        tempInfo.RiseUpYesMinCount = riseUpYesCount[tempInfo.GroupTimeKey % 10000];
                    }
                }
                if (item.PriceType == 1)
                {
                    tempInfo.LimitUpCount++;
                }
                if (item.PriceType == 2)
                {
                    tempInfo.LimitDownCount++;
                }
                if (item.RiseRate > 0)
                {
                    tempInfo.RiseUpCount++;
                }
                else if (item.RiseRate == 0)
                {
                    tempInfo.RiseFlatCount++;
                }
                else 
                {
                    tempInfo.RiseDownCount++;
                }
                if (item.IsLimitUpBomb) 
                {
                    tempInfo.LimitUpBombCount++;
                }
                if (item.IsLimitDownBomb)
                {
                    tempInfo.LimitDownBombCount++;
                }
                if (limitUpYesSession.ContainsKey(item.SharesKey))
                {
                    tempInfo.TotalLimitUpYesRiseRate += item.RiseRate;
                }
                if (highMarkYesSession.ContainsKey(item.SharesKey))
                {
                    tempInfo.TotalHighMarkYesRiseRate += item.RiseRate;
                }
                tempInfo.TotalSharesCount++;
            }
            if (tempInfo != null && tempInfo.TotalSharesCount > baseSharesCount)
            {
                result.Add(tempInfo);
            }
            return result;
        }

        public static void ToWriteToDataBase(List<MarketsentimentBaseinfo> dataList)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("Time", typeof(DateTime));
            table.Columns.Add("GroupTimeKey", typeof(long));
            table.Columns.Add("LimitUpCount", typeof(int));
            table.Columns.Add("LimitDownCount", typeof(int));
            table.Columns.Add("RiseUpCount", typeof(int));
            table.Columns.Add("RiseDownCount", typeof(int));
            table.Columns.Add("RiseFlatCount", typeof(int));
            table.Columns.Add("RiseUpYesCount", typeof(int));
            table.Columns.Add("LimitUpYesRiseRate", typeof(int));
            table.Columns.Add("LimitUpBombCount", typeof(int));
            table.Columns.Add("LimitDownBombCount", typeof(int));
            table.Columns.Add("HighMarkYesRiseRate", typeof(int));

            foreach (var item in dataList)
            {
                DataRow row = table.NewRow();
                row["Date"] = item.Date;
                row["Time"] = item.Time;
                row["GroupTimeKey"] = item.GroupTimeKey;
                row["LimitUpCount"] = item.LimitUpCount;
                row["LimitDownCount"] = item.LimitDownCount;
                row["RiseUpCount"] = item.RiseUpCount;
                row["RiseDownCount"] = item.RiseDownCount;
                row["RiseFlatCount"] = item.RiseFlatCount;
                row["RiseUpYesCount"] = item.RiseUpYesMinCount;
                row["LimitUpYesRiseRate"] = item.LimitUpYesRiseRate;
                row["LimitUpBombCount"] = item.LimitUpBombCount;
                row["LimitDownBombCount"] = item.LimitDownBombCount;
                row["HighMarkYesRiseRate"] = item.HighMarkYesRiseRate;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@marketSentimentBaseinfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.MarketSentimentBaseinfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_MarketSentiment_Update @marketSentimentBaseinfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("Cal_MarketSentiment入库失败", ex);
                    tran.Rollback();
                }
            }
        }
    }
}
