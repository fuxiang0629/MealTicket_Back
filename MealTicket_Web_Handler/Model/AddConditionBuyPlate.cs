using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddConditionBuyPlate
    {
    }

    public class AddConditionBuyPlateRequest
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }
    }
}
