using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBackAccountRightList
    {
    }

    public class GetBackAccountRightListRequest 
    {
        /// <summary>
        /// 角色Id（传入该值会标记角色权限）
        /// </summary>
        public long RoleId { get; set; }
    }

    public class BackAccountRightMenuInfo
    {
        /// <summary>
        /// 菜单Id
        /// </summary>
        public long MenuId { get; set; }

        /// <summary>
        /// 菜单标题
        /// </summary>
        public string MenuTitle { get; set; }

        /// <summary>
        /// 子菜单列表
        /// </summary>
        public List<BackAccountRightMenuInfo> MenuList { get; set; }

        /// <summary>
        /// 权限列表
        /// </summary>
        public List<BackAccountRightInfo> RightList { get; set; }
    }

    public class BackAccountRightInfo
    {
        /// <summary>
        /// 权限Id
        /// </summary>
        public long RightId { get; set; }

        /// <summary>
        /// 权限名称
        /// </summary>
        public string RightName { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsCheck { get; set; }
    }
}
