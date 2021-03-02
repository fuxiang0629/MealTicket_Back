using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler
{
    class AdminLogin
    {
    }

    public class AdminLoginRequest
    {
        /// <summary>
        /// 账户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码（MD5 32位小写）
        /// </summary>
        public string Password { get; set; }
    }

    public class AdminLoginInfo
    {
        /// <summary>
        /// 登录token
        /// </summary>
        public string UserToken { get; set; }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsAdministrator { get; set; }

        /// <summary>
        /// 权限列表（仅非超级管理员有效）
        /// </summary>
        public List<string> RightCodeList { get; set; }
    }
}
