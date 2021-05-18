using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSharesPriorityAutoBuyPlate
    {
    }

    public class AddSharesPriorityAutoBuyPlateRequest 
    {
        /// <summary>
        /// 优先级Id
        /// </summary>
        public long PriorityId { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }
    }
}
