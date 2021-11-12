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
        /// 日期
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// 最大时间
        /// </summary>
        public DateTime? MaxTime { get; set; }
    }

    public class GetSharesMinutetimedataListRes
    {
        /// <summary>
        /// 是否今日数据
        /// </summary>
        public bool IsToday { get; set; }

        /// <summary>
        /// 数据日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 股票唯一数字
        /// </summary>
        public long SharesNumber { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<SharesMinutetimedata> List { get; set; }
    }

    public class BatchGetSharesMtLineListRequest
    {
        /// <summary>
        /// 股票列表
        /// </summary>
        public List<BatchGetSharesMtLineList_Shares> SharesList { get; set; }
    }

    public class BatchGetSharesMtLineList_Shares
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
        /// 最大时间
        /// </summary>
        public long MaxGroupTimeKey { get; set; }
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

        /// <summary>
        /// 成交量
        /// </summary>
        public long TradeStockLong { get; set; }
    }

    public class DB_SharesMinutetimeInfo 
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public long GroupTimeKey { get; set; }

        public DateTime Time { get; set; }

        public long ClosedPrice { get; set; }

        public long YestodayClosedPrice { get; set; }

        public long TradeStock { get; set; }

        public long TradeAmount { get; set; }

        public long LastTradeStock { get; set; }

        public long LastTradeAmount { get; set; }
    }
}
