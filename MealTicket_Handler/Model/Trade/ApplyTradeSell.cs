using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class ApplyTradeSell
    {
    }

    public class ApplyTradeSellRequest
    {
        /// <summary>
        /// 持仓Id
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        /// 卖出数量
        /// </summary>
        public int SellCount { get; set; }

        /// <summary>
        /// 卖出方式1.市价 2.限价 
        /// </summary>
        public int SellType { get; set; }

        /// <summary>
        /// 卖出价格（仅SellType=2时有效）
        /// </summary>
        public long SellPrice { get; set; }
    }
}
