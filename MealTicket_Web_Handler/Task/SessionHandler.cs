using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class SessionHandler : Session_New
    {
        /// <summary>
        /// 最小间隔
        /// </summary>
        const int MIN_TIMER_INTERVAL = 1000;

        public enum Enum_Excute_Type
        {
            Plate_Quotes_Date_Session,
            Plate_Quotes_Today_Session,
            Plate_Tag_FocusOn_Session,
            Plate_Tag_Force_Session,
            Plate_Tag_TrendLike_Session,
            Shares_Quotes_Date_Session,
            Shares_Quotes_Today_Session,
            Plate_Tag_Setting_Session,
            Shares_Tag_Leader_Session,
            Shares_Tag_DayLeader_Session,
            Shares_Tag_MainArmy_Session,
            Plate_Shares_Rel_Tag_Setting_Session,
            Shares_Base_Session,
            Shares_TradeStock_Session
        }

        public enum Enum_Excute_DataKey
        {
            Plate_Quotes_Date_Session,
            Plate_Quotes_Today_Session,
            Plate_Tag_FocusOn_Session,
            Plate_Tag_Force_Session,
            Plate_Tag_TrendLike_Session,
            Shares_Quotes_Date_Session,
            Shares_Quotes_Today_Session,
            Plate_Tag_Setting_Session,
            Shares_Tag_Leader_Session,
            Shares_Tag_DayLeader_Session,
            Shares_Tag_MainArmy_Session,
            Plate_Shares_Rel_Tag_Setting_Session,
            Shares_Base_Session,
            Shares_TradeStock_Session
        }

        public SessionHandler() 
        {
            List<Session_Time_Info> Session_Time_Info_List = Session_Time_Info_List_Init();
            Init(MIN_TIMER_INTERVAL, Session_Time_Info_List);
        }

        private List<Session_Time_Info> Session_Time_Info_List_Init()
        {
            List<Session_Time_Info> result = new List<Session_Time_Info>();
            //板块历史行情缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Quotes_Date_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Plate_Quotes_Date_Session, DateTime.Now.Date.AddHours(1),0,0));
            //板块今日行情缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Quotes_Today_Session.ToString(), 3, (int)Enum_Excute_Type.Plate_Quotes_Today_Session, null, 0, 0));
            //板块重点关注结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Tag_FocusOn_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Plate_Tag_FocusOn_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //板块强势上涨结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Tag_Force_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Plate_Tag_Force_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //板块走势最像结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Tag_TrendLike_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Plate_Tag_TrendLike_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //股票历史行情缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Quotes_Date_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_Quotes_Date_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //股票今日行情缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Quotes_Today_Session.ToString(), 3, (int)Enum_Excute_Type.Shares_Quotes_Today_Session, null, 0, 0));
            //板块标签设置缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Tag_Setting_Session.ToString(), 60, (int)Enum_Excute_Type.Plate_Tag_Setting_Session, null, 0, 0));
            //板块龙头计算结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Tag_Leader_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_Tag_Leader_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //板块日内龙头计算结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Tag_DayLeader_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_Tag_DayLeader_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //板块中军计算结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Tag_MainArmy_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_Tag_MainArmy_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //股票标签设置缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Shares_Rel_Tag_Setting_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Plate_Shares_Rel_Tag_Setting_Session, null, 0, 0));
            //股票基础数据缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Base_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_Base_Session, DateTime.Now.Date.AddHours(9).AddMinutes(15), 0, 0));
            //股票昨日成交量缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_TradeStock_Session.ToString(), 3, (int)Enum_Excute_Type.Shares_TradeStock_Session, null, 0, 0));
            return result;
        }

        private Session_Time_Info _toBuildTimeInfo(string DataKey,int ExcuteInterval,int ExcuteType,DateTime? NextExcuteTime,int TimerStatus,int TimerType)
        {
            return new Session_Time_Info
            {
                DataKey = DataKey,
                ExcuteInterval = ExcuteInterval,
                ExcuteType = ExcuteType,
                NextExcuteTime = NextExcuteTime,
                TimerStatus = TimerStatus,
                TimerType = TimerType,
            };
        }

        public override object UpdateSession(int ExcuteType)
        {
            switch (ExcuteType) 
            {
                case (int)Enum_Excute_Type.Plate_Quotes_Date_Session:
                    return Plate_Quotes_Date_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Quotes_Today_Session:
                    return Plate_Quotes_Today_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Tag_FocusOn_Session:
                    return Plate_Tag_FocusOn_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Tag_Force_Session:
                    return Plate_Tag_Force_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Tag_TrendLike_Session:
                    return Plate_Tag_TrendLike_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_Quotes_Date_Session:
                    return Shares_Quotes_Date_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_Quotes_Today_Session:
                    return Shares_Quotes_Today_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Tag_Setting_Session:
                    return Plate_Tag_Setting_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_Tag_Leader_Session:
                    return Shares_Tag_Leader_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_Tag_DayLeader_Session:
                    return Shares_Tag_DayLeader_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_Tag_MainArmy_Session:
                    return Shares_Tag_MainArmy_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Shares_Rel_Tag_Setting_Session:
                    return Plate_Shares_Rel_Tag_Setting_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_Base_Session:
                    return Shares_Base_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_TradeStock_Session:
                    return Shares_TradeStock_Session.UpdateSession();
                default:
                    return null;
            }
        }

        public void UpdateSessionPart(int ExcuteType, object newData)
        {
            object newSession = new object();
            string dataKey = "";
            switch (ExcuteType)
            {
                case (int)Enum_Excute_Type.Plate_Quotes_Date_Session:
                    newSession=Plate_Quotes_Date_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Plate_Quotes_Date_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Plate_Quotes_Today_Session:
                    newSession = Plate_Quotes_Today_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Plate_Quotes_Today_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Plate_Tag_FocusOn_Session:
                    newSession = Plate_Tag_FocusOn_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Plate_Tag_FocusOn_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Plate_Tag_Force_Session:
                    newSession = Plate_Tag_Force_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Plate_Tag_Force_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Plate_Tag_TrendLike_Session:
                    newSession = Plate_Tag_TrendLike_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Plate_Tag_TrendLike_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Shares_Quotes_Date_Session:
                    newSession = Shares_Quotes_Date_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Shares_Quotes_Date_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Shares_Quotes_Today_Session:
                    newSession = Shares_Quotes_Today_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Shares_Quotes_Today_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Plate_Tag_Setting_Session:
                    newSession = Plate_Tag_Setting_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Plate_Tag_Setting_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Shares_Tag_Leader_Session:
                    newSession = Shares_Tag_Leader_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Shares_Tag_Leader_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Shares_Tag_DayLeader_Session:
                    newSession = Shares_Tag_DayLeader_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Shares_Tag_DayLeader_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Shares_Tag_MainArmy_Session:
                    newSession = Shares_Tag_MainArmy_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Shares_Tag_MainArmy_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Plate_Shares_Rel_Tag_Setting_Session:
                    newSession = Plate_Shares_Rel_Tag_Setting_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Plate_Shares_Rel_Tag_Setting_Session.ToString();
                    break;
                case (int)Enum_Excute_Type.Shares_Base_Session:
                    newSession = Shares_Base_Session.UpdateSessionPart(newData);
                    dataKey = Enum_Excute_DataKey.Shares_Base_Session.ToString();
                    break;
                default:
                    break;
            }
            SetSession(dataKey, newSession);
        }

        public Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> GetPlate_Tag_FocusOn_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Plate_Tag_FocusOn_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>>();
            }
            return session as Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>>;
        }

        public Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> GetPlate_Tag_Force_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Plate_Tag_Force_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            }
            return session as Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>;
        }

        public Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> GetPlate_Tag_TrendLike_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Plate_Tag_TrendLike_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>>();
            }
            return session as Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>>;
        }

        public Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> GetPlate_Quotes_Date_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Plate_Quotes_Date_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>();
            }
            return session as Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>;
        }

        public Dictionary<long, Plate_Quotes_Session_Info> GetPlate_Quotes_Today_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Plate_Quotes_Today_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Plate_Quotes_Session_Info>();
            }
            return session as Dictionary<long, Plate_Quotes_Session_Info>;
        }

        public Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> GetShares_Quotes_Date_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Shares_Quotes_Date_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
            }
            return session as Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>;
        }

        public Dictionary<long, Shares_Quotes_Session_Info> GetShares_Quotes_Today_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Shares_Quotes_Today_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Shares_Quotes_Session_Info>();
            }
            return session as Dictionary<long, Shares_Quotes_Session_Info>;
        }

        public Dictionary<long, Shares_TradeStock_Session_Info> GetShares_TradeStock_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Shares_TradeStock_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Shares_TradeStock_Session_Info>();
            }
            return session as Dictionary<long, Shares_TradeStock_Session_Info>;
        }

        public Dictionary<long, Shares_Quotes_Session_Info_Last> GetShares_Quotes_Last_Session()
        {
            Dictionary<long, Shares_Quotes_Session_Info_Last> result = new Dictionary<long, Shares_Quotes_Session_Info_Last>();
            var date_session = GetShares_Quotes_Date_Session();
            var today_session = GetShares_Quotes_Today_Session();
            foreach (var item in date_session)
            {
                if (!today_session.ContainsKey(item.Key))
                {
                    var last = item.Value.OrderByDescending(e => e.Key).FirstOrDefault().Value;
                    today_session.Add(item.Key, last);
                }
            }

            var stockDic=GetShares_TradeStock_Session();
            foreach (var item in today_session)
            {
                Shares_Quotes_Session_Info_Last temp = new Shares_Quotes_Session_Info_Last();
                temp.shares_quotes_info = item.Value;
                if (stockDic.ContainsKey(item.Key))
                {
                    int TradeStock_Interval_Count = stockDic[item.Key].TradeStock_Interval_Count;
                    long TradeStock_Interval = stockDic[item.Key].TradeStock_Interval;
                    //获取剩余分钟数
                    long remainMinute = Shares_TradeStock_Session.GetRemainMinute(stockDic[item.Key].GroupTimeKey);
                    if (240 - remainMinute > Singleton.Instance.SharesStockCal_MinMinute && TradeStock_Interval_Count>0)
                    {
                        temp.TotalCount_Today_Now = stockDic[item.Key].TradeStock/(240 - remainMinute);
                        temp.TotalCount_Yestoday_Now = stockDic[item.Key].TradeStock_Yestoday / 240;

                        temp.TotalCount_Yestoday_All = stockDic[item.Key].TradeStock_Yestoday;
                        temp.TotalCount_Today_Expect = (TradeStock_Interval / TradeStock_Interval_Count) * remainMinute + temp.TotalCount_Today_Now;


                    }
                }
                result.Add(item.Key, temp);
            }
            return result;
        }

        public Dictionary<long, bool> GetPlate_Tag_Setting_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Plate_Tag_Setting_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, bool>();
            }
            return session as Dictionary<long, bool>;
        }

        public Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>> GetShares_Tag_Leader_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Shares_Tag_Leader_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>>();
            }
            return session as Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>>;
        }

        public Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>> GetShares_Tag_DayLeader_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Shares_Tag_DayLeader_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>>();
            }
            return session as Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>>;
        }

        public Dictionary<long, Dictionary<long, Shares_Tag_MainArmy_Session_Info>> GetShares_Tag_MainArmy_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Shares_Tag_MainArmy_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Shares_Tag_MainArmy_Session_Info>>();
            }
            return session as Dictionary<long, Dictionary<long, Shares_Tag_MainArmy_Session_Info>>;
        }

        public Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> GetPlate_Shares_Rel_Tag_Setting_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Plate_Shares_Rel_Tag_Setting_Session.ToString());
            if (session == null)
            {
                return new Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>>();
            }
            return session as Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>>;
        }

        public Dictionary<long, Shares_Base_Session_Info> GetShares_Base_Session()
        {
            var session = GetSession(Enum_Excute_DataKey.Shares_Base_Session.ToString());
            if (session == null)
            {
                return new Dictionary<long, Shares_Base_Session_Info>();
            }
            return session as Dictionary<long, Shares_Base_Session_Info>;
        }
    }
}
