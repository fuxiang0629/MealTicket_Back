using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesPlateList
    {
    }

    public class SharesPlateInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

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
    }

    public class GetSharesPlateListRequest:DetailsPageRequest 
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 1所属行业 2所属地区 3所属概念
        /// </summary>
        public int Type { get; set; }
    }
}
