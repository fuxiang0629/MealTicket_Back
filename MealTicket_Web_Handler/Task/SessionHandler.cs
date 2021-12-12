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
            Shares_Quotes_Today_Session
        }

        public enum Enum_Excute_DataKey
        {
            Plate_Quotes_Date_Session,
            Plate_Quotes_Today_Session,
            Plate_Tag_FocusOn_Session,
            Plate_Tag_Force_Session,
            Plate_Tag_TrendLike_Session,
            Shares_Quotes_Date_Session,
            Shares_Quotes_Today_Session
        }

        public SessionHandler() 
        {
            List<Session_Time_Info> Session_Time_Info_List = Session_Time_Info_List_Init();
            Init(MIN_TIMER_INTERVAL, Session_Time_Info_List);
        }

        private List<Session_Time_Info> Session_Time_Info_List_Init()
        {
            List<Session_Time_Info> result = new List<Session_Time_Info>();
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.Plate_Quotes_Date_Session.ToString(),
                ExcuteInterval = 60 * 60 * 24,
                ExcuteType = (int)Enum_Excute_Type.Plate_Quotes_Date_Session,
                NextExcuteTime = DateTime.Now.Date.AddHours(1),
                TimerStatus = 0,
                TimerType = 0,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.Plate_Quotes_Today_Session.ToString(),
                ExcuteInterval = 3,
                ExcuteType = (int)Enum_Excute_Type.Plate_Quotes_Today_Session,
                NextExcuteTime = null,
                TimerStatus = 0,
                TimerType = 0,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.Plate_Tag_FocusOn_Session.ToString(),
                ExcuteInterval = 60 * 60 * 24,
                ExcuteType = (int)Enum_Excute_Type.Plate_Tag_FocusOn_Session,
                NextExcuteTime = DateTime.Now.Date.AddHours(1),
                TimerStatus = 0,
                TimerType = 0,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.Plate_Tag_Force_Session.ToString(),
                ExcuteInterval = 60 * 60 * 24,
                ExcuteType = (int)Enum_Excute_Type.Plate_Tag_Force_Session,
                NextExcuteTime = DateTime.Now.Date.AddHours(1),
                TimerStatus = 0,
                TimerType = 0,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.Plate_Tag_TrendLike_Session.ToString(),
                ExcuteInterval = 60 * 60 * 24,
                ExcuteType = (int)Enum_Excute_Type.Plate_Tag_TrendLike_Session,
                NextExcuteTime = DateTime.Now.Date.AddHours(1),
                TimerStatus = 0,
                TimerType = 0,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.Shares_Quotes_Date_Session.ToString(),
                ExcuteInterval = 60 * 60 * 24,
                ExcuteType = (int)Enum_Excute_Type.Shares_Quotes_Date_Session,
                NextExcuteTime = DateTime.Now.Date.AddHours(1),
                TimerStatus = 0,
                TimerType = 0,
            });
            result.Add(new Session_Time_Info
            {
                DataKey = Enum_Excute_DataKey.Shares_Quotes_Today_Session.ToString(),
                ExcuteInterval = 3,
                ExcuteType = (int)Enum_Excute_Type.Shares_Quotes_Today_Session,
                NextExcuteTime = null,
                TimerStatus = 0,
                TimerType = 0,
            });
            return result;
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
                default:
                    return null;
            }
        }

        public override object UpdateSessionPart(int ExcuteType, object newData)
        {
            switch (ExcuteType)
            {
                case (int)Enum_Excute_Type.Plate_Quotes_Date_Session:
                    return Plate_Quotes_Date_Session.UpdateSessionPart(newData);
                case (int)Enum_Excute_Type.Plate_Quotes_Today_Session:
                    return Plate_Quotes_Today_Session.UpdateSessionPart(newData);
                case (int)Enum_Excute_Type.Plate_Tag_FocusOn_Session:
                    return Plate_Tag_FocusOn_Session.UpdateSessionPart(newData);
                case (int)Enum_Excute_Type.Plate_Tag_Force_Session:
                    return Plate_Tag_Force_Session.UpdateSessionPart(newData);
                case (int)Enum_Excute_Type.Plate_Tag_TrendLike_Session:
                    return Plate_Tag_TrendLike_Session.UpdateSessionPart(newData);
                case (int)Enum_Excute_Type.Shares_Quotes_Date_Session:
                    return Shares_Quotes_Date_Session.UpdateSessionPart(newData);
                case (int)Enum_Excute_Type.Shares_Quotes_Today_Session:
                    return Shares_Quotes_Today_Session.UpdateSessionPart(newData);
                default:
                    return null;
            }
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
    }
}
