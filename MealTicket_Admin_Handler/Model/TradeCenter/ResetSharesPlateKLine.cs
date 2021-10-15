using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ResetSharesPlateKLine
    {
    }

    public class ResetSharesPlateKLineRequest
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        ///  数据类型 2.1分钟K线 3.5分钟K线 4.15分钟K线 5.30分钟K线 6.60分钟K线 7.日K 8.周K 9.月K 10.季度K 11.年K
        /// </summary>
        public int DataType { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public long StartGroupTimeKey { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public long EndGroupTimeKey { get; set; }
    }
}
