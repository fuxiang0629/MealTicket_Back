using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddConditiontradeBuyGroupSharesAccount
    {
    }

    public class AddConditiontradeBuyGroupSharesAccountRequest
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 被授权账户分组Id
        /// </summary>
        public long AuthorizedGroupId { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        public string Mobile { get; set; }
    }
}
