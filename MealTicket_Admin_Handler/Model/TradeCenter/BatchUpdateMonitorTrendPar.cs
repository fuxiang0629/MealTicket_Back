using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BatchUpdateMonitorTrendPar
    {
    }

    public class BatchUpdateMonitorTrendParRequest
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 走势Id列表
        /// </summary>
        public List<long> TrendIdList { get; set; }
    }
}
