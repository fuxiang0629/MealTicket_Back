using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_CacheCommon_Session.session
{
    public class SharesPlateRelSession : Session<List<SharesPlateRelInfo_Session>>
    {
        public SharesPlateRelSession()
        {
            Name = "SharesPlateRelSession";
            dataKey = "SharesPlateRelSession";
            username = "mealTicket_baseData";
        }
        public override List<SharesPlateRelInfo_Session> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = "select PlateId,Market,SharesCode from t_shares_plate_rel with(nolock)";
                var result = db.Database.SqlQuery<SharesPlateRelInfo_Session>(sql).ToList();
                return result;
            }
        }
    }
}
