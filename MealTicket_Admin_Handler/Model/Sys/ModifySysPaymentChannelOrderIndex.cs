using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySysPaymentChannelOrderIndex
    {
    }

    public class ModifySysPaymentChannelOrderIndexRequest
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
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
