using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountBuyConditionAutoList
    {
    }

    public class AccountBuyConditionAutoInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

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
        /// 额外关系参数数量
        /// </summary>
        public int OtherParCount { get; set; }

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
