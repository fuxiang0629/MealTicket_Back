using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBannerGroupList
    {
    }

    public class GetBannerGroupListRequest : PageRequest
    {
        /// <summary>
        /// 分组code
        /// </summary>
        public string GroupCode { get; set; }
    }

    public class BannerGroupInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分组code
        /// </summary>
        public string GroupCode { get; set; }

        /// <summary>
        /// 分组描述
        /// </summary>
        public string GroupDes { get; set; }

        /// <summary>
        /// banner数量
        /// </summary>
        public int BannerCount { get; set; }

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
