using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesCycleManagerStockList
    {
    }

    public class SharesCycleManagerStockInfo
    {
        public long Id{ get; set; }
        public long SharesKey { get; set; }

        public int Market { get; set; }

        public string SharesCode { get; set; }

        public string SharesName { get; set; }

        public int Type { get; set; }

        public int Status { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
