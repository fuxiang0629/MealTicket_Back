using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityBarsDataUpdate
{
    public class SecurityBarsDataTaskQueueInfo
    {
        /// <summary>
        /// 任务Guid
        /// </summary>
        public string TaskGuid { get; set; }

        /// <summary>
        /// 总包数量
        /// </summary>
        public int TotalPacketCount { get; set; }

        /// <summary>
        /// 回调包数量
        /// </summary>
        public int CallBackPacketCount { get; set; }

        /// <summary>
        /// 数据包扔出时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int TaskTimeOut { get; set; }

        /// <summary>
        /// 数据列表最大下标
        /// </summary>
        public int DataIndex { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<SecurityBarsDataInfo> DataList { get; set; }
    }

    public class SecurityBarsDataInfo
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
        public DateTime Date { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string TimeStr { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public long MinPrice { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public long MaxPrice { get; set; }

        /// <summary>
        /// 成交量(笔)
        /// </summary>
        public int TradeStock { get; set; }

        /// <summary>
        /// 成交额(元*10000)
        /// </summary>
        public long TradeAmount { get; set; }

        /// <summary>
        /// 上一时段收盘价
        /// </summary>
        public long PreClosePrice { get; set; }
    }
}
