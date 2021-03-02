using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyBrokerDepartment
    {
    }

    public class ModifyBrokerDepartmentRequest
    {
        /// <summary>
        /// 营业部Id
        /// </summary>
        public long Id { get; set; }

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
