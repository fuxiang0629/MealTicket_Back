using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSmsChannelApp
    {
    }

    public class AddSmsChannelAppRequest
    {
        /// <summary>
        /// 渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 应用AppKey
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// 应用AppSecret
        /// </summary>
        public string AppSecret { get; set; }
    }
}
