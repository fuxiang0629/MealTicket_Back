using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class AccountRegister
    {
    }

    public class AccountRegisterRequest
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
        /// 登录密码（32位MD5小写）
        /// </summary>
        public string LoginPassword { get; set; }

        /// <summary>
        /// 推荐人推荐码
        /// </summary>
        public string ReferRecommandCode { get; set; }
    }
}
