using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyAccountRealTimeTrendTriGroup
    {
    }

    public class ModifyAccountRealTimeTrendTriGroupRequest
    {
        /// <summary>
        /// 自选股Id
        /// </summary>
        public long OptionalId { get; set; }

        /// <summary>
        /// 分组列表
        /// </summary>
        public List<AccountTrendTriInfoGroup> GroupList { get; set; }
    }

    public class AccountTrendTriInfoGroup
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

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
