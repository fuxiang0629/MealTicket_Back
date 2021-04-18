using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesTradeService.Model
{
    public class BuyInfo
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
        /// 委托价格
        /// </summary>
        public long EntrustPrice { get; set; }

        /// <summary>
        /// 一手数量
        /// </summary>
        public int SharesHandCount { get; set; }

        /// <summary>
        /// 购买列表
        /// </summary>
        public List<BuyDetails> BuyList { get; set; }

        /// <summary>
        /// 买入数量
        /// </summary>
        public int BuyCount { get; set; }

        /// <summary>
        /// 0客户买入 1自动买入
        /// </summary>
        public int Type { get; set; }
    }

    public class BuyDetails 
    {
        /// <summary>
        /// 委托Id
        /// </summary>
        public long EntrustId { get; set; }

        /// <summary>
        /// 买入数量
        /// </summary>
        public int BuyCount { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }
    }
}
