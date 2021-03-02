using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddBackAccount
    {
    }

    public class AddBackAccountRequest
    {
        /// <summary>
        /// 【必填】用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 【必填】密码（MD5 32位小写）
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 【选填】真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 【选填】手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 【选填】性别0未知 1男 3女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 【选填】电子邮件
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 【选填】所属部门Id
        /// </summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// 【选填】所属职位Id
        /// </summary>
        public long PositionId { get; set; }
    }
}
