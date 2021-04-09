using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddConditiontradeBuyGroup
    {
    }

    public class AddConditiontradeBuyGroupRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string Name { get; set; }
    }
}
