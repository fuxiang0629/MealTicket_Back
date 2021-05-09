using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    public class BatchAddSharesConditionTrendPar2Obj
    {
        /// <summary>
        /// 板块名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 大于/小于
        /// </summary>
        public int Compare { get; set; }

        /// <summary>
        /// 百分比
        /// </summary>
        public string Rate { get; set; }
    }
}
