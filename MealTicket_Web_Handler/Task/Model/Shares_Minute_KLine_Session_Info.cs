using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Minute_KLine_Session_Info
    {
        /// <summary>
        /// 股票市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 上一分钟收盘价
        /// </summary>
        public long PreClosePrice { get; set; }
    }
}
