using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddPushTemplate
    {
    }

    public class AddPushTemplateRequest
    {
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TempName { get; set; }

        /// <summary>
        /// 模板内容
        /// </summary>
        public string TempContent { get; set; }

        /// <summary>
        /// 渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// AppKey
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// 跳转地址
        /// </summary>
        public string JumpUrl { get; set; }
    }
}
