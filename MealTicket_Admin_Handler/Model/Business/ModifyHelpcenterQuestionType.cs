using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyHelpcenterQuestionType
    {
    }

    public class ModifyHelpcenterQuestionTypeRequest
    {
        /// <summary>
        /// 类型id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
