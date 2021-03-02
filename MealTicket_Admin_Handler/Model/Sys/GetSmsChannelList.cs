using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSmsChannelList
    {
    }

    public class GetSmsChannelListRequest:PageRequest
    {
        /// <summary>
        /// 状态 0全部 1有效 2无效
        /// </summary>
        public int Status { get; set; }
    }

    public class SmsChannelInfo
    {
        /// <summary>
        /// 渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 渠道名称
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// 应用数量
        /// </summary>
        public int AppCount { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
