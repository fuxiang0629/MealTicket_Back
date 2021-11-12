using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class AccountSearchTriSession : Session<List<AccountSearchTriInfo_Session>>
    {
        public AccountSearchTriSession()
        {
            Name = "AccountSearchTriSession";
        }
        public override List<AccountSearchTriInfo_Session> UpdateSession()
        {
            DateTime dateNow = Helper.GetLastTradeDate(-9, 0, 0);

            using (var db = new meal_ticketEntities())
            {
                var search_tri = (from item in db.t_search_tri
                                  where item.LastPushTime >= dateNow
                                  select new AccountSearchTriInfo_Session
                                  {
                                      LastPushType=item.LastPushType,
                                      TriCount=item.TriCount,
                                      SharesCode = item.SharesCode,
                                      AccountId = item.AccountId,
                                      Id = item.Id,
                                      LastPushTime = item.LastPushTime.Value,
                                      Market = item.Market
                                  }).ToList();
                return search_tri;
            }
        }
    }
}
