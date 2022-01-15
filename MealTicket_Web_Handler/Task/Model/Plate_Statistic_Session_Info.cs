using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Statistic_Session_Info
    {
        /// <summary>
        /// 板块信息信息
        /// </summary>
        public Dictionary<long, Plate_Statistic_Info> Plate_Info { get; set; }

        /// <summary>
        /// 股票排行
        /// </summary>
        public Plate_Statistic_Rank_Obj Plate_Rank { get; set; }
    }

    public class Plate_Statistic_Info
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateName { get; set; }

        /// <summary>
        /// 板块类型
        /// </summary>
        public int PlateType { get; set; }

        /// <summary>
        /// 流通股本
        /// </summary>
        public long CirculatingCapital { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public long DealAmount { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long DealCount { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public long MaxPrice { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public long MinPrice { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 当前价
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int RiseRate
        {
            get
            {
                if (ClosedPrice == 0)
                {
                    return 0;
                }
                return (int)Math.Round((CurrPrice - ClosedPrice) * 1.0 / ClosedPrice * 10000, 0);
            }
        }

        /// <summary>
        /// 涨停数量
        /// </summary>
        public int RiseLimitCount { get; set; }

        /// <summary>
        /// 跌停数量
        /// </summary>
        public int DownLimitCount { get; set; }

        /// <summary>
        /// 股票总数量
        /// </summary>
        public int SharesCount { get; set; }

        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 综合涨幅
        /// </summary>
        public int OverallRiseRate { get; set; }
    }

    public class Plate_Statistic_Rank_Obj
    {
        /// <summary>
        /// 板块3天排名
        /// </summary>
        public SortedSet<Plate_Statistic_Session_Overall> Rank_3Days { get; set; }

        /// <summary>
        /// 板块5天排名
        /// </summary>
        public SortedSet<Plate_Statistic_Session_Overall> Rank_5Days { get; set; }

        /// <summary>
        /// 板块10天排名
        /// </summary>
        public SortedSet<Plate_Statistic_Session_Overall> Rank_10Days { get; set; }

        /// <summary>
        /// 板块15天排名
        /// </summary>
        public SortedSet<Plate_Statistic_Session_Overall> Rank_15Days { get; set; }

        /// <summary>
        /// 板块综合排名
        /// </summary>
        public SortedSet<Plate_Statistic_Session_Overall> Rank_Overall { get; set; }
    }

    public class Plate_Statistic_Session_Overall : IComparable<Plate_Statistic_Session_Overall>
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 实际天数
        /// </summary>
        public int RealDays { get; set; }

        /// <summary>
        /// 是否真实板块
        /// </summary>
        public bool IsRealPlate { get; set; }

        public int CompareTo(Plate_Statistic_Session_Overall other)
        {
            return (other.RiseRate.CompareTo(this.RiseRate) > 0 ? 1 : -1);
        }
    }
}
