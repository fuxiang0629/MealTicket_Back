using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesTradeAbnormalRecordDetails
    {
    }

    public class FrontAccountSharesTradeAbnormalRecordDetails
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 交易账户code
        /// </summary>
        public string TradeAccountCode { get; set; }

        /// <summary>
        /// 委托编号
        /// </summary>
        public string EntrustId { get; set; }

        /// <summary>
        /// 平台委托数量
        /// </summary>
        public int EntrustCount { get; set; }

        /// <summary>
        /// 券商委托数量
        /// </summary>
        public int RealEntrustCount { get; set; }

        /// <summary>
        /// 委托类型1市价 2限价
        /// </summary>
        public int EntrustType { get; set; }

        /// <summary>
        /// 委托价格
        /// </summary>
        public long EntrustPrice { get; set; }

        /// <summary>
        /// 用户成交数量
        /// </summary>
        public int DealCount { get; set; }

        /// <summary>
        /// 平台回收数量
        /// </summary>
        public int SimulateDealCount { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public long DealPrice { get; set; }

        /// <summary>
        /// 委托状态1申报中 2交易中 3已完成
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDes { get; set; }

        /// <summary>
        /// 委托时间
        /// </summary>
        public DateTime EntrustTime { get; set; }
    }
}
