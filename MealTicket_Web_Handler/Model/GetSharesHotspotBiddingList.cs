using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesHotspotBiddingList
    {
    }

    public class GetSharesHotspotBiddingListRequest
    {
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 题材Id列表
        /// </summary>
        public List<long> HotspotIdList { get; set; }
    }

    public class GetSharesPlateBiddingListRequest
    {
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<long> PlateList { get; set; }
    }

    public class SharesBidding
    {
        public long SharesKey { get; set; }

        public int Market { get; set; }

        public string SharesCode { get; set; }

        public string SharesName { get; set; }

        /// <summary>
        /// 当前涨跌幅
        /// </summary>
        public int CurrRiseRate { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenPrice { get; set; }

        /// <summary>
        /// 开盘涨跌幅
        /// </summary>
        public int OpenRiseRate { get; set; }

        /// <summary>
        /// 开盘涨跌额
        /// </summary>
        public long OpenRiseAmount { get; set; }

        /// <summary>
        /// 当前涨跌额
        /// </summary>
        public long CurrRiseAmount { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        public long MaxPrice { get; set; }

        public long MinPrice { get; set; }

        public long LimitUpPrice { get; set; }

        public long LimitDownPrice { get; set; }

        /// <summary>
        /// 今日竞价买一量
        /// </summary>
        public int BuyCount1Today { get; set; }

        /// <summary>
        /// 昨日竞价买一量
        /// </summary>
        public int BuyCount1Yes { get; set; }

        /// <summary>
        /// 今日竞价涨停类型
        /// </summary>
        public int PriceTypeToday { get; set; }

        public int PriceTypeYes { get; set; }

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
        /// 竞价涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public long TotalAmount { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 量比
        /// </summary>
        public int CountRate { get; set; }

        /// <summary>
        /// 成交额比
        /// </summary>
        public int AmountRate { get; set; }

        /// <summary>
        /// 所属题材Id
        /// </summary>
        public long HotspotId { get; set; }

        public string BgColor { get; set; }

        public int RiseLimitCount { get; set; }

        public int FilterRiseLimitCount { get; set; }

        public bool IsLimitUpToday { get; set; }

        /// <summary>
        /// 昨日换手率
        /// </summary>
        public int HandRateYes { get; set; }

        /// <summary>
        /// 收盘价涨停类型
        /// </summary>
        public int PriceType { get; set; }

        public long AllTodayTotalCount { get; set; }

        public long AllPreTotalCount { get; set; }

        public int RateNow 
        {
            get 
            {
                if (AllPreTotalCount == 0)
                {
                    return 0;
                }
                return (int)(AllTodayTotalCount * 1.0 / AllPreTotalCount*100);
            }
        }
    }

    public class GetSharesBiddingListRequest
    {
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 类型1按板块 2按题材
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public List<long> Id { get; set; }

        /// <summary>
        /// 是否联动板块
        /// </summary>
        public bool PlateLinkage { get; set; }
    }

    public class SharesHotspotBidding
    {
        /// <summary>
        /// 题材Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 题材名称
        /// </summary>
        public string Name { get; set; }

        public int CalType { get; set; }

        /// <summary>
        /// 当前涨跌幅
        /// </summary>
        public int CurrRiseRate { get; set; }

        /// <summary>
        /// 涨跌幅排名
        /// </summary>
        public int RiseRateRank { get; set; }

        /// <summary>
        /// 昨日涨停表现
        /// </summary>
        public int RiseRateYestoday { get; set; }

        /// <summary>
        /// 竞价涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public long TotalAmount { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 量比
        /// </summary>
        public int CountRate { get; set; }

        /// <summary>
        /// 成交额比
        /// </summary>
        public int AmountRate { get; set; }

        /// <summary>
        /// 联动板块
        /// </summary>
        public List<long> PlateLinkageList { get; set; }
    }
}
