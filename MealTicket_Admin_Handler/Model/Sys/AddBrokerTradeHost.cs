using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddBrokerTradeHost
    {
    }

    public class AddBrokerTradeHostRequest
    {
        /// <summary>
        /// 券商Code
        /// </summary>
        public int BrokerCode { get; set; }

        /// <summary>
        /// Ip地址
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }
    }
}
