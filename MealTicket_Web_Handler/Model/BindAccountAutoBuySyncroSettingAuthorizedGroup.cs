using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BindAccountAutoBuySyncroSettingAuthorizedGroup
    {
    }

    public class BindAccountAutoBuySyncroSettingAuthorizedGroupRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 账户关系Id列表
        /// </summary>
        public List<long> AccountIdList { get; set; }
    }
}
