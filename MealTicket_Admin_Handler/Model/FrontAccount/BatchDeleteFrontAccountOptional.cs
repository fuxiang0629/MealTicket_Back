using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BatchDeleteFrontAccountOptional
    {
    }

    public class BatchDeleteFrontAccountOptionalRequest
    {
        /// <summary>
        /// Id列表
        /// </summary>
        public List<long> IdList { get; set; }
    }
}
