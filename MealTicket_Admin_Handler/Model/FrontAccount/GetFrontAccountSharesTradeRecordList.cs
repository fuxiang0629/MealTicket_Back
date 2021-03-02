using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesTradeRecordList
    {
    }

    public class GetFrontAccountSharesTradeRecordListRequest:PageRequest
    {
        /// <summary>
        /// 账户信息（昵称/手机号）
        /// </summary>
        public string AccountInfo { get; set; }

        /// <summary>
        /// 股票信息（股票代码/股票名称）
        /// </summary>
        public string SharesInfo { get; set; }

        /// <summary>
        /// 委托状态1申报中 2交易中 21部成 3已完成 31 部撤 32 已撤
        /// </summary>
        public int EntrustStatus { get; set; }

        /// <summary>
        /// 交易类型0全部 1买入 2卖出
        /// </summary>
        public int TradeType { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public class FrontAccountSharesTradeRecordInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 交易类型1买入 2卖出
        /// </summary>
        public int TradeType { get; set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public int EntrustCount { get; set; }

        /// <summary>
        /// 委托类型1市价 2限价
        /// </summary>
        public int EntrustType { get; set; }

        /// <summary>
        /// 委托价格
        /// </summary>
        public long EntrustPrice { get; set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public int DealCount { get; set; }

        /// <summary>
        /// 成交均价
        /// </summary>
        public long DealPriceAvg { get; set; }

        /// <summary>
        /// 成交金额
        /// </summary>
        public long DealAmount { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 申报日期
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 委托状态1申报中 2交易中 21部成 3已完成 31 部撤 32 已撤
        /// </summary>
        public int EntrustStatus { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime FinishTime { get; set; }
    }
}
