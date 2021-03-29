using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetTradeAccountInfo
    {
    }

    public class TradeAccountInfo
    {
        /// <summary>
        /// 总资产
        /// </summary>
        public long TotalAssets { get; set; }

        /// <summary>
        /// 总市值
        /// </summary>
        public long TotalMarketValue { get; set; }

        /// <summary>
        /// 可用保证金
        /// </summary>
        public long RemainDeposit { get; set; }

        /// <summary>
        /// 已用保证金
        /// </summary>
        public long UseRemainDeposit { get; set; }

        /// <summary>
        /// 总借款
        /// </summary>
        public long TotalFundAmount { get; set; }

        /// <summary>
        /// 总盈亏
        /// </summary>
        public long TotalProfit { get; set; }

        /// <summary>
        /// 今日盈亏
        /// </summary>
        public long TodayProfit { get; set; }
    }
}
