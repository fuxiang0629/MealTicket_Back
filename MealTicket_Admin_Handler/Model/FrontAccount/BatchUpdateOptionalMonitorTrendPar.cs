using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BatchUpdateOptionalMonitorTrendPar
    {
    }

    public class BatchUpdateOptionalMonitorTrendParRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

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
