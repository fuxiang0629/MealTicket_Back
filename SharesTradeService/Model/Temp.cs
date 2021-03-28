using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesTradeService.Model
{
    class Temp
    {
    }

    public class SellEntrustManagerSelect
    {
        public int EntrustCount { get; set; }
        public long BuyId { get; set; }
    }

    public class AccountSharesRelSelect
    {
        public int AccountCanSoldCount { get; set; }
        public int TotalSharesCount { get; set; }
    }
}
