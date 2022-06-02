using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{ 
    public class AccountRiseLimitTriSession:Session<List<AccountRiseLimitTriInfo_Session>>
    {
        public AccountRiseLimitTriSession() 
        {
            Name = "AccountRiseLimitTriSession";
        }

        public override List<AccountRiseLimitTriInfo_Session> UpdateSession()
        {
            DateTime dateNow = Helper.GetLastTradeDate(-9, 0, 0);
            DateTime yesDateNow = Helper.GetLastTradeDate(-9, 0, 0,1);
            DateTime yes2DateNow = Helper.GetLastTradeDate(-9, 0, 0, 2);
            string dateNowStr = dateNow.ToString("yyyy-MM-dd");
            string yesDateNowStr = yesDateNow.ToString("yyyy-MM-dd");
            string yes2DateNowStr = yes2DateNow.ToString("yyyy-MM-dd");
            DateTime InitPushTime = dateNow.AddSeconds(1);

            using (var db = new meal_ticketEntities())
            {
                //昨日涨停股票
                var yesDic = (from item in db.t_shares_quotes_date
                              where item.Date == yesDateNowStr && (item.PriceType == 1 || item.PriceType == 2)
                              select new
                              {
                                  PriceType = item.PriceType,
                                  Market = item.Market,
                                  SharesCode = item.SharesCode,
                              }).ToList().ToDictionary(k => k.PriceType*10000000 + int.Parse(k.SharesCode) * 10 + k.Market, v => v);
                var yesHisDic = (from item in db.t_shares_quotes_history
                                 where item.Date == yesDateNowStr && (item.Type == 1 || item.Type == 2) && item.BusinessType == 1
                                 group item by new
                                 {
                                     item.Market,
                                     item.SharesCode,
                                     item.Type
                                 } into g
                                 select new
                                 {
                                     Market = g.Key.Market,
                                     SharesCode = g.Key.SharesCode,
                                     PriceType = g.Key.Type,
                                     PushTime = g.Min(e => e.CreateTime)
                                 }).ToList().ToDictionary(k => k.PriceType * 10000000 + int.Parse(k.SharesCode) * 10 + k.Market, v => v);
                //前日涨停股票
                var yes2Dic = (from item in db.t_shares_quotes_date
                               where item.Date == yes2DateNowStr && (item.PriceType == 1 || item.PriceType == 2)
                               select new
                               {
                                   PriceType = item.PriceType,
                                   Market = item.Market,
                                   SharesCode = item.SharesCode,
                               }).ToList().ToDictionary(k => k.PriceType * 10000000 + int.Parse(k.SharesCode) * 10 + k.Market, v => v);
                //历史数据
                var hisDic = (from item in db.t_shares_quotes_history
                              where item.Date == dateNowStr && (item.Type==1 || item.Type==2 || (item.Type==3 && item.BusinessType==1) || (item.Type == 4 && item.BusinessType == 1) || (item.Type == 5 && item.BusinessType == 2) || (item.Type == 6 && item.BusinessType == 2)) 
                              group item by new
                              {
                                  item.Market,
                                  item.SharesCode,
                                  item.Type
                              } into g
                              select new
                              {
                                  Market = g.Key.Market,
                                  SharesCode = g.Key.SharesCode,
                                  Type = g.Key.Type == 1 ? -1 : g.Key.Type == 2 ? -2 : g.Key.Type == 3 ? -3 : g.Key.Type == 4 ? -4 : g.Key.Type == 5 ? -5 : g.Key.Type == 6? -6 : 0,
                                  TriCountToday = g.Count(),
                                  PushTime = g.Max(e => e.CreateTime)
                              }).Where(e=>e.Type!=0).ToDictionary(k => -k.Type * 10000000 + int.Parse(k.SharesCode) * 10 + k.Market);
                //配置信息
                var setting = (from item in db.t_shares_limit_fundmultiple
                               select item).ToList();
                //当前是否交易时间
                bool isTradeTime = Helper.CheckTradeTime();

                var result = (from item in db.t_shares_quotes_date
                              where item.Date == dateNowStr
                              select new
                              {
                                  Market = item.Market,
                                  SharesCode = item.SharesCode,
                                  ClosedPrice = item.ClosedPrice,
                                  OpenPrice = item.OpenedPrice,
                                  PresentPrice = item.PresentPrice,
                                  PriceType = item.PriceType,
                                  LimitUpCount = item.LimitUpCount,
                                  LimitDownCount = item.TriLimitDownCount,
                                  TriNearLimitType = item.TriNearLimitType,
                                  LimitUpDays = (!isTradeTime && item.PriceType != 1) ? 0 : (item.LimitUpDays + (item.PriceType == 1 ? 1 : 0))
                              }).ToList();

                List<AccountRiseLimitTriInfo_Session> tempList = new List<AccountRiseLimitTriInfo_Session>();
                foreach (var item in result)
                {
                    if (item.PresentPrice <= 0)
                    {
                        continue;
                    }
                    bool type11 = false;
                    bool type12 = false;
                    if (!isTradeTime)
                    {
                        type11 = true;
                        type12 = true;
                    }
                    if (item.PriceType == 1)
                    {
                        int key = 10000000 + int.Parse(item.SharesCode) * 10 + item.Market;
                        if (hisDic.ContainsKey(key))
                        {
                            tempList.Add(new AccountRiseLimitTriInfo_Session
                            {
                                SharesCode = item.SharesCode,
                                Market = item.Market,
                                LimitUpDays=item.LimitUpDays,
                                PresentPrice = item.PresentPrice,
                                Type = -1,
                                ClosedPrice = item.ClosedPrice,
                                Mark = "",
                                RelId = item.SharesCode,
                                PushTime = hisDic[key].PushTime,
                                TriCountToday = hisDic[key].TriCountToday
                            });
                            if (yesDic.ContainsKey(10000000+int.Parse(item.SharesCode) * 10 + item.Market))
                            {
                                tempList.Add(new AccountRiseLimitTriInfo_Session
                                {
                                    SharesCode = item.SharesCode,
                                    Market = item.Market,
                                    LimitUpDays = item.LimitUpDays,
                                    PresentPrice = item.PresentPrice,
                                    Type = -11,
                                    ClosedPrice = item.ClosedPrice,
                                    Mark = "",
                                    RelId = item.SharesCode,
                                    PushTime = hisDic[key].PushTime,
                                    TriCountToday = 0
                                });
                                type11 = true;
                            }
                        }
                    }
                    if (item.PriceType == 2)
                    {
                        int key = 20000000 + int.Parse(item.SharesCode) * 10 + item.Market;
                        if (hisDic.ContainsKey(key))
                        {
                            tempList.Add(new AccountRiseLimitTriInfo_Session
                            {
                                SharesCode = item.SharesCode,
                                Market = item.Market,
                                LimitUpDays = item.LimitUpDays,
                                PresentPrice = item.PresentPrice,
                                Type = -2,
                                ClosedPrice = item.ClosedPrice,
                                Mark = "",
                                RelId = item.SharesCode,
                                PushTime = hisDic[key].PushTime,
                                TriCountToday = hisDic[key].TriCountToday
                            });
                            if (yesDic.ContainsKey(20000000+int.Parse(item.SharesCode) * 10 + item.Market))
                            {
                                tempList.Add(new AccountRiseLimitTriInfo_Session
                                {
                                    SharesCode = item.SharesCode,
                                    Market = item.Market,
                                    LimitUpDays = item.LimitUpDays,
                                    PresentPrice = item.PresentPrice,
                                    Type = -12,
                                    ClosedPrice = item.ClosedPrice,
                                    Mark = "",
                                    RelId = item.SharesCode,
                                    PushTime = hisDic[key].PushTime,
                                    TriCountToday = 0
                                });
                                type12 = true;
                            }
                        }
                    }
                    if (item.PriceType == 0 && item.LimitUpCount > 0)
                    {
                        int key = 30000000 + int.Parse(item.SharesCode) * 10 + item.Market;
                        if (hisDic.ContainsKey(key))
                        {
                            tempList.Add(new AccountRiseLimitTriInfo_Session
                            {
                                SharesCode = item.SharesCode,
                                Market = item.Market,
                                LimitUpDays = item.LimitUpDays,
                                PresentPrice = item.PresentPrice,
                                Type = -3,
                                ClosedPrice = item.ClosedPrice,
                                Mark = "",
                                RelId = item.SharesCode,
                                PushTime = hisDic[key].PushTime,
                                TriCountToday = hisDic[key].TriCountToday
                            });
                        }
                    }
                    if (item.PriceType == 0 && item.LimitDownCount > 0)
                    {
                        int key = 40000000+int.Parse(item.SharesCode) * 10 + item.Market;
                        if (hisDic.ContainsKey(key))
                        {
                            tempList.Add(new AccountRiseLimitTriInfo_Session
                            {
                                SharesCode = item.SharesCode,
                                Market = item.Market,
                                LimitUpDays = item.LimitUpDays,
                                PresentPrice = item.PresentPrice,
                                Type = -4,
                                ClosedPrice = item.ClosedPrice,
                                Mark = "",
                                RelId = item.SharesCode,
                                PushTime = hisDic[key].PushTime,
                                TriCountToday = hisDic[key].TriCountToday
                            });
                        }
                    }
                    if (item.TriNearLimitType == 1 && item.PriceType == 0)
                    {
                        int key = 50000000 + int.Parse(item.SharesCode) * 10 + item.Market;
                        if (hisDic.ContainsKey(key))
                        {
                            tempList.Add(new AccountRiseLimitTriInfo_Session
                            {
                                SharesCode = item.SharesCode,
                                Market = item.Market,
                                LimitUpDays = item.LimitUpDays,
                                PresentPrice = item.PresentPrice,
                                Type = -5,
                                ClosedPrice = item.ClosedPrice,
                                Mark = item.LimitUpCount > 0 ? "炸" : "",
                                RelId = item.SharesCode,
                                PushTime = hisDic[key].PushTime,
                                TriCountToday = hisDic[key].TriCountToday
                            });
                        }
                    }
                    if (item.TriNearLimitType == 2 && item.PriceType == 0)
                    {
                        int key = 60000000 + int.Parse(item.SharesCode) * 10 + item.Market;
                        if (hisDic.ContainsKey(key))
                        {
                            tempList.Add(new AccountRiseLimitTriInfo_Session
                            {
                                SharesCode = item.SharesCode,
                                Market = item.Market,
                                LimitUpDays = item.LimitUpDays,
                                PresentPrice = item.PresentPrice,
                                Type = -6,
                                ClosedPrice = item.ClosedPrice,
                                Mark = item.LimitDownCount > 0 ? "撬" : "",
                                RelId = item.SharesCode,
                                PushTime = hisDic[key].PushTime,
                                TriCountToday = hisDic[key].TriCountToday
                            });
                        }
                    }

                    if (yesDic.ContainsKey(10000000 + int.Parse(item.SharesCode) * 10 + item.Market))
                    {
                        if (yesHisDic.ContainsKey(10000000 + int.Parse(item.SharesCode) * 10 + item.Market))
                        {
                            tempList.Add(new AccountRiseLimitTriInfo_Session
                            {
                                SharesCode = item.SharesCode,
                                Market = item.Market,
                                LimitUpDays = item.LimitUpDays,
                                PresentPrice = item.PresentPrice,
                                Type = -7,
                                ClosedPrice = item.ClosedPrice,
                                Mark = "",
                                RelId = item.SharesCode,
                                PushTime = InitPushTime,
                                YesLimitTime = yesHisDic[10000000 + int.Parse(item.SharesCode) * 10 + item.Market].PushTime,
                                TriCountToday = 0
                            });
                            //判断连板(前日涨停)
                            if (!type11 && yes2Dic.ContainsKey(10000000 + int.Parse(item.SharesCode) * 10 + item.Market))
                            {
                                tempList.Add(new AccountRiseLimitTriInfo_Session
                                {
                                    SharesCode = item.SharesCode,
                                    Market = item.Market,
                                    LimitUpDays = item.LimitUpDays,
                                    PresentPrice = item.PresentPrice,
                                    Type = -11,
                                    ClosedPrice = item.ClosedPrice,
                                    Mark = "",
                                    RelId = item.SharesCode,
                                    PushTime = InitPushTime,
                                    TriCountToday = 0
                                });
                            }
                        }
                    }
                    if (yesDic.ContainsKey(20000000 + int.Parse(item.SharesCode) * 10 + item.Market))
                    {
                        if (yesHisDic.ContainsKey(20000000 + int.Parse(item.SharesCode) * 10 + item.Market))
                        {
                            tempList.Add(new AccountRiseLimitTriInfo_Session
                            {
                                SharesCode = item.SharesCode,
                                Market = item.Market,
                                LimitUpDays = item.LimitUpDays,
                                PresentPrice = item.PresentPrice,
                                Type = -8,
                                ClosedPrice = item.ClosedPrice,
                                Mark = "",
                                RelId = item.SharesCode,
                                PushTime = InitPushTime,
                                YesLimitTime = yesHisDic[20000000 + int.Parse(item.SharesCode) * 10 + item.Market].PushTime,
                                TriCountToday = 0
                            });
                            //判断连板(前日日跌停或今日跌停)
                            if (!type12 && yes2Dic.ContainsKey(20000000 + int.Parse(item.SharesCode) * 10 + item.Market))
                            {
                                tempList.Add(new AccountRiseLimitTriInfo_Session
                                {
                                    SharesCode = item.SharesCode,
                                    Market = item.Market,
                                    LimitUpDays = item.LimitUpDays,
                                    PresentPrice = item.PresentPrice,
                                    Type = -12,
                                    ClosedPrice = item.ClosedPrice,
                                    Mark = "",
                                    RelId = item.SharesCode,
                                    PushTime = InitPushTime,
                                    TriCountToday = 0
                                });
                            }
                        }
                    }

                    var thisSetting = setting.Where(e => (item.Market == e.LimitMarket || e.LimitMarket == -1) && item.SharesCode.StartsWith(e.LimitKey)).OrderByDescending(e => e.Priority).FirstOrDefault();
                    if (thisSetting != null && item.ClosedPrice > 0 && (item.OpenPrice - item.ClosedPrice) * 10000.0 / item.ClosedPrice >= thisSetting.HighOpenRange)
                    {
                        tempList.Add(new AccountRiseLimitTriInfo_Session
                        {
                            SharesCode = item.SharesCode,
                            Market = item.Market,
                            LimitUpDays = item.LimitUpDays,
                            PresentPrice = item.PresentPrice,
                            Type = -9,
                            ClosedPrice = item.ClosedPrice,
                            Mark = "",
                            RelId = item.SharesCode,
                            PushTime = InitPushTime,
                            TriCountToday = 0
                        });
                    }
                    if (thisSetting != null && item.ClosedPrice > 0 && (item.OpenPrice - item.ClosedPrice) * 10000.0 / item.ClosedPrice <= thisSetting.LowOpenRange)
                    {
                        tempList.Add(new AccountRiseLimitTriInfo_Session
                        {
                            SharesCode = item.SharesCode,
                            Market = item.Market,
                            LimitUpDays = item.LimitUpDays,
                            PresentPrice = item.PresentPrice,
                            Type = -10,
                            ClosedPrice = item.ClosedPrice,
                            Mark = "",
                            RelId = item.SharesCode,
                            PushTime = InitPushTime,
                            TriCountToday = 0
                        });
                    }
                }
                return tempList;
            }
        }
    }
}
