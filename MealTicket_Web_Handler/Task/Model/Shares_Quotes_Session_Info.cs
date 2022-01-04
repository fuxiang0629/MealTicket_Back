﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Quotes_Session_Info
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
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 收盘价格
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 昨日收盘价格
        /// </summary>
        public long YestodayClosedPrice { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int RiseRate
        {
            get
            {
                if (YestodayClosedPrice == 0)
                {
                    return 0;
                }
                return (int)((ClosedPrice - YestodayClosedPrice) * 1.0 / YestodayClosedPrice * 10000 + 0.5);
            }
        }

        /// <summary>
        /// 是否涨停
        /// </summary>
        public bool IsLimitUp { get; set; }

        /// <summary>
        /// 涨停时间
        /// </summary>
        public DateTime? LimitUpTime { get; set; }

        /// <summary>
        /// 开盘价格
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 涨停价格
        /// </summary>
        public long LimitUpPrice { get; set; }

        /// <summary>
        /// 跌停价格
        /// </summary>
        public long LimitDownPrice { get; set; }

        /// <summary>
        /// 成交金额
        /// </summary>
        public long TotalAmount { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 涨停次数
        /// </summary>
        public int LimitUpCount { get; set; }

        /// <summary>
        /// 触发涨停次数
        /// </summary>
        public int TriLimitUpCount { get; set; }

        /// <summary>
        /// 跌停次数
        /// </summary>
        public int LimitDownCount { get; set; }

        /// <summary>
        /// 触发跌停次数
        /// </summary>
        public int TriLimitDownCount { get; set; }

        /// <summary>
        /// 炸板次数
        /// </summary>
        public int LimitUpBombCount { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public long MaxPrice { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public long MinPrice { get; set; }

        /// <summary>
        /// 触发涨停
        /// </summary>
        public int TriPriceType { get; set; }

        /// <summary>
        /// 涨停
        /// </summary>
        public int PriceType { get; set; }
    }

    public class Shares_Quotes_Session_Info_Last
    {
        public Shares_Quotes_Session_Info shares_quotes_info { get; set; }

        /// <summary>
        /// 今日此刻每分钟平均成交量
        /// </summary>
        public long TotalCount_Today_Now { get; set; }

        /// <summary>
        /// 昨日每分钟平均成交量
        /// </summary>
        public long TotalCount_Yestoday_Now { get; set; }

        /// <summary>
        /// 此刻量比
        /// </summary>
        public int RateNow 
        {
            get 
            {
                if (TotalCount_Yestoday_Now == 0)
                {
                    return 0;
                }
                return (int)Math.Round(TotalCount_Today_Now * 1.0 / TotalCount_Yestoday_Now * 100, 0);
            } 
        }

        /// <summary>
        /// 昨日总成交量
        /// </summary>
        public long TotalCount_Yestoday_All { get; set; }

        /// <summary>
        /// 预计今日成交量
        /// </summary>
        public long TotalCount_Today_Expect { get; set; }

        /// <summary>
        /// 预计量比
        /// </summary>
        public int RateExpect
        {
            get
            {
                if (TotalCount_Yestoday_All == 0)
                {
                    return 0;
                }
                return (int)Math.Round(TotalCount_Today_Expect * 1.0 / TotalCount_Yestoday_All * 100, 0);
            }
        }
    }

    public class Shares_Quotes_Session_Info_Obj 
    {
        /// <summary>
        /// 缓存天数
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// 缓存
        /// </summary>
        public Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> SessionDic { get; set; }
    }
}
