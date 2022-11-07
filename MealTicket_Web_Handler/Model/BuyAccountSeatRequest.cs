using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class BuyAccountSeatRequest
    {
        /// <summary>
        /// 席位id
        /// </summary>
        public long SeatId { get; set; }

        /// <summary>
        /// 购买席位数量
        /// </summary>
        public int BuyCount { get; set; }

        /// <summary>
        /// 购买月份
        /// </summary>
        public int BuyMonth { get; set; }
    }
}
