using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesPlateList
    {
    }

    public class GetSharesPlateListRequest:PageRequest
    {
        /// <summary>
        /// 名称搜索
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 1.行业管理 2.地区管理 3.概念管理
        /// </summary>
        public int Type { get; set; }
    }

    public class SharesPlateInfo
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
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
