using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BindSysPaymentCashChannelTransferAccount
    {
    }

    public class BindSysPaymentCashChannelTransferAccountRequest
    {
        /// <summary>
        /// 支付渠道Code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 业务code
        /// </summary>
        public string BusinessCode { get; set; }

        /// <summary>
        /// 支付账户Id列表
        /// </summary>
        public List<long> PaymentAccountIdList { get; set; }
    }
}
