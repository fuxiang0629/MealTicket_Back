using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ImportConditiontradeTemplateBuy
    {
    }

    public class ImportConditiontradeTemplateBuyRequest
    {
        /// <summary>
        /// 导入目标
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 模板Id
        /// </summary>
        public long TemplateId { get; set; }

        /// <summary>
        /// 1客户模板 2系统模板
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 是否清除
        /// </summary>
        public bool IsClear { get; set; }

        /// <summary>
        /// 是否自动生效
        /// </summary>
        public bool IsAuto { get; set; }

        /// <summary>
        /// 股票列表
        /// </summary>
        public List<string> SharesList { get; set; }

        /// <summary>
        /// 跟投用户
        /// </summary>
        public List<long> FollowList { get; set; }

        /// <summary>
        /// 1单个导入（可修改价格） 2批量导入
        /// </summary>
        public int Model { get; set; }

        /// <summary>
        /// 详细数据
        /// </summary>
        public List<ConditiontradeTemplateBuyDetailsInfo> TemplateDetailsList { get; set; }
    }
}
