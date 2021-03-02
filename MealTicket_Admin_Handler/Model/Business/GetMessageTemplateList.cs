using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetMessageTemplateList
    {
    }

    public class GetMessageTemplateListRequest : PageRequest
    {
        /// <summary>
        /// 状态0全部 1有效 2无效
        /// </summary>
        public int Status { get; set; }
    }

    public class MessageTemplateInfo
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
        /// 所属分组
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 所属分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 消息标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; }

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
