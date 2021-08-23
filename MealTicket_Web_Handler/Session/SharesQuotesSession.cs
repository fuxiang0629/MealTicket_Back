using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class SharesQuotesSession : Session<List<v_shares_quotes_last>>
    {
        public SharesQuotesSession()
        {
            Name = "SharesQuotesSession";
        }
        public override List<v_shares_quotes_last> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var shares_quotes_last = (from item in db.v_shares_quotes_last
                                          select item).ToList();
                return shares_quotes_last;
            }
        }
    }
}
