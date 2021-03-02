using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyFrontAccountPassword
    {
    }

    public class ModifyFrontAccountLoginPasswordRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 新密码（MD5 32位小写）
        /// </summary>
        public string NewPassword { get; set; }
    }
}
