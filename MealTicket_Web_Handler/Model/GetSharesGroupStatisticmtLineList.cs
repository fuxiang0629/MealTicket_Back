using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesGroupStatisticmtLineList
    {
    }

    public class GetSharesGroupStatisticmtLineListRes
    {
        public List<SharesGroupStatisticmtLine> List { get; set; }

        public int LimitUpCount { get; set; }

        public int LimitUpCountPre { get; set; }

        public int LimitDownCount { get; set; }

        public int RiseRateYestoday { get; set; }

        public int RiseUpCount { get; set; }

        public int SharesTotalCount { get; set; }

        public int RiseUpCountPre { get; set; }

        public int RiseUpCountRate 
        {
            get 
            {
                if (RiseUpCountPre == 0)
                {
                    return 0;
                }
                return (int)Math.Round(RiseUpCount * 1.0 / RiseUpCountPre * 100, 0);
            } 
        }

        public long TotalAmount { get; set; }

        public long TotalAmountPre { get; set; }

        public int TotalAmountRate
        {
            get
            {
                if (TotalAmountPre == 0)
                {
                    return 0;
                }
                return (int)Math.Round(TotalAmount * 1.0 / TotalAmountPre * 100, 0);
            }
        }
        public int TotalAmountRateExcept { get; set; }

        public int LimitUpCountYestoday { get; set; }

        public int LimitUpCountYestoday_Today { get; set; }

        public List<StatisticSharesInfo> SharesList { get; set; }

        public List<StatisticSharesInfo> SharesTodayList { get; set; }
    }

    public class StatisticSharesInfo 
    {
        public long SharesKey { get; set; }

        public string SharesCode { get; set; }

        public string SharesName { get; set; }

        public int RiseRate { get; set; }

        public DateTime LimitUpTime { get; set; }

        public int PriceType { get; set; }
    }

    public class GetSharesGroupStatisticmtLineListRequest
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public long ContextId { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int ContextType { get; set; }

        /// <summary>
        /// 不获取分时列表
        /// </summary>
        public bool NoGetMtLine { get; set; }
    }

    public class SharesGroupStatisticmtLine
    {
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

    public class SharesGroupStatisticAllTimemtLineInfo
    {
        public DateTime Date { get; set; }

        public DateTime TimeDt { get; set; }

        public string Time
        {
            get
            {
                return TimeDt.ToString("HH:mm");
            }
        }

        public int RiseCountRate { get; set; }

        public int DealCountRate { get; set; }

        public int RiseUpCount { get; set; }

        public int RiseUpYesCount { get; set; }

        public long TradeStock { get; set; }
    }
}
