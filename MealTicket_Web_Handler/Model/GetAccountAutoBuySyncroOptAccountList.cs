using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountAutoBuySyncroOptAccountList
    {
    }

    public class GetAccountAutoBuySyncroOptAccountListRequest : PageRequest
    {
        /// <summary>
        /// 设置Id
        /// </summary>
        public long SettingId { get; set; }
    }

    public class AccountAutoBuySyncroOptAccountInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 操作者账户
        /// </summary>
        public string OptMobile { get; set; }

        /// <summary>
        /// 操作者昵称
        /// </summary>
        public string OptNickName { get; set; }

        /// <summary>
        /// 状态1申请被操作 2已执行被操作
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
