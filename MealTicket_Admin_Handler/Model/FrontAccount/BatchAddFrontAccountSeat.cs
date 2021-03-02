using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BatchAddFrontAccountSeat
    {
    }

    public class BatchAddFrontAccountSeatRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime ValidEndTime { get; set; }
    }
}
