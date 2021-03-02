using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesHoldList
    {
    }

    public class FrontAccountSharesHoldInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 市场代码0深圳 1上海
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
        /// 持仓数
        /// </summary>
        public int RemainCount { get; set; }

        /// <summary>
        /// 可卖数量
        /// </summary>
        public int CanSoldCount { get; set; }

        /// <summary>
        /// 市值
        /// </summary>
        public long MarketValue { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        public long CostPrice { get; set; }

        /// <summary>
        /// 强平价格
        /// </summary>
        public long ColsingPrice { get; set; }

        /// <summary>
        /// 追加保证金数量
        /// </summary>
        public long AddDepositAmount { get; set; }

        /// <summary>
        /// 当前价格
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 盈亏数额
        /// </summary>
        public long ProfitAmount { get; set; }

        /// <summary>
        /// 剩余保证金
        /// </summary>
        public long RemainDeposit { get; set; }

        /// <summary>
        /// 剩余借款金额
        /// </summary>
        public long RemainFundAmount { get; set; }

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
