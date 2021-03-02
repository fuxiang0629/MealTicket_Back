using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class CashApply
    {
    }

    public class CashApplyRequest
    {
        /// <summary>
        /// 银行卡Id
        /// </summary>
        public long BankCardId { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 提现方式1退款优先 2转账优先 3无要求
        /// </summary>
        public int CashType { get; set; }

        /// <summary>
        /// 交易密码（MD5 32位小写）
        /// </summary>
        public string TransactionPassword { get; set; }
    }
}
