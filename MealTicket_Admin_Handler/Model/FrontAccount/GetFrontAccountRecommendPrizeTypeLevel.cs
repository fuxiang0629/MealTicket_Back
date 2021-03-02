using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountRecommendPrizeTypeLevel
    {
    }

    public class FrontAccountRecommendPrizeTypeLevelInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 级别值
        /// </summary>
        public int LevelValue { get; set; }

        /// <summary>
        /// 奖励比例
        /// </summary>
        public int PrizeRate { get; set; }

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
