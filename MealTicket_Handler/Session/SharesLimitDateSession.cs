using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class SharesLimitDateSession : Session<List<SharesLimitDateInfo_Session>>
    {
        public SharesLimitDateSession()
        {
            Name = "SharesLimitDateSession";
        }
        public override List<SharesLimitDateInfo_Session> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select convert(int,convert(varchar(10),t1.BeginDate,112)) BeginDate,convert(int,convert(varchar(10),t1.EndDate,112)) EndDate
from t_shares_limit_date_group t with(nolock)
inner join t_shares_limit_date t1 with(nolock) on t.Id=t1.GroupId
where t.[Status]=1 and t1.[Status]=1 and t1.EndDate>=convert(varchar(10),getdate(),112)";
                var result = db.Database.SqlQuery<SharesLimitDateInfo_Session>(sql).ToList();
                return result;
            }
        }
    }
}
