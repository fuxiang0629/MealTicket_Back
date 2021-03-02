using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddInformation
    {
    }

    public class AddInformationRequest
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容介绍
        /// </summary>
        public string ContentIntroduction { get; set; }

        /// <summary>
        /// 完整内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 开始展示时间
        /// </summary>
        public DateTime StartShowTime { get; set; }

        /// <summary>
        /// 结束展示时间
        /// </summary>
        public DateTime EndShowTime { get; set; }
    }
}
