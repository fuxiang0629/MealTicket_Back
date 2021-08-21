using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_CacheCommon_Session.session
{
    public class SharesPlateQuotesInfo_Session
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public long RiseRate { get; set; }

        /// <summary>
        /// 指数
        /// </summary>
        public long RiseIndex { get; set; }

        /// <summary>
        /// 加权涨跌幅
        /// </summary>
        public long WeightRiseRate { get; set; }

        /// <summary>
        /// 加权指数
        /// </summary>
        public long WeightRiseIndex { get; set; }

        /// <summary>
        /// 涨停数量
        /// </summary>
        public int RiseLimitCount { get; set; }

        /// <summary>
        /// 跌停数量
        /// </summary>
        public int DownLimitCount { get; set; }
    }
}
