using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class AccountModifyPassword
    {
    }

    public class AccountModifyPasswordRequest
    {
        /// <summary>
        /// 旧密码（32位MD5小写）
        /// </summary>
        public string OldPassword { get; set; }

        /// <summary>
        /// 新密码（32位MD5小写）
        /// </summary>
        public string NewPassword { get; set; }
    }
}
