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
    public class SharesPremiumStatisticSourceInfo
    {
        public long ConditionId { get; set; }

        public string ConditionStr { get; set; }
    }

    public class SharesPremiumStatisticPar
    {
        public int? compare1 { get; set; }
        public int? riseType1 { get; set; }
        public double? riseoffset1 { get; set; }
        public int? limitCount1 { get; set; }
        public int? limitCount2 { get; set; }
        public int? compare2 { get; set; }
        public double? riseoffset2 { get; set; }
        public int? compare3 { get; set; }
        public int? riseType2 { get; set; }
        public double? riseoffset3 { get; set; }

        public bool? isReverse { get; set; }
    }

    public class SharesPremiumStatisticResult
    {
        public List<long> YesSharesList { get; set; }
        public List<long> TodaySharesList { get; set; }

        public int PremiumRate
        {
            get 
            {
                if (YesSharesList.Count() == 0)
                {
                    return -1;
                }
                return (int)Math.Round(TodaySharesList.Count() * 1.0 / YesSharesList.Count() * 10000,0);
            }
        }
    }

    public class SharesPremiumStatisticDb
    {
        public long ConditionId { get; set; }

        public int PriceType { get; set; }

        public int SharesCountToday { get; set; }

        public int SharesCountYes { get; set; }

        public int PremiumRateToday { get; set; }

        public string PremiumRate3daysPar { get; set; }

        public int PremiumRate3days { get; set; }

        public string PremiumRate5daysPar { get; set; }

        public int PremiumRate5days { get; set; }

        public DateTime Date { get; set; }
    }

    public class LimitCountInfo
    {
        public DateTime Date { get; set; }

        public long SharesKey { get; set; }

        public int RiseLimitCount { get; set; }

        public int RiseLimitCountCondition { get; set; }
    }

    public class SharesPremiumStatisticHelper
    {
        public static void Cal_SharesPremiumStatistic(DateTime date)
        {
            Dictionary<long, Dictionary<int, List<SharesPremiumStatisticResult>>> resultDic = new Dictionary<long, Dictionary<int, List<SharesPremiumStatisticResult>>>();

            List<SharesPremiumStatisticSourceInfo> sourceList = new List<SharesPremiumStatisticSourceInfo>();
            Dictionary<DateTime, Dictionary<long, LimitCountInfo>> LimitUpSession = new Dictionary<DateTime, Dictionary<long, LimitCountInfo>>();
            Dictionary<DateTime, Dictionary<long, LimitCountInfo>> LimitDownSession = new Dictionary<DateTime, Dictionary<long, LimitCountInfo>>();
            using (var db = new meal_ticketEntities())
            {
                sourceList = (from item in db.t_shares_premium_statistic_condition
                              where item.Status == 1
                              select new SharesPremiumStatisticSourceInfo
                              {
                                  ConditionStr = item.ConditionStr,
                                  ConditionId = item.Id
                              }).ToList();
                LimitUpSession = (from item in db.t_shares_quotes_riselimit
                                  where item.IsLimitUpToday == true
                                  select new LimitCountInfo
                                  {
                                      SharesKey = item.SharesKey.Value,
                                      Date = item.DataDate,
                                      RiseLimitCount = item.RiseLimitCount,
                                      RiseLimitCountCondition = item.RiseLimitCountContinue
                                  }).ToList().GroupBy(e => e.Date).ToDictionary(k => k.Key, v => v.ToDictionary(k2 => k2.SharesKey, v2 => v2));
                LimitDownSession = (from item in db.t_shares_quotes_riselimit_down
                                  where item.IsLimitUpToday == true
                                  select new LimitCountInfo
                                  {
                                      SharesKey = item.SharesKey.Value,
                                      Date = item.DataDate,
                                      RiseLimitCount = item.RiseLimitCount,
                                      RiseLimitCountCondition = item.RiseLimitCountContinue
                                  }).ToList().GroupBy(e => e.Date).ToDictionary(k => k.Key, v => v.ToDictionary(k2 => k2.SharesKey, v2 => v2));
            }
            var shares_limit = Singleton.Instance.sessionHandler.GetShares_Limit_Session(false);

            DateTime dateYes = date;
            for (int i = 1; i <= 5; i++)
            {
                DateTime dateToday = dateYes;
                dateYes = DbHelper.GetLastTradeDate2(0, 0, 0, -1, dateToday);
                var shares_quotes_today = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(dateToday, false);
                var shares_quotes_yes = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(dateYes, false);
                Dictionary<long, LimitCountInfo> limit_up = new Dictionary<long, LimitCountInfo>();
                Dictionary<long, LimitCountInfo> limit_down = new Dictionary<long, LimitCountInfo>();
                if (LimitUpSession.ContainsKey(dateYes))
                {
                    limit_up = LimitUpSession[dateYes];
                }
                if (LimitDownSession.ContainsKey(dateYes))
                {
                    limit_down = LimitDownSession[dateYes];
                }

                foreach (var item in sourceList)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(item.ConditionStr))
                        {
                            continue;
                        }
                        var ConditionPar = JsonConvert.DeserializeObject<SharesPremiumStatisticPar>(item.ConditionStr);
                        if (!ParsePar(ConditionPar))
                        {
                            continue;
                        }
                        if (!resultDic.ContainsKey(item.ConditionId))
                        {
                            resultDic.Add(item.ConditionId, new Dictionary<int, List<SharesPremiumStatisticResult>>());
                        }
                        List<long> yesSharesList1 = GetYesShares(shares_quotes_yes, ConditionPar, 1, limit_up, limit_down, shares_limit);
                        List<long> todaySharesList1 = GetTodayShares(shares_quotes_today, yesSharesList1, ConditionPar, 1);
                        if (!resultDic[item.ConditionId].ContainsKey(1))
                        {
                            resultDic[item.ConditionId].Add(1, new List<SharesPremiumStatisticResult>());
                        }
                        resultDic[item.ConditionId][1].Add(new SharesPremiumStatisticResult
                        {
                            YesSharesList = yesSharesList1,
                            TodaySharesList = todaySharesList1
                        });

                        List<long> yesSharesList2 = GetYesShares(shares_quotes_yes, ConditionPar, 2, limit_up, limit_down, shares_limit);
                        List<long> todaySharesList2 = GetTodayShares(shares_quotes_today, yesSharesList2, ConditionPar, 2);
                        if (!resultDic[item.ConditionId].ContainsKey(2))
                        {
                            resultDic[item.ConditionId].Add(2, new List<SharesPremiumStatisticResult>());
                        }
                        resultDic[item.ConditionId][2].Add(new SharesPremiumStatisticResult
                        {
                            YesSharesList = yesSharesList2,
                            TodaySharesList = todaySharesList2
                        });


                        List<long> yesSharesList3 = GetYesShares(shares_quotes_yes, ConditionPar, 3, limit_up, limit_down, shares_limit);
                        List<long> todaySharesList3 = GetTodayShares(shares_quotes_today, yesSharesList3, ConditionPar, 3);
                        if (!resultDic[item.ConditionId].ContainsKey(3))
                        {
                            resultDic[item.ConditionId].Add(3, new List<SharesPremiumStatisticResult>());
                        }
                        resultDic[item.ConditionId][3].Add(new SharesPremiumStatisticResult
                        {
                            YesSharesList = yesSharesList3,
                            TodaySharesList = todaySharesList3
                        });


                        List<long> yesSharesList4 = GetYesShares(shares_quotes_yes, ConditionPar, 4, limit_up, limit_down, shares_limit);
                        List<long> todaySharesList4 = GetTodayShares(shares_quotes_today, yesSharesList4, ConditionPar, 4);
                        if (!resultDic[item.ConditionId].ContainsKey(4))
                        {
                            resultDic[item.ConditionId].Add(4, new List<SharesPremiumStatisticResult>());
                        }
                        resultDic[item.ConditionId][4].Add(new SharesPremiumStatisticResult
                        {
                            YesSharesList = yesSharesList4,
                            TodaySharesList = todaySharesList4
                        });
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("溢价条件计算出错",ex);
                        continue;
                    }
                }
            }

            List<SharesPremiumStatisticDb> listResult = new List<SharesPremiumStatisticDb>();
            foreach (var item in resultDic)
            {
                foreach (var item2 in item.Value)
                {
                    int SharesCountToday = 0;
                    int SharesCountYes = 0;
                    int PremiumRateToday = 0;

                    int PremiumRate3daysTotal = 0;
                    int PremiumRate3daysCount = 0;
                    string PremiumRate3daysPar = "";

                    int PremiumRate5daysTotal = 0;
                    int PremiumRate5daysCount = 0;
                    string PremiumRate5daysPar = "";

                    int idx = 0;
                    foreach (var item3 in item2.Value)
                    {
                        idx++;
                        if (idx == 1)
                        {
                            SharesCountToday = item3.TodaySharesList.Count();
                            SharesCountYes = item3.YesSharesList.Count();
                            PremiumRateToday = item3.PremiumRate;
                        }
                        if (idx <= 3)
                        {
                            if (item3.PremiumRate >= 0)
                            {
                                PremiumRate3daysTotal = PremiumRate3daysTotal + item3.PremiumRate;
                                PremiumRate3daysCount++;
                            }
                            PremiumRate3daysPar = PremiumRate3daysPar + (item3.PremiumRate == -1 ? "--、" : (Math.Round(item3.PremiumRate * 1.0 / 100, 2) + "%、"));
                        }
                        if (idx <= 5)
                        {
                            if (item3.PremiumRate >= 0)
                            {
                                PremiumRate5daysTotal = PremiumRate5daysTotal + item3.PremiumRate;
                                PremiumRate5daysCount++;
                            }
                            PremiumRate5daysPar = PremiumRate5daysPar + (item3.PremiumRate == -1 ? "--、" : (Math.Round(item3.PremiumRate * 1.0 / 100, 2) + "%、"));
                        }
                    }
                    PremiumRate3daysPar = PremiumRate3daysPar.TrimEnd('、');
                    PremiumRate5daysPar = PremiumRate5daysPar.TrimEnd('、');

                    listResult.Add(new SharesPremiumStatisticDb
                    {
                        Date = date,
                        PriceType = item2.Key,
                        ConditionId = item.Key,
                        SharesCountToday = SharesCountToday,
                        SharesCountYes = SharesCountYes,
                        PremiumRateToday = PremiumRateToday,
                        PremiumRate3days = PremiumRate3daysCount == 0 ? -1 : PremiumRate3daysTotal / PremiumRate3daysCount,
                        PremiumRate5days = PremiumRate5daysCount == 0 ? -1 : PremiumRate5daysTotal / PremiumRate5daysCount,
                        PremiumRate3daysPar = PremiumRate3daysPar,
                        PremiumRate5daysPar = PremiumRate5daysPar
                    });
                }
            }

            WriteToDb(listResult);
        }

        public static List<SharesPremiumStatisticDb> Cal_SharesPremiumStatisticByCondition(DateTime date, long conditionId)
        {
            Dictionary<long, Dictionary<int, List<SharesPremiumStatisticResult>>> resultDic = new Dictionary<long, Dictionary<int, List<SharesPremiumStatisticResult>>>();

            Dictionary<DateTime, Dictionary<long, LimitCountInfo>> LimitUpSession = new Dictionary<DateTime, Dictionary<long, LimitCountInfo>>();
            Dictionary<DateTime, Dictionary<long, LimitCountInfo>> LimitDownSession = new Dictionary<DateTime, Dictionary<long, LimitCountInfo>>();
            SharesPremiumStatisticSourceInfo sourceInfo = new SharesPremiumStatisticSourceInfo();
            using (var db = new meal_ticketEntities())
            {
                sourceInfo = (from item in db.t_shares_premium_statistic_condition
                              where item.Status == 1 && item.Id == conditionId
                              select new SharesPremiumStatisticSourceInfo
                              {
                                  ConditionStr = item.ConditionStr,
                                  ConditionId = item.Id
                              }).FirstOrDefault();
                LimitUpSession = (from item in db.t_shares_quotes_riselimit
                                  where item.IsLimitUpToday == true
                                  select new LimitCountInfo
                                  {
                                      SharesKey = item.SharesKey.Value,
                                      Date = item.DataDate,
                                      RiseLimitCount = item.RiseLimitCount,
                                      RiseLimitCountCondition = item.RiseLimitCountContinue
                                  }).ToList().GroupBy(e => e.Date).ToDictionary(k => k.Key, v => v.ToDictionary(k2 => k2.SharesKey, v2 => v2));
                LimitDownSession = (from item in db.t_shares_quotes_riselimit_down
                                    where item.IsLimitUpToday == true
                                    select new LimitCountInfo
                                    {
                                        SharesKey = item.SharesKey.Value,
                                        Date = item.DataDate,
                                        RiseLimitCount = item.RiseLimitCount,
                                        RiseLimitCountCondition = item.RiseLimitCountContinue
                                    }).ToList().GroupBy(e => e.Date).ToDictionary(k => k.Key, v => v.ToDictionary(k2 => k2.SharesKey, v2 => v2));
            }

            if (sourceInfo == null)
            {
                return new List<SharesPremiumStatisticDb>();
            }
            if (string.IsNullOrEmpty(sourceInfo.ConditionStr))
            {
                return new List<SharesPremiumStatisticDb>();
            }

            var shares_limit = Singleton.Instance.sessionHandler.GetShares_Limit_Session(false);
            DateTime dateYes = date;
            for (int i = 1; i <= 5; i++)
            {
                DateTime dateToday = dateYes;
                dateYes = DbHelper.GetLastTradeDate2(0, 0, 0, -1, dateToday);
                var shares_quotes_today = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(dateToday, false);
                var shares_quotes_yes = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(dateYes, false);

                Dictionary<long, LimitCountInfo> limit_up = new Dictionary<long, LimitCountInfo>();
                Dictionary<long, LimitCountInfo> limit_down = new Dictionary<long, LimitCountInfo>();
                if (LimitUpSession.ContainsKey(dateYes))
                {
                    limit_up = LimitUpSession[dateYes];
                }
                if (LimitDownSession.ContainsKey(dateYes))
                {
                    limit_down = LimitDownSession[dateYes];
                }

                try
                {
                    var ConditionPar = JsonConvert.DeserializeObject<SharesPremiumStatisticPar>(sourceInfo.ConditionStr);
                    if (!ParsePar(ConditionPar))
                    {
                        continue;
                    }
                    if (!resultDic.ContainsKey(sourceInfo.ConditionId))
                    {
                        resultDic.Add(sourceInfo.ConditionId, new Dictionary<int, List<SharesPremiumStatisticResult>>());
                    }
                    List<long> yesSharesList1 = GetYesShares(shares_quotes_yes, ConditionPar, 1, limit_up, limit_down, shares_limit);
                    List<long> todaySharesList1 = GetTodayShares(shares_quotes_today, yesSharesList1, ConditionPar, 1);
                    if (!resultDic[sourceInfo.ConditionId].ContainsKey(1))
                    {
                        resultDic[sourceInfo.ConditionId].Add(1, new List<SharesPremiumStatisticResult>());
                    }
                    resultDic[sourceInfo.ConditionId][1].Add(new SharesPremiumStatisticResult
                    {
                        YesSharesList = yesSharesList1,
                        TodaySharesList = todaySharesList1
                    });

                    List<long> yesSharesList2 = GetYesShares(shares_quotes_yes, ConditionPar, 2, limit_up, limit_down, shares_limit);
                    List<long> todaySharesList2 = GetTodayShares(shares_quotes_today, yesSharesList2, ConditionPar, 2);
                    if (!resultDic[sourceInfo.ConditionId].ContainsKey(2))
                    {
                        resultDic[sourceInfo.ConditionId].Add(2, new List<SharesPremiumStatisticResult>());
                    }
                    resultDic[sourceInfo.ConditionId][2].Add(new SharesPremiumStatisticResult
                    {
                        YesSharesList = yesSharesList2,
                        TodaySharesList = todaySharesList2
                    });


                    List<long> yesSharesList3 = GetYesShares(shares_quotes_yes, ConditionPar, 3, limit_up, limit_down, shares_limit);
                    List<long> todaySharesList3 = GetTodayShares(shares_quotes_today, yesSharesList3, ConditionPar, 3);
                    if (!resultDic[sourceInfo.ConditionId].ContainsKey(3))
                    {
                        resultDic[sourceInfo.ConditionId].Add(3, new List<SharesPremiumStatisticResult>());
                    }
                    resultDic[sourceInfo.ConditionId][3].Add(new SharesPremiumStatisticResult
                    {
                        YesSharesList = yesSharesList3,
                        TodaySharesList = todaySharesList3
                    });


                    List<long> yesSharesList4 = GetYesShares(shares_quotes_yes, ConditionPar, 4, limit_up, limit_down, shares_limit);
                    List<long> todaySharesList4 = GetTodayShares(shares_quotes_today, yesSharesList4, ConditionPar, 4);
                    if (!resultDic[sourceInfo.ConditionId].ContainsKey(4))
                    {
                        resultDic[sourceInfo.ConditionId].Add(4, new List<SharesPremiumStatisticResult>());
                    }
                    resultDic[sourceInfo.ConditionId][4].Add(new SharesPremiumStatisticResult
                    {
                        YesSharesList = yesSharesList4,
                        TodaySharesList = todaySharesList4
                    });
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("溢价条件计算出错", ex);
                    continue;
                }
            }

            List<SharesPremiumStatisticDb> listResult = new List<SharesPremiumStatisticDb>();
            foreach (var item in resultDic)
            {
                foreach (var item2 in item.Value)
                {
                    int SharesCountToday = 0;
                    int SharesCountYes = 0;
                    int PremiumRateToday = 0;

                    int PremiumRate3daysTotal = 0;
                    int PremiumRate3daysCount = 0;
                    string PremiumRate3daysPar = "";

                    int PremiumRate5daysTotal = 0;
                    int PremiumRate5daysCount = 0;
                    string PremiumRate5daysPar = "";

                    int idx = 0;
                    foreach (var item3 in item2.Value)
                    {
                        idx++;
                        if (idx == 1)
                        {
                            SharesCountToday = item3.TodaySharesList.Count();
                            SharesCountYes = item3.YesSharesList.Count();
                            PremiumRateToday = item3.PremiumRate;
                        }
                        if (idx <= 3)
                        {
                            if (item3.PremiumRate >= 0)
                            {
                                PremiumRate3daysTotal = PremiumRate3daysTotal + item3.PremiumRate;
                                PremiumRate3daysCount++;
                            }
                            PremiumRate3daysPar = PremiumRate3daysPar + (item3.PremiumRate == -1 ? "--、" : (Math.Round(item3.PremiumRate * 1.0 / 100, 2) + "%、"));
                        }
                        if (idx <= 5)
                        {
                            if (item3.PremiumRate >= 0)
                            {
                                PremiumRate5daysTotal = PremiumRate5daysTotal + item3.PremiumRate;
                                PremiumRate5daysCount++;
                            }
                            PremiumRate5daysPar = PremiumRate5daysPar + (item3.PremiumRate == -1 ? "--、" : (Math.Round(item3.PremiumRate * 1.0 / 100, 2) + "%、"));
                        }
                    }
                    PremiumRate3daysPar = PremiumRate3daysPar.TrimEnd('、');
                    PremiumRate5daysPar = PremiumRate5daysPar.TrimEnd('、');
                    listResult.Add(new SharesPremiumStatisticDb
                    {
                        Date = date,
                        PriceType = item2.Key,
                        ConditionId = item.Key,
                        SharesCountToday = SharesCountToday,
                        SharesCountYes = SharesCountYes,
                        PremiumRateToday = PremiumRateToday,
                        PremiumRate3days = PremiumRate3daysCount == 0 ? -1 : PremiumRate3daysTotal / PremiumRate3daysCount,
                        PremiumRate5days = PremiumRate5daysCount == 0 ? -1 : PremiumRate5daysTotal / PremiumRate5daysCount,
                        PremiumRate3daysPar = PremiumRate3daysPar,
                        PremiumRate5daysPar = PremiumRate5daysPar
                    });
                }
            }

            return listResult;
        }

        /// <summary>
        /// 获取昨日符合股票
        /// </summary>
        /// <returns></returns>
        private static List<long> GetYesShares(Dictionary<long, Shares_Quotes_Session_Info> shares_quotes_yes, SharesPremiumStatisticPar par,int priceType, Dictionary<long, LimitCountInfo> limit_up, Dictionary<long, LimitCountInfo> limit_down,List<long> shares_limit) 
        {
            List<long> result = new List<long>();
            foreach (var item in shares_quotes_yes)
            {
                if (shares_limit.Contains(item.Key))
                {
                    continue;
                }

                int riseRate = priceType == 1 ? item.Value.RiseRate : priceType == 2 ? item.Value.OpenRiseRate : priceType == 3 ? item.Value.MaxRiseRate : item.Value.MinRiseRate;
                int limitUpRiseRate = (int)Math.Round((item.Value.LimitUpPrice - item.Value.YestodayClosedPrice) * 1.0 / item.Value.YestodayClosedPrice * 10000, 0);
                int limitDownRiseRate = (int)Math.Round((item.Value.LimitDownPrice - item.Value.YestodayClosedPrice) * 1.0 / item.Value.YestodayClosedPrice * 10000, 0);

                int limitUpDays = 0;
                if (limit_up.ContainsKey(item.Key))
                {
                    var limit_up_info = limit_up[item.Key];
                    limitUpDays = par.isReverse == true ? limit_up_info.RiseLimitCount : limit_up_info.RiseLimitCountCondition;
                }
                int limitDownDays = 0;
                if (limit_down.ContainsKey(item.Key))
                {
                    var limit_down_info = limit_down[item.Key];
                    limitDownDays = par.isReverse == true ? limit_down_info.RiseLimitCount : limit_down_info.RiseLimitCountCondition;
                }

                if (par.riseType1 == 1 && ((par.compare1 == 1 && riseRate >= limitUpRiseRate + par.riseoffset1 * 100) || (par.compare1 == 2 && riseRate <= limitUpRiseRate + par.riseoffset1 * 100) || (par.riseoffset1==null && item.Value.PriceType==1)) && ((limitUpDays >= par.limitCount1 && limitUpDays <= par.limitCount2) || par.limitCount1 == null || par.limitCount2 == null))
                {
                    result.Add(item.Key);
                }
                else if (par.riseType1 == 2 && ((par.compare1 == 1 && riseRate >= limitDownRiseRate + par.riseoffset1 * 100) || (par.compare1 == 2 && riseRate <= limitDownRiseRate + par.riseoffset1 * 100) || (par.riseoffset1 == null && item.Value.PriceType == 2)) && ((limitDownDays >= par.limitCount1 && limitDownDays <= par.limitCount2) || par.limitCount1==null || par.limitCount2==null))
                {
                    result.Add(item.Key);
                }
                else if (par.riseType1 == 3 && ((par.compare1 == 1 && riseRate >= par.riseoffset1 * 100) || (par.compare1 == 2 && riseRate <= par.riseoffset1 * 100)) && ((par.compare2 == 1 && riseRate >= par.riseoffset2) || (par.compare2 == 2 && riseRate <= par.riseoffset2) || par.compare2==null))
                {
                    result.Add(item.Key);
                }

            }
            return result;
        }

        /// <summary>
        /// 获取今日符合股票
        /// </summary>
        /// <param name="sourceSharesList"></param>
        /// <returns></returns>
        private static List<long> GetTodayShares(Dictionary<long, Shares_Quotes_Session_Info> shares_quotes_today, List<long> sourceSharesList, SharesPremiumStatisticPar par, int priceType) 
        {
            List<long> result = new List<long>();
            foreach (var sharesKey in sourceSharesList)
            {
                if (!shares_quotes_today.ContainsKey(sharesKey))
                {
                    continue;
                }
                var shares_quotes = shares_quotes_today[sharesKey];
                int riseRate = priceType == 1 ? shares_quotes.RiseRate : priceType == 2 ? shares_quotes.OpenRiseRate : priceType == 3 ? shares_quotes.MaxRiseRate : shares_quotes.MinRiseRate;
                int limitUpRiseRate = (int)Math.Round((shares_quotes.LimitUpPrice - shares_quotes.YestodayClosedPrice) * 1.0 / shares_quotes.YestodayClosedPrice * 10000, 0);
                int limitDownRiseRate = (int)Math.Round((shares_quotes.LimitDownPrice - shares_quotes.YestodayClosedPrice) * 1.0 / shares_quotes.YestodayClosedPrice * 10000, 0);
                
                if (par.riseType2 == 1 && ((par.compare3 == 1 && riseRate >= limitUpRiseRate + par.riseoffset3 * 100) || (par.compare3 == 2 && riseRate <= limitUpRiseRate + par.riseoffset3 * 100) || (par.riseoffset3==null && shares_quotes.PriceType==1) ))
                {
                    result.Add(sharesKey);
                }
                else if (par.riseType2 == 2 && ((par.compare3 == 1 && riseRate >= limitDownRiseRate + par.riseoffset3 * 100) || (par.compare3 == 2 && riseRate <= limitDownRiseRate + par.riseoffset3 * 100) || (par.riseoffset3 == null && shares_quotes.PriceType == 2)))
                {
                    result.Add(sharesKey);
                }
                else if (par.riseType2 == 3 && ((par.compare3 == 1 && riseRate >= par.riseoffset3 * 100) || (par.compare3 == 2 && riseRate <= par.riseoffset3 * 100)))
                {
                    result.Add(sharesKey);
                }
            }
            return result;
        }

        public static void WriteToDb(List<SharesPremiumStatisticDb> listResult) 
        {
            DataTable table = new DataTable();
            table.Columns.Add("ConditionId", typeof(long));
            table.Columns.Add("PriceType", typeof(int));
            table.Columns.Add("SharesCountToday", typeof(int));
            table.Columns.Add("SharesCountYes", typeof(int));
            table.Columns.Add("PremiumRateToday", typeof(int));
            table.Columns.Add("PremiumRate3daysPar", typeof(string));
            table.Columns.Add("PremiumRate3days", typeof(int));
            table.Columns.Add("PremiumRate5daysPar", typeof(string));
            table.Columns.Add("PremiumRate5days", typeof(int));
            table.Columns.Add("Date", typeof(DateTime));

            foreach (var item in listResult)
            {
                DataRow row = table.NewRow();
                row["ConditionId"] = item.ConditionId;
                row["PriceType"] = item.PriceType;
                row["SharesCountToday"] = item.SharesCountToday;
                row["SharesCountYes"] = item.SharesCountYes;
                row["PremiumRateToday"] = item.PremiumRateToday;
                row["PremiumRate3daysPar"] = item.PremiumRate3daysPar;
                row["PremiumRate3days"] = item.PremiumRate3days;
                row["PremiumRate5daysPar"] = item.PremiumRate5daysPar;
                row["PremiumRate5days"] = item.PremiumRate5days;
                row["Date"] = item.Date;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesPremiumStatisticInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesPremiumStatisticInfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_SharesPremiumStatistic_Update @sharesPremiumStatisticInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("Cal_SharesPremiumStatistic入库失败", ex);
                    tran.Rollback();
                }
            }
        }

        public static bool ParsePar(SharesPremiumStatisticPar par)
        {
            if (par.compare1 == null || par.riseType1==null || par.compare3==null || par.riseType2==null)
            {
                return false;
            }
            if (par.riseType1 == 3 && par.riseoffset1 == null)
            {
                par.riseoffset1 = 0;
            }
            if (par.compare2== null)
            {
                par.riseoffset2 = 0;
            }
            if (par.riseType2 == 3 && par.riseoffset3 == null)
            {
                par.riseoffset3 = 0;
            }
            return true;
        }
    }
}
