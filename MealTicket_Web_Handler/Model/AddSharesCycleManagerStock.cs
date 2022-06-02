using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddSharesCycleManagerStock
    {
    }

    public class AddSharesCycleManagerStockRequest
    {
        public long CycleId { get; set; }

        public long SharesKey { get; set; }
    }
}
