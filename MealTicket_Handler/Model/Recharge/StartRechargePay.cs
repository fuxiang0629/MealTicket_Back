using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class StartRechargePay
    {
    }

    public class StartRechargePayRequest
    {
        /// <summary>
        /// 渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 充值金额
        /// </summary>
        public long Amount { get; set; }
    }
}
