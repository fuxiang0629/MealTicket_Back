using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class SetAccountRemainDeposit
    {
    }

    public class SetAccountRemainDepositRequest:DetailsRequest
    {
        /// <summary>
        /// 剩余金额
        /// </summary>
        public long RemainDeposit { get; set; }

        /// <summary>
        /// 最大购买股票数量
        /// </summary>
        public int MaxBuySharesCount { get; set; }
    }
}
