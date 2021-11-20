using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FXCommon.Common
{
    public class SharesPlateQuotesInfo_New_Session
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 1不加权 2加权
        /// </summary>
        public int WeightType { get; set; }

        /// <summary>
        /// 当前价格
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 今日开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public long MaxPrice { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public long MinPrice { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long TodayDealCount { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public long TodayDealAmount { get; set; }

        /// <summary>
        /// 流通股本
        /// </summary>
        public long CirculatingCapital { get; set; }

        /// <summary>
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }

        /// <summary>
        /// 涨停数量
        /// </summary>
        public int RiseLimitCount { get; set; }

        /// <summary>
        /// 跌停数量
        /// </summary>
        public int DownLimitCount { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int RiseRate
        {
            get
            {
                if (ClosedPrice == 0)
                {
                    return 0;
                }
                return (int)Math.Round((PresentPrice - ClosedPrice) * 10000.0 / ClosedPrice, 0);
            }
        }

        /// <summary>
        /// 涨跌幅排名
        /// </summary>
        public int Rank { get; set; }
    }
}
