using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddConditiontradeTemplateJoinDetails
    {
    }

    public class AddConditiontradeTemplateJoinDetailsRequest
    {
        /// <summary>
        /// 模板Id
        /// </summary>
        public long TemplateId { get; set; }

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
    }
}
