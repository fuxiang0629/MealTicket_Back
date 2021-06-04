using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyAccountFollowGroupFollowAccount
    {
    }

    public class ModifyAccountFollowGroupFollowAccountRequest
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
        /// 买入金额
        /// </summary>
        public long BuyAmount { get; set; }
    }
}
