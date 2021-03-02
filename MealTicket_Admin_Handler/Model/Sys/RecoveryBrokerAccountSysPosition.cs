using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class RecoveryBrokerAccountSysPosition
    {
    }

    public class RecoveryBrokerAccountSysPositionRequest
    {
        /// <summary>
        /// 股票持仓id
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        /// Market+SharesCode+TradeAccountCode
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 回收数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 回收价格
        /// </summary>
        public long Price { get; set; }
    }
}
