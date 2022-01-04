using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Minute_KLine_Session_Info
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 上一分钟收盘价
        /// </summary>
        public long PreClosePrice { get; set; }
    }
}
