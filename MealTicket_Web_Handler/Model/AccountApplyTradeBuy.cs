using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    class AccountApplyTradeBuy
    {
    }

    public class AccountApplyTradeBuyRequest
    {
        /// <summary>
        /// 购买用户
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 市场代码0深圳 1上海
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 投入保证金金额
        /// </summary>
        public long BuyAmount { get; set; }

        /// <summary>
        /// 杠杆倍数
        /// </summary>
        public int FundMultiple { get; set; }

        /// <summary>
        /// 买入价格
        /// </summary>
        public long BuyPrice { get; set; }

        /// <summary>
        /// 跟投人员列表
        /// </summary>
        public List<FollowBuyInfo> FollowList { get; set; }

        /// <summary>
        /// 自动买入Id
        /// </summary>
        public long AutoDetailsId { get; set; }

        /// <summary>
        /// 跟投人员列表
        /// </summary>
        public List<long> SyncroAccountList { get; set; }
    }

    public class FollowBuyInfo
    {
        /// <summary>
        /// 跟投账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 购买金额-1表示全仓 0表示跟随主账户百分比 其他表示具体金额
        /// </summary>
        public long BuyAmount { get; set; }
    }
}
