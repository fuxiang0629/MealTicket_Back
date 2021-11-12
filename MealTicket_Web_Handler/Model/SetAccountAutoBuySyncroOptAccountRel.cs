using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class SetAccountAutoBuySyncroOptAccountRel
    {
    }

    public class SetAccountAutoBuySyncroOptAccountRelRequest
    {
        /// <summary>
        /// 设置Id
        /// </summary>
        public long SettingId { get; set; }

        public List<long> IdList { get; set; }
    }
}
