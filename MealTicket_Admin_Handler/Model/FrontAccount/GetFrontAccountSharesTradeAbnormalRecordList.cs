using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesTradeAbnormalRecordList
    {
    }

    public class FrontAccountSharesTradeAbnormalRecordInfo
    {
        /// <summary>
        /// id
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
        /// 成交数量
        /// </summary>
        public int DealCount { get; set; }

        /// <summary>
        /// 申报日期
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 交易账户数量
        /// </summary>
        public int TradeAccountCount { get; set; }

        /// <summary>
        /// 委托类型1市价 2限价
        /// </summary>
        public int EntrustType { get; set; }

        /// <summary>
        /// 委托价格
        /// </summary>
        public long EntrustPrice { get; set; }
    }
}
