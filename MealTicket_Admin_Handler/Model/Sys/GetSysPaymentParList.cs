using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSysPaymentParList
    {
    }

    public class GetSysPaymentParListRequest
    {
        /// <summary>
        /// 支付账户Id
        /// </summary>
        public long PaymentAccountId { get; set; }
    }

    public class SysPaymentParInfo
    {
        /// <summary>
        /// 参数名
        /// </summary>
        public string ParName { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string ParValue { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string ParDescription { get; set; }

        /// <summary>
        /// 数据最后更新时间
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}
