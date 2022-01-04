using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountRiseLimitTriList
    {
    }

    public class GetAccountRiseLimitTriListRequest
    {
        public DateTime? LastDataTime { get; set; }
    }

    public class AccountRiseLimitTriInfo
    {
        /// <summary>
        /// 唯一Id
        /// </summary>
        public string RelId { get; set; }

        /// <summary>
        /// 市场
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
        /// 触发推送时间
        /// </summary>
        public DateTime PushTime { get; set; }

        /// <summary>
        /// 今日触发次数
        /// </summary>
        public int TriCountToday { get; set; }

        /// <summary>
        /// 文字颜色
        /// </summary>
        public string TextColor
        {
            get
            {
                DateTime timeNow = DateTime.Now;
                int intervalSecond = (int)(timeNow - PushTime).TotalSeconds;
                if (TriCountToday > 1)
                {
                    if (intervalSecond < 60)
                    {
                        return "rgb(6, 88, 241)";
                    }
                    else if (intervalSecond < 120)
                    {
                        return "rgb(95, 148, 247)";
                    }
                    else if (intervalSecond < 180)
                    {
                        return "rgb(161, 190, 244)";
                    }
                    else
                    {
                        return "-1";
                    }
                }
                else
                {
                    if (intervalSecond < 60)
                    {
                        return "rgb(255, 0, 0)";
                    }
                    else if (intervalSecond < 120)
                    {
                        return "rgb(255, 100, 100)";
                    }
                    else if (intervalSecond < 180)
                    {
                        return "rgb(255, 200, 200)";
                    }
                    else
                    {
                        return "-1";
                    }
                }
            }
        }

        /// <summary>
        /// 所属行业
        /// </summary>
        public string Industry { get; set; }

        /// <summary>
        /// 主营业务
        /// </summary>
        public string Business { get; set; }

        /// <summary>
        /// 类型-1涨停池 -2跌停池 -3即将涨停池 -4即将跌停池 -5炸板池 -6翘板池
        /// </summary>
        public int Type { get; set; }

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
                return (int)(PreDayDealCount* 1.0 / CirculatingCapital * 10000);
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

        /// <summary>
        /// 上一日当前时刻平均成交量
        /// </summary>
        public long PreNowAvgDealCount { get; set; }

        /// <summary>
        /// 上一日当前时刻平均成交额
        /// </summary>
        public long PreNowAvgDealAmount { get; set; }

        /// <summary>
        /// 上一日当前时刻平均换手率
        /// </summary>
        public int PreNowAvgHandsRate
        {
            get
            {
                if (CirculatingCapital <= 0)
                {
                    return 0;
                }
                return (int)(PreNowAvgDealCount * 1.0 / CirculatingCapital * 10000);
            }
        }

        /// <summary>
        /// x日当前时刻平均成交量
        /// </summary>
        public long DaysNowAvgDealCount { get; set; }

        /// <summary>
        /// x日当前时刻平均成交额
        /// </summary>
        public long DaysNowAvgDealAmount { get; set; }

        /// <summary>
        /// x日当前时刻平均换手率
        /// </summary>
        public int DaysNowAvgHandsRate
        {
            get
            {
                if (CirculatingCapital <= 0)
                {
                    return 0;
                }
                return (int)(DaysNowAvgDealCount * 1.0 / CirculatingCapital * 10000);
            }
        }

        /// <summary>
        /// 标记
        /// </summary>
        public string Mark { get; set; }

        /// <summary>
        /// 连板天数
        /// </summary>
        public int LimitUpDays { get; set; }

        public string TriDesc { get; set; }

        /// <summary>
        /// 自选股分组列表
        /// </summary>
        public List<long> MySharesGroupList { get; set; }

        /// <summary>
        /// 当前与昨日量比
        /// </summary>
        public int StockRate_Now { get; set; }

        /// <summary>
        /// 当前预计与昨日全量比
        /// </summary>
        public int StockRate_All { get; set; }

        public int LeaderType { get; set; }
    }
}
