using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyHelpcenterQuestion
    {
    }

    public class ModifyHelpcenterQuestionRequest
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
    }
}
