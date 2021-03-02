using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddBackAccountPosition
    {
    }

    public class AddBackAccountPositionRequest
    {
        /// <summary>
        /// 职位名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 职位描述
        /// </summary>
        public string Description { get; set; }
    }
}
