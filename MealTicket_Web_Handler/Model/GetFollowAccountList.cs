using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetFollowAccountList
    {
    }

    public class GetFollowAccountListRequest
    {
        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }
    }

    public class FollowAccountInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 账户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 账户昵称
        /// </summary>
        public string AccountName { get; set; }

        public long Deposit { get; set; }

        /// <summary>
        /// 最大可用余额
        /// </summary>
        public long MaxDeposit { get; set; }

        /// <summary>
        /// 最大跟投倍数
        /// </summary>
        public long MaxFundMultiple { get; set; }
    }
}
