using MealTicket_Web_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    class GetAccountFollowRecord
    {
    }

    public class GetAccountFollowRecordRequest:DetailsPageRequest
    {
        /// <summary>
        /// 1.持仓 2委托
        /// </summary>
        public int Type { get; set; } 
    }

    public class AccountFollowRecordInfo
    {
        /// <summary>
        /// 委托Id
        /// </summary>
        public long EntrustId { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        public string AccountInfo { get; set; }

        /// <summary>
        ///交易类型1买入 2卖出
        /// </summary>
        public int TradeType { get; set; }

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
        /// 委托状态1申报中 2交易中 3已完成
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 委托时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否跟投
        /// </summary>
        public bool IsFollow { get; set; }

        /// <summary>
        /// 跟投记录列表
        /// </summary>
        public List<AccountFollowRecordInfo> FollowList { get; set; }
    }
}
