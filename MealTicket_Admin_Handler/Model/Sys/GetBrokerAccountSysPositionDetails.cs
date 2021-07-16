using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBrokerAccountSysPositionDetails
    {
    }

    public class GetBrokerAccountSysPositionDetailsRequest : PageRequest
    {
        /// <summary>
        /// market+sharescode
        /// </summary>
        public string Code { get; set; }
    }

    public class BrokerAccountSysPositionDetails
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 总持仓数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 可卖数量
        /// </summary>
        public int CanSoldCount { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        public long CostPrice { get; set; }

        /// <summary>
        /// 盈亏金额
        /// </summary>
        public long ProfitAmount { get; set; }

        /// <summary>
        /// 当前价格
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 数据最后更新时间
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}
