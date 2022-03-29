using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BatchDeleteSharesHotSpot
    {
    }

    public class BatchDeleteSharesHotSpotRequest
    {
        /// <summary>
        /// Id列表
        /// </summary>
        public List<long> IdList { get; set; }
    }
}
