using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BatchModifySharesSuspensionStatus
    {
    }

    public class BatchModifySharesSuspensionStatusRequest
    {
        /// <summary>
        /// 类型1停牌 2复牌
        /// </summary>
        public int Type { get; set; }
    }
}
