using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class SharesBarsInfo
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
        /// k线类型0.5 分钟 K 线 1.15 分钟 K 线 2.30 分钟 K 线 3.1 小时 K 线 4.日 K 线 5.周 K 线 6.月 K 线 8.1 分钟 K 线 10.季 K 线 11.年 K 线
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 收盘价
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
        /// 成交量
        /// </summary>
        public int Volume { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public long Turnover { get; set; }
    }
}
