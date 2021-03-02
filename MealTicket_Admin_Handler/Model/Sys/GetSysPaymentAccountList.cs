using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSysPaymentAccountList
    {
    }

    public class GetSysPaymentAccountListRequest : PageRequest 
    {
        /// <summary>
        /// 支付渠道Code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 业务code
        /// </summary>
        public string BusinessCode { get; set; }

        /// <summary>
        /// 类型0.不选中 1.支付列表选中 2.打款列表退款选中 3.打款列表转账选中
        /// </summary>
        public int Type { get; set; }


    }

    public class SysPaymentAccountInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 支付渠道Code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 账户名称
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

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsChecked { get; set; }
    }
}
