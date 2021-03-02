using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddAccountFundmultiple
    {
    }

    public class AddAccountFundmultipleRequest
    {
        /// <summary>
        /// 杠杆Id
        /// </summary>
        public long FundmultipleId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 用户杠杆倍数
        /// </summary>
        public int AccountFundMultiple { get; set; }
    }
}
