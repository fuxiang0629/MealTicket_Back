using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesRiskRules
    {
    }

    public class ModifySharesRiskRulesRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 规则
        /// </summary>
        public string Rules { get; set; }

        /// <summary>
        /// 禁止状态1禁止买入 2禁止卖出 3禁止交易
        /// </summary>
        public int ForbidStatus { get; set; }

        /// <summary>
        /// 禁止描述
        /// </summary>
        public string MatchDes { get; set; }
    }
}
