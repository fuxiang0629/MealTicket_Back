using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesPremiumTrendList
    {
    }

    public class GetSharesPremiumTrendListRequest
    {
        /// <summary>
        /// 1收盘价 2开盘价 3最高价 4最低价
        /// </summary>
        public int PriceType { get; set; }
    }

    public class GetSharesPremiumTrendByConditionIdListRequest
    {
        /// <summary>
        /// 1收盘价 2开盘价 3最高价 4最低价
        /// </summary>
        public int PriceType { get; set; }

        /// <summary>
        /// 1今日 2 3日 3 5日 
        /// </summary>
        public int ModelType { get; set; }

        /// <summary>
        /// 条件Id
        /// </summary>
        public long ConditionId { get; set; }
    }

    public class GetSharesPremiumTrendListRes
    { 
        public List<string> XData { get; set; }

        public List<SharesPremiumTrendInfo> SharesPremiumTrendList { get; set; }
    }

    public class SharesPremiumTrendInfo
    {
        public long ConditionId { get; set; }
        public string ConditionName { get; set; }

        public List<SharesPremiumTrend> PremiumRateToday { get; set; }

        public List<SharesPremiumTrend> PremiumRate3Days { get; set; }

        public List<SharesPremiumTrend> PremiumRate5Days { get; set; }
    }

    public class SharesPremiumTrend
    {
        public string Date { get; set; }
        public long ConditionId { get; set; }
        public string ConditionName { get; set; }

        public string PremiumRatePar { get; set; }

        public int PremiumRate { get; set; }

        /// <summary>
        /// 当天涨停数量
        /// </summary>
        public int LimitUpCount { get; set; }
    }
}
