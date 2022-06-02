using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddSharesCycleManager
    {
    }

    public class AddSharesCycleManagerRequest
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否默认激活
        /// </summary>
        public bool DefaultActive { get; set; }

        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public List<long> PlateId { get; set; }

        /// <summary>
        /// 起始日期
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 周期类型1主周期 2子周期
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 线条颜色
        /// </summary>
        public string LineColor { get; set; }

        /// <summary>
        /// 背景色
        /// </summary>
        public string BgColor { get; set; }
    }
}
