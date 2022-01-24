using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Quotes_Session_Info:IComparable<Shares_Quotes_Session_Info>
    {
        /// <summary>
        /// 排序类型0时间 1.开盘价 2收盘价 3最高价 4最低价
        /// </summary>
        public int SortedType { get; set; }


        public int CompareTo(Shares_Quotes_Session_Info other)
        {
            if (SortedType == 1)
            {
                return other.OpenedPrice.CompareTo(this.OpenedPrice) > 0 ? 1 : -1;
            }
            else if (SortedType == 2)
            {
                return other.ClosedPrice.CompareTo(this.ClosedPrice) > 0 ? 1 : -1;
            }
            else if (SortedType == 3)
            {
                return other.MaxPrice.CompareTo(this.MaxPrice) > 0 ? 1 : -1;
            }
            else if (SortedType == 4)
            {
                return other.MinPrice.CompareTo(this.MinPrice) > 0 ? 1 : -1;
            }
            else 
            {
                return other.Date.CompareTo(this.Date);
            }
        }

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
                return (int)Math.Round((ClosedPrice - YestodayClosedPrice) * 1.0 / YestodayClosedPrice * 10000,0);
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

        /// <summary>
        /// 是否炸板
        /// </summary>
        public bool IsLimitUpBomb 
        { 
            get
            {
                if (LimitUpCount > 0 && PriceType != 1)
                {
                    return true;
                }
                return false;
            } 
        }
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

        /// <summary>
        /// 开盘价缓存
        /// </summary>
        public Dictionary<long, SortedSet<Shares_Quotes_Session_Info>> OpenSessionDic { get; set; }

        /// <summary>
        /// 收盘价缓存
        /// </summary>
        public Dictionary<long, SortedSet<Shares_Quotes_Session_Info>> CloseSessionDic { get; set; }

        /// <summary>
        /// 最高价缓存
        /// </summary>
        public Dictionary<long, SortedSet<Shares_Quotes_Session_Info>> MaxSessionDic { get; set; }

        /// <summary>
        /// 最低缓存
        /// </summary>
        public Dictionary<long, SortedSet<Shares_Quotes_Session_Info>> MinSessionDic { get; set; }
    }
}
