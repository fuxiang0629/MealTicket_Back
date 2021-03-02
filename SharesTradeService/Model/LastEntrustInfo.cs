using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesTradeService.Model
{
    public class LastEntrustInfo
    {
        /// <summary>
        /// 交易账户code
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// 委托编号
        /// </summary>
        public string EntrustId { get; set; }

        /// <summary>
        /// 获取数据时间
        /// </summary>
        public DateTime CurrTime { get; set; }
    }
}
