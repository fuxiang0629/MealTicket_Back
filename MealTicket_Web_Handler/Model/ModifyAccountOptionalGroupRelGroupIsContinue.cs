using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyAccountOptionalGroupRelGroupIsContinue
    {
    }

    public class ModifyAccountOptionalGroupRelGroupIsContinueRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 是否持续
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
