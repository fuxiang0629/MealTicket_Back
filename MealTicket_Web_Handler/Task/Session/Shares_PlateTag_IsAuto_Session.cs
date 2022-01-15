using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_PlateTag_IsAuto_Session
    {
        public static Dictionary<long,Shares_Name_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select Market,SharesCode from t_shares_plate_rel_tag_setting where IsAuto=0";
                var result = db.Database.SqlQuery<Shares_Name_Info>(sql).ToList();
                return result.ToDictionary(k => long.Parse(k.SharesCode) * 10 + k.Market, v => v);
            }
        }

        public static Dictionary<long, Shares_Name_Info> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Shares_Name_Info>;
            return new Dictionary<long, Shares_Name_Info>(data);
        }
    }
}
