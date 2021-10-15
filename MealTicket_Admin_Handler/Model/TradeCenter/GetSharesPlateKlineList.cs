using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesPlateKlineList
    {
    }

    public class GetSharesPlateKlineListRequest:PageRequest
    {
        /// <summary>
        /// 板块Id
        /// </summary>
       public long PlateId { get; set; }

        /// <summary>
        /// 数据类型 1.分时 2.1分 3.5分 4.15分 5.30分 6.60分 7.日 8.周 9.月 10.季 11.年
        /// </summary>
        public int DateType { get; set; }

        /// <summary>
        /// 查询起始时间
        /// </summary>
        public long StartGroupTimeKey { get; set; }

        /// <summary>
        /// 查询截止时间
        /// </summary>
        public long EndGroupTimeKey { get; set; }
    }
}
