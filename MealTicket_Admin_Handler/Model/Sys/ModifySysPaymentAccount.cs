using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySysPaymentAccount
    {
    }

    public class ModifySysPaymentAccountRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set;}

        /// <summary>
        /// 账户名称
        /// </summary>
        public string Name { get; set; }
    }
}
