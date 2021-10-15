using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesConditionSearch
    {
    }

    public class GetSharesConditionSearchRequest : PageRequest 
    {
        /// <summary>
        /// 查询条件信息
        /// </summary>
        public List<searchInfo> SearchInfo { get; set; }
    }

    public class searchInfo 
    {
        /// <summary>
        /// 左括号
        /// </summary>
        public int leftbracket { get; set; }

        /// <summary>
        /// 右括号
        /// </summary>
        public int rightbracket { get; set; }

        /// <summary>
        /// 练接词1或 2且
        /// </summary>
        public int connect { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 查询参数
        /// </summary>
        public string content { get; set; }
    }

    public class SharesConditionSearchInfo
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
        /// 现价
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 涨幅
        /// </summary>
        public int RiseRate
        {
            get
            {
                if (ClosedPrice <= 0)
                {
                    return 0;
                }
                return (int)Math.Round(((PresentPrice - ClosedPrice) * 1.0 / ClosedPrice) * 10000, 0);
            }
        }

        /// <summary>
        /// 开盘涨跌幅
        /// </summary>
        public int OpenedRiseRate
        {
            get
            {
                if (ClosedPrice <= 0)
                {
                    return 0;
                }
                return (int)Math.Round(((OpenedPrice - ClosedPrice) * 1.0 / ClosedPrice) * 10000, 0);
            }
        }

        /// <summary>
        /// 昨日涨跌幅
        /// </summary>
        public int YestodayRiseRate { get; set; }

        /// <summary>
        /// 前2日涨跌幅
        /// </summary>
        public int TwoDaysRiseRate { get; set; }

        /// <summary>
        /// 前3日涨跌幅
        /// </summary>
        public int ThreeDaysRiseRate { get; set; }

        /// <summary>
        /// 最大涨跌幅
        /// </summary>
        public long Range { get; set; }

        public int Fundmultiple { get; set; }

        /// <summary>
        /// 所属自定义分组
        /// </summary>
        public List<long> GroupList { get; set; }

        /// <summary>
        /// 条件买入状态1不存在 2未开启 3已开启
        /// </summary>
        public int? ConditionStatus { get; set; }

        public long? ConditionId { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }

        /// <summary>
        /// 总股本
        /// </summary>
        public long TotalCapital { get; set; }

        /// <summary>
        /// 流通股本
        /// </summary>
        public long CirculatingCapital { get; set; }

        /// <summary>
        /// 天数
        /// </summary>
        public int Days
        {
            get
            {
                return Singleton.Instance.QuotesDaysShow;
            }
        }

        /// <summary>
        /// x日平均成交量
        /// </summary>
        public int DaysAvgDealCount { get; set; }

        /// <summary>
        /// x日平均成交额
        /// </summary>
        public long DaysAvgDealAmount { get; set; }

        /// <summary>
        /// x日平均换手率
        /// </summary>
        public int DaysAvgHandsRate
        {
            get
            {
                if (CirculatingCapital <= 0)
                {
                    return 0;
                }
                return (int)(DaysAvgDealCount * 1.0 / CirculatingCapital * 10000);
            }
        }

        /// <summary>
        /// 上一日成交量
        /// </summary>
        public int PreDayDealCount { get; set; }

        /// <summary>
        /// 上一日成交额
        /// </summary>
        public long PreDayDealAmount { get; set; }

        /// <summary>
        /// 上一日换手率
        /// </summary>
        public int PreDayHandsRate
        {
            get
            {
                if (CirculatingCapital <= 0)
                {
                    return 0;
                }
                return (int)(PreDayDealCount * 1.0 / CirculatingCapital * 10000);
            }
        }

        /// <summary>
        /// 当日成交量
        /// </summary>
        public int TodayDealCount { get; set; }

        /// <summary>
        /// 当日成交额
        /// </summary>
        public long TodayDealAmount { get; set; }

        /// <summary>
        /// 当日换手率
        /// </summary>
        public int TodayHandsRate
        {
            get
            {
                if (CirculatingCapital <= 0)
                {
                    return 0;
                }
                return (int)(TodayDealCount * 1.0 / CirculatingCapital * 10000);
            }
        }

        /// <summary>
        /// x日涨停次数
        /// </summary>
        public int LimitUpCount { get; set; }

        /// <summary>
        /// x日跌停次数
        /// </summary>
        public int LimitDownCount { get; set; }

        /// <summary>
        /// 最近几个交易日涨停过
        /// </summary>
        public string LimitUpDay { get; set; }
    }
}
