using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSysPaymentAccount
    {
    }

    public class AddSysPaymentAccountRequest
    {
        /// <summary>
        /// 【必填】支付渠道Code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 【必填】账户名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 【支付宝和微信必填】appId
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 【支付宝和微信必填】sellerId
        /// </summary>
        public string SellerId { get; set; }
    }
}
