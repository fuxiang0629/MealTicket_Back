using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ApplySharesToBuyBySys
    {
    }

    public class ApplySharesToBuyBySysRequest
    {
        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 股票市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public int DealCount { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public long DealPrice { get; set; }

        /// <summary>
        /// 杠杆倍数
        /// </summary>
        public int RealFundMultiple { get; set; }

        /// <summary>
        /// 委托时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 证券账号
        /// </summary>
        public string TradeAccountCode { get; set; }
    }
}
