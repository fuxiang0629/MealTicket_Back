using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetQuestionDetails
    {
    }

    /// <summary>
    /// 问题详情
    /// </summary>
    public class QuestionDetails : Question
    {
        /// <summary>
        /// 答案
        /// </summary>
        public string Answer { get; set; }
    }
}
