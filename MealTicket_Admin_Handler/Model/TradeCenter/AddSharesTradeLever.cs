using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSharesTradeLever
    {
    }

    public class AddSharesTradeLeverRequest
    {
        /// <summary>
        /// 市场名称
        /// </summary>
        public string MarketName { get; set; }

        /// <summary>
        /// 限制市场代码-1全部市场 0深圳 1上海 
        /// </summary>
        public int LimitMarket { get; set; }

        /// <summary>
        /// 股票匹配关键字
        /// </summary>
        public string LimitKey { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int Range { get; set; }

        /// <summary>
        /// 接近涨跌停幅度
        /// </summary>
        public int NearLimitRange { get; set; }

        /// <summary>
        /// 今日高开幅度
        /// </summary>
        public int HighOpenRange { get; set; }

        /// <summary>
        /// 今日低开幅度
        /// </summary>
        public int LowOpenRange { get; set; }

        /// <summary>
        /// 杠杆倍数
        /// </summary>
        public int FundMultiple { get; set; }
    }
}
