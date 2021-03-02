using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesTradeRankList
    {
    }

    public class GetFrontAccountSharesTradeRankListRequest:PageRequest
    {
        /// <summary>
        /// 时间类型1.最近一天 2.最近一周 3.最近一个月 4.最近3个月 5自定义
        /// </summary>
        public int TimeType { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 排序方式1.交易金额 2.买入金额 3.卖出金额 4.交易笔数
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// ascending：升序  descending：降序
        /// </summary>
        public string OrderMethod { get; set; }
    }

    public class FrontAccountSharesTradeRankInfo
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        public long TradeAmount { get; set; }

        /// <summary>
        /// 买入金额
        /// </summary>
        public long BuyAmount { get; set; }

        /// <summary>
        /// 卖出金额
        /// </summary>
        public long SellAmount { get; set; }

        /// <summary>
        /// 交易数量
        /// </summary>
        public int TradeCount { get; set; }

        /// <summary>
        /// 最后交易时间
        /// </summary>
        public DateTime? LastTradeTime { get; set; }
    }
}
