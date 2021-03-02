using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    class GetPaymentList
    {
    }

    public class GetPaymentListRequest
    {
        /// <summary>
        /// 业务code
        /// </summary>
        public string BusinessCode { get; set; }
    }

    public class PaymentInfo
    {
        /// <summary>
        /// 支付渠道Code
        /// </summary>
        public string ChannelCode { get; set; }
    }
}
