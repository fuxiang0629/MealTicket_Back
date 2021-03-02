using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    class AccountApplyTradeCancel
    {
    }

    public class AccountApplyTradeCancelRequest
    {
        /// <summary>
        /// 委托Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 跟投人员列表
        /// </summary>
        public List<long> FollowList { get; set; }
    }
}
