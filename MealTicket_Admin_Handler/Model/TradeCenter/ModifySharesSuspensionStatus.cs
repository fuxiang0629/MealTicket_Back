using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesSuspensionStatus
    {
    }

    public class ModifySharesSuspensionStatusRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 是否停牌
        /// </summary>
        public bool IsSuspension { get; set; }
    }
}
