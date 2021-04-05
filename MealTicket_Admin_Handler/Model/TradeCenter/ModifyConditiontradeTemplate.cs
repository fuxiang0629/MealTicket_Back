using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyConditiontradeTemplate
    {
    }

    public class ModifyConditiontradeTemplateRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; }
    }
}
