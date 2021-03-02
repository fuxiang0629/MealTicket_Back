using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddBrokerHqHost
    {

    }

    public class AddBrokerHqHostRequest
    {
        /// <summary>
        /// Ip地址
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 券商名称
        /// </summary>
        public string BrokerName { get; set; }
    }
}
