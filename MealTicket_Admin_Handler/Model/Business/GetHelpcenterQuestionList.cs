using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetHelpcenterQuestionList
    {
    }

    public class GetHelpcenterQuestionListRequest : PageRequest
    {
        /// <summary>
        /// 问题名称
        /// </summary>
        public string QuestionName { get; set; }
    }

    public class HelpcenterQuestionInfo
    {
        /// <summary>
        /// 问题id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 问题名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 问题答案
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTop { get; set; }

        /// <summary>
        /// 是否常见问题
        /// </summary>
        public bool IsCommon { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 所属分类
        /// </summary>
        public string Type { get; set; }
    }
}
