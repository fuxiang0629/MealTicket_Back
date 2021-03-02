using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BindBackAccountRoleRight
    {
    }

    public class BindBackAccountRoleRightRequest
    {
        /// <summary>
        /// 角色id
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 一级级菜单Id
        /// </summary>
        public long MenuId { get; set; }

        /// <summary>
        /// 权限Id列表
        /// </summary>
        public List<long> RightIdList { get; set; }
    }
}
