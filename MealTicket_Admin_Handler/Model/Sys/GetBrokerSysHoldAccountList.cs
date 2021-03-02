using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBrokerSysHoldAccountList
    {
    }

    public class GetBrokerSysHoldAccountListRequest
    {
        /// <summary>
        /// Market+SharesCode
        /// </summary>
        public string Code { get; set; }
    }

    public class BrokerSysHoldAccountInfo
    {
        /// <summary>
        /// 券商账户code
        /// </summary>
        public string TradeAccountCode { get; set; }

        /// <summary>
        /// 用户可卖数量
        /// </summary>
        public int AccountCanSoldCount { get; set; }

        /// <summary>
        /// 平台可卖数量
        /// </summary>
        public int SimulateCanSoldCount { get; set; }
    }
}
