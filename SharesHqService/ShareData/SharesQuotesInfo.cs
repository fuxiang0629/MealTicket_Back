using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class SharesQuotesInfo
    {
        /// <summary>
        /// 市场代码0深圳 1上海
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 原股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 返回股票代码
        /// </summary>
        public string BackSharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 现价
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public long MaxPrice { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public long MinPrice { get; set; }

        /// <summary>
        /// 总量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 现量
        /// </summary>
        public int PresentCount { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public long TotalAmount { get; set; }

        /// <summary>
        /// 内盘
        /// </summary>
        public int InvolCount { get; set; }

        /// <summary>
        /// 外盘
        /// </summary>
        public int OuterCount { get; set; }

        /// <summary>
        /// 买一价
        /// </summary>
        public long BuyPrice1 { get; set; }

        /// <summary>
        /// 买一量
        /// </summary>
        public int BuyCount1 { get; set; }

        /// <summary>
        /// 买二价
        /// </summary>
        public long BuyPrice2 { get; set; }

        /// <summary>
        /// 买二量
        /// </summary>
        public int BuyCount2 { get; set; }

        /// <summary>
        /// 买三价
        /// </summary>
        public long BuyPrice3 { get; set; }

        /// <summary>
        /// 买三量
        /// </summary>
        public int BuyCount3 { get; set; }

        /// <summary>
        /// 买四价
        /// </summary>
        public long BuyPrice4 { get; set; }

        /// <summary>
        /// 买四量
        /// </summary>
        public int BuyCount4 { get; set; }

        /// <summary>
        /// 买五价
        /// </summary>
        public long BuyPrice5 { get; set; }

        /// <summary>
        /// 买五量
        /// </summary>
        public int BuyCount5 { get; set; }

        /// <summary>
        /// 卖一价
        /// </summary>
        public long SellPrice1 { get; set; }

        /// <summary>
        /// 卖一量
        /// </summary>
        public int SellCount1 { get; set; }

        /// <summary>
        /// 卖二价
        /// </summary>
        public long SellPrice2 { get; set; }

        /// <summary>
        /// 卖二量
        /// </summary>
        public int SellCount2 { get; set; }

        /// <summary>
        /// 卖三价
        /// </summary>
        public long SellPrice3 { get; set; }

        /// <summary>
        /// 卖三量
        /// </summary>
        public int SellCount3 { get; set; }

        /// <summary>
        /// 卖四价
        /// </summary>
        public long SellPrice4 { get; set; }

        /// <summary>
        /// 卖四量
        /// </summary>
        public int SellCount4 { get; set; }

        /// <summary>
        /// 卖五价
        /// </summary>
        public long SellPrice5 { get; set; }

        /// <summary>
        /// 卖五量
        /// </summary>
        public int SellCount5 { get; set; }

        /// <summary>
        /// 涨速
        /// </summary>
        public string SpeedUp { get; set; }

        /// <summary>
        /// 活跃度
        /// </summary>
        public string Activity { get; set; }

        /// <summary>
        /// 涨停价
        /// </summary>
        public long LimitUpPrice { get; set; }

        /// <summary>
        /// 跌停价
        /// </summary>
        public long LimitDownPrice { get; set; }

        /// <summary>
        /// 0普通 1封涨停板 2封跌停板
        /// </summary>
        public int PriceType { get; set; }

        /// <summary>
        /// 0普通 1触发涨停 2触发封跌停
        /// </summary>
        public int TriPriceType { get; set; }

        /// <summary>
        /// 0普通 1接近停板 2接近停板
        /// </summary>
        public int TriNearLimitType { get; set; }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}
