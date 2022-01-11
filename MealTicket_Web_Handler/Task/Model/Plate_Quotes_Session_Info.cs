using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Quotes_Session_Info
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

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
        /// 真实天数（计算板块排名用）
        /// </summary>
        public int RealDays { get; set; }

        /// <summary>
        /// 板块排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 涨停数量
        /// </summary>
        public int RiseUpLimitCount { get; set; }

        /// <summary>
        /// 跌停数量
        /// </summary>
        public int DownUpLimitCount { get; set; }

        /// <summary>
        /// 股票总数量
        /// </summary>
        public int SharesTotalCount { get; set; }

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

    public class Plate_Quotes_Session_Info_Obj
    {
        /// <summary>
        /// 缓存天数
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// 缓存
        /// </summary>
        public Dictionary<long, Dictionary<DateTime, Plate_Quotes_Session_Info>> SessionDic { get; set; }
    }
}
