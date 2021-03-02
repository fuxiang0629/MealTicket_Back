using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesHoldStatistics
    {
    }

    public class GetFrontAccountSharesHoldStatisticsRequest:PageRequest
    {
        /// <summary>
        /// 用户信息（手机号/用户名）
        /// </summary>
        public string AccountInfo { get; set; }
    }

    public class FrontAccountSharesHoldStatisticsInfo
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 持仓数量
        /// </summary>
        public int HoldCount { get; set; }

        /// <summary>
        /// 进入警戒线数量
        /// </summary>
        public int CordonCount { get; set; }

        /// <summary>
        /// 进入强平线数量
        /// </summary>
        public int ClosingCount { get; set; }

        /// <summary>
        /// 总市值
        /// </summary>
        public long TotalMarketValue { get; set; }

        /// <summary>
        /// 总盈亏
        /// </summary>
        public long TotalProfit { get; set; }

        /// <summary>
        /// 总保证金
        /// </summary>
        public long TotalDeposit { get; set; }

        /// <summary>
        /// 总借款金额
        /// </summary>
        public long TotalFundAmount { get; set; }

        /// <summary>
        /// 初始保证金
        /// </summary>
        public long InitialDeposit { get; set; }

        /// <summary>
        /// 初始借款金额
        /// </summary>
        public long InitialFundAmount { get; set; }
    }
}
