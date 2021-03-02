using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySysPar
    {
    }

    public class ModifySysParRequest
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParName { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string ParValue { get; set; }

        /// <summary>
        /// 参数描述
        /// </summary>
        public string ParDescription { get; set; }
    }
}
