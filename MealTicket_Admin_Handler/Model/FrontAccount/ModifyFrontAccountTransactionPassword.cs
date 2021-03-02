using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyFrontAccountTransactionPassword
    {
    }

    public class ModifyFrontAccountTransactionPasswordRequest
    {
        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 交易密码（MD5 32位小写）
        /// </summary>
        public string TransactionPassword { get; set; }
    }
}
