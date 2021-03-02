using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountOptionalTrend
    {
    }

    public class FrontAccountOptionalTrendInfo
    {

        /// <summary>
        /// RelId
        /// </summary>
        public long RelId { get; set; }

        /// <summary>
        /// 走势Id
        /// </summary>
        public long TrendId { get; set; }

        /// <summary>
        /// 走势名称
        /// </summary>
        public string TrendName { get; set; }

        /// <summary>
        /// 走势描述
        /// </summary>
        public string TrendDescription { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
