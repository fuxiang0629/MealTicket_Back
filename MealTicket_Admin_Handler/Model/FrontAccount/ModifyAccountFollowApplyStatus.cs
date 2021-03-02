using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyAccountFollowApplyStatus
    {
    }

    public class ModifyAccountFollowApplyStatusRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 2开始处理 3完成 4拒绝申请
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDesc { get; set; }

        /// <summary>
        /// 操盘用户Id
        /// </summary>
        public long MainAccountId { get; set; }

        /// <summary>
        /// 提现状态
        /// </summary>
        public int CashStatus { get; set; }

        /// <summary>
        /// 交易状态
        /// </summary>
        public int ForbidStatus { get; set; }
    }
}
