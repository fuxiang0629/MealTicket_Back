using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSharesPositionRulesOther
    {
    }

    public class AddSharesPositionRulesOtherRequest
    {
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
    }
}
