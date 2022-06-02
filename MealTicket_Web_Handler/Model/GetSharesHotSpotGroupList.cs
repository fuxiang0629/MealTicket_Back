using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesHotSpotGroupList
    {
    }

    public class GetSharesHotSpotGroupListRequest:PageRequest
    {
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        public bool GetAll { get; set; }
    }

    public class SharesHotSpotGroupInfo
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
        /// 数据时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 题材数量
        /// </summary>
        public int HotspotCount { get; set; }

        public int ValidHotspotCount { get; set; }
    }
}
