using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyHelpcenterQuestionIsTop
    {
    }

    public class ModifyHelpcenterQuestionIsTopRequest
    {
        /// <summary>
        /// 问题Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsTop { get; set; }
    }
}
