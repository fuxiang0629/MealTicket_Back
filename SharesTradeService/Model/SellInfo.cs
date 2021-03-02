using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesTradeService.Model
{
    public class SellInfo
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
        /// 委托类型
        /// </summary>
        public int EntrustType { get; set; }

        /// <summary>
        /// 委托价格
        /// </summary>
        public long EntrustPrice { get; set; }

        /// <summary>
        /// 委托账户
        /// </summary>
        public List<SellDetails> EntrustManagerList { get; set; }
       
        /// <summary>
        /// 卖出数量
        /// </summary>
        public int SellCount { get; set; }
    }

    public class SellDetails 
    {
        /// <summary>
        /// 账户Id
        /// </summary>
        public long EntrustManagerId { get; set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public int EntrustCount { get; set; }
    }
}
