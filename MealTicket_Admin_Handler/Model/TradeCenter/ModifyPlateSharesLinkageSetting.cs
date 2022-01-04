using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyPlateSharesLinkageSetting
    {
    }

    public class ModifyPlateSharesLinkageSettingRequest
    {
        /// <summary>
        /// 主板块Id
        /// </summary>
        public long MainPlateId { get; set; }

        /// <summary>
        /// 是否真实在走这个板块
        /// </summary>
        public bool IsReal { get; set; }

        /// <summary>
        /// 联动股票列表
        /// </summary>
        public List<long> LinkageSharesList { get; set; }
    }
}

