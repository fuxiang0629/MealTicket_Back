using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetConditiontradeTemplateList
    {
    }

    public class GetConditiontradeTemplateListRequest : PageRequest
    {
        /// <summary>
        /// 1买入 2卖出 3自动加入
        /// </summary>
        public int Type { get; set; }
    }

    public class ConditiontradeTemplateInfo
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
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 背景色
        /// </summary>
        public string BgColor { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
