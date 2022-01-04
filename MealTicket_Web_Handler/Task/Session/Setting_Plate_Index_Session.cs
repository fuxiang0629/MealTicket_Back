using MealTicket_DBCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Setting_Plate_Index_Session
    {
        public static Dictionary<int, Setting_Plate_Index_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select Type,ValueJson from t_setting_plate_index where [Status]=1";
                var result = db.Database.SqlQuery<Setting_Plate_Index_Session_Info>(sql).ToList();
                return BuildDic(result);
            }
        }

        private static Dictionary<int, Setting_Plate_Index_Session_Info> BuildDic(List<Setting_Plate_Index_Session_Info> dataList)
        {
            Dictionary<int, Setting_Plate_Index_Session_Info> session_Dic = new Dictionary<int, Setting_Plate_Index_Session_Info>();
            foreach (var item in dataList)
            {
                var temp=JsonConvert.DeserializeObject<dynamic>(item.ValueJson);
                var Coefficient = temp.coefficient;
                int CalDays = 0;
                int CalPriceType = 0;
                if (item.Type == 4)
                {
                    CalDays = temp.calDays;
                    CalPriceType = temp.calPriceType;
                }
                item.Coefficient = Coefficient;
                item.CalDays = CalDays;
                item.CalPriceType = CalPriceType;
                session_Dic.Add(item.Type, item);
            }
            return session_Dic;
        }

        public static Dictionary<int, Setting_Plate_Index_Session_Info> CopySessionData(object objData)
        {
            var data = objData as Dictionary<int, Setting_Plate_Index_Session_Info>;
            return new Dictionary<int, Setting_Plate_Index_Session_Info>(data);
        }
    }
}
