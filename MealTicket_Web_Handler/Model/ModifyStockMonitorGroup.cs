using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyStockMonitorGroup
    {
    }

    public class ModifyStockMonitorGroupRequest
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
