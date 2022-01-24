using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Minute_KLine_Session_Info
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 上一分钟收盘价
        /// </summary>
        public long PreClosePrice { get; set; }
    }
    public class Plate_Minute_KLine_RiseRate
    {
        /// <summary>
        /// 1分钟涨幅
        /// </summary>
        public int Minute1RiseRate { get; set; }

        /// <summary>
        /// 3分钟涨幅
        /// </summary>
        public int Minute3RiseRate { get; set; }

        /// <summary>
        /// 5分钟涨幅
        /// </summary>
        public int Minute5RiseRate { get; set; }

        /// <summary>
        /// 10分钟涨幅
        /// </summary>
        public int Minute10RiseRate { get; set; }

        /// <summary>
        /// 15分钟涨幅
        /// </summary>
        public int Minute15RiseRate { get; set; }
    }

    public class Plate_Minute_KLine_RiseRate_Rank
    {
        public SortedSet<Plate_Minute_KLine_RiseRate_Rank_Info> Minute1Rank { get; set; }
        public SortedSet<Plate_Minute_KLine_RiseRate_Rank_Info> Minute3Rank { get; set; }
        public SortedSet<Plate_Minute_KLine_RiseRate_Rank_Info> Minute5Rank { get; set; }
        public SortedSet<Plate_Minute_KLine_RiseRate_Rank_Info> Minute10Rank { get; set; }
        public SortedSet<Plate_Minute_KLine_RiseRate_Rank_Info> Minute15Rank { get; set; }
    }

    public class Plate_Minute_KLine_RiseRate_Rank_Info:IComparable<Plate_Minute_KLine_RiseRate_Rank_Info>
    {
        public long PlateId { get; set; }

        public int RiseRate { get; set; }

        public int CompareTo(Plate_Minute_KLine_RiseRate_Rank_Info other)
        {
            return (other.RiseRate.CompareTo(this.RiseRate) > 0 ? 1 : -1);
        }
    }

    public class Plate_Minute_KLine_RiseRate_Obj
    {
        public Dictionary<long,Plate_Minute_KLine_RiseRate> RiseRateInfo { get; set; }

        public Plate_Minute_KLine_RiseRate_Rank RankInfo { get; set; }

        public Dictionary<long, List<Plate_Minute_KLine_Session_Info>> DataDic { get; set; }
    }
}
