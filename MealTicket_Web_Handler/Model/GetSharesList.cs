using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesList
    {
    }

    public class GetSharesListRequest
    {
        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }
    }

    public class SharesInfo
    {
        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }
    }
}
