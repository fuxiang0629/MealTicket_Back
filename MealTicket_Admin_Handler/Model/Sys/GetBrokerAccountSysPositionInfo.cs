using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBrokerAccountSysPositionInfo
    {
    }

    public class GetBrokerAccountSysPositionInfoRequest:PageRequest
    { 
        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }
    }

    public class BrokerAccountSysPositionInfo 
    {
        /// <summary>
        /// 市场0深圳 1上海
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 持仓总数
        /// </summary>
        public int TotalSharesCount { get; set; }

        /// <summary>
        /// 可卖出量
        /// </summary>
        public int CanSoldSharesCount { get; set; }
    }
}
