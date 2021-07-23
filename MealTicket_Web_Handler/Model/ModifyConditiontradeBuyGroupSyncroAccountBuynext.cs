using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyConditiontradeBuyGroupSyncroAccountBuynext
    {
    }

    public class ModifyConditiontradeBuyGroupSyncroAccountBuynextRequest
    {
        /// <summary>
        /// 自定义分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 呗授权账户分组Id
        /// </summary>
        public long AuthorizedGroupId { get; set; }
    }
}
