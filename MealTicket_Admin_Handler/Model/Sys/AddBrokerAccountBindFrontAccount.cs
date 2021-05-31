using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddBrokerAccountBindFrontAccount
    {
    }

    public class AddBrokerAccountBindFrontAccountRequest
    {
        /// <summary>
        /// 券商账户Id
        /// </summary>
        public long BrokerAccountId { get; set; }

        /// <summary>
        /// 前端账户Id
        /// </summary>
        public long FrontAccountId { get; set; }
    }
}
