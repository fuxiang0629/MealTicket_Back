using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetFollowAccountList
    {
    }

    public class FollowAccountInfo
    {
        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 账户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 账户昵称
        /// </summary>
        public string AccountName { get; set; }
    }
}
