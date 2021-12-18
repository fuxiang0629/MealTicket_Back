using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FXCommon.Common
{
    [Serializable]
    public class SharesPlateRelInfo_Session
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 计算走势最像用
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 计算龙头用
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 计算日内龙头用
        /// </summary>
        public DateTime? LimitUpTime { get; set; }

        /// <summary>
        /// 计算中军用
        /// </summary>
        public long MarketValue { get; set; }
    }
}
