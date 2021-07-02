using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionDataUpdate_NO4
{
    public class TransactiondataAnalyseInfo
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set;}

        /// <summary>
        /// 股票唯一数字
        /// </summary>
        public int SharesInfoNum { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 成交笔数
        /// </summary>
        public int iTradeStock { get; set; }

        /// <summary>
        /// 成交金额
        /// </summary>
        public long lTradeAmount { get; set; }

        /// <summary>
        /// 开盘价格
        /// </summary>
        public long iOpeningPrice { get; set; }

        /// <summary>
        /// 收盘价格
        /// </summary>
        public long iLastestPrice { get; set; }

        /// <summary>
        /// 最低价格
        /// </summary>
        public long iMinPrice { get; set; }

        /// <summary>
        /// 最高价格
        /// </summary>
        public long iMaxPrice { get; set; }
    }

    public class TransactiondataTaskQueueInfo
    {
        /// <summary>
        /// 任务Guid
        /// </summary>
        public string TaskGuid { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<TransactiondataAnalyseInfo> DataList { get; set; }
    }
}
