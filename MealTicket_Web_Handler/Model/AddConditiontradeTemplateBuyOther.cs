using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddConditiontradeTemplateBuyOther
    {
    }

    public class AddConditiontradeTemplateBuyOtherRequest
    {

        /// <summary>
        /// 关系Id
        /// </summary>
        public long RelId { get; set; }

        /// <summary>
        /// 走势Id
        /// </summary>
        public long TrendId { get; set; }

        /// <summary>
        /// 走势名称
        /// </summary>
        public string TrendName { get; set; }

        /// <summary>
        /// 走势描述
        /// </summary>
        public string TrendDescription { get; set; }
    }
}
