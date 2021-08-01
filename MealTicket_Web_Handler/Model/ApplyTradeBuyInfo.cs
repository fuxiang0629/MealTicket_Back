using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    public class ApplyTradeBuyInfo
    {
        public long BuyId { get; set; }

        public int BuyCount { get; set; }

        public DateTime BuyTime { get; set; }

        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; }
    }
}
