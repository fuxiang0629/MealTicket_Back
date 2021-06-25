using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddAccountAutoBuySyncroSetting
    {
    }

    public class AddAccountAutoBuySyncroSettingRequest
    {
        /// <summary>
        /// 操作账户Id
        /// </summary>
        public long MainAccountId { get; set; }

        /// <summary>
        /// 购买金额
        /// </summary>
        public long BuyAmount { get; set; }

        /// <summary>
        /// 跟投类型1分组 0指定用户
        /// </summary>
        public int FollowType { get; set; }

        /// <summary>
        /// 跟投账户列表
        /// </summary>
        public List<long> FollowAccountList { get; set; }

        /// <summary>
        /// 分组列表
        /// </summary>
        public List<long> GroupList { get; set; }
    }
}
