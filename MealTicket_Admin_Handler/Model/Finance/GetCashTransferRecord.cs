using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetCashTransferRecord
    {
    }

    public class CashTransferRecord
    {
        /// <summary>
        /// 提现Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 总打款金额
        /// </summary>
        public long TotalAmount { get; set; }

        /// <summary>
        /// 总服务费
        /// </summary>
        public long TotalServiceFee { get; set; }

        /// <summary>
        /// 打款类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 提现订单号
        /// </summary>
        public string CashOrderSn { get; set; }

        /// <summary>
        /// 打款次数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 最后打款时间
        /// </summary>
        public DateTime LastTime { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        public long ApplyAmount { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// 转账方式
        /// </summary>
        public string TransferType { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string AccountMobile { get; set; }
    }
}
