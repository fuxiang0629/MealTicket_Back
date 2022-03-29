using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetPlateBaseList
    {
    }

    public class GetPlateBaseListRequest
    {
        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateName { get; set; }

        /// <summary>
        /// 股票唯一Key
        /// </summary>
        public long SharesKey { get; set; }
    }
}
