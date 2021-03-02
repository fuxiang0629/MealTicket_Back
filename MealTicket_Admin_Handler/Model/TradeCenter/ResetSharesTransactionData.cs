using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ResetSharesTransactionData
    {
    }

    public class ResetSharesTransactionDataRequest
    {
        /// <summary>
        /// 市场0深圳 1上海
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
