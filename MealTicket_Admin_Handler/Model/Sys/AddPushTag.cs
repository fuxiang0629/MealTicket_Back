using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddPushTag
    {
    }

    public class AddPushTagRequest
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 触发api
        /// </summary>
        public string TriApiUrl { get; set; }
    }
}
