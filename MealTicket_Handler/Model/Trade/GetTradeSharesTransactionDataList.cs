using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetTradeSharesTransactionDataList
    {
    }

    public class GetTradeSharesTransactionDataListRequest 
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 市场代码0深圳 1上海
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 最大排序值
        /// </summary>
        public int MaxOrderIndex { get; set; }

        /// <summary>
        /// 最小排序值
        /// </summary>
        public int MinOrderIndex { get; set; }

        /// <summary>
        /// 获取数量
        /// </summary>
        public int TakeCount { get; set; }
    }

    public class GetTradeSharesTransactionDataListRes
    {
        /// <summary>
        /// 最大排序值
        /// </summary>
        public int MaxOrderIndex { get; set; }

        /// <summary>
        /// 最小排序值
        /// </summary>
        public int MinOrderIndex { get; set; }

        /// <summary>
        /// 列表数据
        /// </summary>
        public List<TradeSharesTransactionDataInfo> List { get; set; }
    }

    public class TradeSharesTransactionDataInfo
    {
        /// <summary>
        /// 市场0深圳 1上海
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string TimeStr { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public long Price { get; set; }

        /// <summary>
        /// 成交笔数
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 类型0买入 1卖出 2中性
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
