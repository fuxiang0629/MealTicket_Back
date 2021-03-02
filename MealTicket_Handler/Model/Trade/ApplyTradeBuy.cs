using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class ApplyTradeBuy
    {
    }

    public class ApplyTradeBuyRequest
    {
        /// <summary>
        /// 市场代码0深圳 1上海
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 投入保证金金额
        /// </summary>
        public long BuyAmount { get; set; }

        /// <summary>
        /// 杠杆倍数
        /// </summary>
        public int FundMultiple { get; set; }

        /// <summary>
        /// 买入价格
        /// </summary>
        public long BuyPrice { get; set; }

        /// <summary>
        /// 自动平仓时间（不设置为null）
        /// </summary>
        public DateTime? ClosingTime { get; set; }
    }
}
