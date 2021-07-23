using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountFollowGroupFollowAccountList
    {
    }

    public class AccountFollowGroupFollowAccountInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 跟投Id
        /// </summary>
        public long FollowId { get; set; }

        /// <summary>
        /// 跟投账户Id
        /// </summary>
        public long FollowAccountId { get; set; }

        /// <summary>
        /// 跟投人员手机号
        /// </summary>
        public string FollowAccountMobile { get; set; }

        /// <summary>
        /// 跟投人员名称
        /// </summary>
        public string FollowAccountName { get; set; }

        /// <summary>
        /// 买入金额
        /// </summary>
        public long BuyAmount { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最大可用余额
        /// </summary>
        public long MaxDeposit { get; set; }

        /// <summary>
        /// 最大跟投倍数
        /// </summary>
        public long MaxFundMultiple { get; set; }
    }
}
