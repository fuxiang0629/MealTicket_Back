using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesGroupStatisticLineList
    {
    }

    public class GetSharesGroupStatisticLineListRequest
    {
        public long ContextId{ get; set; }

        public int ContextType { get; set; }
    }

    public class SharesGroupStatisticLine
    {
        public string Date
        {
            get
            {
                return DateDt.ToString("yyyy-MM-dd");
            }
        }

        public DateTime DateDt { get; set; }

        public int LimitUpCount { get; set; }

        public int LimitDownCount { get; set; }

        public int LimitUpCountYestoday { get; set; }

        public int LimitUpCountYestoday_Today { get; set; }

        public int RiseRateYestoday { get; set; }

        public int RiseRate { get; set; }

        public long TotalCount { get; set; }

        public long TotalAmount { get; set; }

        public int BiddingRiseRate { get; set; }

        public long BiddingTotalCount { get; set; }

        public long BiddingTotalAmount { get; set; }
    }
}
