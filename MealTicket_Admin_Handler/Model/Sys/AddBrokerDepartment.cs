using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddBrokerDepartment
    {
    }

    public class AddBrokerDepartmentRequest
    {
        /// <summary>
        /// 券商Code
        /// </summary>
        public int BrokerCode { get; set; }

        /// <summary>
        /// 营业部Code
        /// </summary>
        public int DepartmentCode { get; set; }

        /// <summary>
        /// 营业部名称
        /// </summary>
        public string DepartmentName { get; set; }
    }
}
