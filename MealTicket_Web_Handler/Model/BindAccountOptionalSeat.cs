using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BindAccountOptionalSeat
    {
    }

    public class BindAccountOptionalSeatRequest
    {
        /// <summary>
        /// 自选股Id
        /// </summary>
        public long OptionalId { get; set; }

        /// <summary>
        /// 席位Id(-1：取消绑定)
        /// </summary>
        public long SeatId { get; set; }
    }
}
