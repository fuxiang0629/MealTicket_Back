using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountSharesGroupShareActiveList
    {
    }

    public class AccountSharesGroupShareActive
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 共享给某个账户的Id
        /// </summary>
        public long ShareAccountId { get; set; }

        /// <summary>
        /// 分享给某个账户的手机号
        /// </summary>
        public string ShareAccountMobile { get; set; }

        /// <summary>
        /// 共享分组列表
        /// </summary>
        public List<AccountSharesGroupInfo> ShareGroupList { get; set; }

        /// <summary>
        /// 共享时间
        /// </summary>
        public DateTime ShareTime { get; set; }
    }

    public class AccountSharesGroupInfo
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }
    }
}
