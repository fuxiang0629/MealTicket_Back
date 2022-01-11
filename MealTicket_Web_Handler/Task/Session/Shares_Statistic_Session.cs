using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FXCommon.Common.Session_New;
using static MealTicket_Web_Handler.SessionHandler;

namespace MealTicket_Web_Handler
{
    public class Shares_Statistic_Session
    {
        public static Shares_Statistic_Session_Obj UpdateSession()
        {
            GET_DATA_CXT gdc = new GET_DATA_CXT(GET_ALL_SHARES_STATISTIC_INFO, null);
            var result= (Singleton.Instance.sessionHandler.GetDataWithLock("", gdc)) as Shares_Statistic_Session_Obj;
            return result;
        }

        public static Shares_Statistic_Session_Obj Cal_Shares_Statistic() 
        {
            var result = new Shares_Statistic_Session_Obj();
            result.Shares_Info = new Dictionary<long, Statistic_Shares_Info>();
            result.Plate_Rank = new Statistic_Plate_Rank_Obj();
            result.Shares_Rank = new Statistic_Shares_Rank_Obj();
            result.Shares_Rank.Rank_3Days = new SortedSet<Shares_Statistic_Session_Overall>();
            result.Shares_Rank.Rank_5Days = new SortedSet<Shares_Statistic_Session_Overall>();
            result.Shares_Rank.Rank_10Days = new SortedSet<Shares_Statistic_Session_Overall>();
            result.Shares_Rank.Rank_15Days = new SortedSet<Shares_Statistic_Session_Overall>();
            result.Plate_Rank.Rank_3Days = new Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>>();
            result.Plate_Rank.Rank_5Days = new Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>>();
            result.Plate_Rank.Rank_10Days = new Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>>();
            result.Plate_Rank.Rank_15Days = new Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>>();

            var shares_quotes_date = Singleton.Instance.sessionHandler.GetShares_Quotes_Date_Session(0, false);
            var shares_quotes_today = Singleton.Instance.sessionHandler.GetShares_Quotes_Today_Session(false);
            var shares_base = Singleton.Instance.sessionHandler.GetShares_Base_Session(false);
            var shares_quotes = Singleton.Instance.sessionHandler.GetShares_Quotes_Last_Session(true, false);
            var plate_base = Singleton.Instance.sessionHandler.GetPlate_Base_Session(false);
            var shares_plate_rel = Singleton.Instance.sessionHandler.GetShares_Plate_Rel_Session(false);
            var plate_quotes = Singleton.Instance.sessionHandler.GetPlate_Quotes_Last_Session(false, false);
            var shares_focusOn = Singleton.Instance.sessionHandler.GetPlate_Tag_FocusOn_Session_ByShares(false);
            var shares_trendlike = Singleton.Instance.sessionHandler.GetPlate_Tag_TrendLike_Session_ByShares(false);
            var shares_force = Singleton.Instance.sessionHandler.GetPlate_Tag_Force_Session_ByShares(false);
            var shares_Tag_Leader_Session = Singleton.Instance.sessionHandler.GetShares_Tag_Leader_Session_ByShares(false);
            var shares_Tag_DayLeader_Session = Singleton.Instance.sessionHandler.GetShares_Tag_DayLeader_Session_ByShares(false);
            var shares_Tag_Mainarmy_Session = Singleton.Instance.sessionHandler.GetShares_Tag_MainArmy_Session_ByShares(false);

            int[] daysType = Singleton.Instance.SharesLeaderDaysType;//计算天数类型
            calSharesBaseInfo(shares_base, shares_quotes, plate_base, shares_plate_rel, plate_quotes, shares_focusOn, shares_trendlike, ref result);

            WaitHandle[] taskArr = new WaitHandle[daysType.Length];
            for (int i = 0; i < daysType.Length; i++)
            {
                taskArr[i] = TaskThread.CreateTask((par) =>
                {
                    int dayType = (int)par;
                    var shares_plate_real = Singleton.Instance.sessionHandler.GetShares_Real_Plate_Session(dayType, false);
                    Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> new_shares_quotes_date = getNewSharesDateQuotes(dayType, shares_quotes_date, shares_quotes_today);
                    calSharesRiseRate(dayType, new_shares_quotes_date, shares_plate_real, shares_force, shares_Tag_Leader_Session, shares_Tag_DayLeader_Session, shares_Tag_Mainarmy_Session, ref result);
                }, daysType[i]);
            }
            TaskThread.WaitAll(taskArr, Timeout.Infinite);
            TaskThread.CloseAllTasks(taskArr);
            calSharesOverallRiseRate(ref result);
            return result;
        }

        private static void calSharesBaseInfo(Dictionary<long, Shares_Base_Session_Info> shares_base, Dictionary<long, Shares_Quotes_Session_Info_Last> shares_quotes, Dictionary<long, Plate_Base_Session_Info> plate_base,Dictionary<long,List<Plate_Shares_Rel_Session_Info>> shares_plate_rel, Dictionary<long, Plate_Quotes_Session_Info> plate_quotes, Dictionary<long, Dictionary<long,Plate_Tag_FocusOn_Session_Info>> shares_focusOn, Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> shares_trendlike,ref Shares_Statistic_Session_Obj result) 
        {
            foreach (var item in shares_base)
            {
                Statistic_Shares_Info statistic_Shares_Info = new Statistic_Shares_Info();
                statistic_Shares_Info.CirculatingCapital = item.Value.CirculatingCapital;
                statistic_Shares_Info.SharesKey = item.Key;
                statistic_Shares_Info.Market = item.Value.Market;
                statistic_Shares_Info.MarketStatus = item.Value.MarketStatus;
                statistic_Shares_Info.SharesCode = item.Value.SharesCode;
                statistic_Shares_Info.SharesName = item.Value.SharesName;
                statistic_Shares_Info.Quotes_Info = new Shares_Statistic_Session_Quotes_Info();
                if (shares_quotes.ContainsKey(item.Key))
                {
                    var temp_quotes = shares_quotes[item.Key];
                    statistic_Shares_Info.Quotes_Info.ClosedPrice = temp_quotes.shares_quotes_info.YestodayClosedPrice;
                    statistic_Shares_Info.Quotes_Info.CurrPrice = temp_quotes.shares_quotes_info.ClosedPrice;
                    statistic_Shares_Info.Quotes_Info.DealAmount = temp_quotes.shares_quotes_info.TotalAmount;
                    statistic_Shares_Info.Quotes_Info.DealCount = temp_quotes.shares_quotes_info.TotalCount;
                    statistic_Shares_Info.Quotes_Info.OpenedPrice = temp_quotes.shares_quotes_info.OpenedPrice;
                    statistic_Shares_Info.Quotes_Info.RateNow = temp_quotes.RateNow;
                    statistic_Shares_Info.Quotes_Info.RateExpect = temp_quotes.RateExpect;
                }
                statistic_Shares_Info.Plate_Dic = new Dictionary<long, Statistic_Plate_Info>();
                if (shares_plate_rel.ContainsKey(item.Key))
                {
                    var FocusOn_Info = new Dictionary<long, Plate_Tag_FocusOn_Session_Info>();
                    if (shares_focusOn.ContainsKey(item.Key))
                    {
                        FocusOn_Info = shares_focusOn[item.Key];
                    }
                    var TrendLike_Info = new Dictionary<long, Plate_Tag_TrendLike_Session_Info>();
                    if (shares_trendlike.ContainsKey(item.Key))
                    {
                        TrendLike_Info = shares_trendlike[item.Key];
                    }
                    foreach (var plate in shares_plate_rel[item.Key])
                    {
                        if (!plate_base.ContainsKey(plate.PlateId))
                        {
                            continue;
                        }
                        var baseInfo = plate_base[plate.PlateId];
                        if (baseInfo.BaseStatus != 1)
                        {
                            continue;
                        }
                        //a.板块基础数据
                        Statistic_Plate_Info plate_base_info = new Statistic_Plate_Info();
                        plate_base_info.PlateId = baseInfo.PlateId;
                        plate_base_info.PlateName = baseInfo.PlateName;
                        plate_base_info.PlateType = baseInfo.PlateType;
                        plate_base_info.SharesCount = baseInfo.SharesCount;
                        if (plate_quotes.ContainsKey(plate.PlateId))
                        {
                            //b.板块五档数据
                            var quotes_info = plate_quotes[plate.PlateId];
                            plate_base_info.ClosedPrice = quotes_info.YestodayClosedPrice;
                            plate_base_info.CurrPrice = quotes_info.ClosedPrice;
                            plate_base_info.DownLimitCount = quotes_info.DownUpLimitCount;
                            plate_base_info.RiseLimitCount = quotes_info.RiseUpLimitCount;
                            plate_base_info.Rank = quotes_info.Rank;
                        }
                        //c.板块标签数据
                        if (FocusOn_Info.ContainsKey(plate.PlateId))
                        {
                            plate_base_info.IsFocusOn = true;
                        }
                        if (TrendLike_Info.ContainsKey(plate.PlateId))
                        {
                            plate_base_info.IsTrendLike = true;
                        }

                        statistic_Shares_Info.Plate_Dic.Add(plate.PlateId, plate_base_info);
                    }
                }

                statistic_Shares_Info.Plate_Tag = new Statistic_Plate_Tag_Obj();
                result.Shares_Info.Add(item.Key, statistic_Shares_Info);
            }
        }

        private static Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> getNewSharesDateQuotes(int dayType, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> shares_quotes_date, Dictionary<long, Shares_Quotes_Session_Info> shares_quotes_today)
        {
            Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> new_shares_quotes_date = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
            foreach (var item in shares_quotes_date)
            {
                new_shares_quotes_date.Add(item.Key, new Dictionary<DateTime, Shares_Quotes_Session_Info>(item.Value));
            }
            foreach (var item in shares_quotes_today)
            {
                long shareKey = item.Key;
                if (!new_shares_quotes_date.ContainsKey(shareKey))
                {
                    new_shares_quotes_date.Add(shareKey, new Dictionary<DateTime, Shares_Quotes_Session_Info>());
                }
                if (!new_shares_quotes_date[shareKey].ContainsKey(item.Value.Date))
                {
                    new_shares_quotes_date[shareKey].Add(item.Value.Date, item.Value);
                }
            }
            return new_shares_quotes_date;
        }

        private static void calSharesRiseRate(int dayType, Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> new_shares_quotes_date,Dictionary<long,List<long>> shares_plate_real,Dictionary<long,Dictionary<long,Plate_Tag_Force_Session_Info>> shares_force,Dictionary<long,Dictionary<long,Shares_Tag_Leader_Session_Info>> shares_Tag_Leader_Session, Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>> shares_Tag_DayLeader_Session, Dictionary<long, Dictionary<long, Shares_Tag_MainArmy_Session_Info>> shares_Tag_MainArmy_Session, ref Shares_Statistic_Session_Obj result) 
        {
            int CalDays = dayType == 1 ? 3 : dayType == 2 ? 5 : dayType == 3 ? 10 : dayType == 4 ? 15 : 0;
            if (CalDays == 0)
            {
                return;
            }
            SortedSet<Shares_Statistic_Session_Overall> tempSharesRank = calObjDays_Shares_Rank(dayType, result.Shares_Rank);
            Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>> tempPlateRank = calObjDays_Plate_Rank(dayType, result.Plate_Rank);
            foreach (var share in result.Shares_Info)
            {
                Dictionary<long, Statistic_Plate_Tag> tempPlateTag = calObjDays_Plate_Tag(dayType, share.Value.Plate_Tag);
                if (!new_shares_quotes_date.ContainsKey(share.Key))
                {
                    continue;
                }
                var share_quote = new_shares_quotes_date[share.Key];
                var shares_quotes_date_new = share_quote.OrderByDescending(e => e.Key).Take(CalDays).ToDictionary(k => k.Key, v => v.Value);
                var lastQuote = shares_quotes_date_new.OrderByDescending(e => e.Key).FirstOrDefault().Value;
                var tempQuote = shares_quotes_date_new.OrderBy(e => e.Value.ClosedPrice).FirstOrDefault().Value;
                if (lastQuote == null || tempQuote == null)
                {
                    continue;
                }
                if (tempQuote.ClosedPrice >= lastQuote.ClosedPrice)
                {
                    tempQuote = share_quote.OrderByDescending(e => e.Value.ClosedPrice).FirstOrDefault().Value;
                }
                int RealDays = share_quote.Where(e => e.Key >= tempQuote.Date && e.Key <= lastQuote.Date).Count();
                int RiseRate = tempQuote.ClosedPrice == 0 ? 0 : (int)Math.Round((lastQuote.ClosedPrice - tempQuote.ClosedPrice) * 1.0 / tempQuote.ClosedPrice * 10000, 0);

                Shares_Statistic_Session_Overall shares_Overall = new Shares_Statistic_Session_Overall
                {
                    SharesKey = share.Key,
                    RealDays = RealDays,
                    RiseRate = RiseRate
                };
                tempSharesRank.Add(shares_Overall);
                //股票所属真实板块
                if (shares_plate_real.ContainsKey(share.Key))
                {
                    long forceKey = share.Key * 100 + dayType;
                    Dictionary<long, Plate_Tag_Force_Session_Info> forceInfo = new Dictionary<long, Plate_Tag_Force_Session_Info>();
                    if (shares_force.ContainsKey(forceKey))
                    {
                        forceInfo = shares_force[forceKey];
                    }
                    Dictionary<long, Shares_Tag_Leader_Session_Info> leader_info = new Dictionary<long, Shares_Tag_Leader_Session_Info>();
                    if (shares_Tag_Leader_Session.ContainsKey(share.Key))
                    {
                        leader_info = shares_Tag_Leader_Session[share.Key];
                    }
                    Dictionary<long, Shares_Tag_DayLeader_Session_Info> dayleader_info = new Dictionary<long, Shares_Tag_DayLeader_Session_Info>();
                    if (shares_Tag_DayLeader_Session.ContainsKey(share.Key))
                    {
                        dayleader_info = shares_Tag_DayLeader_Session[share.Key];
                    }
                    Dictionary<long, Shares_Tag_MainArmy_Session_Info> mainArmy_info = new Dictionary<long, Shares_Tag_MainArmy_Session_Info>();
                    if (shares_Tag_MainArmy_Session.ContainsKey(share.Key))
                    {
                        mainArmy_info = shares_Tag_MainArmy_Session[share.Key];
                    }
                    foreach (long plateId in shares_plate_real[share.Key])
                    {
                        if (!tempPlateRank.ContainsKey(plateId))
                        {
                            tempPlateRank.Add(plateId, new SortedSet<Shares_Statistic_Session_Overall>());
                        }
                        tempPlateRank[plateId].Add(shares_Overall);

                        if (!tempPlateTag.ContainsKey(plateId))
                        {
                            tempPlateTag.Add(plateId, new Statistic_Plate_Tag());
                        }
                        var tagInfo = tempPlateTag[plateId];
                        if (forceInfo.ContainsKey(plateId))
                        {
                            var forceBase = forceInfo[plateId];
                            tagInfo.IsForce1 = forceBase.IsForce1;
                            tagInfo.IsForce2 = forceBase.IsForce2;
                        }
                        long leaderKey = plateId * 100 + dayType;
                        if (leader_info.ContainsKey(leaderKey))
                        {
                            tagInfo.LeaderType = leader_info[leaderKey].LeaderType;
                        }
                        if (dayleader_info.ContainsKey(leaderKey))
                        {
                            tagInfo.DayLeaderType = dayleader_info[leaderKey].DayLeaderType;
                        }
                        if (mainArmy_info.ContainsKey(leaderKey))
                        {
                            tagInfo.MainarmyType = mainArmy_info[leaderKey].MainArmyType;
                        }
                    }
                }
            }
        }

        private static void calSharesOverallRiseRate(ref Shares_Statistic_Session_Obj result) 
        {
            foreach (var item in result.Shares_Info)
            {
                int totalRiseRate = 0;
                int totalCount = 0;

                if (result.Shares_Rank != null && result.Shares_Rank.Rank_3Days != null)
                {
                    var days3 = result.Shares_Rank.Rank_3Days.Where(e => e.SharesKey == item.Key).FirstOrDefault();
                    if (days3 != null)
                    {
                        totalRiseRate += days3.RiseRate;
                        totalCount++;
                    }
                }
                if (result.Shares_Rank != null && result.Shares_Rank.Rank_5Days != null)
                {
                    var days5 = result.Shares_Rank.Rank_5Days.Where(e => e.SharesKey == item.Key).FirstOrDefault();
                    if (days5 != null)
                    {
                        totalRiseRate += days5.RiseRate;
                        totalCount++;
                    }
                }
                if (result.Shares_Rank != null && result.Shares_Rank.Rank_10Days != null)
                {
                    var days10 = result.Shares_Rank.Rank_10Days.Where(e => e.SharesKey == item.Key).FirstOrDefault();
                    if (days10 != null)
                    {
                        totalRiseRate += days10.RiseRate;
                        totalCount++;
                    }
                }
                if (result.Shares_Rank != null && result.Shares_Rank.Rank_15Days != null)
                {
                    var days15 = result.Shares_Rank.Rank_15Days.Where(e => e.SharesKey == item.Key).FirstOrDefault();
                    if (days15 != null)
                    {
                        totalRiseRate += days15.RiseRate;
                        totalCount++;
                    }
                }
                if (totalCount == 0)
                {
                    continue;
                }
                item.Value.OverallRiseRate = (int)Math.Round(totalRiseRate * 1.0 / totalCount, 0);
            }
        }

        private static SortedSet<Shares_Statistic_Session_Overall> calObjDays_Shares_Rank(int dayType,Statistic_Shares_Rank_Obj obj)
        {
            if (dayType == 1)
            {
                obj.Rank_3Days = new SortedSet<Shares_Statistic_Session_Overall>();
                return obj.Rank_3Days;
            }
            else if (dayType == 2)
            {
                obj.Rank_5Days = new SortedSet<Shares_Statistic_Session_Overall>();
                return obj.Rank_5Days;
            }
            else if (dayType == 3)
            {
                obj.Rank_10Days = new SortedSet<Shares_Statistic_Session_Overall>();
                return obj.Rank_10Days;
            }
            else if (dayType == 4)
            {
                obj.Rank_15Days = new SortedSet<Shares_Statistic_Session_Overall>();
                return obj.Rank_15Days;
            }
            else
            {
                return null;
            }
        }

        private static Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>> calObjDays_Plate_Rank(int dayType, Statistic_Plate_Rank_Obj obj)
        {
            if (dayType == 1)
            {
                obj.Rank_3Days = new Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>>();
                return obj.Rank_3Days;
            }
            else if (dayType == 2)
            {
                obj.Rank_5Days = new Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>>();
                return obj.Rank_5Days;
            }
            else if (dayType == 3)
            {
                obj.Rank_10Days = new Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>>();
                return obj.Rank_10Days;
            }
            else if (dayType == 4)
            {
                obj.Rank_15Days = new Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>>();
                return obj.Rank_15Days;
            }
            else
            {
                return null;
            }
        }
        private static Dictionary<long, Statistic_Plate_Tag> calObjDays_Plate_Tag(int dayType, Statistic_Plate_Tag_Obj obj)
        {
            if (dayType == 1)
            {
                obj.Tag_3Days = new Dictionary<long, Statistic_Plate_Tag>();
                return obj.Tag_3Days;
            }
            else if (dayType == 2)
            {
                obj.Tag_5Days = new Dictionary<long, Statistic_Plate_Tag>();
                return obj.Tag_5Days;
            }
            else if (dayType == 3)
            {
                obj.Tag_10Days = new Dictionary<long, Statistic_Plate_Tag>();
                return obj.Tag_10Days;
            }
            else if (dayType == 4)
            {
                obj.Tag_15Days = new Dictionary<long, Statistic_Plate_Tag>();
                return obj.Tag_15Days;
            }
            else
            {
                return null;
            }
        }

        public static Shares_Statistic_Session_Obj CopySessionData(object objData)
        {
            var data = objData as Shares_Statistic_Session_Obj;
            var resultData = new Shares_Statistic_Session_Obj();
            resultData.Shares_Info = data.Shares_Info;
            resultData.Shares_Rank = data.Shares_Rank;
            resultData.Plate_Rank = data.Plate_Rank;
            return resultData;
        }
    }
}
