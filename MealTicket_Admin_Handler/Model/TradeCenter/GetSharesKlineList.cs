using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesKlineList
    {
    }

    public class GetSharesKlineListRequest : PageRequest
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
        /// 数据类型 1.分时 2.1分 3.5分 4.15分 5.30分 6.60分 7.日 8.周 9.月 10.季 11.年
        /// </summary>
        public int DateType { get; set; }

        /// <summary>
        /// 查询起始时间
        /// </summary>
        public long StartGroupTimeKey { get; set; }

        /// <summary>
        /// 查询截止时间
        /// </summary>
        public long EndGroupTimeKey { get; set; }
    }

    public class SharesKlineInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Date
        {
            get
            {
                return Time.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 时间
        /// </summary>
        public string TimeStr
        {
            get
            {
                return Time.ToString("HH:mm");
            }
        }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public long MinPrice { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public long MaxPrice { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long TradeStock { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public long TradeAmount { get; set; }

        /// <summary>
        /// 数据更新时间
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}
