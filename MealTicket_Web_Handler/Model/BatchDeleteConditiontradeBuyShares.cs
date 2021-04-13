using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BatchDeleteConditiontradeBuyShares
    {
    }

    public class BatchDeleteConditiontradeBuySharesRequest
    {
        /// <summary>
        /// 删除Id列表
        /// </summary>
        public List<long> List { get; set; }

        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 0删除股票 1仅删除关系
        /// </summary>
        public int Type { get; set; }
    }
}
