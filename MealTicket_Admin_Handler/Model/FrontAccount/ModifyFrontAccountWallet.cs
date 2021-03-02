using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifyFrontAccountWallet
    {
    }

    public class ModifyFrontAccountWalletRequest
    {
        /// <summary>
        /// 前端账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 增加保证金金额
        /// </summary>
        public long AddDeposit { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
