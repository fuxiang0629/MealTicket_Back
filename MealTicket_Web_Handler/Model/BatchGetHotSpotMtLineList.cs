using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BatchGetHotSpotMtLineList
    {
    }

    public class GetSharesHotSpotStatisticRequest
    {
        public long SharesKey { get; set; }
    }

    public class BatchGetHotSpotMtLineListRequest
    {
        /// <summary>
        /// 题材Id
        /// </summary>
        public List<long> HotspotIdList { get; set; }
    }

    public class HotSpotStatisticData
    {
        public long HotspotId { get; set; }
        public int RiseUpCount { get; set; }
        public long TotalCount { get; set; }
        public long GroupTimeKey { get; set; }
    }

    public class BatchGetHotSpotMtLineListRes
    {
        public long HotspotId { get; set; }

        public List<HotSpotMtLineInfo> MtLineList { get; set; }
    }

    public class HotSpotMtLineInfo
    {
        public long HotspotId { get; set; }
        public DateTime Date { get; set; }

        public DateTime TimeDt { get; set; }

        public string Time
        {
            get
            {
                return TimeDt.ToString("HH:mm");
            }
        }

        public int RiseRate { get; set; }

        public long TradeStock { get; set; }

        public long TradeAmount { get; set; }
    }

    public class BatchGetHotSpotAllMtLineListRes
    {
        public long HotspotId { get; set; }

        public List<HotSpotAllMtLineInfo> MtLineList { get; set; }
    }

    public class HotSpotAllMtLineInfo
    {
        public long HotspotId { get; set; }
        public DateTime Date { get; set; }

        public DateTime TimeDt { get; set; }

        public string Time
        {
            get
            {
                return TimeDt.ToString("HH:mm");
            }
        }

        public long TradeStock { get; set; }

        public long RiseUpCount { get; set; }

        public int RiseCountRate { get; set; }

        public long RiseUpYesCount { get; set; }

        public long TradeStockRate { get; set; }
    }

    public class HotSpotStatistic
    {
        public long HotspotId { get; set; }

        public string HotspotName { get; set; }

        public string BgColor { get; set; }

        public int LimitUpCount { get; set; }

        public int LimitDownCount { get; set; }

        public int RiseRateYestoday { get; set; }

        public int RiseCountRate { get; set; }

        public int RiseCountToday { get; set; }

        public int SharesTotalCount { get; set; }

        public long TradeStock { get; set; }

        public int TradeStockRate { get; set; }

        public int TradeStockRateExcept { get; set; }

        public int LimitUpCountYestoday { get; set; }

        public int LimitUpCountYestoday_Today { get; set; }

        public List<StatisticSharesInfo> SharesList { get; set; }

        public string SharesListStr { get; set; }

        public List<StatisticSharesInfo> SharesTodayList { get; set; }
    }
}
