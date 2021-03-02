using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesDateForbid
    {
    }

    public class ModifySharesDateForbidRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

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
