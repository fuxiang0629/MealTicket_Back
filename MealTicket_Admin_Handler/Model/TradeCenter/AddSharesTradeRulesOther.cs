using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSharesTradeRulesOther
    {
    }

    public class AddSharesTradeRulesOtherRequest
    {
        /// <summary>
        /// 交易规则Id
        /// </summary>
        public long RulesId { get; set; }

        /// <summary>
        /// 类型 0系统级 1账户级
        /// </summary>
        public int Type { get; set; }

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
