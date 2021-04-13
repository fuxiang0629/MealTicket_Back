using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyConditiontradeBuySharesMyGroup
    {
    }

    public class ModifyConditiontradeBuySharesMyGroupRequest
    {
        /// <summary>
        /// 分组列表
        /// </summary>
        public List<long> GroupList { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 股票code
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }
    }
}
