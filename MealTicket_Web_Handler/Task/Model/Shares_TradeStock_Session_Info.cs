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
        public long SharesKey { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 昨日此刻成交量
        /// </summary>
        public long TradeStock_Yes { get; set; }

        /// <summary>
        /// 此刻成交量
        /// </summary>
        public long TradeStock_Now { get; set; }

        /// <summary>
        /// 每分钟内成交量
        /// </summary>
        public long TradeStock_Avg { get; set; }

        /// <summary>
        /// 此刻时间
        /// </summary>
        public long TimeSpan { get; set; }
    }
}
