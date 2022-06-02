using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BuildSharesGroupStatisticData
    {
    }

    public class BuildSharesGroupStatisticDataRequest
    {
        public long ContextId { get; set; }

        public int DateCount { get; set; }

        //public DateTime StartDate { get; set; }

        //public DateTime EndDate { get; set; }
    }
}
