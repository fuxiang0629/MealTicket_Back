using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    public class SharesQuotesRiselimit
    {
        public long SharesKey { get; set; }

        public int RiseLimitDays { get; set; }

        public int RiseLimitCount { get; set; }

        public bool IsLimitUpToday { get; set; }

        public bool IsLimitUpYesday { get; set; }

        /// <summary>
        /// 昨指连板数
        /// </summary>
        public int FilterRiseLimitCount { get; set; }
    }

    public class SharesTagInfo
    {
        public long SharesKey { get; set; }

        /// <summary>
        /// 炸板次数
        /// </summary>
        public int LimitUpBombCount { get; set; }

        /// <summary>
        /// 是否一字
        /// </summary>
        public bool IsLimitUpLikeOne { get; set; }

        /// <summary>
        /// 是否T字
        /// </summary>
        public bool IsLimitUpLikeT { get; set; }

        /// <summary>
        /// 是否反包
        /// </summary>
        public bool IsReverse { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }
    }
}
