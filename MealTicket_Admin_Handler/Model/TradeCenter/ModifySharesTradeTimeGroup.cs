using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesTradeTimeGroup
    {
    }

    public class ModifySharesTradeTimeGroupRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 市场名称
        /// </summary>
        public string MarketName { get; set; }

        /// <summary>
        /// 限制市场代码-1全部市场 0深圳 1上海 
        /// </summary>
        public int LimitMarket { get; set; }

        /// <summary>
        /// 股票匹配关键字
        /// </summary>
        public string LimitKey { get; set; }
    }
}
