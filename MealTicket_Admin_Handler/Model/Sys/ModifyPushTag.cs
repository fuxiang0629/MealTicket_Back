using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyPushTag
    {
    }

    public class ModifyPushTagRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

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
