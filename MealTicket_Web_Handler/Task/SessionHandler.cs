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

        public const int GET_ALL_PLATE_STATISTIC_INFO = 4;//板块统计信息

        public const int GET_SHARES_ENERGY_TABLE_SESSION_INFO = 5;//板块龙头股票列表

        public const int GET_SHARES_RISERATE_RANKLIST = 6;//股票排名列表

        public const int GET_PLATE_QUOTES_INFO = 7;//获取单个板块行情数据

        public const int GET_PLATE_LEADERMODEL_SHARESLIST = 8;//获取某个板块龙头股票

        public const int GET_SHARES_ENERGYINDEX_LIST = 9;//获取某个板块龙头股票

        public const int GET_SHARES_HOTSPOT_LIST = 10;//获取股票热点题材

        public const int GET_SHARES_ALLLIMITUP_LIST = 11;//获取股票热点题材

        public const int GET_PLATE_ENERGY_INDEX_LIST = 12;//获取板块动能指数

        public const int CAL_SHARESMONITORTTR = 13;

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
            Shares_Statistic_Session,
            Shares_PlateTag_IsAuto_Session,
            Plate_Statistic_Session,
            Shares_Energy_Table_Session,
            Shares_Limit_Fundmultiple_Session,
            Shares_RiseRate_His_Session,
            Shares_RiseLimit_Session,
            Search_Mark_Tri_Session,
            Shares_Quotes_Tri_Session,
            Shares_Hotspot_Session
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
            Shares_Statistic_Session,
            Shares_PlateTag_IsAuto_Session,
            Plate_Statistic_Session,
            Shares_Energy_Table_Session,
            Shares_Limit_Fundmultiple_Session, 
            Shares_RiseRate_His_Session,
            Shares_RiseLimit_Session,
            Search_Mark_Tri_Session,
            Shares_Quotes_Tri_Session,
            Shares_Hotspot_Session
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
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Quotes_Date_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Plate_Quotes_Date_Session, DateTime.Now.Date.AddSeconds(15), 0, 0));
            //板块重点关注结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Tag_FocusOn_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Plate_Tag_FocusOn_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //板块强势上涨结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Tag_Force_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Plate_Tag_Force_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //板块走势最像结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Tag_TrendLike_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Plate_Tag_TrendLike_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //股票历史行情缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Quotes_Date_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_Quotes_Date_Session, DateTime.Now.Date.AddSeconds(15), 0, 0));
            //板块龙头计算结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Tag_Leader_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_Tag_Leader_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //板块日内龙头计算结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Tag_DayLeader_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_Tag_DayLeader_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //板块中军计算结果缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Tag_MainArmy_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_Tag_MainArmy_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //股票基础数据缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Base_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_Base_Session, DateTime.Now.Date.AddHours(9).AddMinutes(15), 0, 0));
            //涨停板缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_RiseLimit_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_RiseLimit_Session, DateTime.Now.Date.AddHours(15).AddMinutes(5).AddSeconds(15), 0, 0));
            //股票综合涨幅排名缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_RiseRate_His_Session.ToString(), 60 * 60 * 24, (int)Enum_Excute_Type.Shares_RiseRate_His_Session, DateTime.Now.Date.AddHours(1), 0, 0));
            //股票限制涨跌停缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Limit_Fundmultiple_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Shares_Limit_Fundmultiple_Session, null, 0, 0));
            //板块内股票缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Shares_Rel_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Plate_Shares_Rel_Session, null, 0, 0));
            //股票标签设置缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Shares_Rel_Tag_Setting_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Plate_Shares_Rel_Tag_Setting_Session, null, 0, 0));
            //板块缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Base_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Plate_Base_Session, null, 0, 0));
            //限制股票缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Limit_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Shares_Limit_Session, null, 0, 0));
            //板块指数系数配置缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Setting_Plate_Index_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Setting_Plate_Index_Session, null, 0, 0));
            //板块联动配置缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Setting_Plate_Linkage_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Setting_Plate_Linkage_Session, null, 0, 0));
            //板块股票联动配置缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Setting_Plate_Shares_Linkage_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Setting_Plate_Shares_Linkage_Session, null, 0, 0));
            //股票板块标签计算是否可修改缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_PlateTag_IsAuto_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Shares_PlateTag_IsAuto_Session, null, 0, 1));
            //股票交易时间缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Limit_Time_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Shares_Limit_Time_Session, null, 0, 0));
            //板块标签设置缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Tag_Setting_Session.ToString(), 60, (int)Enum_Excute_Type.Plate_Tag_Setting_Session, null, 0, 0));
            //股票昨日成交量缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_TradeStock_Session.ToString(), 15, (int)Enum_Excute_Type.Shares_TradeStock_Session, null, 0, 0));
            //板块分钟K线缓存缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Minute_KLine_Session.ToString(), 15, (int)Enum_Excute_Type.Plate_Minute_KLine_Session, null, 0, 0));
            //板块今日行情缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Quotes_Today_Session.ToString(), 3, (int)Enum_Excute_Type.Plate_Quotes_Today_Session, null, 0, 0));
            //股票今日行情缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Quotes_Today_Session.ToString(), 3, (int)Enum_Excute_Type.Shares_Quotes_Today_Session, null, 0, 0));
            //板块成交量缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_TradeStock_Session.ToString(), 3, (int)Enum_Excute_Type.Plate_TradeStock_Session, null, 0, 0));
            //股票统计信息
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Statistic_Session.ToString(), 3, (int)Enum_Excute_Type.Shares_Statistic_Session, null, 0, 1));
            //板块统计信息
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Statistic_Session.ToString(), 3, (int)Enum_Excute_Type.Plate_Statistic_Session, null, 0, 1));
            //闪烁搜索缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Search_Mark_Tri_Session.ToString(), 3, (int)Enum_Excute_Type.Search_Mark_Tri_Session, null, 0, 0));
            //即将涨停触发缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Quotes_Tri_Session.ToString(), 3, (int)Enum_Excute_Type.Shares_Quotes_Tri_Session, null, 0, 0));
            //题材缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Shares_Hotspot_Session.ToString(), 3, (int)Enum_Excute_Type.Shares_Hotspot_Session, null, 0, 0));
            return result;
        }

        private Session_Time_Info _toBuildTimeInfo(string DataKey,int ExcuteInterval,int ExcuteType,DateTime? NextExcuteTime,int TimerStatus,int TimerType,bool WriteToSecond=false)
        {
            return new Session_Time_Info
            {
                DataKey = DataKey,
                ExcuteInterval = ExcuteInterval,
                ExcuteType = ExcuteType,
                NextExcuteTime = NextExcuteTime,
                TimerStatus = TimerStatus,
                TimerType = TimerType
            };
        }

        public override object UpdateSession(int ExcuteType, object oct = null)
        {
            try
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
                    case (int)Enum_Excute_Type.Shares_PlateTag_IsAuto_Session:
                        return Shares_PlateTag_IsAuto_Session.UpdateSession();
                    case (int)Enum_Excute_Type.Plate_Statistic_Session:
                        return Plate_Statistic_Session.UpdateSession();
                    case (int)Enum_Excute_Type.Shares_Energy_Table_Session:
                        return Shares_Energy_Table_Session.UpdateSession();
                    case (int)Enum_Excute_Type.Shares_Limit_Fundmultiple_Session:
                        return Shares_Limit_Fundmultiple_Session.UpdateSession();
                    case (int)Enum_Excute_Type.Shares_RiseRate_His_Session:
                        return Shares_RiseRate_His_Session.UpdateSession();
                    case (int)Enum_Excute_Type.Shares_RiseLimit_Session:
                        return Shares_RiseLimit_Session.UpdateSession();
                    case (int)Enum_Excute_Type.Search_Mark_Tri_Session:
                        return Search_Mark_Tri_Session.UpdateSession();
                    case (int)Enum_Excute_Type.Shares_Quotes_Tri_Session:
                        return Shares_Quotes_Tri_Session.UpdateSession();
                    case (int)Enum_Excute_Type.Shares_Hotspot_Session:
                        return Shares_Hotspot_Session.UpdateSession();
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("UpdateSession出错,类型："+ ExcuteType, ex);
                return null;
            }
        }

        public object UpdateSessionPart(int ExcuteType, object newData)
        {
            try
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
                    case (int)Enum_Excute_Type.Shares_RiseRate_His_Session:
                        newSession = Shares_RiseRate_His_Session.UpdateSessionPart(newData);
                        dataKey = Enum_Excute_DataKey.Shares_RiseRate_His_Session.ToString();
                        break;
                    default:
                        break;
                }
                SetSessionToSecond(dataKey, newSession);
                return newSession;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("UpdateSessionPart出错", ex);
                return null;
            }
        }

        public override object CopySessionData(object objData, string dataKey)
        {
            try
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
                    case Enum_Excute_DataKey.Shares_PlateTag_IsAuto_Session:
                        return Shares_PlateTag_IsAuto_Session.CopySessionData(objData);
                    case Enum_Excute_DataKey.Plate_Statistic_Session:
                        return Plate_Statistic_Session.CopySessionData(objData);
                    case Enum_Excute_DataKey.Shares_Energy_Table_Session:
                        return Shares_Energy_Table_Session.CopySessionData(objData);
                    case Enum_Excute_DataKey.Shares_Limit_Fundmultiple_Session:
                        return Shares_Limit_Fundmultiple_Session.CopySessionData(objData);
                    case Enum_Excute_DataKey.Shares_RiseRate_His_Session:
                        return Shares_RiseRate_His_Session.CopySessionData(objData);
                    case Enum_Excute_DataKey.Search_Mark_Tri_Session:
                        return Search_Mark_Tri_Session.CopySessionData(objData);
                    case Enum_Excute_DataKey.Shares_Quotes_Tri_Session:
                        return Shares_Quotes_Tri_Session.CopySessionData(objData);
                    case Enum_Excute_DataKey.Shares_Hotspot_Session:
                        return Shares_Hotspot_Session.CopySessionData(objData);
                    default:
                        return base.CopySessionData(objData, dataKey);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("CopySessionData出错,dataKey:"+ dataKey, ex);
                return null;
            }
        }

        public override object OnGetData(string DataKey, GET_DATA_CXT _cxt)
        {
            if (_cxt == null)
            {
                return base.OnGetData(DataKey, _cxt);
            }
            TrendHandler handler = new TrendHandler();
            switch (_cxt.CmdId)
            {
                case GET_DATA_CMD_ID_SHARESTAGCALHELPER_CALCULATE:
                    SharesTagCalHelper._Calculate();
                    return null;
                case GET_ALL_SHARES_STATISTIC_INFO:
                    return Shares_Statistic_Session.Cal_Shares_Statistic();
                case GET_DATA_CMD_ID_PlATETAG_CALCULATE:
                    PlateTagCalHelper._Calculate();
                    return null;
                case GET_ALL_PLATE_STATISTIC_INFO:
                    return Plate_Statistic_Session.Cal_Plate_Statistic();
                case GET_SHARES_ENERGY_TABLE_SESSION_INFO:
                     PlateLeaderTriHelper._cal_Shares_Energy_Table();
                    return null;
                case GET_SHARES_RISERATE_RANKLIST:
                    return handler._getSharesRiserateRankList(_cxt.cmd_cxt);
                case GET_PLATE_QUOTES_INFO:
                    return handler._getPlateQuotesInfo(_cxt.cmd_cxt);
                case GET_PLATE_LEADERMODEL_SHARESLIST:
                    return handler._getPlateLeaderModelSharesList(_cxt.cmd_cxt);
                case GET_SHARES_ENERGYINDEX_LIST:
                    return handler._getSharesEnergyIndexList(_cxt.cmd_cxt);
                case GET_SHARES_HOTSPOT_LIST:
                    return handler._getSharesHotSpotList(_cxt.cmd_cxt);
                case GET_SHARES_ALLLIMITUP_LIST:
                    return handler._getSharesAllLimitUpList(_cxt.cmd_cxt);
                case GET_PLATE_ENERGY_INDEX_LIST:
                    return handler._getPlateEnergyIndexList(_cxt.cmd_cxt);
                case CAL_SHARESMONITORTTR:
                    SharesMonitorTriHelper._cal_SharesMonitorTri();
                    return null;
                default:
                    return base.OnGetData(DataKey, _cxt);
            }
        }

        protected override void OnSessionAfterWriting(string key)
        {
            if (tempPlate_Quotes_Date_Session != null)
            {
                string dataKey = Enum_Excute_DataKey.Plate_Quotes_Date_Session.ToString();
                SetSessionWithNolock(dataKey, tempPlate_Quotes_Date_Session);
                tempPlate_Quotes_Date_Session = null;
            }

            if (tempShares_Quotes_Date_Session != null)
            {
                string dataKey = Enum_Excute_DataKey.Shares_Quotes_Date_Session.ToString();
                SetSessionWithNolock(dataKey, tempShares_Quotes_Date_Session);
                tempShares_Quotes_Date_Session = null;
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

        //股票限制涨跌停缓存
        public Dictionary<long, t_shares_limit_fundmultiple> GetShares_Limit_Fundmultiple_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Limit_Fundmultiple_Session.ToString();
            var sessionData = (withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey)) as List<t_shares_limit_fundmultiple>;
            if (sessionData == null)
            {
                sessionData = new List<t_shares_limit_fundmultiple>();
            }
            var baseSession = GetShares_Base_Session(withlock);

            Dictionary<long, t_shares_limit_fundmultiple> result = new Dictionary<long, t_shares_limit_fundmultiple>();
            foreach (var item in baseSession)
            {
                var temp = (from x in sessionData
                            where item.Value.SharesCode.StartsWith(x.LimitKey) && (x.LimitMarket == -1 || x.LimitMarket == item.Value.Market)
                            select x).FirstOrDefault();
                if (temp != null)
                {
                    result.Add(item.Key, temp);
                }
            }
            return result;
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

        //股票板块计算是否可修改缓存
        public Dictionary<long, Shares_Name_Info> GetShares_PlateTag_IsAuto_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_PlateTag_IsAuto_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Shares_Name_Info>();
            }
            return session as Dictionary<long, Shares_Name_Info>;
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

        //关注缓存
        public List<Plate_Tag_FocusOn_Session_Info> GetPlate_Tag_FocusOn_Session_List(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Tag_FocusOn_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new List<Plate_Tag_FocusOn_Session_Info>();
            }
            return (session as Plate_Tag_FocusOn_Session_Obj).FocusOn_List;
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

        //涨缓存
        public List<Plate_Tag_Force_Session_Info> GetPlate_Tag_Force_Session_List(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Tag_Force_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new List<Plate_Tag_Force_Session_Info>();
            }
            return (session as Plate_Tag_Force_Session_Obj).Force_List;
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

        //走势缓存
        public List<Plate_Tag_TrendLike_Session_Info> GetPlate_Tag_TrendLike_Session_List(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Tag_TrendLike_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new List<Plate_Tag_TrendLike_Session_Info>();
            }
            return (session as Plate_Tag_TrendLike_Session_Obj).TrendLike_List;
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

        //计算龙头股票(从股票出发)
        public Dictionary<long, List<long>> GetLeaderShares_ByShares(bool withlock = true)
        {
            Dictionary<long, List<long>> result = new Dictionary<long, List<long>>();
            var plate_leader=GetLeaderShares_ByPlate(withlock);
            foreach (var item in plate_leader)
            {
                foreach (long share in item.Value)
                {
                    if (!result.ContainsKey(share))
                    {
                        result.Add(share, new List<long>());
                    }
                    result[share].Add(item.Key);
                }
            }
            return result;
        }

        //计算全市场龙头股票(从全市场股票出发)
        public List<long> GetLeaderShares_ByAllShares(bool withlock = true)
        {
            List<long> result = new List<long>();
            Dictionary<long, List<long>> leader_dic = GetLeaderShares_ByPlate(withlock);
            foreach (var item in leader_dic)
            {
                result.AddRange(item.Value);
            }
            return result.Distinct().ToList();
        }


        //板块历史行情缓存
        object GetPlate_Quotes_Date_Session_Lock = new object();
        Plate_Quotes_Session_Info_Obj tempPlate_Quotes_Date_Session = null;

        public Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> GetPlate_Quotes_Date_Session(int days = 0,bool withlock = true)
        {
            return GetPlate_Quotes_Date(days, withlock).SessionDic;
        }

        private Plate_Quotes_Session_Info_Obj _getPlate_Quotes_Date_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Quotes_Date_Session.ToString();
            var session = GetDataWithNoLock(dataKey, withlock);
            //var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
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

        //板块某一日行情缓存
        public Dictionary<long, Plate_Quotes_Session_Info> GetPlate_Quotes_AppointDate_Session(DateTime date, bool withlock = true)
        {
            Dictionary<long, Plate_Quotes_Session_Info> result = new Dictionary<long, Plate_Quotes_Session_Info>();
            if (date == DateTime.Now.Date)
            {
                result= GetPlate_Quotes_Today_Session(withlock);
            }
            else
            {
                var temp = GetPlate_Quotes_Date(60, withlock).SessionDic;
                foreach (var item in temp)
                {
                    if (!item.Value.ContainsKey(date))
                    {
                        continue;
                    }
                    result.Add(item.Key, item.Value[date]);
                }
            }
            var tempResult=result.Values.OrderByDescending(e=>e.RiseRate).ToList();

            Dictionary<long, Plate_Quotes_Session_Info> resultAll = new Dictionary<long, Plate_Quotes_Session_Info>();
            int idx = 0;
            foreach (var item in tempResult)
            {
                if (item.PlateBaseStatus == 1)
                {
                    idx++;
                    item.Rank = idx;
                }
                resultAll.Add(item.PlateId, item);
            }
            return result;
        }

        //板块最后行情缓存
        public Dictionary<long, Plate_Quotes_Session_Info> GetPlate_Quotes_Last_Session(bool isGetVolumeRate = true, bool withlock = true)
        {
            Dictionary<long, Plate_Quotes_Session_Info> result = new Dictionary<long, Plate_Quotes_Session_Info>();
            var date_session = GetPlate_Quotes_Date_Session(0, withlock);
            var plate_base_session = GetPlate_Base_Session(withlock);

            var newData = new Dictionary<long, Plate_Quotes_Session_Info>(GetPlate_Quotes_Today_Session(withlock));

            foreach (var item in date_session)
            {
                if (!newData.ContainsKey(item.Key))
                {
                    var last = item.Value.FirstOrDefault().Value;
                    newData.Add(item.Key, last);
                }
            }

            newData = newData.OrderByDescending(e => e.Value.RiseRate).ThenBy(e => e.Value.PlateId).ToDictionary(k => k.Key, v => v.Value);
            int idx = 0;
            foreach (var item in newData)
            {
                if (!plate_base_session.ContainsKey(item.Key))
                {
                    continue;
                }
                if (plate_base_session[item.Key].BaseStatus != 1 && plate_base_session[item.Key].PlateType!=4)
                {
                    continue;
                }
                idx++;
                item.Value.Rank = idx;
            }

            int tempSharesStockCal_Type = Singleton.Instance.SharesStockCal_Type;
            var stockDic = isGetVolumeRate ? GetPlate_TradeStock_Session(withlock) : new Dictionary<long, Plate_TradeStock_Session_Info>();
            foreach (var item in newData)
            {
                Plate_Quotes_Session_Info temp = new Plate_Quotes_Session_Info();
                temp.ClosedPrice = item.Value.ClosedPrice;
                temp.MaxPrice = item.Value.MaxPrice;
                temp.MinPrice = item.Value.MinPrice;
                temp.OpenedPrice= item.Value.OpenedPrice;
                temp.DealCount = item.Value.DealCount;
                temp.DealAmount = item.Value.DealAmount;
                temp.Date = item.Value.Date;
                temp.PlateId = item.Value.PlateId;
                temp.Rank = item.Value.Rank;
                temp.RealDays = item.Value.RealDays;
                temp.YestodayClosedPrice = item.Value.YestodayClosedPrice;
                temp.DownUpLimitCount = item.Value.DownUpLimitCount;
                temp.RiseUpLimitCount = item.Value.RiseUpLimitCount;
                
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
            return (sessionData as Plate_Minute_KLine_RiseRate_Obj).DataDic;
        }
        //板块分钟涨速缓存
        public Plate_Minute_KLine_RiseRate_Obj GetPlate_Minute_KLine_RiseRate_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Minute_KLine_Session.ToString();
            var sessionData = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (sessionData == null)
            {
                return new Plate_Minute_KLine_RiseRate_Obj();
            }
            return sessionData as Plate_Minute_KLine_RiseRate_Obj;
        }


        //股票历史行情缓存
        object GetShares_Quotes_Date_Session_Lock = new object();

        private Shares_Quotes_Session_Info_Obj _getShares_Quotes_Date_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Quotes_Date_Session.ToString();
            var session =  GetDataWithNoLock(dataKey, withlock);
            //var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
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

        public Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> GetShares_Quotes_Date_Session(int days = 0, bool withlock = true)
        {
            return GetShares_Quotes_Date(days, withlock).SessionDic;
        }
        public Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> GetShares_Quotes_Total_Session(int days = 0, bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Quotes_Today_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
            }
            var result= (session as Shares_Quotes_Today_Session_Obj).Shares_Quotes_Total;
            return result;
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
            return (session as Shares_Quotes_Today_Session_Obj).Shares_Quotes_Today;
        }

        //股票某一日行情缓存
        public Dictionary<long, Shares_Quotes_Session_Info> GetShares_Quotes_AppointDate_Session(DateTime date,bool withlock = true)
        {
            if (date == DateTime.Now.Date)
            {
                return GetShares_Quotes_Today_Session(withlock);
            }
            else
            {
                var result = new Dictionary<long, Shares_Quotes_Session_Info>();
                var temp=GetShares_Quotes_Date_Session(60, withlock);
                foreach (var item in temp)
                {
                    if (!item.Value.ContainsKey(date))
                    {
                        continue;
                    }
                    result.Add(item.Key,item.Value[date]);
                }
                return result;
            }
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
            DateTime dateNow = DbHelper.GetLastTradeDate2(0, 0, 0, 0, DateTime.Now.AddHours(-9));
            var newData = GetShares_Quotes_AppointDate_Session(dateNow, withlock);

            if (!isGetVolumeRate)
            {
                foreach (var item in newData)
                {
                    result.Add(item.Key, new Shares_Quotes_Session_Info_Last
                    {
                        shares_quotes_info = item.Value
                    });
                }
                return result;
            }

            DateTime datePre = DbHelper.GetLastTradeDate2(0, 0, 0, -1, dateNow);
            var yesData = GetShares_Quotes_AppointDate_Session(datePre, withlock);

            int tempSharesStockCal_Type = Singleton.Instance.SharesStockCal_Type;
            var stockDic = GetShares_TradeStock_Session(withlock);
            foreach (var item in newData)
            {
                Shares_Quotes_Session_Info_Last temp = new Shares_Quotes_Session_Info_Last();
                temp.shares_quotes_info = item.Value;

                long yesTotalCount = 0;
                if (yesData.ContainsKey(item.Key))
                {
                    yesTotalCount=yesData[item.Key].TotalCount*100;
                }
                if (stockDic.ContainsKey(item.Key))
                {
                    Shares_TradeStock_Session_Info _tradeStock = stockDic[item.Key];
                    long TradeStock_Avg = _tradeStock.TradeStock_Avg;
                    //获取剩余分钟数
                    long remainMinute = Shares_TradeStock_Session.GetRemainMinute(_tradeStock.TimeSpan);
                    if (240 - remainMinute >= Singleton.Instance.SharesStockCal_MinMinute && TradeStock_Avg > 0)
                    {
                        if (tempSharesStockCal_Type == 2)
                        {
                            temp.TotalCount_Today_Now = _tradeStock.TradeStock_Now;
                            temp.TotalCount_Yestoday_Now = _tradeStock.TradeStock_Yes;

                            temp.TotalCount_Yestoday_All = yesTotalCount;
                            temp.TotalCount_Today_Expect = TradeStock_Avg * remainMinute + _tradeStock.TradeStock_Now;
                        }
                        else
                        {
                            temp.TotalCount_Today_Now = _tradeStock.TradeStock_Now / (240 - remainMinute);
                            temp.TotalCount_Yestoday_Now = _tradeStock.TradeStock_Yes / (240 - remainMinute);

                            temp.TotalCount_Yestoday_All = yesTotalCount;
                            temp.TotalCount_Today_Expect = TradeStock_Avg * remainMinute + _tradeStock.TradeStock_Now;
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


        //板块股票关系缓存（根据板块获取）
        public Dictionary<long, Dictionary<long,Plate_Shares_Rel_Session_Info>> GetPlate_Shares_Rel_Dic_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Shares_Rel_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Shares_Rel_Session_Info>>();
            }
            return (session as Plate_Shares_Rel_Session_Obj).Plate_Shares_Rel_Dic_Session;
        }

        //板块股票关系缓存（根据股票获取）
        public Dictionary<long, Dictionary<long, Plate_Shares_Rel_Session_Info>> GetShares_Plate_Rel_Dic_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Shares_Rel_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Shares_Rel_Session_Info>>();
            }
            return (session as Plate_Shares_Rel_Session_Obj).Shares_Plate_Rel_Dic_Session;
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
                        if (dayType == 0)
                        {
                            if (Force_Session.ContainsKey(forceKey+1) && Force_Session[forceKey + 1].ContainsKey(plate.PlateId) && Singleton.Instance.SharesLeaderDaysType.Contains(1))
                            {
                                result[item.Key].Add(plate.PlateId);
                                continue;
                            }
                            if (Force_Session.ContainsKey(forceKey + 2) && Force_Session[forceKey + 2].ContainsKey(plate.PlateId) && Singleton.Instance.SharesLeaderDaysType.Contains(2))
                            {
                                result[item.Key].Add(plate.PlateId);
                                continue;
                            }
                            if (Force_Session.ContainsKey(forceKey + 3) && Force_Session[forceKey + 3].ContainsKey(plate.PlateId) && Singleton.Instance.SharesLeaderDaysType.Contains(3))
                            {
                                result[item.Key].Add(plate.PlateId);
                                continue;
                            }
                            if (Force_Session.ContainsKey(forceKey + 4) && Force_Session[forceKey + 4].ContainsKey(plate.PlateId) && Singleton.Instance.SharesLeaderDaysType.Contains(4))
                            {
                                result[item.Key].Add(plate.PlateId);
                                continue;
                            }
                        }
                        else
                        {
                            if (Force_Session.ContainsKey(forceKey) && Force_Session[forceKey].ContainsKey(plate.PlateId))
                            {
                                result[item.Key].Add(plate.PlateId);
                                continue;
                            }
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
                        if (dayType > 0)
                        {
                            tempList.AddRange(Force_Session[item.Key].Keys.Where(e => e % 100 == dayType).Select(e => e / 100).ToList());
                        }
                        else
                        {
                            var keys = Force_Session[item.Key].Keys.ToList();
                            foreach (var key in keys)
                            {
                                if (key % 100 == 1 && Singleton.Instance.SharesLeaderDaysType.Contains(1))
                                {
                                    tempList.Add(key / 100);
                                }
                                if (key % 100 == 2 && Singleton.Instance.SharesLeaderDaysType.Contains(2))
                                {
                                    tempList.Add(key / 100);
                                }
                                if (key % 100 == 3 && Singleton.Instance.SharesLeaderDaysType.Contains(3))
                                {
                                    tempList.Add(key / 100);
                                }
                                if (key % 100 == 4 && Singleton.Instance.SharesLeaderDaysType.Contains(4))
                                {
                                    tempList.Add(key / 100);
                                }
                            }
                        }
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

        //板块统计信息缓存
        public Plate_Statistic_Session_Info GetPlate_Statistic_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Statistic_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Plate_Statistic_Session_Info();
            }
            return session as Plate_Statistic_Session_Info;
        }

        //板块龙头信息缓存
        public Dictionary<long, Shares_Energy_Table_Session_Info> GetShares_Energy_Table_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Energy_Table_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Shares_Energy_Table_Session_Info>();
            }
            return session as Dictionary<long, Shares_Energy_Table_Session_Info>;
        }

        //股票综合涨幅排名缓存
        public Dictionary<DateTime,List<SharesRiseRateStc>> GetShares_RiseRate_His_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_RiseRate_His_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<DateTime, List<SharesRiseRateStc>>();
            }
            return session as Dictionary<DateTime, List<SharesRiseRateStc>>;
        }

        //股票涨停板缓存
        public Shares_RiseLimit_Session_Obj GetShares_RiseLimit_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_RiseLimit_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Shares_RiseLimit_Session_Obj();
            }
            return session as Shares_RiseLimit_Session_Obj;
        }

        //闪烁搜索缓存
        public Dictionary<long, Dictionary<long, t_sys_conditiontrade_template>> GetSearch_Mark_Tri_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Search_Mark_Tri_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Dictionary<long, t_sys_conditiontrade_template>>();
            }
            return session as Dictionary<long, Dictionary<long, t_sys_conditiontrade_template>>;
        }

        //即将涨停触发缓存
        public Dictionary<long, t_shares_quotes_tri> GetShares_Quotes_Tri_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Quotes_Tri_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, t_shares_quotes_tri>();
            }
            return session as Dictionary<long, t_shares_quotes_tri>;
        }

        //题材缓存
        public Dictionary<long, Shares_Hotspot_Session_Info> GetShares_Hotspot_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Shares_Hotspot_Session.ToString();
            var session = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (session == null)
            {
                return new Dictionary<long, Shares_Hotspot_Session_Info>();
            }
            return session as Dictionary<long, Shares_Hotspot_Session_Info>;
        }
    }
}
