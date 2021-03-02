using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyBackAccountPassword
    {
    }

    public class ModifyBackAccountPasswordRequest 
    {
        /// <summary>
        /// 后台用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 新密码
        /// </summary>
        public string NewPassword { get; set; }
    }
}
