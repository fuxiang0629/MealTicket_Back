using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountRealTimeRiselimitRecordDetails
    {
    }

    public class GetAccountRealTimeRiselimitRecordDetailsRequest : PageRequest
    {
        /// <summary>
        /// 类型
        /// </summary>
        public int Type { get; set; }

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

    public class AccountRealTimeRiselimitRecordDetailsInfo
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
        /// 触发价格
        /// </summary>
        public long TrendPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 最新触发涨跌幅(1/万)
        /// </summary>
        public int TriRiseRate
        {
            get
            {
                if (ClosedPrice <= 0 || TrendPrice <= 0)
                {
                    return 0;
                }
                return (int)((TrendPrice - ClosedPrice) * 1.0 / ClosedPrice * 10000);
            }
        }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
