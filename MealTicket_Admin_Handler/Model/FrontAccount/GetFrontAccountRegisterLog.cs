using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountRegisterLog
    {
    }

    public class GetFrontAccountRegisterLogRequest:PageRequest
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public string AccountInfo { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public class FrontAccountRegisterLogInfo
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 注册时手机号
        /// </summary>
        public string RegisterMobile { get; set; }

        /// <summary>
        /// 注册ip
        /// </summary>
        public string RegIp{ get; set; }

        /// <summary>
        /// 注册设备信息
        /// </summary>
        public string DeviceUA { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
