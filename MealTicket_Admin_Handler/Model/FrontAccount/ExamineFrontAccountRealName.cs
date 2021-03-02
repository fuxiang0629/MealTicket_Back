using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ExamineFrontAccountRealName
    {
    }

    public class ExamineFrontAccountRealNameRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 2审核成功 3审核失败
        /// </summary>
        public int ExamineStatus { get; set; }

        /// <summary>
        /// 审核状态描述
        /// </summary>
        public string ExamineStatusDes { get; set; }
    }
}
