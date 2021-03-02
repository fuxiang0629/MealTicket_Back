using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class AccountForgetPassword
    {
    }

    public class AccountForgetPasswordRequest 
    {
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 操作码
        /// </summary>
        public string ActionCode { get; set; }

        /// <summary>
        /// 短信验证码
        /// </summary>
        public string Verifycode { get; set; }

        /// <summary>
        /// 新登录密码（32位MD5小写）
        /// </summary>
        public string NewLoginPassword { get; set; }
    }
}
