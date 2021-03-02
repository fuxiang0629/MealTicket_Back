using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddMessageTemplate
    {
    }

    public class AddMessageTemplateRequest
    {
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TempName { get; set; }

        /// <summary>
        /// 所属分组
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 消息标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; }
    }
}
