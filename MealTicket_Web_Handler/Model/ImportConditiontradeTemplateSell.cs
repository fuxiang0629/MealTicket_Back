using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ImportConditiontradeTemplateSell
    {
    }

    public class ImportConditiontradeTemplateSellRequest
    {
        /// <summary>
        /// 模板Id
        /// </summary>
        public long TempLateId { get; set; }

        /// <summary>
        /// 1客户模板 2系统模板
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 持仓
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        /// 持仓用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 是否清除
        /// </summary>
        public bool IsClear { get; set; }

        /// <summary>
        /// 跟投人员列表
        /// </summary>
        public List<long> FollowList { get; set; }

        /// <summary>
        /// 是否同时导入到跟投账户
        /// </summary>
        public bool IsImportToFollow { get; set; }
    }
}
