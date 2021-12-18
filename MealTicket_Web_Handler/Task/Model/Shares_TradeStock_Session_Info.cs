using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    [Serializable]
    public class Shares_TradeStock_Session_Info
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
        /// 昨日总成交量
        /// </summary>
        public long TradeStock_Yestoday { get; set; }

        /// <summary>
        /// 昨日此刻成交量
        /// </summary>
        public long TradeStock_Now { get; set; }

        /// <summary>
        /// 此刻时间
        /// </summary>
        public long GroupTimeKey { get; set; }

        /// <summary>
        /// 今日总成交量
        /// </summary>
        public long TradeStock { get; set; }

        /// <summary>
        /// 5分钟内成交量
        /// </summary>
        public long TradeStock_Interval { get; set; }

        /// <summary>
        /// 5分钟内成交量真实分钟数
        /// </summary>
        public int TradeStock_Interval_Count { get; set; }
    }
}
