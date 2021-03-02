using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class SetTradeClosingTime
    {
    }

    public class SetTradeClosingTimeRequest 
    {
        /// <summary>
        /// 持仓Id
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        /// 自动平仓时间(null表示不设置自动平仓)
        /// </summary>
        public DateTime? ClosingTime { get; set; }
    }
}
