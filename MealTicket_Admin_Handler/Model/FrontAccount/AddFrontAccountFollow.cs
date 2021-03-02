using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddFrontAccountFollow
    {
    }

    public class AddFrontAccountFollowRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 跟投用户Id
        /// </summary>
        public long FollowAccountId { get; set; }

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
