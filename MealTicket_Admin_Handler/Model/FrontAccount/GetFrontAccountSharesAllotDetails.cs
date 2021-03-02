using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesAllotDetails
    {
    }

    public class FrontAccountSharesAllotDetails
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
        /// 账户名称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 账户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 初始剩余股数
        /// </summary>
        public int InitialCount { get; set; }

        /// <summary>
        /// 最终剩余股数
        /// </summary>
        public int LatestCount { get; set; }

        /// <summary>
        /// 初始成本价
        /// </summary>
        public long InitialCostPrice { get; set; }

        /// <summary>
        /// 最终成本价
        /// </summary>
        public long LatestCostPrice { get; set; }

        /// <summary>
        /// 分红金额
        /// </summary>
        public long BonusAmount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
