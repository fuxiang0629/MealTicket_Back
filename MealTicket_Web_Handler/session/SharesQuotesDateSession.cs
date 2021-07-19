using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.session
{
    public class SharesQuotesDateSession : Session<List<t_shares_quotes_date>>
    {
        public override List<t_shares_quotes_date> UpdateSession()
        {
            DateTime EndTime = Helper.GetLastTradeDate(-9, 0, 0);
            DateTime startTime = Helper.GetLastTradeDate(-9, 0, 0,3);
            using (var db = new meal_ticketEntities())
            {
                var shares_quotes_date = (from item in db.t_shares_quotes_date
                                          where item.LastModified > startTime && item.LastModified < EndTime
                                          select item).ToList();
                return shares_quotes_date;
            }
        }
    }
}
