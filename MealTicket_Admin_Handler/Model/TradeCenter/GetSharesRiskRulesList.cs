using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesRiskRulesList
    {
    }

    public class SharesRiskRulesInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 1停牌 2复牌 3涨幅 4跌幅 5最低价 6最高价 7新股
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 规则
        /// </summary>
        public string Rules { get; set; }

        /// <summary>
        /// 禁止状态0正常 1禁止买入 2禁止卖出 3禁止交易
        /// </summary>
        public int ForbidStatus { get; set; }

        /// <summary>
        /// 禁止描述
        /// </summary>
        public string MatchDes { get; set; }

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
