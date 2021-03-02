using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySmsTemplate
    {
    }

    public class ModifySmsTemplateRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public string TempName { get; set; }

        /// <summary>
        /// 模板短信类型2通知类 3营销类
        /// </summary>
        public int TempType { get; set; }

        /// <summary>
        /// 短信内容
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
    }
}
