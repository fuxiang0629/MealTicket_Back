using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetAdminBaseInfo
    {
    }

    public class AdminBaseInfo
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 性别0未知 1男 3女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 所属部门Id
        /// </summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// 所属部门名称
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// 所属职位Id
        /// </summary>
        public long PositionId { get; set; }

        /// <summary>
        /// 所属职位名称
        /// </summary>
        public string PositionName { get; set; }
    }
}
