using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesPremiumStatisticConditionList
    {
    }

    public class GetSharesPremiumStatisticConditionListRequest
    {
        /// <summary>
        /// 状态0全部 1有效
        /// </summary>
        public int Status { get; set; }
    }

    public class SharesPremiumStatisticConditionInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int OrderIndex { get; set; }

        public string ConditionStr { get; set; }

        public int Status { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
