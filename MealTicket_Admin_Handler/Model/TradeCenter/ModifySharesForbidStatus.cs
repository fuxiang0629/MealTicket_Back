using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesForbidStatus
    {
    }

    public class ModifySharesForbidStatusRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 禁止状态 0不禁 1禁止买入 2禁止卖出 3禁止交易
        /// </summary>
        public int ForbidStatus { get; set; }
    }
}
