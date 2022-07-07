using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifySharesHighMarkManagerStatus
    {
    }

    public class ModifySharesHighMarkManagerStatusRequest
    {
        public DateTime Date { get; set; }

        public long SharesKey { get; set; }

        public int Status { get; set; }
    }

    public class ModifySharesHighMarkManagerRiseLimitCountCustomRequest
    {
        public DateTime Date { get; set; }

        public long SharesKey { get; set; }

        public int RiseLimitCountCustom { get; set; }
    }
}
