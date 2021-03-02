using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountRecommendPrizeType
    {
    }

    public class FrontAccountRecommendPrizeTypeInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 类别1.开仓费 2.资金占用费 3.印花税 4.佣金 5.盈利分成 6.其他
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 级别数量
        /// </summary>
        public int LevelCount { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }
    }
}
