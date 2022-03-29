using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesAllLimitUpHisList
    {
    }

    public class GetSharesAllLimitUpHisListRequest
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        public int DaysType { get; set; }
    }
}
