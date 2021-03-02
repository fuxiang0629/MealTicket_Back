using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddAccountOptionalGroupRel
    {
    }

    public class AddAccountOptionalGroupRelRequest
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 自选股Id
        /// </summary>
        public long OptionalId { get; set; }

        /// <summary>
        /// 是否持续监控
        /// </summary>
        public bool GroupIsContinue { get; set; }

        /// <summary>
        /// 有效期起始时间
        /// </summary>
        public DateTime? ValidStartTime { get; set; }

        /// <summary>
        /// 有效期截止时间
        /// </summary>
        public DateTime? ValidEndTime { get; set; }
    }
}
