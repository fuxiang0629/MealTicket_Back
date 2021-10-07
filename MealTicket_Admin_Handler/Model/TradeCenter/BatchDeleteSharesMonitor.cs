using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BatchDeleteSharesMonitor
    {
    }

    public class BatchDeleteSharesMonitorRequest
    {
        public List<long> IdList { get; set; }
    }
}
