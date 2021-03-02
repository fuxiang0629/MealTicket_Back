using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionDataUpdate_NO3
{
    public class SharesTransactionDataInfo
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
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 时间字符串
        /// </summary>
        public string TimeStr { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public long Price { get; set; }

        /// <summary>
        /// 现量
        /// </summary>
        public int Volume { get; set; }

        /// <summary>
        /// 笔数
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 买卖类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
