using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class AccountModifyTransactionPassword
    {
    }

    public class AccountModifyTransactionPasswordRequest
    {

        /// <summary>
        /// 旧交易密码（MD5 32位小写）
        /// </summary>
        public string OldTransactionPassword { get; set; }

        /// <summary>
        /// 新交易密码（MD5 32位小写）
        /// </summary>
        public string NewTransactionPassword { get; set; }
    }
}
