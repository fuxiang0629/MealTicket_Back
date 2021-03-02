using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyHelpcenterQuestionIsCommon
    {
    }

    public class ModifyHelpcenterQuestionIsCommonRequest
    {
        /// <summary>
        /// 问题Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 是否常见问题
        /// </summary>
        public bool IsCommon { get; set; }
    }
}
