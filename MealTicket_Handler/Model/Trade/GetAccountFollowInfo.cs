using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetAccountFollowInfo
    {
    }

    public class AccountFollowInfo
    {
        /// <summary>
        /// 跟投状态0未跟投 1申请中 2处理中 3已完成 4拒绝
        /// </summary>
        public int FollowStatus { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDesc { get; set; }
    }
}
