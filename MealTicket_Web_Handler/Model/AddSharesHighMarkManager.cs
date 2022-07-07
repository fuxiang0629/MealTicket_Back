using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddSharesHighMarkManager
    {
    }

    public class AddSharesHighMarkManagerRequest
    {
        public DateTime Date { get; set; }

        public List<long> SharesKeyList { get; set; }
    }

    public class AddSharesHighMarkManager2Request
    {
        public DateTime Date { get; set; }

        public long SharesKey { get; set; }

        public int RiseLimitCountCustom { get; set; }
    }
}
