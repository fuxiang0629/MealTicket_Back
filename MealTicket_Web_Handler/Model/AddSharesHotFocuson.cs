using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddSharesHotFocuson
    {
    }

    public class AddSharesHotFocusonRequest
    {
        /// <summary>
        /// 题材Id
        /// </summary>
        public long HotId { get; set; }

        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }
    }

    public class BatchAddSharesHotFocusonRequest
    {

        /// <summary>
        /// 题材Id列表
        /// </summary>
        public List<long> HotIdList { get; set; }

        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }
    }
}
