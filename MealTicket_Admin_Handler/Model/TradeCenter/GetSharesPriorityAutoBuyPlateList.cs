using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesPriorityAutoBuyPlateList
    {
    }

    public class GetSharesPriorityAutoBuyPlateListRequest:PageRequest
    { 
        /// <summary>
        /// Id
        /// </summary>
        public long PriorityId { get; set; }

        /// <summary>
        /// 类型1行业 2地区 3概念
        /// </summary>
        public int Type { get; set; }
    }

    public class SharesPriorityAutoBuyPlateInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateName { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
