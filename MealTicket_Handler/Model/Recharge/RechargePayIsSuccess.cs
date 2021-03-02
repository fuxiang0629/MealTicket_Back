using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class RechargePayIsSuccess
    {
    }

    public class RechargePayIsSuccessRequest
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderSn { get; set; }
    }

    public class PayIsSuccess
    {
        /// <summary>
        /// 支付状态 0等待支付 1支付成功 2支付失败
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusRemark { get; set; }
    }
}
