using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetPushTagList
    {
    }

    public class GetPushTagListRequest : PageRequest
    {
        /// <summary>
        /// 状态0全部 1有效 2无效
        /// </summary>
        public int Status { get; set; }
    }

    public class PushTagInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 触发api
        /// </summary>
        public string TriApiUrl { get; set; }

        /// <summary>
        /// 触发api描述
        /// </summary>
        public string TriApiDes { get; set; }

        /// <summary>
        /// 状态1有效2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
