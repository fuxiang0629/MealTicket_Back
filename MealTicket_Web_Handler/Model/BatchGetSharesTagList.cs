using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BatchGetSharesTagList
    {
    }

    public class BatchGetSharesTagListRequest
    {
        public DateTime Date { get; set; }

        public List<long> SharesKeyList { get; set; }
    }
}
