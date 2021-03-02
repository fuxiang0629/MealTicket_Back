using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBrokerTradeHostList
    {
    }

    public class GetBrokerTradeHostListRequest:PageRequest
    {
        /// <summary>
        /// 券商code
        /// </summary>
        public int BrokerCode { get; set; }

        /// <summary>
        /// Ip地址
        /// </summary>
        public string Ip { get; set; }
    }

    public class BrokerTradeHostInfo
    {
        /// <summary>
        /// 地址Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 券商code
        /// </summary>
        public int BrokerCode { get; set; }

        /// <summary>
        /// 券商名称
        /// </summary>
        public string BrokerName { get; set; }

        /// <summary>
        /// Ip地址
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
