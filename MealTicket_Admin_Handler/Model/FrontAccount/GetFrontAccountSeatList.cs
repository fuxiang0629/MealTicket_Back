using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSeatList
    {
    }

    public class GetFrontAccountSeatListRequest : DetailsPageRequest
    {
        /// <summary>
        /// 席位名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 状态0全部 1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 过期状态0全部 1未过期 2已过期
        /// </summary>
        public int ExpireStatus{get;set;}
    }

    public class FrontAccountSeatInfo
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
        /// 有效期
        /// </summary>
        public string ValidEndTimeStr { get; set; }

        /// <summary>
        /// 有效期时间
        /// </summary>
        public DateTime ValidEndTime { get; set; }

        /// <summary>
        /// 绑定的自选股Id
        /// </summary>
        public long OptionalId { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public int SourceFrom { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
