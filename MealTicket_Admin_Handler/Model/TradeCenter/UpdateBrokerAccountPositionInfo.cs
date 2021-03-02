using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class UpdateBrokerAccountPositionInfo
    {
    }

    public class UpdateBrokerAccountPositionInfoRequest
    {
        /// <summary>
        /// 券商账户
        /// </summary>
        public string TradeAccountCode { get; set; }
    }
}
