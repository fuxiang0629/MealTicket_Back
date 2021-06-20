using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesPlateTypeCount
    {
    }

    public class SharesPlateTypeCount
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
        /// 是否包含基础板块
        /// </summary>
        public int IsBasePlate { get; set; }

        /// <summary>
        /// 板块数量
        /// </summary>
        public int PlateCount { get; set; }

        /// <summary>
        /// 数据更新时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
