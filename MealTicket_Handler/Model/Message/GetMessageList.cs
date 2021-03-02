using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetMessageList
    {
    }

    public class GetMessageListRequest : PageRequest
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }
    }

    public class MessageInfo
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public long MessageId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
