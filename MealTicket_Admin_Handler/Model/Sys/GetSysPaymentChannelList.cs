using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSysPaymentChannelList
    {
    }

    public class GetSysPaymentChannelListRequest:PageRequest
    {
        /// <summary>
        /// 支付渠道
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int PayType { get; set; }
    }

    public class SysPaymentChannelInfo
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
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 支付类型1.线上支付 2线下支付
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 是否支持原路退款
        /// </summary>
        public bool SupportRefund { get; set; }

        /// <summary>
        /// 支付账户数量
        /// </summary>
        public int PaymentAccountCount { get; set; }

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
