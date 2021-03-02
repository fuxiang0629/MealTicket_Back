using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class MakeupDeposit
    {
    }

    public class MakeupDepositRequest
    {
        /// <summary>
        /// 持仓Id
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        /// 补交保证金数量（元后4位小数）
        /// </summary>
        public long Deposit { get; set; }
    }
}
