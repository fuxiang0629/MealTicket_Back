using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSharesDateForbid
    {
    }

    public class AddSharesDateForbidRequest
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 起始日期
        /// </summary>
        public string BeginDate { get; set; }

        /// <summary>
        /// 截止日期
        /// </summary>
        public string EndDate { get; set; }
    }
}
