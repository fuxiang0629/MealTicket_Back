using FXCommon.Common;
using MealTicket_DBCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class SessionHandler: Session_New
    {
        /// <summary>
        /// 最小间隔
        /// </summary>
        const int MIN_TIMER_INTERVAL = 1000;

        enum Enum_Excute_Type
        {
            HqServerPar,
            SharesCodeMatch0,
            SharesCodeMatch1,
            RangeList,
            LimitTimeList,
            HostList,
            SharesBaseInfoList,
            LastSharesQuotesList
        }

        public enum Enum_Excute_DataKey
        {
            HqServerPar,
            SharesCodeMatch0,
            SharesCodeMatch1,
            RangeList,
            LimitTimeList,
            HostList,
            SharesBaseInfoList,
            LastSharesQuotesList
        }

        private List<Session_Time_Info>  Session_Time_Info_List_Init() 
        {
            List<Session_Time_Info> result = new List<Session_Time_Info>();
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.HqServerPar.ToString(),
                ExcuteInterval = 60*10,
                ExcuteType = (int)Enum_Excute_Type.HqServerPar,
                NextExcuteTime = null,
                TimerStatus=1,
                TimerType=1,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.SharesCodeMatch0.ToString(),
                ExcuteInterval = 60 * 10,
                ExcuteType = (int)Enum_Excute_Type.SharesCodeMatch0,
                NextExcuteTime = null,
                TimerStatus = 1,
                TimerType = 1,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.SharesCodeMatch1.ToString(),
                ExcuteInterval = 60 * 10,
                ExcuteType = (int)Enum_Excute_Type.SharesCodeMatch1,
                NextExcuteTime = null,
                TimerStatus = 1,
                TimerType = 1,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.RangeList.ToString(),
                ExcuteInterval = 60 * 10,
                ExcuteType = (int)Enum_Excute_Type.RangeList,
                NextExcuteTime = null,
                TimerStatus = 1,
                TimerType = 1,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.LimitTimeList.ToString(),
                ExcuteInterval = 60 * 10,
                ExcuteType = (int)Enum_Excute_Type.LimitTimeList,
                NextExcuteTime = null,
                TimerStatus = 1,
                TimerType = 1,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.HostList.ToString(),
                ExcuteInterval = 60 * 10,
                ExcuteType = (int)Enum_Excute_Type.HostList,
                NextExcuteTime = null,
                TimerStatus = 0,
                TimerType = 1,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.SharesBaseInfoList.ToString(),
                ExcuteInterval = 60 * 60,
                ExcuteType = (int)Enum_Excute_Type.SharesBaseInfoList,
                NextExcuteTime = null,
                TimerStatus = 1,
                TimerType = 1,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.LastSharesQuotesList.ToString(),
                ExcuteInterval = 60 * 60 * 24,
                ExcuteType = (int)Enum_Excute_Type.LastSharesQuotesList,
                NextExcuteTime = DateTime.Now.Date.AddHours(1),
                TimerStatus = 1,
                TimerType = 0,
            });

            return result;
        }

        public SessionHandler()
        {
            List<Session_Time_Info> Session_Time_Info_List = Session_Time_Info_List_Init();
            Init(MIN_TIMER_INTERVAL, Session_Time_Info_List);
        }

        public override object UpdateSession(int ExcuteType, object oct = null)
        {
            using (var db = new meal_ticketEntities())
            {
                switch (ExcuteType)
                {
                    case (int)Enum_Excute_Type.HqServerPar:
                        return DoHqServerParUpdate(db);
                    case (int)Enum_Excute_Type.SharesCodeMatch0:
                        return DoSharesCodeMatch0Update(db);
                    case (int)Enum_Excute_Type.SharesCodeMatch1:
                        return DoSharesCodeMatch1Update(db);
                    case (int)Enum_Excute_Type.RangeList:
                        return DoRangeListUpdate(db);
                    case (int)Enum_Excute_Type.LimitTimeList:
                        return DoLimitTimeListUpdate(db);
                    case (int)Enum_Excute_Type.HostList:
                        return DoHostListUpdate(db);
                    case (int)Enum_Excute_Type.SharesBaseInfoList:
                        return DoSharesBaseInfoListUpdate(db);
                    case (int)Enum_Excute_Type.LastSharesQuotesList:
                        return DoLastSharesQuotesListUpdate(db);
                    default:
                        return null;
                }
            }
        }

        private HqServerParInfo DoHqServerParUpdate(meal_ticketEntities db)
        {
            HqServerParInfo result = new HqServerParInfo 
            {
                SshqUpdateRate=3000,
                AllSharesEndHour= TimeSpan.Parse("04:00:00"),
                AllSharesStartHour= TimeSpan.Parse("01:00:00"),
                BusinessRunTime= 3000,
                QuotesCount= 75
            };
            try
            {
                var sysPar = (from item in db.t_system_param
                              where item.ParamName == "HqServerPar"
                              select item).FirstOrDefault();
                if (sysPar!=null)
                {
                    var sysValue = JsonConvert.DeserializeObject<HqServerParInfo>(sysPar.ParamValue);
                    if (sysValue.SshqUpdateRate > 0 && sysValue.SshqUpdateRate < 60000)
                    {
                        result.SshqUpdateRate = sysValue.SshqUpdateRate;
                    }
                    if (sysValue.QuotesCount > 0 && sysValue.QuotesCount <= 80)
                    {
                        result.QuotesCount = sysValue.QuotesCount;
                    }
                    if (sysValue.AllSharesStartHour != null)
                    {
                        result.AllSharesStartHour = sysValue.AllSharesStartHour;
                    }
                    if (sysValue.AllSharesEndHour != null)
                    {
                        result.AllSharesEndHour = sysValue.AllSharesEndHour;
                    }
                    if (sysValue.BusinessRunTime > 0 && sysValue.BusinessRunTime < 1800000)
                    {
                        result.BusinessRunTime = sysValue.BusinessRunTime;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("行情系统参数更新有误", ex);
            }
            return result;
        }
        private string DoSharesCodeMatch0Update(meal_ticketEntities db)
        {
            try
            {
                var sysPar = (from item in db.t_system_param
                              where item.ParamName == "SharesCodeMatch0"
                              select item).FirstOrDefault();
                if (sysPar != null)
                {
                    return sysPar.ParamValue;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("行情系统参数更新有误", ex);
            }
            return null;
        }
        private string DoSharesCodeMatch1Update(meal_ticketEntities db)
        {
            try
            {
                var sysPar = (from item in db.t_system_param
                              where item.ParamName == "SharesCodeMatch1"
                              select item).FirstOrDefault();
                if (sysPar!=null)
                {
                    return sysPar.ParamValue;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("行情系统参数更新有误", ex);
            }
            return null;
        }
        private List<t_shares_limit_fundmultiple> DoRangeListUpdate(meal_ticketEntities db)
        {
            try
            {
                var RangeList = (from item in db.t_shares_limit_fundmultiple
                                 select item).ToList();
                return RangeList;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("杠杆参数更新有误", ex);
                return null;
            }
        }
        private List<t_shares_limit_time> DoLimitTimeListUpdate(meal_ticketEntities db)
        {
            try
            {
                var Limit_Time_Session = (from item in db.t_shares_limit_time
                                          select item).ToList();
                return Limit_Time_Session;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("杠杆参数更新有误", ex);
                return null;
            }
        }
        private List<HostInfo> DoHostListUpdate(meal_ticketEntities db)
        {
            try
            {
                var HostList = (from item in db.t_shares_hq_host
                                where item.Status == 1
                                select new HostInfo
                                {
                                    Ip = item.IpAddress,
                                    Port = item.Port
                                }).ToList();
                return HostList;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("行情服务器参数更新有误", ex);
                return null;
            }
        }
        private List<SharesBaseInfo> DoSharesBaseInfoListUpdate(meal_ticketEntities db)
        {
            try
            {
                var SharesBaseInfoList = (from x in db.t_shares_all
                                          select new SharesBaseInfo
                                          {
                                              ShareCode = x.SharesCode,
                                              ShareHandCount = x.SharesHandCount,
                                              ShareName = x.SharesName,
                                              Pyjc = x.SharesPyjc,
                                              ShareClosedPrice = x.ShareClosedPrice,
                                              Market = x.Market
                                          }).ToList();
                return SharesBaseInfoList;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("所有股票列表参数更新有误", ex);
                return null;
            }
        }
        private Dictionary<int, SharesQuotesInfo> DoLastSharesQuotesListUpdate(meal_ticketEntities db)
        {
            try
            {
                DateTime dateNow = DateTime.Now.Date;
                var LastSharesQuotesList = (from x in db.v_shares_quotes_last
                                            where x.LastModified > dateNow
                                            select new SharesQuotesInfo
                                            {
                                                SellCount1 = x.SellCount1,
                                                SellCount2 = x.SellCount2,
                                                SellCount3 = x.SellCount3,
                                                SellCount4 = x.SellCount4,
                                                Activity = x.Activity,
                                                SellCount5 = x.SellCount5,
                                                SellPrice1 = x.SellPrice1,
                                                SellPrice2 = x.SellPrice2,
                                                SellPrice3 = x.SellPrice3,
                                                SellPrice4 = x.SellPrice4,
                                                SellPrice5 = x.SellPrice5,
                                                SharesCode = x.SharesCode,
                                                SpeedUp = x.SpeedUp,
                                                BuyCount1 = x.BuyCount1,
                                                BuyCount2 = x.BuyCount2,
                                                BuyCount3 = x.BuyCount3,
                                                BuyCount4 = x.BuyCount4,
                                                BuyCount5 = x.BuyCount5,
                                                BuyPrice1 = x.BuyPrice1,
                                                BuyPrice2 = x.BuyPrice2,
                                                BuyPrice3 = x.BuyPrice3,
                                                BuyPrice4 = x.BuyPrice4,
                                                BuyPrice5 = x.BuyPrice5,
                                                ClosedPrice = x.ClosedPrice,
                                                InvolCount = x.InvolCount,
                                                LastModified = x.LastModified,
                                                LimitDownPrice = x.LimitDownPrice,
                                                LimitUpPrice = x.LimitUpPrice,
                                                MaxPrice = x.MaxPrice,
                                                MinPrice = x.MinPrice,
                                                OpenedPrice = x.OpenedPrice,
                                                OuterCount = x.OuterCount,
                                                PresentCount = x.PresentCount,
                                                PresentPrice = x.PresentPrice,
                                                PriceType = x.PriceType,
                                                TriNearLimitType = x.TriNearLimitType,
                                                TotalAmount = x.TotalAmount,
                                                TotalCount = x.TotalCount,
                                                TriPriceType = x.TriPriceType,
                                                Market = x.Market
                                            }).ToList().ToDictionary(k => int.Parse(k.SharesCode) * 10 + k.Market, v => v);
                return LastSharesQuotesList;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("股票行情参数更新有误", ex);
                return null;
            }
        }

        public int GetSshqUpdateRate() 
        {
            var HqServerPar = GetDataWithLock(Enum_Excute_DataKey.HqServerPar.ToString());
            if (HqServerPar == null)
            {
                return 3000;
            }
            return (HqServerPar as HqServerParInfo).SshqUpdateRate;
        }
        public int GetQuotesCount()
        {
            var HqServerPar = GetDataWithLock(Enum_Excute_DataKey.HqServerPar.ToString());
            if (HqServerPar == null)
            {
                return 75;
            }
            return (HqServerPar as HqServerParInfo).QuotesCount;
        }
        public TimeSpan GetAllSharesStartHour()
        {
            var HqServerPar = GetDataWithLock(Enum_Excute_DataKey.HqServerPar.ToString());
            if (HqServerPar == null)
            {
                return TimeSpan.Parse("01:00:00");
            }
            return (HqServerPar as HqServerParInfo).AllSharesStartHour;
        }
        public TimeSpan GetAllSharesEndHour()
        {
            var HqServerPar = GetDataWithLock(Enum_Excute_DataKey.HqServerPar.ToString());
            if (HqServerPar == null)
            {
                return TimeSpan.Parse("04:00:00");
            }
            return (HqServerPar as HqServerParInfo).AllSharesEndHour;
        }
        public int GetBusinessRunTime()
        {
            var HqServerPar = GetDataWithLock(Enum_Excute_DataKey.HqServerPar.ToString());
            if (HqServerPar == null)
            {
                return 3000;
            }
            return (HqServerPar as HqServerParInfo).BusinessRunTime;
        }
        public string GetSharesCodeMatch0()
        {
            var SharesCodeMatch0 = GetDataWithLock(Enum_Excute_DataKey.SharesCodeMatch0.ToString());
            if (SharesCodeMatch0 == null)
            {
                return "(^00.*)|(^30.*)";
            }
            return SharesCodeMatch0.ToString();
        }
        public string GetSharesCodeMatch1()
        {
            var SharesCodeMatch1 = GetDataWithLock(Enum_Excute_DataKey.SharesCodeMatch1.ToString());
            if (SharesCodeMatch1 == null)
            {
                return "(^6.*)";
            }
            return SharesCodeMatch1.ToString();
        }
        public List<t_shares_limit_fundmultiple> GetRangeList()
        {
            var RangeList = GetDataWithLock(Enum_Excute_DataKey.RangeList.ToString());
            if (RangeList == null)
            {
                return new List<t_shares_limit_fundmultiple>();
            }
            return RangeList as List<t_shares_limit_fundmultiple>;
        }
        public List<t_shares_limit_time> GetLimitTimeList()
        {
            var LimitTimeList = GetDataWithLock(Enum_Excute_DataKey.LimitTimeList.ToString());
            if (LimitTimeList == null)
            {
                return new List<t_shares_limit_time>();
            }
            return LimitTimeList as List<t_shares_limit_time>;
        }
        public List<HostInfo> GetHostList()
        {
            var HostList = GetDataWithLock(Enum_Excute_DataKey.HostList.ToString());
            if (HostList == null)
            {
                return new List<HostInfo>();
            }
            return HostList as List<HostInfo>;
        }
        public List<SharesBaseInfo> GetSharesBaseInfoList()
        {
            var SharesBaseInfoList = GetDataWithLock(Enum_Excute_DataKey.SharesBaseInfoList.ToString());
            if (SharesBaseInfoList == null)
            {
                return new List<SharesBaseInfo>();
            }
            return SharesBaseInfoList as List<SharesBaseInfo>;
        }
        public Dictionary<int, SharesQuotesInfo> GetLastSharesQuotesList()
        {
            var LastSharesQuotesList = GetDataWithLock(Enum_Excute_DataKey.LastSharesQuotesList.ToString());
            if (LastSharesQuotesList == null)
            {
                return new Dictionary<int, SharesQuotesInfo>();
            }
            return LastSharesQuotesList as Dictionary<int, SharesQuotesInfo>;
        }
    }
}
