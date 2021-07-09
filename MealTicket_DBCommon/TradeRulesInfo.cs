using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_DBCommon
{
    public class TradeRulesInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 0系统级 1账户级
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 限制市场代码-1全部市场 0深圳 1上海 
        /// </summary>
        public int LimitMarket { get; set; }

        /// <summary>
        /// 限制关键词
        /// </summary>
        public string LimitKey { get; set; }

        /// <summary>
        /// 限制类型1股票代码 2股票名称
        /// </summary>
        public int LimitType { get; set; }

        /// <summary>
        /// 警戒线（1/万）
        /// </summary>
        public int Cordon { get; set; }

        /// <summary>
        /// 平仓线（1/万）
        /// </summary>
        public int ClosingLine { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<TradeRulesOtherInfo> OtherList { get; set; }
    }

    public class TradeRulesOtherInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 0系统级 1账户级
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 规则Id
        /// </summary>
        public long RulesId { get; set; }

        /// <summary>
        /// 时间段
        /// </summary>
        public string Times { get; set; }

        /// <summary>
        /// 警戒线（1/万）
        /// </summary>
        public int Cordon { get; set; }

        /// <summary>
        /// 平仓线（1/万）
        /// </summary>
        public int ClosingLine { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
    }
}
