using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBackAccountPositionList
    {
    }

    public class GetBackAccountPositionListRequest:PageRequest
    {
        /// <summary>
        /// 职位名称
        /// </summary>
        public string PositionName { get; set; }

        /// <summary>
        /// 状态0全部 1有效 2无效
        /// </summary>
        public int Status { get; set; }
    }

    public class BackAccountPositionInfo
    {
        /// <summary>
        /// 职位Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 职位名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 职位描述
        /// </summary>
        public string Description { get; set; }

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
