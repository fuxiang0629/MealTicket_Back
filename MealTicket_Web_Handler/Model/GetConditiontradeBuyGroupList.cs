using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetConditiontradeBuyGroupList
    {
    }

    public class GetConditiontradeBuyGroupListRequest:DetailsPageRequest
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string Name { get; set; }
    }

    public class ConditiontradeBuyGroupInfo
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
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }

        /// <summary>
        /// 有效股票数量
        /// </summary>
        public int ValidCount { get; set; }

        /// <summary>
        /// 无效股票数量
        /// </summary>
        public int InValidCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
