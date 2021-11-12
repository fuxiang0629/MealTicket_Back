using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
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
                string sql = @"select t.Id PlateId,t.Name PlateName,t.[Type] PlateType,WeightMarket,WeightSharesCode,NoWeightMarket,NoWeightSharesCode,BaseDate,isnull(BaseDateWeightPrice,0)BaseDateWeightPrice,isnull(BaseDateNoWeightPrice,0)BaseDateNoWeightPrice,CalType from t_shares_plate t with(nolock)";
                var result = db.Database.SqlQuery<SharesPlateInfo_Session>(sql).ToList();
                return result;
            }
        }
    }
}
