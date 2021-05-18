using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSharesLimitAutoBuy
    {
    }

    public class AddSharesLimitAutoBuyRequest
    {
        /// <summary>
        /// 市场0深圳 1上海 -1全部
        /// </summary>
        public int LimitMarket { get; set; }

        /// <summary>
        /// 限制股票code
        /// </summary>
        public string LimitKey { get; set; }

        /// <summary>
        /// 最大可买账户数量
        /// </summary>
        public int MaxBuyCount { get; set; }
    }
}
