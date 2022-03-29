using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesHotSpotHisList
    {
    }

    public class GetSharesHotSpotHisListRequest
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        public int DaysType { get; set; }

        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }
    }
}
