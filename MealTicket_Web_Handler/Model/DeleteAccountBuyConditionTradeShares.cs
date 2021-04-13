using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class DeleteAccountBuyConditionTradeShares
    {
    }

    public class DeleteAccountBuyConditionTradeSharesRequest : DetailsRequest
    {
        /// <summary>
        /// 关系Id
        /// </summary>
        public long RelId { get; set; }

        /// <summary>
        /// 0删除股票 1仅删除关系
        /// </summary>
        public int Type { get; set; }
    }
}
