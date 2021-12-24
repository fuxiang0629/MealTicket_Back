using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Shares_Rel_Tag_Setting_Session
    {
        public static Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select t.SettingType,t.BaseCount,t.DisCount
  from
  (
	  select [Type] SettingType,99999 BaseCount,DefaultCount DisCount
	  from t_plate_shares_rel_tag_setting
	  where [Status]=1
	  union all
	  select t.SettingType,t.BaseCount,t.DisCount
	  from t_plate_shares_rel_tag_setting_details t
	  inner join t_plate_shares_rel_tag_setting t1 on t.SettingType=t1.[Type]
	  where t.[Status]=1 and t1.[Status]=1
  )t
  order by t.SettingType,t.BaseCount";
                var result = db.Database.SqlQuery<Plate_Shares_Rel_Tag_Setting_Session_Info>(sql).ToList();
                return BuildDic(result);
            }
        }

        private static Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> BuildDic(List<Plate_Shares_Rel_Tag_Setting_Session_Info> dataList)
        {
            Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> session_Dic = new Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>>();
            foreach (var item in dataList)
            {
                if (!session_Dic.ContainsKey(item.SettingType))
                {
                    session_Dic.Add(item.SettingType, new List<Plate_Shares_Rel_Tag_Setting_Session_Info>());
                }
                session_Dic[item.SettingType].Add(item);
            }
            return session_Dic;
        }

        public static Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> UpdateSessionPart(object newData)
        {
            throw new NotSupportedException();
        }

        public static Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>> CopySessionData(object objData)
        {
            var data = objData as Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>>;
            var resultData = new Dictionary<int, List<Plate_Shares_Rel_Tag_Setting_Session_Info>>();
            foreach (var item in data)
            {
                resultData[item.Key] = new List<Plate_Shares_Rel_Tag_Setting_Session_Info>(item.Value);
            }
            return resultData;
        }
    }
}
