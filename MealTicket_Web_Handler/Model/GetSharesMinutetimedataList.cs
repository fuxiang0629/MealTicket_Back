using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesMinutetimedataList
    {
    }

    public class GetSharesMinutetimedataListRequest
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 最大时间
        /// </summary>
        public DateTime? MaxTime { get; set; }
    }

    public class SharesMinutetimedata
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// 均价
        /// </summary>
        public string AvgPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public string ClosedPrice { get; set; }

        /// <summary>
        /// 涨跌额
        /// </summary>
        public string RiseAmount { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public string RiseRate { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public string TradeStock { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public string TradeAmount { get; set; }
    }
}
