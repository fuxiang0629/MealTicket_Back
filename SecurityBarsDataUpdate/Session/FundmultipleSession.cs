using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityBarsDataUpdate
{
    public class FundmultipleSession : Session<List<t_shares_limit_fundmultiple>>
    {
        public FundmultipleSession()
        {
            Name = "FundmultipleSession";
        }
        public override List<t_shares_limit_fundmultiple> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_limit_fundmultiple
                              select item).ToList();

                return result;
            }
        }
    }
}
