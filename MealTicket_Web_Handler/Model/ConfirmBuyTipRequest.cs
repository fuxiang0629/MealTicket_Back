using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    public class ConfirmBuyTipRequest
    {
        /// <summary>
        /// 确认Id列表
        /// </summary>
        public List<long> IdList { get; set; }
    }
}
