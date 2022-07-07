using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class DeleteSharesHighMarkManager
    {
    }

    public class DeleteSharesHighMarkManagerRequest
    {
        public DateTime Date { get; set; }

        public long SharesKey { get; set; }
    }
}
