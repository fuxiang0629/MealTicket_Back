using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifySharesCycleManagerFather
    {
    }

    public class ModifySharesCycleManagerFatherRequest
    {
        /// <summary>
        /// 周期Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 上一级周期Id
        /// </summary>
        public long FatherId { get; set; }
    }
}
