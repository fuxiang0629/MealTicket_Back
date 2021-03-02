using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesForbid
    {
    }

    public class ModifySharesForbidRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 限制类型1股票代码 2股票名称
        /// </summary>
        public int LimitType { get; set; }

        /// <summary>
        /// 限制关键字
        /// </summary>
        public string LimitKey { get; set; }

        /// <summary>
        /// 限制市场代码-1全部市场 0深圳 1上海 
        /// </summary>
        public int LimitMarket { get; set; }

        /// <summary>
        /// 禁止类型1禁止买入 2禁止卖出 3禁止交易
        /// </summary>
        public int ForbidType { get; set; }
    }
}
