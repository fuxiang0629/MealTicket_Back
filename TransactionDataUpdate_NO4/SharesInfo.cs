using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionDataUpdate_NO4
{
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
        /// 股票Id数字
        /// </summary>
        public int SharesInfoNum { get; set; }
    }

    public class LastData 
    {
        public DateTime Time { get; set; }

        /// <summary>
        /// 股票数字
        /// </summary>
        public int SharesInfoNumArr { get; set; }
    }
}
