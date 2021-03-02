using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyAdminBaseInfo
    {
    }

    public class ModifyAdminBaseInfoRequest
    {
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 性别0未知 1男 2女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>
        public string Email { get; set; }
    }
}
