using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountAutoBuySyncroOptAccountRelList
    {
    }

    public class GetAccountAutoBuySyncroOptAccountRelListRequest
    {
        public long SettingId { get; set; }
    }

    public class AccountAutoBuySyncroOptAccountRelInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string AccountNickName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
    }
}
