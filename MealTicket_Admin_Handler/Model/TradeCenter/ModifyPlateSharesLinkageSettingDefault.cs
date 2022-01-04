using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyPlateSharesLinkageSettingDefault
    {
    }

    public class ModifyPlateSharesLinkageSettingDefaultRequest
    {
        /// <summary>
        /// 主板块Id
        /// </summary>
        public long MainPlateId { get; set; }

        /// <summary>
        /// 是否默认
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
