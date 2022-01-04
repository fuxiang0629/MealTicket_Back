using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class Plate_Shares_Rel_Session
    {
        public static List<Plate_Shares_Rel_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select PlateId,Market,SharesCode from t_shares_plate_rel";
                var result = db.Database.SqlQuery<Plate_Shares_Rel_Session_Info>(sql).ToList();
                return result;
            }
        }

        public static List<Plate_Shares_Rel_Session_Info> CopySessionData(object objData)
        {
            var data = objData as List<Plate_Shares_Rel_Session_Info>;
            var resultData = new List<Plate_Shares_Rel_Session_Info>(data);
            return resultData;
        }
    }
}
