using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesDateGroupForbidList
    {
    }

    public class GetSharesDateGroupForbidListRequest : PageRequest 
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }
    }

    public class SharesDateGroupForbidInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 机器数量
        /// </summary>
        public int DateCount { get; set; }

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
