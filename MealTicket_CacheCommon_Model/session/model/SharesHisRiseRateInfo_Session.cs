using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_CacheCommon_Session.session
{
    public class SharesHisRiseRateInfo_Session
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 昨日涨跌幅
        /// </summary>
        public int YestodayRiseRate { get; set; }

        /// <summary>
        /// 前2日涨跌幅
        /// </summary>
        public int TwoDaysRiseRate { get; set; }

        /// <summary>
        /// 前3日涨跌幅
        /// </summary>
        public int ThreeDaysRiseRate { get; set; }

        /// <summary>
        /// x日平均成交量
        /// </summary>
        public int DaysAvgDealCount { get; set; }

        /// <summary>
        /// x日平均成交额
        /// </summary>
        public long DaysAvgDealAmount { get; set; }

        /// <summary>
        /// 上一日成交量
        /// </summary>
        public int PreDayDealCount { get; set; }

        /// <summary>
        /// 上一日成交额
        /// </summary>
        public long PreDayDealAmount { get; set; }

        /// <summary>
        /// x日涨停次数
        /// </summary>
        public int LimitUpCount { get; set; }

        /// <summary>
        /// x日跌停次数
        /// </summary>
        public int LimitDownCount { get; set; }

        /// <summary>
        /// 最近涨停日期
        /// </summary>
        public string LimitUpDay { get; set; }
    }
}
