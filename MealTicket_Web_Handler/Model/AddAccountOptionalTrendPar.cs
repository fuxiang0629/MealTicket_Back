using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddAccountOptionalTrendPar
    {
    }

    public class AddAccountOptionalTrendParRequest
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
