using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class HqServerParInfo
    {
        /// <summary>
        /// 实时行情更新频率（毫秒）
        /// </summary>
        public int SshqUpdateRate { get; set; }

        /// <summary>
        /// 获取实时行情每批次数量（最大80）
        /// </summary>
        public int QuotesCount { get; set; }

        /// <summary>
        /// 股票基础数据起始时间
        /// </summary>
        public TimeSpan AllSharesStartHour { get; set; }

        /// <summary>
        /// 股票基础数据截止时间
        /// </summary>
        public TimeSpan AllSharesEndHour { get; set; }

        /// <summary>
        /// 行情业务定时器（毫秒）
        /// </summary>
        public int BusinessRunTime { get; set; }
    }
}
