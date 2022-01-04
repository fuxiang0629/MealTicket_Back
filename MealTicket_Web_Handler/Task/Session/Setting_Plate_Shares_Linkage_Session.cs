using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Setting_Plate_Shares_Linkage_Session
    {
        public static Dictionary<long, Setting_Plate_Shares_Linkage_Session_Info_Group> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select MainPlateId,Market,SharesCode,IsReal,IsDefault from t_setting_plate_shares_linkage";
                var result = db.Database.SqlQuery<Setting_Plate_Shares_Linkage_Session_Info>(sql).ToList();
                return result.GroupBy(e => new { e.MainPlateId, e.IsReal,e.IsDefault }).ToDictionary(k => k.Key.MainPlateId, v => new Setting_Plate_Shares_Linkage_Session_Info_Group 
                {
                    IsDefault=v.Key.IsDefault,
                    IsReal=v.Key.IsReal,
                    SessionList= v.ToList()
                });
            }
        }

        public static Dictionary<long, Setting_Plate_Shares_Linkage_Session_Info_Group> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Setting_Plate_Shares_Linkage_Session_Info_Group>;
            Dictionary<long, Setting_Plate_Shares_Linkage_Session_Info_Group> result = new Dictionary<long, Setting_Plate_Shares_Linkage_Session_Info_Group>();
            foreach (var item in data)
            {
                result.Add(item.Key,new Setting_Plate_Shares_Linkage_Session_Info_Group 
                {
                    IsReal= item.Value.IsReal,
                    IsDefault=item.Value.IsDefault,
                    SessionList=new List<Setting_Plate_Shares_Linkage_Session_Info>(item.Value.SessionList)
                });
            }
            return result;
        }
    }
}
