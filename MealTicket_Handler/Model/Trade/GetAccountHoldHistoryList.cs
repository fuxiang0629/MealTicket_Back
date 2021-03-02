using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetAccountHoldHistoryList
    {
    }

    public class GetAccountHoldHistoryListRequest:PageRequest
    {
        /// <summary>
        /// 起始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public string EndTime { get; set; }
    }

    public class GetAccountHoldHistoryListRes:PageRes<AccountHoldHistoryInfo>
    {
        /// <summary>
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }

        /// <summary>
        /// 总盈亏金额（元*10000）
        /// </summary>
        public long TotalProfitAmount { get; set; }

        /// <summary>
        /// 买入总额
        /// </summary>
        public long TotalBuyAmount { get; set; }
    }

    public class AccountHoldHistoryInfo
    {
        /// <summary>
        /// 持仓Id
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        /// 市场
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
        /// 卖出数量
        /// </summary>
        public int SellCount { get; set; }

        /// <summary>
        /// 卖出金额
        /// </summary>
        public long SellAmount { get; set; }

        /// <summary>
        /// 买入总金额
        /// </summary>
        public long BuyTotalAmount { get; set; }

        /// <summary>
        /// 首次持仓时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 最终卖出时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 盈利金额(市值-剩余成本)
        /// </summary>
        public long ProfitAmount
        {
            get
            {
                return SellAmount - BuyTotalAmount - DBUtils.CalculateOtherCost(new List<long>
                {
                    HoldId
                }, 1);
            }
        }

        /// <summary>
        /// 涨幅(总盈利金额/初始总成本价)
        /// </summary>
        public string RiseRate
        {
            get
            {
                return Math.Round((ProfitAmount * 1.0 / BuyTotalAmount) * 100, 2).ToString();
            }
        }
    }
}
