using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetPlateLinkageSettingList
    {
    }

    public class PlateLinkageSettingInfo
    {
        /// <summary>
        /// 主板块Id
        /// </summary>
        public long MainPlateId { get; set; }

        /// <summary>
        /// 主板块名称
        /// </summary>
        public string MainPlateName { get; set; }

        /// <summary>
        /// 联动板块列表
        /// </summary>
        public List<long> LinkagePlateList { get; set; }
    }

    public class PlateLinkageAllInfo
    {
        /// <summary>
        /// 板块类型
        /// </summary>
        public long PlateType { get; set; }

        /// <summary>
        /// 板块类型名称
        /// </summary>
        public string PlateTypeName { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<PlateNameInfo> PlateList { get; set; }
    }

    public class PlateNameInfo 
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateName { get; set; }
    }
}
