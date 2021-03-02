using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class SellFrontAccountSharesHold
    {
    }

    public class SellFrontAccountSharesHoldRequest
    {
        /// <summary>
        /// 持仓Id
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        /// 卖出股票数量
        /// </summary>
        public int SellCount{get;set; }

        /// <summary>
        /// 卖出类型1市价 2限价
        /// </summary>
        public int SellType { get; set; }

        /// <summary>
        /// 卖出股票价格（限价卖出有效）
        /// </summary>
        public long SellPrice { get; set; }
    }
}
