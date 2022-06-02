using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesKLineList
    {
    }

    public class GetSharesKLineListRequest
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
        /// 类型1.1分钟线 2.5分钟线 3.15分钟线 4.30分钟线 5.60分钟线 6.日线 7.周线 8.月线 9.季度线 10.年线
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 最小时间
        /// </summary>
        public long MinGroupTimeKey { get; set; }

        /// <summary>
        /// 最大时间
        /// </summary>
        public long MaxGroupTimeKey { get; set; }

        /// <summary>
        /// 获取条数
        /// </summary>
        public int GetCount { get; set; }
    }

    public class GetSharesKLineListRes
    {
        /// <summary>
        /// 类型1最新数据（覆盖） 2最新数据累加 3以前数据累加
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 股票唯一数字
        /// SharesCode*10+Market
        /// </summary>
        public int SharesNumber { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<SharesKLineInfo> DataList { get; set; }
    }

    public class BatchGetSharesKLineListRequest
    {
        /// <summary>
        /// 获取条数
        /// </summary>
        public int GetCount { get; set; }

        /// <summary>
        /// 股票列表
        /// </summary>
        public List<BatchGetSharesKLineList_Shares> SharesList { get; set; }
    }

    public class BatchGetSharesKLineList_Shares
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

    public class SharesKLineInfo
    {
        /// <summary>
        /// 时间值
        /// </summary>
        public string DateKey { get; set; }

        public long GroupTimeKey { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public string OpenedPrice { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public string ClosedPrice { get; set; }

        /// <summary>
        /// 上一时段收盘价
        /// </summary>
        public string PreClosedPrice { get; set; }


        /// <summary>
        /// 最低价
        /// </summary>
        public string MinPrice { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public string MaxPrice { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long TradeStockNum { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public string TradeStock { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public string TradeAmount { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public long TradeAmountNum { get; set; }

        /// <summary>
        /// 流通量
        /// </summary>
        public string Tradable { get; set; }

        /// <summary>
        /// 换手率
        /// </summary>
        public string TradableRate { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public string RiseRate { get; set; }

        /// <summary>
        /// 竞价成交量
        /// </summary>
        public string BiddingTradeStock { get; set; }

        /// <summary>
        /// 竞价成交量
        /// </summary>
        public long BiddingTradeStockNum { get; set; }

        /// <summary>
        /// 竞价成交额
        /// </summary>
        public string BiddingTradeAmount { get; set; }

        /// <summary>
        /// 竞价成交额
        /// </summary>
        public long BiddingTradeAmountNum { get; set; }
    }

    public class DB_SharesKLineInfo 
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public long GroupTimeKey { get; set; }

        public DateTime Time { get; set; }

        public long OpenedPrice { get; set; }

        public long ClosedPrice { get; set; }

        public long PreClosePrice { get; set; }

        public long MinPrice { get; set; }

        public long MaxPrice { get; set; }

        public long TradeStock { get; set; }

        public long TradeAmount { get; set; }

        public long Tradable { get; set; }

        public bool IsVaild { get; set; }
    }
}
