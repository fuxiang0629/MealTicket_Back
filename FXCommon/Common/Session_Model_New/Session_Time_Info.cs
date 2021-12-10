using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FXCommon.Common
{
    public class Session_Time_Info
    {
        /// <summary>
        /// 执行类型
        /// </summary>
        public int ExcuteType { get; set; }

        /// <summary>
        /// 数据缓存Key
        /// </summary>
        public string DataKey { get; set; }

        /// <summary>
        /// 执行间隔(秒)
        /// </summary>
        public int ExcuteInterval { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextExcuteTime { get; set; }

        /// <summary>
        /// 任务类型0无需等待定时器 1需定时器执行完成
        /// </summary>
        public int TimerType { get; set; }

        /// <summary>
        /// 定时器状态 0有效 1无效
        /// </summary>
        public int TimerStatus { get; set; }
    }

    public class Session_Time_Info_Msg 
    {
        public int Msg_Id { get; set; }

        public Session_Time_Info Session_Time_Info { get; set; }
    }
}
