using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FXCommon.Common
{
    public class AccountRiseLimitTriInfo_Session
    {
        /// <summary>
        /// 唯一Id
        /// </summary>
        public string RelId { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 现价
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 类型-1涨停池 -2跌停池 -3即将涨停池 -4即将跌停池 -5炸板池 -6翘板池
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 触发推送时间
        /// </summary>
        public DateTime PushTime { get; set; }

        /// <summary>
        /// 今日触发次数
        /// </summary>
        public int TriCountToday { get; set; }

        /// <summary>
        /// 标记
        /// </summary>
        public string Mark { get; set; }

        /// <summary>
        /// 昨日涨跌停时间
        /// </summary>
        public DateTime? YesLimitTime { get; set; }

        /// <summary>
        /// 涨停连板次数
        /// </summary>
        public int LimitUpDays { get; set; }
    }
}
