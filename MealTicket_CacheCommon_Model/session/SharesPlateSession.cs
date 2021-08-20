using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_CacheCommon_Session.session
{
    public class SharesPlateSession : Session<List<SharesPlateInfo_Session>>
    {
        public SharesPlateSession()
        {
            Name = "SharesPlateSession";
            dataKey = "SharesPlateSession";
            username = "mealTicket_baseData";
        }
        public override List<SharesPlateInfo_Session> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = "select Id,Name,[Type],BaseStatus,ChooseStatus from t_shares_plate with(nolock) where [Status]=1";
                var result = db.Database.SqlQuery<SharesPlateInfo_Session>(sql).ToList();
                return result;
            }
        }
    }
}
