using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_CacheCommon_Session.session
{
    public class SharesPlateInfo_Session
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateName { get; set; }

        /// <summary>
        /// 板块类型
        /// </summary>
        public int PlateType { get; set; }

        /// <summary>
        /// 基础板块状态
        /// </summary>
        public int BaseStatus { get; set; }

        /// <summary>
        /// 业务板块状态
        /// </summary>
        public int ChooseStatus { get; set; }
    }
}
