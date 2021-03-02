using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BindBackAccountRole
    {
    }

    public class BindBackAccountRoleRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 角色Id列表
        /// </summary>
        public List<long> RoleIdList { get; set; }
    }
}
