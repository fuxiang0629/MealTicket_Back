using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesPositionRulesList
    {
    }

    public class SharesPositionRulesInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 市场0深圳 1上海
        /// </summary>

        public int LimitMarket { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public string LimitKey { get; set; }

        /// <summary>
        /// 仓位比例
        /// </summary>
        public int Rate { get; set; }

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
