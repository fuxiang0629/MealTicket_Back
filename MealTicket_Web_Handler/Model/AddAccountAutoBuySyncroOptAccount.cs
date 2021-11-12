using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddAccountAutoBuySyncroOptAccount
    {
    }

    public class AddAccountAutoBuySyncroOptAccountRequest
    {
        /// <summary>
        /// 设置Id
        /// </summary>
        public long SettingId { get; set; }

        /// <summary>
        /// 操作账户Id
        /// </summary>
        public long OptAccountId { get; set; }
    }
}
