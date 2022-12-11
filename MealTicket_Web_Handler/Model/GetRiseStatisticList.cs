using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    public class GetRiseStatisticList
    {
    }

    public class GetRiseStatisticListRes
    {
        /// <summary>
        /// 股票总数量
        /// </summary>
        public int TotalCount { get; set; }

        public List<RiseStatistic> StatisticList { get; set; }

        public int RiseCount { get; set; }

        public int DownCount { get; set; }

        public int FlatCount { get; set; }

        public int RiseLimitCount { get; set; }

        public int DownLimitCount{get;set;}

        public List<string> KeyName { get; set; }
    }

    public class RiseStatistic 
    {
        public double MinCount { get; set; }

        public double MaxCount { get; set; }

        public int Count { get; set; }
    }
}
