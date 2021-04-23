using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class DeleteConditiontradeBuyGroup
    {
    }

    public class DeleteConditiontradeBuyGroupRequest:DetailsRequest 
    {
        /// <summary>
        /// 是否删除分组
        /// </summary>
        public bool IsDeleteGroup { get; set; }

        /// <summary>
        /// 是否删除股票
        /// </summary>
        public bool IsDeleteShares { get; set; }

        /// <summary>
        /// 是否保留其他分组股票
        /// </summary>
        public bool IsRetainOtherGroupShares { get; set; }
    }
}
