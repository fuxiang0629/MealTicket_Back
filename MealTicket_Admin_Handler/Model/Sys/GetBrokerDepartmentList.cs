using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBrokerDepartmentList
    {
    }

    public class GetBrokerDepartmentListRequest:PageRequest
    {
        /// <summary>
        /// 券商code
        /// </summary>
        public int BrokerCode { get; set; }

        /// <summary>
        /// 营业部名称
        /// </summary>
        public string DepartmentName { get; set; }
    }

    public class BrokerDepartmentInfo
    {
        /// <summary>
        /// 营业部Id
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
        /// 营业商code
        /// </summary>
        public int DepartmentCode { get; set; }

        /// <summary>
        /// 营业商名称
        /// </summary>
        public string DepartmentName { get; set; }

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
