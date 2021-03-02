using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBrokerList
    {
    }

    public class GetBrokerListRequest:PageRequest
    {
        /// <summary>
        /// 券商名称
        /// </summary>
        public string BrokerName { get; set; }
    }

    public class BrokerInfo
    {
        /// <summary>
        /// 券商Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 券商Code
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
