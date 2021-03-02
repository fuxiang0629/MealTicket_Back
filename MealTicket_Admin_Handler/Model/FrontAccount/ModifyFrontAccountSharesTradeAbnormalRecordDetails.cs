using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyFrontAccountSharesTradeAbnormalRecordDetails
    {
    }

    public class ModifyFrontAccountSharesTradeAbnormalRecordDetailsRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 委托Id
        /// </summary>
        public string EntrustId { get; set; }

        /// <summary>
        /// 券商委托数量
        /// </summary>
        public int RealEntrustCount { get; set; }

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
