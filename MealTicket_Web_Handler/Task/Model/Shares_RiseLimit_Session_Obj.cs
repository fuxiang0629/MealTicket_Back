using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_RiseLimit_Session_Obj
    {
        /// <summary>
        /// 数据日期时间
        /// </summary>
        public DateTime DataDate { get; set; }

        /// <summary>
        /// 数据字典
        /// </summary>
        public Dictionary<long, Shares_RiseLimit_Session_Info> DataDic { get; set; }
    }

    public class Shares_RiseLimit_Session_Info
    {
        /// <summary>
        /// 股票Key
        /// </summary>
        public long SharesKey { get; set; }

        /// <summary>
        /// 市场code
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票Code
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 几天
        /// </summary>
        public int RiseLimitDays { get; set; }

        /// <summary>
        /// 几板
        /// </summary>
        public int RiseLimitCount { get; set; }

        /// <summary>
        /// 涨停时间
        /// </summary>
        public DateTime LastRiseLimitTime { get; set; }

        /// <summary>
        /// 涨停日期
        /// </summary>
        public DateTime LastRiseLimitDate { get; set; }

        /// <summary>
        /// 今天是否涨停
        /// </summary>
        public bool IsLimitUpToday { get; set; }

        /// <summary>
        /// 昨天是否涨停
        /// </summary>
        public bool IsLimitUpYesday { get; set; }
    }
}
