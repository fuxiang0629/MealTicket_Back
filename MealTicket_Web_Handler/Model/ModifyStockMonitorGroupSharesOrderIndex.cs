using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyStockMonitorGroupSharesOrderIndex
    {
    }

    public class ModifyStockMonitorGroupSharesOrderIndexRequest
    {
        public long GroupId { get; set; }

        public List<StockMonitorGroupSharesOrderIndexInfo> SharesList { get; set; }
    }

    public class StockMonitorGroupSharesOrderIndexInfo
    {
        public long SharesKey { get; set; }

        public int OrderIndex { get; set; }
    }
}
