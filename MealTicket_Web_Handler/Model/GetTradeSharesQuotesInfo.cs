using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetTradeSharesQuotesInfo
    {
    }

    public class TradeSharesQuotesInfo
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
        /// 持仓数量
        /// </summary>
        public int HoldCount { get; set; }

        /// <summary>
        /// 可卖数量
        /// </summary>
        public int CanSoldCount { get; set; }

        /// <summary>
        /// 盈亏
        /// </summary>
        public long ProfitAmount { get; set; }

        /// <summary>
        /// 一手多少股
        /// </summary>
        public int HandCount { get; set; }

        /// <summary>
        /// 持仓Id
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        /// 杠杆倍数
        /// </summary>
        public int FundMultiple { get; set; }

        /// <summary>
        /// 总涨跌幅
        /// </summary>
        public int TotalRise { get; set; }
    }

    public class GetTradeSharesQuotesInfoRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 市场代码0深圳 1上海
        /// </summary>
        public int Market { get; set; }
    }
}
