using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddFrontAccountSharesTradeAbnormalRecordDetails
    {
    }

    public class AddFrontAccountSharesTradeAbnormalRecordDetailsRequest
    {
        /// <summary>
        /// 平台委托编号
        /// </summary>
        public long BuyId { get; set; }

        /// <summary>
        /// 交易账户code
        /// </summary>
        public string TradeAccountCode { get; set; }

        /// <summary>
        /// 委托编号
        /// </summary>
        public string EntrustId { get; set; }

        /// <summary>
        /// 平台委托数量
        /// </summary>
        public int EntrustCount { get; set; }

        /// <summary>
        /// 委托类型1市价 2限价
        /// </summary>
        public int EntrustType { get; set; }

        /// <summary>
        /// 委托价格
        /// </summary>
        public long EntrustPrice { get; set; }

        /// <summary>
        /// 用户成交数量
        /// </summary>
        public int DealCount { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public long DealPrice { get; set; }
    }
}
