using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class SharesBaseInfo
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string ShareCode { get; set; }

        /// <summary>
        /// 股票一手数量
        /// </summary>
        public int ShareHandCount { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string ShareName { get; set; }

        /// <summary>
        /// 拼音简称
        /// </summary>
        public string Pyjc { get; set; }

        /// <summary>
        /// 股票昨日收盘价格
        /// </summary>
        public long ShareClosedPrice { get; set; }

        /// <summary>
        /// 市场0深证 1上海
        /// </summary>
        public int Market { get; set; }
    }
}
