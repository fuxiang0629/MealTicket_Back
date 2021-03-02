using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddFrontAccountOptionalTrendPar
    {
    }

    public class AddFrontAccountOptionalTrendParRequest
    {
        /// <summary>
        /// 自选股走势RelId
        /// </summary>
        public long RelId { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public string ParamsInfo { get; set; }
    }
}
