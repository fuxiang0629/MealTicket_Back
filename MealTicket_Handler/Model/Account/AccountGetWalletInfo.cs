using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
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
    }
}
