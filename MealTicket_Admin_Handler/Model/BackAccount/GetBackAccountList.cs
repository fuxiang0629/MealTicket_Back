using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBackAccountList
    {
    }

    public class GetBackAccountListRequest:PageRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 状态0全部 1有效 2无效
        /// </summary>
        public int Status { get; set; }
    }

    public class BackAccountInfo
    {
        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

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

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsAdministrator { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
