using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSharesMonitorTrendRel
    {
    }

    public class AddSharesMonitorTrendRelRequest
    {
        /// <summary>
        /// 监控Id
        /// </summary>
        public long MonitorId { get; set; }

        /// <summary>
        /// 走势Id
        /// </summary>
        public List<long> TrendId { get; set; }
    }
}
