using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountRealTimeTrendRecordDetails
    {
    }

    public class GetAccountRealTimeTrendRecordDetailsRequest : PageRequest
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
        /// 日期
        /// </summary>
        public string Date { get; set; }
    }

    public class AccountRealTimeTrendRecordDetailsInfo
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 触发类型
        /// </summary>
        public string TrendName { get; set; }

        /// <summary>
        /// 触发价格
        /// </summary>
        public long TrendPrice { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 触发触发描述
        /// </summary>
        public string TriDesc { get; set; }
    }
}
