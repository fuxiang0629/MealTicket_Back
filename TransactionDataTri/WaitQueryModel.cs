using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionDataTri
{
    public class WaitQueryModel
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 最后发送时间
        /// </summary>
        public DateTime? LastSendTime { get; set; }

        /// <summary>
        /// 是否需要发送数据0.放弃发送 1等待发送 2已发送
        /// </summary>
        public int IsSend { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime TriTime { get; set; }

        /// <summary>
        /// 计时器
        /// </summary>
        public Timer timer { get; set; }
    }
}
