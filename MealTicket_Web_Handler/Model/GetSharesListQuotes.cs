using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesListQuotes
    {
    }

    public class SharesListQuotesInfo
    {
        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 当前价
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 涨跌价
        /// </summary>
        public long RisePrice { get; set; }

        /// <summary>
        /// 数据时间
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
                        return "rgb(50, 0, 255)";
                    }
                    else if (intervalSecond < 120)
                    {
                        return "rgb(50, 0, 180)";
                    }
                    else if (intervalSecond < 180)
                    {
                        return "rgb(50, 0, 100)";
                    }
                    else
                    {
                        return "rgb(0, 0, 0)";
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
                        return "rgb(180, 0, 0)";
                    }
                    else if (intervalSecond < 180)
                    {
                        return "rgb(100, 0, 0)";
                    }
                    else
                    {
                        return "rgb(0, 0, 0)";
                    }
                }
            }
        }

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
    }
}
