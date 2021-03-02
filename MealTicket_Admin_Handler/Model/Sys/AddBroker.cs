using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddBroker
    {
    }

    public class AddBrokerRequest
    {
        /// <summary>
        /// 券商code
        /// </summary>
        public int BrokerCode { get; set; }

        /// <summary>
        /// 券商名称
        /// </summary>
        public string BrokerName { get; set; }

        /// <summary>
        /// 客户端版本号
        /// </summary>
        public string Version { get; set; }
    }
}
