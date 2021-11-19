﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetShareQuotesInfo
    {
    }

    public class GetShareQuotesInfoRequest
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }
    }

    public class ShareQuotesInfo
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
    }
}
