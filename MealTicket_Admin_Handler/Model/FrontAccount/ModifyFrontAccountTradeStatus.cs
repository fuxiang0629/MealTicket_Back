using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyFrontAccountTradeStatus
    {
    }

    public class ModifyFrontAccountTradeStatusRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 交易状态0正常 1禁止买入 2禁止卖出 3禁止交易
        /// </summary>
        public int ForbidStatus { get; set; }
    }
}
