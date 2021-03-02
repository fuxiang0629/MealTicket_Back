using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddAccountOptionalGroup
    {
    }

    public class AddAccountOptionalGroupRequest:DetailsRequest
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 分组描述
        /// </summary>
        public string GroupDescription { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public long OrderIndex { get; set; }
    }
}
