using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetMyCashRefundInfo
    {
    }

    public class CashRefundInfo
    {
        /// <summary>
        /// 最大可退金额
        /// </summary>
        public long MaxRefundAmount { get; set; }

        /// <summary>
        /// 节省金额
        /// </summary>
        public long SaveAmount { get; set; }
    }
}
