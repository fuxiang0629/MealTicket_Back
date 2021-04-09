using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BatchModifyAccountSharesPlateStatus
    {
    }

    public class BatchModifyAccountSharesPlateStatusRequest:ModifyStatusRequest
    {
        /// <summary>
        /// 0系统分组 1自定义分组
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }
    }
}
