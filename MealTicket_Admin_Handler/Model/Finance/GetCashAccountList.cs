using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetCashAccountList
    {
    }

    public class GetCashAccountListRequest
    {
        /// <summary>
        /// 【必填】渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 【必填】打款方式1.退款 2转账
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 提现记录Id（Type为1时必填）
        /// </summary>
        public long CashRecordId { get; set; }
    }

    public class CashAccountInfo
    {
        /// <summary>
        /// 打款账户Id
        /// </summary>
        public long PaymentAccountId { get; set; }

        /// <summary>
        /// 打款账户名称
        /// </summary>
        public string PaymentAccountName { get; set; }

        /// <summary>
        /// 最多打款金额
        /// </summary>
        public long MaxAmount { get; set; }
    }
}
