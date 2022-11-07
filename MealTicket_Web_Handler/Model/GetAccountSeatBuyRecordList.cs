using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class GetAccountSeatBuyRecordList
    {
        
    }

    public class AccountSeatBuyRecordInfo
    {
        public long Id { get; set; }

        public int MonthCount { get; set; }

        public int SeatCount { get; set; }

        public int Price { get; set; }

        public int PayAmount { get; set; }

        public DateTime PaySuccessTime { get; set; }
    }
}
