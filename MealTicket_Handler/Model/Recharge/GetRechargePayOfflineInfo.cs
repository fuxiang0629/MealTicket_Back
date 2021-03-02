using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetRechargePayOfflineInfo
    {
    }

    public class RechargePayOflineInfo 
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
        /// 账户名称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// 开户行
        /// </summary>
        public string Bank { get; set; }

        /// <summary>
        /// 客服电话
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 其他信息
        /// </summary>
        public string Other { get; set; }
    }
}
