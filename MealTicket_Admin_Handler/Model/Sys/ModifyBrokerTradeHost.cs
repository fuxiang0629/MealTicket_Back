using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyBrokerTradeHost
    {
    }

    public class ModifyBrokerTradeHostRequest
    {
        /// <summary>
        /// 地址Id
        /// </summary>
        public long Id { get; set; }

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
