using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    class GetAccountTradeInfo
    {
    }

    public class AccountTradeInfo
    {
        /// <summary>
        /// 总资产
        /// </summary>
        public long TotalAssets { get; set; }

        /// <summary>
        /// 总权益
        /// </summary>
        public long TotalFundAmount { get; set; }

        /// <summary>
        /// 已用保证经
        /// </summary>
        public long UsedDepositAmount { get; set; }

        /// <summary>
        /// 总市值
        /// </summary>
        public long TotalMarketValue { get; set; }

        /// <summary>
        /// 可用保证金
        /// </summary>
        public long RemainDeposit { get; set; }

        /// <summary>
        /// 总盈亏
        /// </summary>
        public long TotalProfit { get; set; }

        /// <summary>
        /// 今日盈亏
        /// </summary>
        public long TodayProfit { get; set; }

        /// <summary>
        /// 保留余额
        /// </summary>
        public long RetainDeposit { get; set; }

        /// <summary>
        /// 最大购买股票数量
        /// </summary>
        public int MaxBuySharesCount { get; set; }

        /// <summary>
        /// 每日最大持仓数量
        /// </summary>
        public int MaxBuySharesCount_Day { get; set; }
    }
}
