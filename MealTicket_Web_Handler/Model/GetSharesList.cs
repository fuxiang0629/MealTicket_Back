using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesList
    {
    }

    public class GetSharesListRequest
    {
        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }

        /// <summary>
        /// 是否所有股票
        /// </summary>
        public bool IsAll { get; set; }

        /// <summary>
        /// 是否获取实时信息
        /// </summary>
        public bool RealInfo { get; set; }
    }

    public class SharesInfo
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
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 总参数数量
        /// </summary>
        public int ParTotalCount { get; set; }

        /// <summary>
        /// 有效参数数量
        /// </summary>
        public int ParValidCount { get; set; }

        /// <summary>
        /// 已执行参数数量
        /// </summary>
        public int ParExecuteCount { get; set; }

        /// <summary>
        /// 当前价格
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 昨日收盘价格
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 最大涨跌幅
        /// </summary>
        public long Range { get; set; }

        public long MonitorId { get; set; }
    }
}
