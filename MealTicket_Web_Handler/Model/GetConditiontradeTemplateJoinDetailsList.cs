using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetConditiontradeTemplateJoinDetailsList
    {
    }

    public class GetConditiontradeTemplateJoinDetailsListRequest : PageRequest
    {
        /// <summary>
        /// 模板Id
        /// </summary>
        public long TemplateId { get; set; }
    }

    public class ConditiontradeTemplateJoinDetailsInfo
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
        /// 是否清除原数据
        /// </summary>
        public bool IsClearOriginal { get; set; }

        /// <summary>
        /// 周期类型1小时 2天 3周 4月 5年
        /// </summary>
        public int TimeCycleType { get; set; }

        /// <summary>
        /// 周期次数
        /// </summary>
        public int TimeCycle { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 触发条件数量
        /// </summary>
        public int OtherCount { get; set; }
    }
}
