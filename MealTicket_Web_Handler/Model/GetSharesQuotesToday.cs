using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesQuotesToday
    {
    }

    public class GetSharesQuotesTodayRes
    {
        /// <summary>
        /// 数据时间
        /// </summary>
        public string DataTime { get; set; }

        /// <summary>
        /// 列表数据
        /// </summary>
        public List<SharesQuotesTodayInfo> List { get; set; }
    }

    public class SharesQuotesTodayInfo 
    {

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 当前价格
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 涨幅
        /// </summary>
        public double Rise
        {
            get
            {
                if (ClosedPrice <= 0 || PresentPrice <= 0)
                {
                    return 0;
                }
                return Math.Round((PresentPrice - ClosedPrice) * 100.0 / ClosedPrice, 2);
            }
        }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime DataTime { get; set; }
    }
}
