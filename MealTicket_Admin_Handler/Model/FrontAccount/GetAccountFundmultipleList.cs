using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetAccountFundmultipleList
    {
    }

    public class AccountFundmultipleInfo
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
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int Range { get; set; }

        /// <summary>
        /// 最高杠杆倍数
        /// </summary>
        public int FundMultiple { get; set; }

        /// <summary>
        /// 用户杠杆倍数
        /// </summary>
        public int AccountFundMultiple { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
