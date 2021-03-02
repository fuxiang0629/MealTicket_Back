using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class StartCash
    {
    }

    public class StartCashRequest
    {
        /// <summary>
        /// 提现id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 打款渠道
        /// </summary>
        public string PaymentChannel { get; set; }

        /// <summary>
        /// 打款方式1.退款 2转账
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 打款账户
        /// </summary>
        public long PaymentAccountId { get; set; }

        /// <summary>
        /// 打款金额
        /// </summary>
        public long ApplyAmount { get; set; }

        /// <summary>
        /// 打款凭证
        /// </summary>
        public string VoucherImg { get; set; }
    }
}
