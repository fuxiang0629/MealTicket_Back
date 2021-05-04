using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyConditiontradeBuyGroup
    {
    }

    public class ModifyConditiontradeBuyGroupRequest
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
        /// 今天最大购买次数
        /// </summary>
        public int MaxBuyCount { get; set; }
    }
}
