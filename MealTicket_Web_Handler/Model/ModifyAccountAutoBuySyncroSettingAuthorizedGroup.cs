using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyAccountAutoBuySyncroSettingAuthorizedGroup
    {
    }

    public class ModifyAccountAutoBuySyncroSettingAuthorizedGroupRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set;}

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }
    }
}
