using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_CacheCommon_Session
{
    public class LoginInfo
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }

    public class LoginResult:LoginInfo
    {
        /// <summary>
        /// 访问密钥
        /// </summary>
        public string VisitKey { get; set; }
    }

    public class CacheInfo 
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
