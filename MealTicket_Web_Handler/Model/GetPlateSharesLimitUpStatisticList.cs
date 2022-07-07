using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetPlateSharesLimitUpStatisticList
    {
    }

    public class GetPlateSharesLimitUpStatisticListRequest
    {
        public DateTime? Date { get; set; }
    }
    public class PlateSharesLimitUpStatisticInfo
    {
        public long PlateId { get; set; }

        public string PlateName { get; set; }

        public int PlateType { get; set; }

        public int PlateRiseRate { get; set; }

        public int PlateRank { get; set; }

        public DateTime Date { get; set; }

        public int ChangeIndex 
        {
            get 
            {
                if (LimitUpCount < LimitUpCount1Days)
                {
                    return -1;
                }
                int temp = LimitUpCount1Days;
                if (temp > LimitUpCount2Days)
                {
                    temp = LimitUpCount2Days;
                }
                if (LimitUpCount == 0)
                {
                    return 0;
                }
                return (int)Math.Round(LimitUpCount * 1.0 / (LimitUpCount + temp) * 10000, 0);
            }
        }

        public int LimitUpCount { get; set; }

        public int LimitUpCount1Days { get; set; }

        public int LimitUpCount2Days { get; set; }

        public List<LimitUpSharesDays> LimitUpShares { get; set; }

        public int LinkageChangeIndex { get; set; }

        public int LinkageLimitUpCount { get; set; }

        public int LinkageLimitUpCount1Days { get; set; }

        public int LinkageLimitUpCount2Days { get; set; }

        public List<LimitUpSharesDays> LinkageLimitUpShares { get; set; }
    }

    public class LimitUpSharesDays
    {
        public DateTime Date { get; set; }

        public List<long> LimitUpShares { get; set; }
    }
}
