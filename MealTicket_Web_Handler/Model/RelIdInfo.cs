using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    public class RelIdInfo
    {
        /// <summary>
        /// 模板Id
        /// </summary>
        public long TemplateId { get; set; }

        /// <summary>
        /// 实际Id
        /// </summary>
        public long RelId { get; set; }
    }
}
