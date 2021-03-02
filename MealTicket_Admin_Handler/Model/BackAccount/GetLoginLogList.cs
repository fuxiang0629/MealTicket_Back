using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetLoginLogList
    {
    }

    public class GetLoginLogListRequest:PageRequest
    {
        /// <summary>
        /// 登入用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 登录时Ip地址
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public class LoginLogInfo
    {
        /// <summary>
        /// 日志Id
        /// </summary>
        public long LogId { get; set; }

        /// <summary>
        /// 登录用户名
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// 登录错误码
        /// </summary>
        public string LoginError { get; set; }

        /// <summary>
        /// 登出时间
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// 登出错误码
        /// </summary>
        public string LoginoutError { get; set; }

        /// <summary>
        /// 登录Ip
        /// </summary>
        public string LoginIp { get; set; }

        /// <summary>
        /// Ua
        /// </summary>
        public string DeviceUA { get; set; }
    }
}
