using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesLimitAutoBuyList
    {
    }

    public class SharesLimitAutoBuyInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 限制市场类型0深圳 1上海 -1全部
        /// </summary>
        public int LimitMarket { get; set; }

        /// <summary>
        /// 限制股票匹配代码
        /// </summary>
        public string LimitKey { get; set; }

        /// <summary>
        /// 最大可购买账户数量
        /// </summary>
        public int MaxBuyCount { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
