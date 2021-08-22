using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class SharesPlateSession : Session<List<SharesPlateInfo_Session>>
    {
        public SharesPlateSession()
        {
            Name = "SharesPlateSession";
        }
        public override List<SharesPlateInfo_Session> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select t.Id PlateId,t.Name PlateName,t.[Type] PlateType,t.BaseStatus,t.ChooseStatus,isnull(t1.IsBasePlate,2)IsBasePlate
from t_shares_plate t with(nolock)
inner join t_shares_plate_type_business t1 on t.[Type]= t1.Id
where t.[Status]= 1";
                var result = db.Database.SqlQuery<SharesPlateInfo_Session>(sql).ToList();
                return result;
            }
        }
    }
}
