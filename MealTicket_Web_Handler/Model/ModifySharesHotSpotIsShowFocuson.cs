using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifySharesHotSpotIsShowFocuson
    {
    }

    public class ModifySharesHotSpotIsShowFocusonRequest
    {
        /// <summary>
        /// 题材Id
        /// </summary>
        public long HotId { get; set; }

        /// <summary>
        /// 是否只显示关注股票
        /// </summary>
        public bool IsShowFocuson { get; set; }
    }
}
