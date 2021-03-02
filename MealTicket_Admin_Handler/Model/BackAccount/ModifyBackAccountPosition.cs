using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyBackAccountPosition
    {
    }

    public class ModifyBackAccountPositionRequest
    {
        /// <summary>
        /// 职位id
        /// </summary>
        public long Id { get; set; }

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
