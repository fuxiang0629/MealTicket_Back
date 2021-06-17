using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ImportConditiontradeBuyShare
    {
    }

    public class ImportConditiontradeBuyShareRequest
    {
        /// <summary>
        /// 导入目标
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 条件股票Id列表
        /// </summary>
        public List<long> ConditionIdList { get; set; }

        /// <summary>
        /// 买入金额
        /// </summary>
        public long BuyAmount { get; set; }

        /// <summary>
        /// 条件所属账户Id
        /// </summary>
        public long MainAccountId { get; set; }

        /// <summary>
        /// 是否清除
        /// </summary>
        public bool IsClear { get; set; }

        /// <summary>
        /// 跟投用户
        /// </summary>
        public List<long> FollowList { get; set; }

        /// <summary>
        /// 分组Id列表
        /// </summary>
        public List<long> GroupIdList { get; set; }

        /// <summary>
        /// 新分组名称
        /// </summary>
        public string NewGroupName { get; set; }
    }
}
