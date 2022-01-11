using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Statistic_Session_Obj
    {
        /// <summary>
        /// 股票信息
        /// </summary>
        public Dictionary<long, Statistic_Shares_Info> Shares_Info { get; set; }

        /// <summary>
        /// 股票排行
        /// </summary>
        public Statistic_Shares_Rank_Obj Shares_Rank { get; set; }

        /// <summary>
        /// 板块排行
        /// </summary>
        public Statistic_Plate_Rank_Obj Plate_Rank { get; set; }
    }

    public class Statistic_Shares_Base
    {
        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }
    }

    public class Statistic_Shares_Info: Statistic_Shares_Base
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 上市状态
        /// </summary>
        public int MarketStatus { get; set; }

        /// <summary>
        /// 流通股
        /// </summary>
        public long CirculatingCapital { get; set; }

        /// <summary>
        /// 股票综合涨幅
        /// </summary>
        public int OverallRiseRate { get; set; }

        /// <summary>
        /// 五档信息
        /// </summary>

        public Shares_Statistic_Session_Quotes_Info Quotes_Info { get; set; }

        /// <summary>
        /// 所属板块
        /// </summary>
        public Dictionary<long, Statistic_Plate_Info> Plate_Dic { get; set; }

        /// <summary>
        /// 板块标签
        /// </summary>
        public Statistic_Plate_Tag_Obj Plate_Tag { get; set; }
    }

    public class Shares_Statistic_Session_Quotes_Info
    {
        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 当前价
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 涨跌额
        /// </summary>
        public long RisePrice
        {
            get
            {
                return CurrPrice - ClosedPrice;
            }
        }

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
        /// 成交量
        /// </summary>
        public long DealCount { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public long DealAmount { get; set; }

        /// <summary>
        /// 此刻量比
        /// </summary>
        public int RateNow { get; set; }

        /// <summary>
        /// 预期量比
        /// </summary>
        public int RateExpect { get; set; }
    }

    public class Shares_Statistic_Session_Overall:IComparable<Shares_Statistic_Session_Overall>
    {
        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 实际天数
        /// </summary>
        public int RealDays { get; set; }

        public int CompareTo(Shares_Statistic_Session_Overall other)
        {
            return (other.RiseRate.CompareTo(this.RiseRate) > 0 ? 1 : -1);
        }
    }

    public class Statistic_Plate_Base
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }
    }

    public class Statistic_Plate_Info: Statistic_Plate_Base
    {
        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateName { get; set; }

        /// <summary>
        /// 板块类型
        /// </summary>
        public int PlateType { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

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
        /// 是否走势最像
        /// </summary>
        public bool IsTrendLike { get; set; }

        /// <summary>
        /// 是否重点关注
        /// </summary>
        public bool IsFocusOn { get; set; }
    }

    public class Statistic_Plate_Tag
    {
        /// <summary>
        /// 是否涨1
        /// </summary>
        public bool IsForce1 { get; set; }

        /// <summary>
        /// 是否涨2
        /// </summary>
        public bool IsForce2 { get; set; }

        /// <summary>
        /// 龙类型
        /// </summary>
        public int LeaderType { get; set; }

        /// <summary>
        /// 日内龙类型
        /// </summary>
        public int DayLeaderType { get; set; }

        /// <summary>
        /// 中军类型
        /// </summary>
        public int MainarmyType { get; set; }
    }

    public class Statistic_Shares_Rank_Obj
    {
        /// <summary>
        /// 股票3天排名
        /// </summary>
        public SortedSet<Shares_Statistic_Session_Overall> Rank_3Days { get; set; }

        /// <summary>
        /// 股票5天排名
        /// </summary>
        public SortedSet<Shares_Statistic_Session_Overall> Rank_5Days { get; set; }

        /// <summary>
        /// 股票10天排名
        /// </summary>
        public SortedSet<Shares_Statistic_Session_Overall> Rank_10Days { get; set; }

        /// <summary>
        /// 股票15天排名
        /// </summary>
        public SortedSet<Shares_Statistic_Session_Overall> Rank_15Days { get; set; }
    }

    public class Statistic_Plate_Rank_Obj
    {
        public Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>> Rank_3Days { get; set; }
        public Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>> Rank_5Days { get; set; }
        public Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>> Rank_10Days { get; set; }
        public Dictionary<long, SortedSet<Shares_Statistic_Session_Overall>> Rank_15Days { get; set; }
    }

    public class Statistic_Plate_Tag_Obj
    {
        public Dictionary<long, Statistic_Plate_Tag> Tag_3Days { get; set; }
        public Dictionary<long, Statistic_Plate_Tag> Tag_5Days { get; set; }
        public Dictionary<long, Statistic_Plate_Tag> Tag_10Days { get; set; }
        public Dictionary<long, Statistic_Plate_Tag> Tag_15Days { get; set; }
    }
}