using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetStockMonitorGroupSharesList
    {
    }

    public class StockMonitorGroupSharesInfo
    {
        public long SharesKey { get; set; }

        public int Market { get; set; }

        public string SharesCode { get; set; }

        public string SharesName { get; set; }

        public int OrderIndex { get; set; }
    }
}
