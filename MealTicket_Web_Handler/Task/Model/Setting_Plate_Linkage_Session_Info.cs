using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Setting_Plate_Linkage_Session_Info
    {
        /// <summary>
        /// 主版块Id
        /// </summary>
        public long MainPlateId { get; set; }

        /// <summary>
        /// 联动板块Id
        /// </summary>
        public long LinkagePlateId { get; set; }

        /// <summary>
        /// 是否真实概念板块
        /// </summary>
        public bool IsReal { get; set; }
    }

    public class Setting_Plate_Linkage_Session_Info_Group
    {
        /// <summary>
        /// 是否真实概念板块
        /// </summary>
        public bool IsReal { get; set; }

        /// <summary>
        /// 缓存列表
        /// </summary>
        public List<Setting_Plate_Linkage_Session_Info> SessionList { get; set; }
    }
}
