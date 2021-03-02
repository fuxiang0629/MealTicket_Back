using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class AccountModifyMobile
    {
    }

    public class AccountModifyMobileRequest
    {
        /// <summary>
        /// 新手机号
        /// </summary>
        public string NewMobile { get; set; }

        /// <summary>
        /// 操作码
        /// </summary>
        public string ActionCode { get; set; }

        /// <summary>
        /// 短信验证码
        /// </summary>
        public string Verifycode { get; set; }
    }
}
