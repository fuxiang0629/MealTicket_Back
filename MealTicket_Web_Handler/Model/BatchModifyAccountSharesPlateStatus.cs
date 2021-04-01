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
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }
    }
}
