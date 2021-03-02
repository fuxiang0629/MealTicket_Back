using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesTradeTodayStatistics
    {
    }

    public class FrontAccountSharesTradeTodayStatisticsInfo
    {
        /// <summary>
        /// 完成笔数
        /// </summary>
        public int TradeFinishCount { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        public long TradeAmount { get; set; }

        /// <summary>
        /// 买入金额
        /// </summary>
        public long TradeBuyAmount { get; set; }

        /// <summary>
        /// 卖出金额
        /// </summary>
        public long TradeSoldAmount { get; set; }

        /// <summary>
        /// 交易股票数量
        /// </summary>
        public long TradeSharesCount { get; set; }

        /// <summary>
        /// 买入股票数量
        /// </summary>
        public long TradeBuySharesCount { get; set; }

        /// <summary>
        /// 卖出股票数量
        /// </summary>
        public long TradeSoldSharesCount { get; set; }
    }
}
