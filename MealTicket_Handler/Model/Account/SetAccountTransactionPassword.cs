using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class SetAccountTransactionPassword
    {
    }

    public class AccountSetTransactionPasswordRequest
    {
        /// <summary>
        /// 交易密码（MD5 32位小写）
        /// </summary>
        public string TransactionPassword { get; set; }
    }
}
