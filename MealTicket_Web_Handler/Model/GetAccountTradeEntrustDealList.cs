using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    class GetAccountTradeEntrustDealList
    {
    }

    public class AccountTradeEntrustDealInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 交易类型1买入 2卖出
        /// </summary>
        public int TradeType { get; set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public int DealCount { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public long DealPrice { get; set; }

        /// <summary>
        /// 成交金额
        /// </summary>
        public long DealAmount { get; set; }

        /// <summary>
        /// 成交时间
        /// </summary>
        public DateTime DealTime { get; set; }
    }
}
