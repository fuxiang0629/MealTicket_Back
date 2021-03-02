using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetQuestionList
    {
    }

    /// <summary>
    /// 问题
    /// </summary>
    public class Question
    {
        /// <summary>
        /// 问题Id
        /// </summary>
        public long QuestionId { get; set; }

        /// <summary>
        /// 问题名称
        /// </summary>
        public string QuestName { get; set; }

        /// <summary>
        /// 问题所属分类
        /// </summary>
        public string QuestionTypeName { get; set; }

        /// <summary>
        /// 数据最后更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTop { get; set; }
    }

    public class GetQuestionListRequest:PageRequest
    {
        /// <summary>
        /// 问题类型Id(0表示所有)
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 是否常见问题
        /// </summary>
        public bool IsCommon { get; set; }
    }
}
