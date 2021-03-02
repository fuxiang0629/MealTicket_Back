using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesTradeService.Model
{
    public class SharesEntrustDealInfo
    {
        /// <summary>
        /// 成交时间
        /// </summary>
        public DateTime DealTime { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public long DealPrice { get; set; }

        /// <summary>
        /// 成交金额
        /// </summary>
        public long DealAmount { get; set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public int DealCount { get; set; }

        /// <summary>
        /// 成交Id
        /// </summary>
        public string DealId { get; set; }

        /// <summary>
        /// 委托编号
        /// </summary>
        public string EntrustId { get; set; }

        /// <summary>
        /// 交易账户code
        /// </summary>
        public string TradeAccountCode { get; set; }
    }
}
