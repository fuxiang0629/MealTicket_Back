using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSharesMonitorTrendPar
    {
    }

    public class AddSharesMonitorTrendParRequest
    {
        /// <summary>
        /// 走势Id
        /// </summary>
        public long TrendId { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public string ParamsInfo { get; set; }
    }
}
