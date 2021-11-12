using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountSearchReTri
    {
    }

    public class AccountSearchReTriInfo
    {
        public long Id { get; set; }

        public int MinPushMaxRateInterval { get; set; }

        public int MinPushMinRateInterval { get; set; }

        public int MinPushRiseRateInterval { get; set; }

        public int MinPushDownRateInterval { get; set; }
    }
}
