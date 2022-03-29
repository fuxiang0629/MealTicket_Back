using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetPlateQuotesInfo
    {
    }

    public class GetPlateQuotesInfoRequest
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 是否需要排名数据
        /// </summary>
        public bool IsNeedRank { get; set; }
    }

    public class BatchGetPlateQuotesInfoRequest
    {
        /// <summary>
        /// 板块Key
        /// </summary>
        public List<long> PlateKey { get; set; }
    }

    public class PlateRankInfo
    {
        public int Day1RiseRate { get; set; }

        public int Day1Rank { get; set; }

        public int Day1RealDays { get; set; }

        public int Day3RiseRate { get; set; }

        public int Day3Rank { get; set; }

        public int Day3RealDays { get; set; }

        public int Day5RiseRate { get; set; }

        public int Day5Rank { get; set; }

        public int Day5RealDays { get; set; }

        public int Day10RiseRate { get; set; }

        public int Day10Rank { get; set; }

        public int Day10RealDays { get; set; }

        public int Day15RiseRate { get; set; }

        public int Day15Rank { get; set; }

        public int Day15RealDays { get; set; }

        /// <summary>
        /// 综合涨跌幅
        /// </summary>
        public int OverallRiseRate { get; set; }

        /// <summary>
        /// 综合排名
        /// </summary>
        public int OverallRank { get; set; }

        public int FocusOnRank 
        {
            get 
            {
                return Singleton.Instance.SharesLeaderPlateDayRank;
            }
        }
    }

    public class PlateQuotesInfo
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
        /// 当前价格
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 今日开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

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
                return (int)Math.Round((PresentPrice - ClosedPrice) * 10000.0 / ClosedPrice, 0);
            }
        }

        /// <summary>
        /// 最高价
        /// </summary>
        public long MaxPrice { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public long MinPrice { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long TodayDealCount { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public long TodayDealAmount { get; set; }

        /// <summary>
        /// 流通股本
        /// </summary>
        public long CirculatingCapital { get; set; }

        /// <summary>
        /// 换手率
        /// </summary>
        public int TodayHandsRate
        {
            get
            {
                if (CirculatingCapital == 0)
                {
                    return 0;
                }
                return (int)Math.Round(TodayDealCount * 1.0 / CirculatingCapital * 10000, 0);
            }
        }

        /// <summary>
        /// 板块排名
        /// </summary>
        public PlateRankInfo RankInfo { get; set; }

        /// <summary>
        /// 涨速
        /// </summary>
        public MinuteRiseInfo MinuteRise { get; set; }
    }
}
