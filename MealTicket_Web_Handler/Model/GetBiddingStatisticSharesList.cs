using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetBiddingStatisticSharesList
    {
    }

    public class GetBiddingStatisticSharesListRequest
    {
        /// <summary>
        /// 1.自定义 2系统
        /// </summary>
        public int FromType { get; set; }
    }

    public class GetBiddingStatisticSharesListRes
    {
        /// <summary>
        /// 统计天数
        /// </summary>
        public int StatisticDays { get; set; }

        public List<BiddingStatisticInfo> List { get; set; }
    }

    public class BiddingStatisticInfo
    {
        public DateTime Date { get; set; }

        public string DateStr 
        {
            get 
            {
                return Date.ToString("yyyy-MM-dd");
            }
        }

        public List<BiddingStatisticSharesInfo> SharesList { get; set; }
    }

    public class BiddingStatisticSharesInfo
    {
        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

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
        /// 当前涨跌幅
        /// </summary>
        public int CurrRiseRate { get; set; }

        /// <summary>
        /// 当前涨跌额
        /// </summary>
        public long CurrRiseAmount { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenPrice { get; set; }

        /// <summary>
        /// 开盘涨跌幅
        /// </summary>
        public int OpenRiseRate
        {
            get
            {
                if (ClosedPrice == 0 || OpenPrice == 0)
                {
                    return 0;
                }
                return (int)Math.Round((OpenPrice - ClosedPrice) * 1.0 / ClosedPrice * 10000, 0);
            }
        }

        /// <summary>
        /// 开盘涨跌额
        /// </summary>
        public long OpenRiseAmount
        {
            get
            {
                if (ClosedPrice == 0 || OpenPrice == 0)
                {
                    return 0;
                }
                return OpenPrice - ClosedPrice;
            }
        }

        /// <summary>
        /// 最高价
        /// </summary>
        public long MaxPrice { get; set; }

        /// <summary>
        /// 最高涨跌幅
        /// </summary>
        public int MaxRiseRate
        {
            get
            {
                if (ClosedPrice == 0 || MaxPrice == 0)
                {
                    return 0;
                }
                return (int)Math.Round((MaxPrice - ClosedPrice) * 1.0 / ClosedPrice * 10000, 0);
            }
        }

        /// <summary>
        /// 最高涨跌额
        /// </summary>
        public long MaxRiseAmount
        {
            get
            {
                if (ClosedPrice == 0 || MaxPrice == 0)
                {
                    return 0;
                }
                return MaxPrice - ClosedPrice;
            }
        }

        /// <summary>
        /// 最低价
        /// </summary>
        public long MinPrice { get; set; }

        /// <summary>
        /// 最低涨跌幅
        /// </summary>
        public int MinRiseRate
        {
            get
            {
                if (ClosedPrice == 0 || MinPrice == 0)
                {
                    return 0;
                }
                return (int)Math.Round((MinPrice - ClosedPrice) * 1.0 / ClosedPrice * 10000, 0);
            }
        }

        /// <summary>
        /// 最低涨跌额
        /// </summary>
        public long MinRiseAmount
        {
            get
            {
                if (ClosedPrice == 0 || MinPrice == 0)
                {
                    return 0;
                }
                return MinPrice - ClosedPrice;
            }
        }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 炸板次数
        /// </summary>
        public int LimitUpBombCount { get; set; }

        /// <summary>
        /// 是否一字
        /// </summary>
        public bool IsLimitUpLikeOne { get; set; }

        /// <summary>
        /// 是否T字
        /// </summary>
        public bool IsLimitUpLikeT { get; set; }

        /// <summary>
        /// 是否反包
        /// </summary>
        public bool IsReverse { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 涨停价
        /// </summary>
        public long LimitUpPrice { get; set; }

        /// <summary>
        /// 跌停价
        /// </summary>
        public long LimitDownPrice { get; set; }

        /// <summary>
        /// 竞价涨跌幅
        /// </summary>
        public int BiddingRiseRate { get; set; }

        /// <summary>
        /// 竞价成交额
        /// </summary>
        public long BiddingTotalAmount { get; set; }

        /// <summary>
        /// 竞价成交量
        /// </summary>
        public long BiddingTotalCount { get; set; }

        /// <summary>
        /// 竞价量比
        /// </summary>
        public int BiddingCountRate { get; set; }

        /// <summary>
        /// 竞价成交比
        /// </summary>
        public int BiddingAmountRate { get; set; }

        /// <summary>
        /// 昨日换手率
        /// </summary>
        public int HandsRateYes { get; set; }

        /// <summary>
        /// 今日竞价买一量
        /// </summary>
        public int BuyCount1Today { get; set; }

        /// <summary>
        /// 昨日竞价买一量
        /// </summary>
        public int BuyCount1Yes { get; set; }

        public int PriceTypeToday { get; set; }

        public int PriceTypeYes { get; set; }

        /// <summary>
        /// 综合涨跌幅
        /// </summary>
        public int OvallRiseRate { get; set; }

        /// <summary>
        /// 综合涨跌幅排名
        /// </summary>
        public int OverallRank { get; set; }

        /// <summary>
        /// 预期量比
        /// </summary>
        public int RateExpect { get; set; }

        /// <summary>
        /// 当前量比
        /// </summary>
        public int RateNow { get; set; }

        /// <summary>
        /// 题材背景色
        /// </summary>
        public string BgColor { get; set; }

        /// <summary>
        /// 所属搜索模板信息
        /// </summary>
        public List<BiddingKlineTemplateInfo> TemplateList { get; set; }
        public DateTime Date { get; set; }

        public int PriceType { get; set; }
    }

    public class BiddingStatisticSource
    {
        public DateTime Date { get; set; }

        public long SharesKey { get; set; }

        public string ContextData { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
