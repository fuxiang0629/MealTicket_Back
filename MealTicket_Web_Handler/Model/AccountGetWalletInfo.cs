using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AccountGetWalletInfo
    {
    }

    public class AccountWalletInfo
    {
        /// <summary>
        /// 保证金余额
        /// </summary>
        public long DepositAmount { get; set; }

        /// <summary>
        /// 保留余额
        /// </summary>
        public long RemainDeposit { get; set; }
    }
}
