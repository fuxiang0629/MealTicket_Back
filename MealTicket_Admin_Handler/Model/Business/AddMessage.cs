using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddMessage
    {
    }

    public class AddMessageRequest
    {
        /// <summary>
        /// 图标
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
