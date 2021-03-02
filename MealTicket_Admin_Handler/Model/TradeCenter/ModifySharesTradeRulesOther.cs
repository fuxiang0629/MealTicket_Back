using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesTradeRulesOther
    {
    }

    public class ModifySharesTradeRulesOtherRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 时间段
        /// </summary>
        public string Times { get; set; }

        /// <summary>
        /// 警戒线（1/万）
        /// </summary>
        public int Cordon { get; set; }

        /// <summary>
        /// 平仓线（1/万）
        /// </summary>
        public int ClosingLine { get; set; }
    }
}
