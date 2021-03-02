using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyBackAccountDepartment
    {
    }

    public class ModifyBackAccountDepartmentRequest
    {
        /// <summary>
        /// 部门id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 部门描述
        /// </summary>
        public string Description { get; set; }
    }
}
