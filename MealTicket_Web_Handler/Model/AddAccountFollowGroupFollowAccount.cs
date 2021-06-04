using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddAccountFollowGroupFollowAccount
    {
    }

    public class AddAccountFollowGroupFollowAccountRequest
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

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
