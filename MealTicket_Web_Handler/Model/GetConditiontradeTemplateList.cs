using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetConditiontradeTemplateList
    {
    }

    public class GetConditiontradeTemplateListRequest : PageRequest
    {
        /// <summary>
        /// 1买入 2卖出
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 是否获取所有模板
        /// </summary>
        public bool IsGetAll { get; set; }
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
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 1客户模板 2系统模板
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 唯一值
        /// </summary>
        public long Key 
        {
            get 
            {
                return Id * 10 + Type;
            }
        }
    }
}
