using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesTradeTimeGroupList
    {
    }

    public class SharesTradeTimeGroupInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 市场名称
        /// </summary>
        public string MarketName { get; set; }

        /// <summary>
        /// 限制市场代码-1全部市场 0深圳 1上海 
        /// </summary>
        public int LimitMarket { get; set; }

        /// <summary>
        /// 股票匹配关键字
        /// </summary>
        public string LimitKey { get; set; }

        /// <summary>
        /// 开市竞价时间(格式：09:00:00-11:30:00,13:00:00-15:00:00)
        /// </summary>
        public string Time1 { get; set; }

        /// <summary>
        /// 等待开市时间(格式：09:00:00-11:30:00,13:00:00-15:00:00)
        /// </summary>
        public string Time2 { get; set; }

        /// <summary>
        /// 开市时间(格式：09:00:00-11:30:00,13:00:00-15:00:00)
        /// </summary>
        public string Time3 { get; set; }

        /// <summary>
        /// 尾市竞价时间(格式：09:00:00-11:30:00,13:00:00-15:00:00)
        /// </summary>
        public string Time4 { get; set; }

        /// <summary>
        /// 闭市时间(格式：09:00:00-09:15:00)
        /// </summary>
        public string Time5 { get; set; }

        /// <summary>
        /// 交易时间(格式：09:00:00-09:15:00)
        /// </summary>
        public string Time6 { get; set; }

        /// <summary>
        /// K线时间(格式：09:30:00-11:30:00,13:00:00-15:00:00)
        /// </summary>
        public string Time7 { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
