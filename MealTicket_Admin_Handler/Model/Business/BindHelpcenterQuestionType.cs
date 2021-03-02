using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BindHelpcenterQuestionType
    {
    }

    public class BindHelpcenterQuestionTypeRequest
    {
        /// <summary>
        /// 问题Id
        /// </summary>
        public long QuestionId { get; set; }

        /// <summary>
        /// 所属分类Id
        /// </summary>
        public List<long> TypeId { get; set; }
    }
}
