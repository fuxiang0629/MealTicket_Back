using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesHotSearch
    {
    }

    public class SharesHotSearchInfo
    {
        /// <summary>
        /// 市场代码0深圳 1上海
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

        /// <summary>
        /// 查询次数
        /// </summary>
        public int SearchCount { get; set; }

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
                if (ClosedPrice <= 0 || PresentPrice<=0)
                {
                    return 0;
                }
                return Math.Round((PresentPrice - ClosedPrice) * 100.0 / ClosedPrice, 2);
            }
        }
    }
}
