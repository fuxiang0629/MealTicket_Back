using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetRechargeRecordList
    {
    }

    public class GetRechargeRecordListRequest : PageRequest
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderSn { get; set; }

        /// <summary>
        /// 账户昵称/手机号
        /// </summary>
        public string AccountInfo { get; set; }

        /// <summary>
        /// 状态0全部1.支付中 2.支付关闭 3,已支付 4.成功充值 5.支付关闭导致退款 6.管理员后台退款 7.退款中 8退款成功
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public class RechargeRecordInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderSN { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 支付渠道名称
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// 收款账户名称
        /// </summary>
        public string PaymentAccountName { get; set; }

        /// <summary>
        /// 充值金额
        /// </summary>
        public long RechargeAmount { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public long PayAmount { get; set; }

        /// <summary>
        /// 支付状态1.支付中 2.支付关闭 3,已支付 4.成功充值 5.支付关闭导致退款 7.退款中 8退款成功
        /// </summary>
        public int PayStatus { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
