using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyAccountSharesGroupShareActive
    {
    }

    public class ModifyAccountSharesGroupShareActiveRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分组Id列表
        /// </summary>
        public List<long> GroupIdList { get; set; }
    }
}
