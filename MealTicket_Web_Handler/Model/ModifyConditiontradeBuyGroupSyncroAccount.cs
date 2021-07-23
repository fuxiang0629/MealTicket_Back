using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyConditiontradeBuyGroupSyncroAccount
    {
    }

    public class ModifyConditiontradeBuyGroupSyncroAccountRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 被授权账户分组Id
        /// </summary>
        public long AuthorizedGroupId { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
