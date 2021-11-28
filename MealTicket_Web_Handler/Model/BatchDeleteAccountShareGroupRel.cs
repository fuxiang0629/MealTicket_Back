using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BatchDeleteAccountShareGroupRel
    {
    }

    public class BatchDeleteAccountShareGroupRelRequest
    {
        /// <summary>
        /// Id列表
        /// </summary>
        public List<long> RelIdList { get; set; }
    }
}
