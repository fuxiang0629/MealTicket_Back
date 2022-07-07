using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesHighMarkStatisticInfo
    {
    }

    public class GetSharesHighMarkStatisticInfoRes
    {
        /// <summary>
        /// 天数
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// 最高板数
        /// </summary>
        public int MaxLimitUpCount { get; set; }

        public List<SharesHighMarkStatisticInfo> MaxSharesList { get; set; }

        /// <summary>
        /// 最低板数
        /// </summary>
        public int MinLimitUpCount { get; set; }

        public List<SharesHighMarkStatisticInfo> MinSharesList { get; set; }
    }

    public class SharesHighMarkStatisticInfo
    {
        public long SharesKey { get; set; }

        public int Market { get; set; }

        public string SharesCode { get; set; }

        public string SharesName { get; set; }

        public int RiseRate { get; set; }

        public bool IsSuspension { get; set; }

        public string Date { get; set; }
    }

    public class SharesHighMarkStatisticDb
    {
        public long SharesKey { get; set; }

        public DateTime Date { get; set; }

        /// <summary>
        /// 连板数
        /// </summary>
        public int RiseLimitCount { get; set; }

        /// <summary>
        /// 1最高板 2最低板
        /// </summary>
        public int Type { get; set; }
    }
}
