using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class FinishCash
    {
    }

    public class FinishCashRequest
    {
        /// <summary>
        /// 提现id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
