using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountSharesGroupSharePassiveList
    {
    }

    public class AccountSharesGroupSharePassive
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 共享给我账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 共享给我账户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 共享时间
        /// </summary>
        public DateTime ShareTime { get; set; }

        /// <summary>
        /// 合并数量
        /// </summary>
        public int MergeCount { get; set; }

        /// <summary>
        /// 共享分组唯一Id
        /// </summary>
        public long RelId { get; set; }

        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 分组排序值
        /// </summary>
        public int GroupOrderIndex { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int GroupStatus { get; set; }

        /// <summary>
        /// 分组共享时间
        /// </summary>
        public DateTime GroupShareTime { get; set; }
    }
}
