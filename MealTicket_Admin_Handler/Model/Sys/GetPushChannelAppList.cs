using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetPushChannelAppList
    {
    }

    public class GetPushChannelAppListRequest : PageRequest
    {
        /// <summary>
        /// 渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 状态 0全部 1有效 2无效
        /// </summary>
        public int Status { get; set; }
    }

    public class PushChannelAppInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// appKey
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// appSecret
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// 状态 1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
