using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSysPaymentCashChannelList
    {
    }

    public class GetSysPaymentCashChannelListRequest:PageRequest
    {
        /// <summary>
        /// 支付渠道
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }
    }

    public class SysPaymentCashChannelInfo
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

        /// <summary>
        /// 业务名称
        /// </summary>
        public string BusinessName { get; set; }

        /// <summary>
        /// 渠道描述
        /// </summary>
        public string ChannelDescription { get; set; }

        /// <summary>
        /// 支付类型1.线上支付 2线下支付
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 退款账户数量
        /// </summary>
        public int PaymentCashRefundAccountCount { get; set; }

        /// <summary>
        /// 转账账户数量
        /// </summary>
        public int PaymentCashTransferAccountCount { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
