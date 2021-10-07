using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BatchDeleteAccountOptional
    {
    }

    public class BatchDeleteAccountOptionalRequest
    {
        public List<long> IdList { get; set; }
    }
}
