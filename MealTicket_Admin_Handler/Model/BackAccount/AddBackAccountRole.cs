using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddBackAccountRole
    {
    }

    public class AddBackAccountRoleRequest
    {
        /// <summary>
        /// 【必填】角色名称
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 【选填】角色描述
        /// </summary>
        public string RoleDescription { get; set; }
    }
}
