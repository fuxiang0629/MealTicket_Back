using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Base_Session
    {
        public static Dictionary<long, Plate_Base_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select t.Id PlateId,t.Name PlateName,t.[Type] PlateType,t.CalType,t.BaseStatus,isnull(t1.SharesCount,0) SharesCount,isnull(t1.CirculatingCapital,0)CirculatingCapital
from t_shares_plate t
left join 
(
	select m.PlateId,count(PlateId) SharesCount,sum(isnull(m2.CirculatingCapital,0))CirculatingCapital
	from t_shares_plate_rel m
	inner join t_shares_all m1 on m.Market=m1.Market and m.SharesCode=m1.SharesCode
	left join t_shares_markettime m2 on m.Market=m2.Market and m.SharesCode=m2.SharesCode
	where m1.[Status]=1
	group by m.PlateId
)t1 on t.Id=t1.PlateId
where t.[Status]=1";
                var result = db.Database.SqlQuery<Plate_Base_Session_Info>(sql).ToList();
                return result.ToDictionary(k => k.PlateId, v => v);
            }
        }

        public static Dictionary<long, Plate_Base_Session_Info> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Plate_Base_Session_Info>;
            return new Dictionary<long, Plate_Base_Session_Info>(data);
        }
    }
}
