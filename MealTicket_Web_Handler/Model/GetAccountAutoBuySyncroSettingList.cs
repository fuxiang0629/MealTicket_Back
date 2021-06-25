using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountAutoBuySyncroSettingList
    {
    }

    public class AccountAutoBuySyncroSettingInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 操作账户Id
        /// </summary>
        public long MainAccountId { get; set; }

        /// <summary>
        /// 操作账户昵称
        /// </summary>
        public string MainAccountName { get; set; }

        /// <summary>
        /// 操作账户手机号
        /// </summary>
        public string MainAccountMobile { get; set; }

        /// <summary>
        /// 购买金额
        /// </summary>
        public long BuyAmount { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 跟投类型1分组 0指定用户
        /// </summary>
        public int FollowType { get; set; }

        /// <summary>
        /// 跟投账户列表
        /// </summary>
        public List<long> FollowList { get; set; }

        /// <summary>
        /// 分组列表
        /// </summary>
        public List<long> GroupList { get; set; }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
