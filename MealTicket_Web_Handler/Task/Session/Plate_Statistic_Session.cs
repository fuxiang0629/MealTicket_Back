using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FXCommon.Common.Session_New;

namespace MealTicket_Web_Handler
{
    public class Plate_Statistic_Session
    {
        public static Plate_Statistic_Session_Info UpdateSession()
        {
            GET_DATA_CXT gdc = new GET_DATA_CXT(SessionHandler.GET_ALL_PLATE_STATISTIC_INFO, null);
            var result = (Singleton.Instance.sessionHandler.GetDataWithLock("", gdc)) as Plate_Statistic_Session_Info;
            return result;
        }

        private static void ToWriteDebugLog(string message, Exception ex)
        {
            return;
            Logger.WriteFileLog(message, ex);
        }
        public static Plate_Statistic_Session_Info Cal_Plate_Statistic()
        {
            ToWriteDebugLog("1：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            var result = new Plate_Statistic_Session_Info();
            result.Plate_Info = new Dictionary<long, Plate_Statistic_Info>();
            result.Plate_Rank = new Plate_Statistic_Rank_Obj();

            var plate_base = Singleton.Instance.sessionHandler.GetPlate_Base_Session(false);
            var plate_quotes = Singleton.Instance.sessionHandler.GetPlate_Quotes_Last_Session(false, false);
            var plate_shares_rel = Singleton.Instance.sessionHandler.GetPlate_Shares_Rel_Session(false);
            var plate_quotes_date = Singleton.Instance.sessionHandler.GetPlate_Quotes_Date_Session(0,false);
            var plate_quotes_today = Singleton.Instance.sessionHandler.GetPlate_Quotes_Today_Session(false);

            int[] daysType = Singleton.Instance.SharesLeaderDaysType;//计算天数类型
            ToWriteDebugLog("2：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            calPlateBaseInfo(plate_base, plate_quotes, ref result);
            ToWriteDebugLog("3：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);

            WaitHandle[] taskArr = new WaitHandle[daysType.Length];
            for (int i = 0; i < daysType.Length; i++)
            {
                taskArr[i] = TaskThread.CreateTask((par) =>
                {
                    int dayType = (int)par;
                    Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> new_plate_quotes_date = getNewPlateDateQuotes(dayType, plate_quotes_date, plate_quotes_today);
                    calPlateRiseRate(dayType, new_plate_quotes_date, ref result);
                }, daysType[i]);
            }
            TaskThread.WaitAll(taskArr, Timeout.Infinite);
            TaskThread.CloseAllTasks(taskArr);
            ToWriteDebugLog("4：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            calPlateOverallRiseRate(ref result);
            ToWriteDebugLog("5：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            return result;
        }

        private static void calPlateBaseInfo(Dictionary<long, Plate_Base_Session_Info> plate_base, Dictionary<long, Plate_Quotes_Session_Info> plate_quotes, ref Plate_Statistic_Session_Info result)
        {
            foreach (var item in plate_base)
            {
                if (item.Value.BaseStatus != 1)
                {
                    continue;
                }
                Plate_Statistic_Info statistic_Plate_Info = new Plate_Statistic_Info();
                statistic_Plate_Info.PlateId = item.Value.PlateId;
                statistic_Plate_Info.PlateName = item.Value.PlateName;
                statistic_Plate_Info.PlateType = item.Value.PlateType;
                statistic_Plate_Info.SharesCount = item.Value.SharesCount;
                statistic_Plate_Info.CirculatingCapital = item.Value.CirculatingCapital;
                if (plate_quotes.ContainsKey(item.Key))
                {
                    var plateQuotes = plate_quotes[item.Key];
                    statistic_Plate_Info.ClosedPrice = plateQuotes.YestodayClosedPrice;
                    statistic_Plate_Info.CurrPrice = plateQuotes.ClosedPrice;
                    statistic_Plate_Info.MaxPrice = plateQuotes.MaxPrice;
                    statistic_Plate_Info.MinPrice = plateQuotes.MinPrice;
                    statistic_Plate_Info.OpenedPrice = plateQuotes.OpenedPrice;
                    statistic_Plate_Info.DealAmount = plateQuotes.DealAmount;
                    statistic_Plate_Info.DealCount = plateQuotes.DealCount;
                    statistic_Plate_Info.RiseLimitCount = plateQuotes.RiseUpLimitCount;
                    statistic_Plate_Info.Rank = plateQuotes.Rank;
                    statistic_Plate_Info.DownLimitCount = plateQuotes.DownUpLimitCount;
                }
                result.Plate_Info.Add(item.Key, statistic_Plate_Info);
            }
        }


        private static Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> getNewPlateDateQuotes(int dayType, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> plate_quotes_date, Dictionary<long, Plate_Quotes_Session_Info> plate_quotes_today)
        {
            Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> result = new Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>();
            Dictionary<long, SortedDictionary<DateTime, Plate_Quotes_Session_Info>> new_plate_quotes_date = new Dictionary<long, SortedDictionary<DateTime, Plate_Quotes_Session_Info>>();
            foreach (var item in plate_quotes_date)
            {
                if (!new_plate_quotes_date.ContainsKey(item.Key))
                {
                    new_plate_quotes_date.Add(item.Key, new SortedDictionary<DateTime, Plate_Quotes_Session_Info>());
                    result.Add(item.Key, new Dictionary<DateTime, Plate_Quotes_Session_Info>());
                }
                foreach (var item2 in item.Value)
                {
                    new_plate_quotes_date[item.Key].Add(item2.Key, item2.Value);
                }
            }
            int CalDays = dayType == 1 ? 3 : dayType == 2 ? 5 : dayType == 3 ? 10 : dayType == 4 ? 15 : 0;
            foreach (var item in plate_quotes_today)
            {
                if (!new_plate_quotes_date.ContainsKey(item.Key))
                {
                    new_plate_quotes_date.Add(item.Key, new SortedDictionary<DateTime, Plate_Quotes_Session_Info>());
                }
                if (!new_plate_quotes_date[item.Key].ContainsKey(item.Value.Date))
                {
                    new_plate_quotes_date[item.Key].Add(item.Value.Date, item.Value);
                }
            }
            foreach (var item in new_plate_quotes_date)
            {
                long plateKey = item.Key;
                int count = item.Value.Count();
                count = count < CalDays ? 0 : (count - CalDays);
                result[plateKey] = item.Value.Skip(count).ToDictionary(k => k.Key, v => v.Value);
            }
            return result;
        }

        private static void calPlateRiseRate(int dayType, Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> new_plate_quotes_date,ref Plate_Statistic_Session_Info result)
        {
            SortedSet<Plate_Statistic_Session_Overall> tempPlateRank = calObjDays_Plate_Rank(dayType, result.Plate_Rank);
            foreach (var plate in result.Plate_Info)
            {
                if (!new_plate_quotes_date.ContainsKey(plate.Key))
                {
                    continue;
                }
                var plate_quote = new_plate_quotes_date[plate.Key];
                var lastQuote = plate_quote.LastOrDefault().Value;
                var tempQuote = plate_quote.OrderBy(e => e.Value.ClosedPrice).FirstOrDefault().Value;
                if (lastQuote == null || tempQuote == null)
                {
                    continue;
                }
                if (tempQuote.ClosedPrice >= lastQuote.ClosedPrice)
                {
                    tempQuote = plate_quote.OrderByDescending(e => e.Value.ClosedPrice).FirstOrDefault().Value;
                }
                int RealDays = plate_quote.Where(e => e.Key >= tempQuote.Date && e.Key <= lastQuote.Date).Count();
                int RiseRate = tempQuote.ClosedPrice == 0 ? 0 : (int)Math.Round((lastQuote.ClosedPrice - tempQuote.ClosedPrice) * 1.0 / tempQuote.ClosedPrice * 10000, 0);

                Plate_Statistic_Session_Overall shares_Overall = new Plate_Statistic_Session_Overall
                {
                    PlateId = plate.Key,
                    RealDays = RealDays,
                    RiseRate = RiseRate
                };
                tempPlateRank.Add(shares_Overall);
            }
        }

        private static SortedSet<Plate_Statistic_Session_Overall> calObjDays_Plate_Rank(int dayType, Plate_Statistic_Rank_Obj obj)
        {
            if (dayType == 1)
            {
                obj.Rank_3Days = new SortedSet<Plate_Statistic_Session_Overall>();
                return obj.Rank_3Days;
            }
            else if (dayType == 2)
            {
                obj.Rank_5Days = new SortedSet<Plate_Statistic_Session_Overall>();
                return obj.Rank_5Days;
            }
            else if (dayType == 3)
            {
                obj.Rank_10Days = new SortedSet<Plate_Statistic_Session_Overall>();
                return obj.Rank_10Days;
            }
            else if (dayType == 4)
            {
                obj.Rank_15Days = new SortedSet<Plate_Statistic_Session_Overall>();
                return obj.Rank_15Days;
            }
            else
            {
                return null;
            }
        }

        private static void calPlateOverallRiseRate(ref Plate_Statistic_Session_Info result)
        {
            if (result.Plate_Rank == null)
            {
                return;
            }
            var Rank_3Days_dic = new Dictionary<long, Plate_Statistic_Session_Overall>();
            var Rank_5Days_dic = new Dictionary<long, Plate_Statistic_Session_Overall>();
            var Rank_10Days_dic = new Dictionary<long, Plate_Statistic_Session_Overall>();
            var Rank_15Days_dic = new Dictionary<long, Plate_Statistic_Session_Overall>();
            if (result.Plate_Rank.Rank_3Days != null && result.Plate_Rank.Rank_3Days.Count() > 0)
            {
                Rank_3Days_dic = result.Plate_Rank.Rank_3Days.ToDictionary(k => k.PlateId, v => v);
            }
            if (result.Plate_Rank.Rank_5Days != null && result.Plate_Rank.Rank_5Days.Count() > 0)
            {
                Rank_5Days_dic = result.Plate_Rank.Rank_5Days.ToDictionary(k => k.PlateId, v => v);
            }
            if (result.Plate_Rank.Rank_10Days != null && result.Plate_Rank.Rank_10Days.Count() > 0)
            {
                Rank_10Days_dic = result.Plate_Rank.Rank_10Days.ToDictionary(k => k.PlateId, v => v);
            }
            if (result.Plate_Rank.Rank_15Days != null && result.Plate_Rank.Rank_15Days.Count() > 0)
            {
                Rank_15Days_dic = result.Plate_Rank.Rank_15Days.ToDictionary(k => k.PlateId, v => v);
            }
            result.Plate_Rank.Rank_Overall = new SortedSet<Plate_Statistic_Session_Overall>();
            foreach (var item in result.Plate_Info)
            {
                int totalRiseRate = 0;
                int totalCount = 0;

                if (Rank_3Days_dic.ContainsKey(item.Key))
                {
                    totalRiseRate += Rank_3Days_dic[item.Key].RiseRate;
                    totalCount++;
                }
                if (Rank_5Days_dic.ContainsKey(item.Key))
                {
                    totalRiseRate += Rank_5Days_dic[item.Key].RiseRate;
                    totalCount++;
                }
                if (Rank_10Days_dic.ContainsKey(item.Key))
                {
                    totalRiseRate += Rank_10Days_dic[item.Key].RiseRate;
                    totalCount++;
                }
                if (Rank_15Days_dic.ContainsKey(item.Key))
                {
                    totalRiseRate += Rank_15Days_dic[item.Key].RiseRate;
                    totalCount++;
                }
                if (totalCount == 0)
                {
                    continue;
                }
                item.Value.OverallRiseRate = (int)Math.Round(totalRiseRate * 1.0 / totalCount, 0);

                result.Plate_Rank.Rank_Overall.Add(new Plate_Statistic_Session_Overall 
                {
                    PlateId= item.Key,
                    RiseRate= item.Value.OverallRiseRate
                });
            }
        }



        public static Plate_Statistic_Session_Info CopySessionData(object objData)
        {
            var data = objData as Plate_Statistic_Session_Info;
            var resultData = new Plate_Statistic_Session_Info();
            resultData.Plate_Rank = data.Plate_Rank;
            resultData.Plate_Info = data.Plate_Info;
            return resultData;
        }
    }
}
