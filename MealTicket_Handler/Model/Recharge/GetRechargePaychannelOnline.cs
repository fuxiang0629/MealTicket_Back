using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetRechargePaychannelOnline
    {
    }

    public class RechargePaychannelOnlineInfo
    {
        /// <summary>
        /// 支付渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 业务code
        /// </summary>
        public string BusinessCode { get; set; }

        /// <summary>
        /// 渠道名称
        /// </summary>
        public string ChannelName { get; set; }
    }
}
