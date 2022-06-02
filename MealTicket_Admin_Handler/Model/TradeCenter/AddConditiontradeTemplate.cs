using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddConditiontradeTemplate
    {
    }

    public class AddConditiontradeTemplateRequest
    {
        /// <summary>
        /// 1买入 2卖出
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 背景色
        /// </summary>
        public string BgColor { get; set; }
    }
}
