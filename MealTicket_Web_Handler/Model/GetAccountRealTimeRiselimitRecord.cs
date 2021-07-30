using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountRealTimeRiselimitRecord
    {
    }

    public class GetAccountRealTimeRiselimitRecordRequest:PageRequest
    {
        /// <summary>
        /// 类型1.即将涨停 2涨停池 3炸板池 4即将跌停 5跌停池 6撬板池
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime? Date { get; set; }
    }

    public class AccountRealTimeRiselimitRecordInfo
    {
        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 当日触发数量
        /// </summary>
        public int TriCount { get; set; }

        /// <summary>
        /// 当前价格
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 当前涨跌幅(1/万)
        /// </summary>
        public int RiseRate
        {
            get
            {
                if (ClosedPrice <= 0 || CurrPrice <= 0)
                {
                    return 0;
                }
                return (int)((CurrPrice - ClosedPrice) * 1.0 / ClosedPrice * 10000);
            }
        }

        /// <summary>
        /// 最新触发价格
        /// </summary>
        public long LastTriPrice { get; set; }

        /// <summary>
        /// 最新触发涨跌幅(1/万)
        /// </summary>
        public int LastTriRiseRate
        {
            get
            {
                if (ClosedPrice <= 0 || LastTriPrice <= 0)
                {
                    return 0;
                }
                return (int)((LastTriPrice - ClosedPrice) * 1.0 / ClosedPrice * 10000);
            }
        }

        /// <summary>
        /// 最新触发时间
        /// </summary>
        public DateTime LastTime { get; set; }
    }
}
