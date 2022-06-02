﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BatchGetSharesBiddingKlineBaseInfo
    {
    }

    public class BatchGetSharesBiddingKlineBaseInfoRequest:PageRequest
    {
        /// <summary>
        /// 股票列表
        /// </summary>
        public List<long> SharesKeyList { get; set; }

        /// <summary>
        /// 模板Id列表
        /// </summary>
        public List<long> TemplateIdList { get; set; }

        /// <summary>
        /// 是否全市场股票
        /// </summary>
        public bool IsGetAll { get; set; }

        /// <summary>
        /// 1竞价金额/2金额占比（默认）/3竞价成交量/4成交量比
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// 是否降序
        /// </summary>
        public bool OrderModel { get; set; }
    }

    public class SharesBiddingKlineBaseInfo
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
        /// 当前涨跌额
        /// </summary>
        public long CurrRiseAmount { get; set; }

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

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }

        /// <summary>
        /// 板块排名
        /// </summary>
        public List<PlateSharesRank> PlateSharesRank { get; set; }
    }

    public class BiddingKlineTemplateInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string BgColor { get; set; }

        public string TemplateContext { get; set; }
    }
}
