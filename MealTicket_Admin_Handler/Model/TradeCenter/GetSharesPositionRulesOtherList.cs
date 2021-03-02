using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesPositionRulesOtherList
    {
    }

    public class SharesPositionRulesOtherInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 仓位规则Id
        /// </summary>
        public long QuotaId { get; set; }

        /// <summary>
        /// 账户code
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// 仓位占比
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
