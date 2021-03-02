using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class AccountLogin
    {
    }

    public class AccountLoginInfo
    {
        /// <summary>
        /// 用户登录token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 状态1登入成功 3需要设置交易密码
        /// </summary>
        public int Status { get; set; }
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
}
