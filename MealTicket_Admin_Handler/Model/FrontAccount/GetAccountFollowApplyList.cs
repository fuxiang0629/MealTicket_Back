using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetAccountFollowApplyList
    {
    }

    public class GetAccountFollowApplyListRequest:PageRequest
    {
        /// <summary>
        /// 状态 -2已处理 -1未处理 0全部 1申请中 2处理中 3已完成 4拒绝申请 5用户主动取消申请
        /// </summary>
        public int Status { get; set; }
    }

    public class AccountFollowApplyInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 操盘用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long FollowAccountId { get; set; }

        /// <summary>
        /// 用户Mobile
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDesc { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime? HandlerTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? FinishTime { get; set; }

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
