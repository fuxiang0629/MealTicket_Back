using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class Plate_Base_Session
    {
        public static Dictionary<long, Plate_Base_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select Id PlateId,Name PlateName,[Type] PlateType,CalType,BaseStatus,WeightMarket,WeightSharesCode,NoWeightMarket,NoWeightSharesCode,BaseDate from t_shares_plate where [Status]=1";
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
