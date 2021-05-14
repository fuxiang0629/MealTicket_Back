using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddFrontAccountMain
    {
    }

    public class AddFrontAccountMainRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 操盘用户Id
        /// </summary>
        public long MainAccountId { get; set; }

        /// <summary>
        /// 提现状态
        /// </summary>
        public int CashStatus { get; set; }

        /// <summary>
        /// 交易状态
        /// </summary>
        public int ForbidStatus { get; set; }

        /// <summary>
        /// 倍数修改状态
        /// </summary>
        public int MultipleChangeStatus { get; set; }
    }
}
