using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetTradeEntrustList
    {
    }

    public class GetTradeEntrustListRequest:PageRequest
    {
        /// <summary>
        /// 持仓Id
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        /// 委托类别0所有 1买入 2卖出
        /// </summary>
        public int EntrustType { get; set; }

        /// <summary>
        /// 委托状态0所有 1未完成 2已完成
        /// </summary>
        public int EntrustStatus { get; set; }
    }

    public class TradeEntrustInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 持仓Id
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        ///交易类型1买入 2卖出
        /// </summary>
        public int TradeType { get; set; }

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
        /// 当前价格
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 委托价格（0表示市价委托）
        /// </summary>
        public long EntrustPrice { get; set; }

        /// <summary>
        /// 委托类型1市价 2限价
        /// </summary>
        public int EntrustType { get; set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public int EntrustCount { get; set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public int DealCount { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public long DealPrice { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 自动平仓时间（EntrustType=1时有效）
        /// </summary>
        public DateTime? ClosingTime { get; set; }

        /// <summary>
        /// 自动平仓是否已经执行（EntrustType=1时有效）
        /// </summary>
        public bool IsExecuteClosing { get; set; }

        /// <summary>
        /// 委托状态1申报中 2交易中 3已完成
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 盈亏金额
        /// </summary>
        public long SoldProfitAmount { get; set; }

        /// <summary>
        /// 卖出类型0主动卖出 1自动平仓 2强制平仓
        /// </summary>
        public int SoldType { get; set; }
    }
}
