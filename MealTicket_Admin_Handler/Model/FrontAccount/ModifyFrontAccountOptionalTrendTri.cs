using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyFrontAccountOptionalTrendTri
    {
    }

    public class ModifyFrontAccountOptionalTrendTriRequest
    {
        /// <summary>
        /// RelId
        /// </summary>
        public long RelId { get; set; }

        /// <summary>
        /// 最小触发推送时间间隔
        /// </summary>
        public int MinPushTimeInterval { get; set; }

        /// <summary>
        /// 最小触发推送涨幅间隔
        /// </summary>
        public int MinPushRateInterval { get; set; }

        /// <summary>
        /// 最小触发推送价格
        /// </summary>
        public long MinTodayPrice { get; set; }
    }
}
