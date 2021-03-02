using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetMenuList
    {
    }

    public class GetMenuListRequest
    {
        /// <summary>
        /// 一级菜单Id（0表示获取所有一级菜单）
        /// </summary>
        public long FatherMenuId { get; set; }
    }

    public class MenuInfo
    {
        /// <summary>
        /// 菜单Id
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// 菜单标题
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 菜单地址
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// 菜单图标
        /// </summary>
        public string icon { get; set; }

        /// <summary>
        /// 子菜单列表
        /// </summary>
        public List<MenuInfo> routes { get; set; }
    }
}
