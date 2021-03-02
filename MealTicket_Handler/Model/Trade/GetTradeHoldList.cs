using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetTradeHoldList
    {
    }

    public class TradeHoldInfo 
    {
        /// <summary>
        /// 持仓Id
        /// </summary>
        public long HoldId { get; set; }

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
        /// 涨幅(总盈利金额/初始总成本价)
        /// </summary>
        public string RiseRate
        {
            get
            {
                if (CostPrice <= 0)
                {
                    return "0.00";
                }
                if (RemainCount <= 0)
                {
                    return Math.Round((ProfitAmount * 1.0 / BuyTotalAmount) * 100, 2).ToString();
                }
                return Math.Round(((PresentPrice - CostPrice) * 1.0 / CostPrice) * 100, 2).ToString();
            }
        }

        /// <summary>
        /// 最初成本
        /// </summary>
        public long BuyTotalAmount { get; set; }

        /// <summary>
        /// 盈利金额(市值-剩余成本)
        /// </summary>
        public long ProfitAmount { get; set; }

        /// <summary>
        /// 现价
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        public long CostPrice { get; set; }

        /// <summary>
        /// 持有数量
        /// </summary>
        public int RemainCount { get; set; }

        /// <summary>
        /// 可卖数量
        /// </summary>
        public int CanSellCount { get; set; }

        /// <summary>
        /// 市值
        /// </summary>
        public long MarketValue { get; set; }

        /// <summary>
        /// 剩余保证金
        /// </summary>
        public long RemainDeposit { get; set; }

        /// <summary>
        /// 自动平仓时间
        /// </summary>
        public DateTime? ClosingTime { get; set; }

        /// <summary>
        /// 平仓线列表
        /// </summary>
        public List<ClosingLineInfo> ClosingLineList { get; set; }
    }

    public class ClosingLineInfo
    {
        /// <summary>
        /// 类型1全天 2优先时段
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 时间段
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// 强平价格
        /// </summary>
        public long ClosingPrice{get;set;}

        /// <summary>
        /// 建议追加保证金数量
        /// </summary>
        public long AddDepositAmount { get; set; }

        /// <summary>
        /// 1.正常 2.达到平仓线 3.达到警戒线
        /// </summary>
        public int ColorType { get; set; }
    }
}
