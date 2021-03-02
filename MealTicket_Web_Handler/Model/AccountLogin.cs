using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AccountLogin
    {
    }

    public class AccountLoginRequest
    {
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 登录密码（32位MD5加密小写）
        /// </summary>
        public string LoginPassword { get; set; }
    }

    public class AccountLoginInfo
    {
        /// <summary>
        /// 用户登录token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 推荐码
        /// </summary>
        public string RecommandCode { get; set; }
    }
}
