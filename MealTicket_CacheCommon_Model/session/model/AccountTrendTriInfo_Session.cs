using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_CacheCommon_Session.session
{
    public class AccountTrendTriInfo_Session
    {
        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// RelId
        /// </summary>
        public long RelId { get; set; }

        /// <summary>
        /// 自选股Id
        /// </summary>
        public long OptionalId { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 触发推送时间
        /// </summary>
        public DateTime PushTime { get; set; }

        /// <summary>
        /// 今日触发次数
        /// </summary>
        public int TriCountToday { get; set; }

        /// <summary>
        /// 走势Id
        /// </summary>
        public long TrendId { get; set; }

        /// <summary>
        /// 触发描述
        /// </summary>
        public string TriDesc { get; set; }
    }
}
