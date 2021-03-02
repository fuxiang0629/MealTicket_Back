using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesTradeRulesList
    {
    }

    public class GetSharesTradeRulesListRequest : PageRequest
    {
        /// <summary>
        /// 股票匹配关键字
        /// </summary>
        public string SharesKey { get; set; }

        /// <summary>
        /// 股票匹配类型0全部 1股票代码 2股票名称
        /// </summary>
        public int SharesType { get; set; }
    }

    public class SharesTradeRulesInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 股票关键字
        /// </summary>
        public string LimitKey { get; set; }

        /// <summary>
        /// 股票类型1股票代码 2股票名称
        /// </summary>
        public int LimitType { get; set; }

        /// <summary>
        /// 限制市场代码-1全部市场 0深圳 1上海 
        /// </summary>
        public int LimitMarket { get; set; }

        /// <summary>
        /// 警戒线（1/万）
        /// </summary>
        public int Cordon { get; set; }

        /// <summary>
        /// 平仓线（1/万）
        /// </summary>
        public int ClosingLine { get; set; }

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
