using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class SessionHandler : Session_New
    {
        public const int GET_DATA_CMD_ID_SHARESTAGCALHELPER_CALCULATE = 1;

        public const int GET_ALL_SHARES_STATISTIC_INFO = 2;//获取所有股票统计信息

        public const int GET_DATA_CMD_ID_PlATETAG_CALCULATE = 3;//股票标签计算

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
            Shares_TradeStock_Session,
            Plate_Shares_Rel_Session,
            Shares_Limit_Session,
            Plate_Base_Session,
            Setting_Plate_Index_Session,
            Setting_Plate_Linkage_Session,
            Setting_Plate_Shares_Linkage_Session,
            Plate_TradeStock_Session,
            Plate_Minute_KLine_Session,
            Shares_Minute_KLine_Session,
            Shares_Limit_Time_Session,
            Shares_Statistic_Session
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
            Shares_TradeStock_Session,
            Plate_Shares_Rel_Session,
            Shares_Limit_Session,
            Plate_Base_Session,
            Setting_Plate_Index_Session,
            Setting_Plate_Linkage_Session,
            Setting_Plate_Shares_Linkage_Session,
            Plate_TradeStock_Session,
            Plate_Minute_KLine_Session,
            Shares_Minute_KLine_Session,
            Shares_Limit_Time_Session,
            Shares_Statistic_Session
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
            //板块内股票缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Shares_Rel_Session.ToString(), 60*10, (int)Enum_Excute_Type.Plate_Shares_Rel_Session, null, 0, 0));
            //限制股票缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Limit_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Shares_Limit_Session, null, 0, 0));
            //板块缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Base_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Plate_Base_Session, null, 0, 0));
            //板块指数系数配置缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Setting_Plate_Index_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Setting_Plate_Index_Session, null, 0, 0));
            //板块联动配置缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Setting_Plate_Linkage_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Setting_Plate_Linkage_Session, null, 0, 0));
            //板块股票联动配置缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Setting_Plate_Shares_Linkage_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Setting_Plate_Shares_Linkage_Session, null, 0, 0));
            //板块成交量缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_TradeStock_Session.ToString(), 3, (int)Enum_Excute_Type.Plate_TradeStock_Session, null, 0, 0));
            //板块分钟K线缓存缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Minute_KLine_Session.ToString(), 15, (int)Enum_Excute_Type.Plate_Minute_KLine_Session, null, 0, 0));
            //股票分钟K线缓存缓存
            //result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Minute_KLine_Session.ToString(), 15, (int)Enum_Excute_Type.Shares_Minute_KLine_Session, null, 0, 0));
            //股票交易时间缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Limit_Time_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Shares_Limit_Time_Session, null, 0, 0));
            //股票统计信息
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Statistic_Session.ToString(), 3, (int)Enum_Excute_Type.Shares_Statistic_Session, null, 0, 1));
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

        public override object UpdateSession(int ExcuteType, object oct = null)
        {
            switch (ExcuteType) 
            {
                case (int)Enum_Excute_Type.Plate_Quotes_Date_Session:
                    return Plate_Quotes_Date_Session.UpdateSession(oct);
                case (int)Enum_Excute_Type.Plate_Quotes_Today_Session:
                    return Plate_Quotes_Today_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Tag_FocusOn_Session:
                    return Plate_Tag_FocusOn_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Tag_Force_Session:
                    return Plate_Tag_Force_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Tag_TrendLike_Session:
                    return Plate_Tag_TrendLike_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_Quotes_Date_Session:
                    return Shares_Quotes_Date_Session.UpdateSession(oct);
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
                case (int)Enum_Excute_Type.Plate_Shares_Rel_Session:
                    return Plate_Shares_Rel_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_Limit_Session:
                    return Shares_Limit_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Base_Session:
                    return Plate_Base_Session.UpdateSession();
                case (int)Enum_Excute_Type.Setting_Plate_Index_Session:
                    return Setting_Plate_Index_Session.UpdateSession();
                case (int)Enum_Excute_Type.Setting_Plate_Linkage_Session:
                    return Setting_Plate_Linkage_Session.UpdateSession();
                case (int)Enum_Excute_Type.Setting_Plate_Shares_Linkage_Session:
                    return Setting_Plate_Shares_Linkage_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_TradeStock_Session:
                    return Plate_TradeStock_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Minute_KLine_Session:
                    return Plate_Minute_KLine_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_Minute_KLine_Session:
                    return Shares_Minute_KLine_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_Limit_Time_Session:
                    return Shares_Limit_Time_Session.UpdateSession();
                case (int)Enum_Excute_Type.Shares_Statistic_Session:
                    return Shares_Statistic_Session.UpdateSession();
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
                default:
                    break;
            }
            SetSessionWithNolock(dataKey, newSession);
        }

        public override object CopySessionData(object objData, string dataKey)
        {
            switch ((Enum_Excute_DataKey)System.Enum.Parse(typeof(Enum_Excute_DataKey), dataKey))
            {
                case Enum_Excute_DataKey.Plate_Tag_FocusOn_Session:
                    return Plate_Tag_FocusOn_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Plate_Tag_Force_Session:
                    return Plate_Tag_Force_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Plate_Tag_TrendLike_Session:
                    return Plate_Tag_TrendLike_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Plate_Quotes_Date_Session:
                    return Plate_Quotes_Date_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Plate_Quotes_Today_Session:
                    return Plate_Quotes_Today_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Shares_Quotes_Date_Session:
                    return Shares_Quotes_Date_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Shares_Quotes_Today_Session:
                    return Shares_Quotes_Today_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Shares_TradeStock_Session:
                    return Shares_TradeStock_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Plate_Tag_Setting_Session:
                    return Plate_Tag_Setting_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Shares_Tag_Leader_Session:
                    return Shares_Tag_Leader_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Shares_Tag_DayLeader_Session:
                    return Shares_Tag_DayLeader_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Shares_Tag_MainArmy_Session:
                    return Shares_Tag_MainArmy_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Plate_Shares_Rel_Tag_Setting_Session:
                    return Plate_Shares_Rel_Tag_Setting_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Shares_Base_Session:
                    return Shares_Base_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Plate_Shares_Rel_Session:
                    return Plate_Shares_Rel_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Shares_Limit_Session:
                    return Shares_Limit_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Plate_Base_Session:
                    return Plate_Base_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Setting_Plate_Index_Session:
                    return Setting_Plate_Index_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Setting_Plate_Linkage_Session:
                    return Setting_Plate_Linkage_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Setting_Plate_Shares_Linkage_Session:
                    return Setting_Plate_Shares_Linkage_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Plate_TradeStock_Session:
                    return Plate_TradeStock_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Plate_Minute_KLine_Session:
                    return Plate_Minute_KLine_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Shares_Minute_KLine_Session:
                    return Shares_Minute_KLine_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Shares_Limit_Time_Session:
                    return Shares_Limit_Time_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Shares_Statistic_Session:
                    return Shares_Statistic_Session.CopySessionData(objData);
                default:
                    return base.CopySessionData(objData, dataKey);
            }
        }

        public override object OnGetData(string DataKey, GET_DATA_CXT _cxt)
        {
            if (_cxt == null)
            {
                return base.OnGetData(DataKey, _cxt);
            }
            switch(_cxt.CmdId)
            {
                case GET_DATA_CMD_ID_SHARESTAGCALHELPER_CALCULATE:
                    SharesTagCalHelper._Calculate();
                    return null;
                case GET_ALL_SHARES_STATISTIC_INFO:
                    return Shares_Statistic_Session.Cal_Shares_Statistic();
                case GET_DATA_CMD_ID_PlATETAG_CALCULATE:
                    PlateTagCalHelper._Calculate();
                    return null; 
                default:
                    return base.OnGetData(DataKey, _cxt);

            }
        }


        //股票交易时间缓存
        public List<t_shares_limit_time> GetShares_Limit_Time_Session(bool withlock=true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Limit_Time_Session.ToString();
            var sessionData = withlock? GetDataWithLock(dataKey) :GetDataWithNoLock(dataKey);
            if (sessionData == null)
            {
                return new List<t_shares_limit_time>();
            }
            return sessionData as List<t_shares_limit_time>;
        }

        //股票ST限制缓存
        public List<long> GetShares_Limit_Session(bool withlock = true)
        {
            List<long> result = new List<long>();

            string dataType = Enum_Excute_DataKey.Shares_Limit_Session.ToString();
            var limitSession = (withlock ? GetDataWithLock(dataType) : GetDataWithNoLock(dataType)) as List<Shares_Limit_Session_Info>;
            if (limitSession == null)
            {
                limitSession = new List<Shares_Limit_Session_Info>();
            }
            var baseSession = GetShares_Base_Session(withlock);
            foreach (var item in baseSession)
            {
                var temp = (from x in limitSession
                            where ((x.LimitType == 1 && item.Value.SharesCode.StartsWith(x.LimitKey)) || (x.LimitType == 2 && item.Value.SharesName.StartsWith(x.LimitKey))) && (x.LimitMarket == -1 || x.LimitMarket == item.Value.Market)
                            select x).FirstOrDefault();
                if (temp != null)
                {
                    result.Add(item.Key);
                }
            }
            return result;
        }

        //股票基础数据缓存
        public Dictionary<long, Shares_Base_Session_Info> GetShares_Base_Session(bool withlock=true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Base_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Shares_Base_Session_Info>();
            }
            return session as Dictionary<long, Shares_Base_Session_Info>;
        }

        //板块指数系数设置缓存
        public Dictionary<int, Setting_Plate_Index_Session_Info> GetSetting_Plate_Index_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Setting_Plate_Index_Session.ToString();
            var sessionData = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (sessionData == null)
            {
                return new Dictionary<int, Setting_Plate_Index_Session_Info>();
            }
            return sessionData as Dictionary<int, Setting_Plate_Index_Session_Info>;
        }

        //联动板块设置缓存
        public Dictionary<long, Setting_Plate_Linkage_Session_Info_Group> GetSetting_Plate_Linkage_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Setting_Plate_Linkage_Session.ToString();
            var sessionData = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (sessionData == null)
            {
                return new Dictionary<long, Setting_Plate_Linkage_Session_Info_Group>();
            }
            return sessionData as Dictionary<long, Setting_Plate_Linkage_Session_Info_Group>;
        }

        //联动股票设置缓存
        public Dictionary<long, Setting_Plate_Shares_Linkage_Session_Info_Group> GetSetting_Plate_Shares_Linkage_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Setting_Plate_Shares_Linkage_Session.ToString();
            var sessionData = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (sessionData == null)
            {
                return new Dictionary<long, Setting_Plate_Shares_Linkage_Session_Info_Group>();
            }
            return sessionData as Dictionary<long, Setting_Plate_Shares_Linkage_Session_Info_Group>;
        }

        //板块标签设置缓存
        public Dictionary<long, bool> GetPlate_Tag_Setting_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Tag_Setting_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, bool>();
            }
            return session as Dictionary<long, bool>;
        }

        //股票标签类型缓存
        public Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> GetPlate_Shares_Rel_Tag_Setting_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Shares_Rel_Tag_Setting_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>>();
            }
            return session as Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>>;
        }

        //板块基础数据缓存
        public Dictionary<long, Plate_Base_Session_Info> GetPlate_Base_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Base_Session.ToString();
            var sessionData = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (sessionData == null)
            {
                return new Dictionary<long, Plate_Base_Session_Info>();
            }
            return sessionData as Dictionary<long, Plate_Base_Session_Info>;
        }


        //关注缓存（根据股票获取）
        public Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> GetPlate_Tag_FocusOn_Session_ByShares(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Tag_FocusOn_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>>();
            }
            return (session as Plate_Tag_FocusOn_Session_Obj).Shares_Plate_FocusOn_Session;
        }

        //关注缓存（根据板块获取）
        public Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> GetPlate_Tag_FocusOn_Session_ByPlate(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Tag_FocusOn_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>>();
            }
            return (session as Plate_Tag_FocusOn_Session_Obj).Plate_Shares_FocusOn_Session;
        }
        
        //涨缓存（根据股票获取）
        public Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> GetPlate_Tag_Force_Session_ByShares(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Tag_Force_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            }
            return (session as Plate_Tag_Force_Session_Obj).Shares_Plate_Force_Session;
        }

        //涨缓存（根据板块获取）
        public Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> GetPlate_Tag_Force_Session_ByPlate(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Tag_Force_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            }
            return (session as Plate_Tag_Force_Session_Obj).Plate_Shares_Force_Session;
        }

        //走势缓存（根据股票获取）
        public Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> GetPlate_Tag_TrendLike_Session_ByShares(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Tag_TrendLike_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>>();
            }
            return (session as Plate_Tag_TrendLike_Session_Obj).Shares_Plate_TrendLike_Session;
        }

        //走势缓存（根据板块获取）
        public Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> GetPlate_Tag_TrendLike_Session_ByPlate(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Tag_TrendLike_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>>();
            }
            return (session as Plate_Tag_TrendLike_Session_Obj).Plate_Shares_TrendLike_Session;
        }


        //龙头缓存(板块出发)
        public Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>> GetShares_Tag_Leader_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Tag_Leader_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>>();
            }
            return (session as Shares_Tag_Leader_Session_Obj).Shares_Tag_Leader_Session_ByPlate;
        }

        //龙头缓存(股票出发)
        public Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>> GetShares_Tag_Leader_Session_ByShares(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Tag_Leader_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>>();
            }
            return (session as Shares_Tag_Leader_Session_Obj).Shares_Tag_Leader_Session_ByShares;
        }

        //日内龙缓存(板块出发)
        public Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>> GetShares_Tag_DayLeader_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Tag_DayLeader_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>>();
            }
            return (session as Shares_Tag_DayLeader_Session_Obj).Shares_Tag_DayLeader_Session_ByPlate;
        }

        //日内龙缓存(股票出发)
        public Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>> GetShares_Tag_DayLeader_Session_ByShares(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Tag_DayLeader_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>>();
            }
            return (session as Shares_Tag_DayLeader_Session_Obj).Shares_Tag_DayLeader_Session_ByShares;
        }

        //中军缓存(板块出发)
        public Dictionary<long, Dictionary<long, Shares_Tag_MainArmy_Session_Info>> GetShares_Tag_MainArmy_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Tag_MainArmy_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Shares_Tag_MainArmy_Session_Info>>();
            }
            return (session as Shares_Tag_MainArmy_Session_Obj).Shares_Tag_MainArmy_Session_BuyPlate;
        }

        //中军缓存(股票出发)
        public Dictionary<long, Dictionary<long, Shares_Tag_MainArmy_Session_Info>> GetShares_Tag_MainArmy_Session_ByShares(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Tag_MainArmy_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Shares_Tag_MainArmy_Session_Info>>();
            }
            return (session as Shares_Tag_MainArmy_Session_Obj).Shares_Tag_MainArmy_Session_BuyShares;
        }

        //计算龙头股票(从板块出发)
        public Dictionary<long, List<long>> GetLeaderShares_ByPlate(bool withlock = true)
        {
            Dictionary<long, List<long>> result = new Dictionary<long, List<long>>();
            Dictionary<long, List<long>> leader = new Dictionary<long, List<long>>();
            Dictionary<long, List<long>> dayleader = new Dictionary<long, List<long>>();
            Dictionary<long, List<long>> mainarmy = new Dictionary<long, List<long>>();
            if (Singleton.Instance.SharesLeaderType.Contains(1))
            {
                leader = GetShares_Tag_Leader_Session(withlock).ToDictionary(k => k.Key, v => v.Value.Keys.ToList());
            }
            if (Singleton.Instance.SharesLeaderType.Contains(2))
            {
                dayleader = GetShares_Tag_DayLeader_Session(withlock).ToDictionary(k => k.Key, v => v.Value.Keys.ToList());
            }
            if (Singleton.Instance.SharesLeaderType.Contains(3))
            {
                mainarmy = GetShares_Tag_MainArmy_Session(withlock).ToDictionary(k => k.Key, v => v.Value.Keys.ToList());
            }
            var plateBase = GetPlate_Base_Session(withlock);
            var daysType = Singleton.Instance.SharesLeaderDaysType;
            foreach (var item in plateBase)
            {
                if (item.Value.BaseStatus != 1)
                {
                    continue;
                }
                List<long> list = new List<long>();
                for (int i = 0; i < daysType.Length; i++)
                {
                    long key = item.Key * 100 + daysType[i];
                    if (leader.ContainsKey(key))
                    {
                        list.AddRange(leader[key]);
                    }
                    if (dayleader.ContainsKey(key))
                    {
                        list.AddRange(dayleader[key]);
                    }
                    if (mainarmy.ContainsKey(key))
                    {
                        list.AddRange(mainarmy[key]);
                    }
                }
                list = list.Distinct().ToList();
                result.Add(item.Key, list);
            }
            return result;
        }


        //板块历史行情缓存
        object GetPlate_Quotes_Date_Session_Lock = new object();
        Plate_Quotes_Session_Info_Obj tempPlate_Quotes_Date_Session = null;

        public void WriteToPlateQuoteCache()
        {
            lock (GetPlate_Quotes_Date_Session_Lock)
            {
                if (tempPlate_Quotes_Date_Session == null)
                {
                    return;
                }
                string dataKey = Enum_Excute_DataKey.Plate_Quotes_Date_Session.ToString();
                SetSessionWithLock(dataKey, tempPlate_Quotes_Date_Session);
                tempPlate_Quotes_Date_Session = null;
            }
        }
        public Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> GetPlate_Quotes_Date_Session(int days = 0,bool withlock = true)
        {
            return GetPlate_Quotes_Date(days, withlock).SessionDic;
        }
        private Plate_Quotes_Session_Info_Obj _getPlate_Quotes_Date_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Quotes_Date_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Plate_Quotes_Session_Info_Obj
                {
                    Days = 0,
                    SessionDic = new Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>>(),
                };
            }
            return session as Plate_Quotes_Session_Info_Obj;

        }

        private Plate_Quotes_Session_Info_Obj GetPlate_Quotes_Date(int days, bool withlock)
        {
            lock (GetPlate_Quotes_Date_Session_Lock)
            {
                var temp = _getPlate_Quotes_Date_Session(withlock);
                if (temp.Days < days)
                {
                    if (tempPlate_Quotes_Date_Session == null || tempPlate_Quotes_Date_Session.Days < days)
                    {
                        tempPlate_Quotes_Date_Session = Plate_Quotes_Date_Session.UpdateSession(days);
                    }
                    temp = new Plate_Quotes_Session_Info_Obj();
                    temp.Days = tempPlate_Quotes_Date_Session.Days;
                    temp.SessionDic = tempPlate_Quotes_Date_Session.SessionDic;
                }
                return temp;
            }
        }

        //板块今日行情缓存
        public Dictionary<long, Plate_Quotes_Session_Info> GetPlate_Quotes_Today_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Quotes_Today_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Plate_Quotes_Session_Info>();
            }
            return session as Dictionary<long, Plate_Quotes_Session_Info>;
        }

        //板块最后行情缓存
        public Dictionary<long, Plate_Quotes_Session_Info> GetPlate_Quotes_Last_Session(bool isGetVolumeRate = true, bool withlock = true)
        {
            Dictionary<long, Plate_Quotes_Session_Info> result = new Dictionary<long, Plate_Quotes_Session_Info>();
            var date_session = GetPlate_Quotes_Date_Session(0, withlock);
            var today_session = GetPlate_Quotes_Today_Session(withlock);
            var plate_base_session = GetPlate_Base_Session(withlock);
            foreach (var item in date_session)
            {
                if (!today_session.ContainsKey(item.Key))
                {
                    var last = item.Value.FirstOrDefault().Value;
                    today_session.Add(item.Key, last);
                }
            }

            today_session = today_session.OrderByDescending(e => e.Value.RiseRate).ThenBy(e => e.Value.PlateId).ToDictionary(k => k.Key, v => v.Value);
            int idx = 0;
            foreach (var item in today_session)
            {
                if (!plate_base_session.ContainsKey(item.Key))
                {
                    continue;
                }
                if (plate_base_session[item.Key].BaseStatus != 1)
                {
                    continue;
                }
                idx++;
                item.Value.Rank = idx;
            }

            int tempSharesStockCal_Type = Singleton.Instance.SharesStockCal_Type;
            var stockDic = isGetVolumeRate ? GetPlate_TradeStock_Session(withlock) : new Dictionary<long, Plate_TradeStock_Session_Info>();
            foreach (var item in today_session)
            {
                Plate_Quotes_Session_Info temp = new Plate_Quotes_Session_Info();
                temp.ClosedPrice = item.Value.ClosedPrice;
                temp.Date = item.Value.Date;
                temp.PlateId = item.Value.PlateId;
                temp.Rank = item.Value.Rank;
                temp.RealDays = item.Value.RealDays;
                temp.YestodayClosedPrice = item.Value.YestodayClosedPrice;
                if (isGetVolumeRate && stockDic.ContainsKey(item.Key))
                {
                    int TradeStock_Interval_Count = stockDic[item.Key].TradeStock_Interval_Count;
                    long TradeStock_Interval = stockDic[item.Key].TradeStock_Interval;
                    //获取剩余分钟数
                    long remainMinute = Shares_TradeStock_Session.GetRemainMinute(stockDic[item.Key].GroupTimeKey);
                    if (240 - remainMinute >= Singleton.Instance.SharesStockCal_MinMinute && TradeStock_Interval_Count > 0)
                    {
                        if (tempSharesStockCal_Type == 2)
                        {
                            temp.TotalCount_Today_Now = stockDic[item.Key].TradeStock;
                            temp.TotalCount_Yestoday_Now = stockDic[item.Key].TradeStock_Now;

                            temp.TotalCount_Yestoday_All = stockDic[item.Key].TradeStock_Yestoday;
                            temp.TotalCount_Today_Expect = (TradeStock_Interval / TradeStock_Interval_Count) * remainMinute + stockDic[item.Key].TradeStock;
                        }
                        else
                        {
                            temp.TotalCount_Today_Now = stockDic[item.Key].TradeStock / (240 - remainMinute);
                            temp.TotalCount_Yestoday_Now = stockDic[item.Key].TradeStock_Yestoday / 240;

                            temp.TotalCount_Yestoday_All = stockDic[item.Key].TradeStock_Yestoday;
                            temp.TotalCount_Today_Expect = (TradeStock_Interval / TradeStock_Interval_Count) * remainMinute + stockDic[item.Key].TradeStock;
                        }
                    }
                }
                result.Add(item.Key, temp);
            }

            return result;
        }

        //板块成交量缓存
        public Dictionary<long, Plate_TradeStock_Session_Info> GetPlate_TradeStock_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_TradeStock_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Plate_TradeStock_Session_Info>();
            }
            return session as Dictionary<long, Plate_TradeStock_Session_Info>;
        }

        //板块分钟K线缓存
        public Dictionary<long, List<Plate_Minute_KLine_Session_Info>> GetPlate_Minute_KLine_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Minute_KLine_Session.ToString();
            var sessionData = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (sessionData == null)
            {
                return new Dictionary<long, List<Plate_Minute_KLine_Session_Info>>();
            }
            return sessionData as Dictionary<long, List<Plate_Minute_KLine_Session_Info>>;
        }


        //股票历史行情缓存
        object GetShares_Quotes_Date_Session_Lock = new object();

        private Shares_Quotes_Session_Info_Obj _getShares_Quotes_Date_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Quotes_Date_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Shares_Quotes_Session_Info_Obj
                {
                    Days = 0,
                    SessionDic = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>(),
                    MinSessionDic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>(),
                    MaxSessionDic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>(),
                    OpenSessionDic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>(),
                    CloseSessionDic = new Dictionary<long, SortedSet<Shares_Quotes_Session_Info>>()
                };
            }
            return session as Shares_Quotes_Session_Info_Obj;
        }

        Shares_Quotes_Session_Info_Obj tempShares_Quotes_Date_Session = null;

        public void WriteToSharesQuoteCache()
        {
            lock (GetShares_Quotes_Date_Session_Lock)
            {
                if (tempShares_Quotes_Date_Session == null)
                {
                    return;
                }
                string dataKey = Enum_Excute_DataKey.Shares_Quotes_Date_Session.ToString();
                SetSessionWithLock(dataKey, tempShares_Quotes_Date_Session);
                tempShares_Quotes_Date_Session = null;
            }
        }

        public Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> GetShares_Quotes_Date_Session(int days = 0, bool withlock = true)
        {
            return GetShares_Quotes_Date(days, withlock).SessionDic;
        }

        public Shares_Quotes_Session_Info_Obj GetShares_Quotes_Date_Sort_Session(int days = 0, bool withlock = true)
        {
            return GetShares_Quotes_Date(days, withlock);
        }

        private Shares_Quotes_Session_Info_Obj GetShares_Quotes_Date(int days,bool withlock) 
        {
            lock (GetShares_Quotes_Date_Session_Lock)
            {
                var temp = _getShares_Quotes_Date_Session(withlock);
                if (temp.Days < days)
                {
                    if (tempShares_Quotes_Date_Session == null || tempShares_Quotes_Date_Session.Days < days)
                    {
                        tempShares_Quotes_Date_Session = Shares_Quotes_Date_Session.UpdateSession(days);
                    }
                    temp = new Shares_Quotes_Session_Info_Obj();
                    temp.Days = tempShares_Quotes_Date_Session.Days;
                    temp.CloseSessionDic = tempShares_Quotes_Date_Session.CloseSessionDic;
                    temp.MaxSessionDic = tempShares_Quotes_Date_Session.MaxSessionDic;
                    temp.MinSessionDic = tempShares_Quotes_Date_Session.MinSessionDic;
                    temp.OpenSessionDic = tempShares_Quotes_Date_Session.OpenSessionDic;
                    temp.SessionDic = tempShares_Quotes_Date_Session.SessionDic;
                }
                return temp;
            }
        }

        //股票今日行情缓存
        public Dictionary<long, Shares_Quotes_Session_Info> GetShares_Quotes_Today_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Quotes_Today_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Shares_Quotes_Session_Info>();
            }
            return session as Dictionary<long, Shares_Quotes_Session_Info>;
        }

        //股票成交量缓存
        public Dictionary<long, Shares_TradeStock_Session_Info> GetShares_TradeStock_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_TradeStock_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Shares_TradeStock_Session_Info>();
            }
            return session as Dictionary<long, Shares_TradeStock_Session_Info>;
        }

        //股票最后行情缓存
        public Dictionary<long, Shares_Quotes_Session_Info_Last> GetShares_Quotes_Last_Session(bool isGetVolumeRate = true, bool withlock = true)
        {
            Dictionary<long, Shares_Quotes_Session_Info_Last> result = new Dictionary<long, Shares_Quotes_Session_Info_Last>();
            var date_session = GetShares_Quotes_Date_Session(0,withlock);
            var today_session = GetShares_Quotes_Today_Session(withlock);

            foreach (var item in date_session)
            {
                if (!today_session.ContainsKey(item.Key))
                {
                    var last = item.Value.FirstOrDefault().Value;
                    today_session.Add(item.Key, last);
                }
            }

            if (!isGetVolumeRate)
            {
                foreach (var item in today_session)
                {
                    result.Add(item.Key, new Shares_Quotes_Session_Info_Last
                    {
                        shares_quotes_info = item.Value
                    });
                }
                return result;
            }

            int tempSharesStockCal_Type = Singleton.Instance.SharesStockCal_Type;
            var stockDic = GetShares_TradeStock_Session(withlock);
            foreach (var item in today_session)
            {
                Shares_Quotes_Session_Info_Last temp = new Shares_Quotes_Session_Info_Last();
                temp.shares_quotes_info = item.Value;
                if (stockDic.ContainsKey(item.Key))
                {
                    Shares_TradeStock_Session_Info _tradeStock = stockDic[item.Key];
                    int TradeStock_Interval_Count = _tradeStock.TradeStock_Interval_Count;
                    long TradeStock_Interval = _tradeStock.TradeStock_Interval;
                    //获取剩余分钟数
                    long remainMinute = Shares_TradeStock_Session.GetRemainMinute(_tradeStock.GroupTimeKey);
                    if (240 - remainMinute >= Singleton.Instance.SharesStockCal_MinMinute && TradeStock_Interval_Count > 0)
                    {
                        if (tempSharesStockCal_Type == 2)
                        {
                            temp.TotalCount_Today_Now = _tradeStock.TradeStock;
                            temp.TotalCount_Yestoday_Now = _tradeStock.TradeStock_Now;

                            temp.TotalCount_Yestoday_All = _tradeStock.TradeStock_Yestoday;
                            temp.TotalCount_Today_Expect = (TradeStock_Interval / TradeStock_Interval_Count) * remainMinute + _tradeStock.TradeStock;
                        }
                        else
                        {
                            temp.TotalCount_Today_Now = _tradeStock.TradeStock / (240 - remainMinute);
                            temp.TotalCount_Yestoday_Now = _tradeStock.TradeStock_Yestoday / 240;

                            temp.TotalCount_Yestoday_All = _tradeStock.TradeStock_Yestoday;
                            temp.TotalCount_Today_Expect = (TradeStock_Interval / TradeStock_Interval_Count) * remainMinute + _tradeStock.TradeStock;
                        }
                    }
                }

                result.Add(item.Key, temp);
            }
            return result;
        }

        //股票分钟K线缓存
        public Dictionary<long, List<Shares_Minute_KLine_Session_Info>> GetShares_Minute_KLine_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Minute_KLine_Session.ToString();
            var sessionData = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (sessionData == null)
            {
                return new Dictionary<long, List<Shares_Minute_KLine_Session_Info>>();
            }
            return sessionData as Dictionary<long, List<Shares_Minute_KLine_Session_Info>>;
        }


        //板块股票关系缓存（根据板块获取）
        public Dictionary<long, List<Plate_Shares_Rel_Session_Info>> GetPlate_Shares_Rel_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Shares_Rel_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, List<Plate_Shares_Rel_Session_Info>>();
            }
            return (session as Plate_Shares_Rel_Session_Obj).Plate_Shares_Rel_Session;
        }
       
        //板块股票关系缓存（根据股票获取）
        public Dictionary<long, List<Plate_Shares_Rel_Session_Info>> GetShares_Plate_Rel_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Shares_Rel_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, List<Plate_Shares_Rel_Session_Info>>();
            }
            return (session as Plate_Shares_Rel_Session_Obj).Shares_Plate_Rel_Session;
        }

        //获取股票在走的真实板块
        public Dictionary<long, List<long>> GetShares_Real_Plate_Session(int dayType,bool withlock = true)
        {
            Dictionary<long, List<long>> result = new Dictionary<long, List<long>>();
            var shares_plate_rel = GetShares_Plate_Rel_Session(withlock);
            var plate_base = GetPlate_Base_Session(withlock);
            var FocusOn_Session = GetPlate_Tag_FocusOn_Session_ByShares(withlock);
            var Force_Session = GetPlate_Tag_Force_Session_ByShares(withlock);
            var TrendLike_Session = GetPlate_Tag_TrendLike_Session_ByShares(withlock);
            foreach (var item in shares_plate_rel)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, new List<long>());
                }
                long forceKey = item.Key * 100 + dayType;
                foreach (var plate in item.Value)
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
                    if (baseInfo.PlateType == 1)
                    {
                        result[item.Key].Add(plate.PlateId);
                        continue;
                    }
                    if (baseInfo.PlateType == 3)
                    {
                        if (FocusOn_Session.ContainsKey(item.Key) && FocusOn_Session[item.Key].ContainsKey(plate.PlateId))
                        {
                            result[item.Key].Add(plate.PlateId);
                            continue;
                        }
                        if (TrendLike_Session.ContainsKey(item.Key) && TrendLike_Session[item.Key].ContainsKey(plate.PlateId))
                        {
                            result[item.Key].Add(plate.PlateId);
                            continue;
                        }
                        if (Force_Session.ContainsKey(forceKey) && Force_Session[forceKey].ContainsKey(plate.PlateId))
                        {
                            result[item.Key].Add(plate.PlateId);
                            continue;
                        }
                    }
                }
            }
            return result;
        }

        //获取板块在走的真实股票
        public Dictionary<long, List<long>> GetPlate_Real_Shares_Session(int dayType, bool withlock = true)
        {
            Dictionary<long, List<long>> result = new Dictionary<long, List<long>>();
            var plate_shares_rel = GetPlate_Shares_Rel_Session(withlock);
            var plate_base = GetPlate_Base_Session(withlock);
            var FocusOn_Session = GetPlate_Tag_FocusOn_Session_ByPlate(withlock);
            var Force_Session = GetPlate_Tag_Force_Session_ByPlate(withlock);
            var TrendLike_Session = GetPlate_Tag_TrendLike_Session_ByPlate(withlock);
            foreach (var item in plate_shares_rel)
            {
                if (!plate_base.ContainsKey(item.Key))
                {
                    continue;
                }
                var baseInfo = plate_base[item.Key];
                if (baseInfo.BaseStatus != 1)
                {
                    continue;
                }
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, new List<long>());
                }
                List<long> tempList = new List<long>();
                if (baseInfo.PlateType == 1)
                {
                    tempList.AddRange(item.Value.Select(e => e.SharesKey).ToList());
                }
                if (baseInfo.PlateType == 3)
                {
                    if (FocusOn_Session.ContainsKey(item.Key))
                    {
                        tempList.AddRange(FocusOn_Session[item.Key].Keys);
                    }
                    if (Force_Session.ContainsKey(item.Key))
                    {
                        tempList.AddRange(Force_Session[item.Key].Keys.Select(e => e / 100).ToList());
                    }
                    if (TrendLike_Session.ContainsKey(item.Key))
                    {
                        tempList.AddRange(TrendLike_Session[item.Key].Keys);
                    }
                }
                tempList = tempList.Distinct().ToList();
                result[item.Key] = tempList;
            }
            return result;
        }

        //股票统计信息缓存
        public Shares_Statistic_Session_Obj GetShares_Statistic_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Statistic_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Shares_Statistic_Session_Obj();
            }
            return session as Shares_Statistic_Session_Obj;
        }
    }
}
