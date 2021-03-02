using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesProfitStatistics
    {
    }

    public class GetFrontAccountSharesProfitStatisticsRequest:PageRequest
    {
        /// <summary>
        /// 用户信息（手机号/用户名）
        /// </summary>
        public string AccountInfo { get; set; }
    }

    public class FrontAccountSharesProfitStatisticsInfo
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
        /// 总盈亏
        /// </summary>
        public long TotalProfit { get; set; }

        /// <summary>
        /// 最后交易时间
        /// </summary>
        public DateTime? LastTradeTime { get; set; }

        /// <summary>
        /// 最后交易类别 1买 2卖
        /// </summary>
        public int LastTradeType { get; set; }

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

        /// <summary>
        /// 资金占用费
        /// </summary>
        public long ServiceFee { get; set; }

        /// <summary>
        /// 开仓费
        /// </summary>
        public long BuyService { get; set; }

        /// <summary>
        /// 盈利税
        /// </summary>
        public long ProfitService { get; set; }

        /// <summary>
        /// 证管费
        /// </summary>
        public long AdministrationFee { get; set; }

        /// <summary>
        /// 佣金
        /// </summary>
        public long Commission { get; set; }

        /// <summary>
        /// 经手费
        /// </summary>
        public long HandlingFee { get; set; }

        /// <summary>
        /// 结算费
        /// </summary>
        public long SettlementFee { get; set; }

        /// <summary>
        /// 印花税
        /// </summary>
        public long StampFee { get; set; }

        /// <summary>
        /// 过户费
        /// </summary>
        public long TransferFee { get; set; }
    }
}
