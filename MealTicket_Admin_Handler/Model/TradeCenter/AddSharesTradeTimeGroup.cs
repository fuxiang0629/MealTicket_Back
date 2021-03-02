using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSharesTradeTimeGroup
    {
    }

    public class AddSharesTradeTimeGroupRequest
    {
        /// <summary>
        /// 市场名称
        /// </summary>
        public string MarketName { get; set; }

        /// <summary>
        /// 市场代码0深圳 1上海 
        /// </summary>
        public int LimitMarket { get; set; }

        /// <summary>
        /// 股票匹配关键字
        /// </summary>
        public string LimitKey { get; set; }
    }
}
