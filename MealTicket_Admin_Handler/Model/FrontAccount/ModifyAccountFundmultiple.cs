using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyAccountFundmultiple
    {
    }

    public class ModifyAccountFundmultipleRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 杠杆倍数
        /// </summary>
        public int AccountFundMultiple { get; set; }
    }
}
