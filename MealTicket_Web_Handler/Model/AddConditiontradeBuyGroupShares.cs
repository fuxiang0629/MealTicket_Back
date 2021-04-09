using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddConditiontradeBuyGroupShares
    {
    }

    public class AddConditiontradeBuyGroupSharesRequest
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }
    }
}
