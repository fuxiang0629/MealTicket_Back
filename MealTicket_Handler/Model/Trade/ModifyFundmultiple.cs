using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class ModifyFundmultiple
    {
    }

    public class ModifyFundmultipleRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long FundmultipleId { get; set; }

        /// <summary>
        /// 倍数
        /// </summary>
        public int FundMultiple { get; set; }
    }
}
