using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Shares_Rel_Session
    {
        public static Dictionary<long, List<Plate_Shares_Rel_Session_Info>> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select PlateId,Market,SharesCode from t_shares_plate_rel";
                var result = db.Database.SqlQuery<Plate_Shares_Rel_Session_Info>(sql).ToList();
                return result.GroupBy(e=>e.PlateId).ToDictionary(k => k.Key, v => v.ToList());
            }
        }

        public static Dictionary<long, List<Plate_Shares_Rel_Session_Info>> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, List<Plate_Shares_Rel_Session_Info>>;
            var resultData = new Dictionary<long, List<Plate_Shares_Rel_Session_Info>>();
            foreach (var item in data)
            {
                resultData[item.Key] = new List<Plate_Shares_Rel_Session_Info>(item.Value);
            }
            return resultData;
        }
    }
}
