using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ResetSharesPlateBaseDate
    {
    }

    public class ResetSharesPlateBaseDateRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 基准日期
        /// </summary>
        public DateTime BaseDate { get; set; }
    }
}
