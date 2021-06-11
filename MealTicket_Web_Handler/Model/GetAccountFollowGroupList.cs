using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountFollowGroupList
    {
    }

    public class GetAccountFollowGroupListRequest : PageRequest
    {
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
    }

    public class AccountFollowGroupInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 跟投账户列表
        /// </summary>
        public List<AccountFollowGroupFollowAccountInfo> FollowAccountList { get; set; }

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
