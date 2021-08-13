using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    class GetTradeSharesList
    {
    }



    public class GetTradeSharesListRequest
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 上下文
        /// </summary>
        public string Context { get; set; }
    }

    public class GetTradeSharesListRes
    {
        /// <summary>
        /// 上下文
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// 列表数据
        /// </summary>
        public List<TradeSharesInfo> List { get; set; }
    }

    public class TradeSharesInfo
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
        /// 当前价格
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 涨幅
        /// </summary>
        public string Rise { get; set; }

        public DateTime Time { get; set; }
    }
}
