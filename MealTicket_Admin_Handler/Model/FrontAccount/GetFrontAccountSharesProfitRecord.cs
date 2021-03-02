using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesProfitRecord
    {
    }

    public class GetFrontAccountSharesProfitRecordRequest:DetailsPageRequest
    {
        /// <summary>
        /// 股票名称/代码
        /// </summary>
        public string SharesInfo { get; set; }
    }

    public class FrontAccountSharesProfitRecordInfo
    {
        /// <summary>
        /// 持仓Id
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
        /// 盈亏数额
        /// </summary>
        public long ProfitAmount { get; set; }

        /// <summary>
        /// 持仓状态1持仓中 2已清仓
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 最后交易时间
        /// </summary>
        public DateTime LastTradeTime { get; set; }

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
