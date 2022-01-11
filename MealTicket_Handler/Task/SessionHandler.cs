using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class SessionHandler : Session_New
    {
        /// <summary>
        /// 最小间隔
        /// </summary>
        const int MIN_TIMER_INTERVAL = 1000;

        public enum Enum_Excute_Type
        {
            Plate_Base_Session,
            Plate_Shares_Rel_Session
        }

        public enum Enum_Excute_DataKey
        {
            Plate_Base_Session,
            Plate_Shares_Rel_Session
        }

        public SessionHandler()
        {
            List<Session_Time_Info> Session_Time_Info_List = Session_Time_Info_List_Init();
            Init(MIN_TIMER_INTERVAL, Session_Time_Info_List);
        }

        private List<Session_Time_Info> Session_Time_Info_List_Init()
        {
            List<Session_Time_Info> result = new List<Session_Time_Info>();
            //板块基础数据缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Base_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Plate_Base_Session, null, 0, 0));
            //板块股票关系数据缓存
            result.Add(_toBuildTimeInfo(Enum_Excute_DataKey.Plate_Shares_Rel_Session.ToString(), 60 * 10, (int)Enum_Excute_Type.Plate_Shares_Rel_Session, null, 0, 0));
            return result;
        }

        private Session_Time_Info _toBuildTimeInfo(string DataKey, int ExcuteInterval, int ExcuteType, DateTime? NextExcuteTime, int TimerStatus, int TimerType)
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
                case (int)Enum_Excute_Type.Plate_Base_Session:
                    return Plate_Base_Session.UpdateSession();
                case (int)Enum_Excute_Type.Plate_Shares_Rel_Session:
                    return Plate_Shares_Rel_Session.UpdateSession();
                default:
                    return null;
            }
        }

        public override object CopySessionData(object objData, string dataKey)
        {
            switch ((Enum_Excute_DataKey)System.Enum.Parse(typeof(Enum_Excute_DataKey), dataKey))
            {
                case Enum_Excute_DataKey.Plate_Base_Session:
                    return Plate_Base_Session.CopySessionData(objData);
                case Enum_Excute_DataKey.Plate_Shares_Rel_Session:
                    return Plate_Shares_Rel_Session.CopySessionData(objData);
                default:
                    return base.CopySessionData(objData, dataKey);
            }
        }

        //板块基础数据缓存
        public Dictionary<long, Plate_Base_Session_Info> GetPlate_Base_Session(bool withlock=true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Base_Session.ToString();
            var sessionData = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (sessionData == null)
            {
                return new Dictionary<long, Plate_Base_Session_Info>();
            }
            return sessionData as Dictionary<long, Plate_Base_Session_Info>;
        }

        //板块股票关系数据缓存（板块分组）
        public Dictionary<long, Dictionary<long,Plate_Shares_Rel_Session_Info>> GetPlate_Shares_Rel_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Shares_Rel_Session.ToString();
            var sessionData = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (sessionData == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Shares_Rel_Session_Info>>();
            }
            var sessionResult=sessionData as List<Plate_Shares_Rel_Session_Info>;
            return sessionResult.GroupBy(e => e.PlateId).ToDictionary(k => k.Key, v => v.ToDictionary(ik => long.Parse(ik.SharesCode) * 10 + ik.Market, iv => iv));
        }

        //板块股票关系数据缓存（股票分组）
        public Dictionary<long, Dictionary<long, Plate_Shares_Rel_Session_Info>> GetShares_Plate_Rel_Session(bool withlock = true)
        {
            string dataKey = Enum_Excute_DataKey.Plate_Shares_Rel_Session.ToString();
            var sessionData = withlock ? GetDataWithLock(dataKey) : GetDataWithNoLock(dataKey);
            if (sessionData == null)
            {
                return new Dictionary<long, Dictionary<long, Plate_Shares_Rel_Session_Info>>();
            }
            var sessionResult = sessionData as List<Plate_Shares_Rel_Session_Info>;
            return sessionResult.GroupBy(e => new { e.Market,e.SharesCode}).ToDictionary(k => long.Parse(k.Key.SharesCode)*10+k.Key.Market, v => v.ToDictionary(ik => ik.PlateId, iv => iv));
        }
    }
}
