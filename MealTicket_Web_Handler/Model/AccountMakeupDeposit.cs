using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AccountMakeupDeposit
    {
    }

    public class AccountMakeupDepositRequest
    {
        /// <summary>
        /// 持仓Id
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        /// 补交保证金数量（元后4位小数）
        /// </summary>
        public long Deposit { get; set; }

        /// <summary>
        /// 跟投人员列表
        /// </summary>
        public List<long> FollowList { get; set; }
    }
}
