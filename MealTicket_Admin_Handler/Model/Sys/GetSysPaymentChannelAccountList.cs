using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSysPaymentChannelAccountList
    {
    }

    public class GetSysPaymentChannelAccountListRequest : PageRequest
    {
        /// <summary>
        /// 【必填】支付渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 【必填】业务code
        /// </summary>
        public string BusinessCode { get; set; }
    }

    public class SysPaymentChannelAccountInfo
    {
        /// <summary>
        /// 渠道账户Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 渠道账户名称
        /// </summary>
        public string Name { get; set; }

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
