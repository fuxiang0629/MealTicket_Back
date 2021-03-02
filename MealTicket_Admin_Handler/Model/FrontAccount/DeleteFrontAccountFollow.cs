using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class DeleteFrontAccountFollow
    {
    }

    public class DeleteFrontAccountFollowRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 跟投用户Id
        /// </summary>
        public long FollowAccountId { get; set; }
    }
}
