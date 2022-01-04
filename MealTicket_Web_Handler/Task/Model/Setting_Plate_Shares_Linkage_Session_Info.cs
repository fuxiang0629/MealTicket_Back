using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Setting_Plate_Shares_Linkage_Session_Info
    {
        /// <summary>
        /// 主板块Id
        /// </summary>
        public long MainPlateId { get; set; }

        /// <summary>
        /// 股票市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 是否真实概念板块
        /// </summary>
        public bool IsReal { get; set; }

        /// <summary>
        /// 是否默认真实股票
        /// </summary>
        public bool IsDefault { get; set; }
    }

    public class Setting_Plate_Shares_Linkage_Session_Info_Group
    {
        /// <summary>
        /// 是否真实概念板块
        /// </summary>
        public bool IsReal { get; set; }

        /// <summary>
        /// 是否默认真实股票
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 缓存列表
        /// </summary>
        public List<Setting_Plate_Shares_Linkage_Session_Info> SessionList { get; set; }
    }
}
