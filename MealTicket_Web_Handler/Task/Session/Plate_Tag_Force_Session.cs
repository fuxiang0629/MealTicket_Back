using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Tag_Force_Session
    {
        //SharesCode*1000+Market*100+ForceType
        //PlateId
        public static Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                DateTime dateNow = DateTime.Now.Date;
                string sql = string.Format(@"merge into t_shares_plate_rel_tag_force_date as t
using (select * from t_shares_plate_rel_tag_force) as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode and t.PlateId=t1.PlateId and t.[Type]=t1.[Type] and t.[Date]='{0}'
when matched
then update set t.IsForce1 = t1.IsForce1,t.IsForce2 = t1.IsForce2
when not matched by target
then insert([Type],Market,SharesCode,PlateId,IsForce1,IsForce2,[Date]) values(t1.[Type],t1.Market,t1.SharesCode,t1.PlateId,t1.IsForce1,t1.IsForce2,'{0}');", dateNow.ToString("yyyy-MM-dd"));
                db.Database.ExecuteSqlCommand(sql);

                var tag_force = (from item in db.t_shares_plate_rel_tag_force
                                 select new Plate_Tag_Force_Session_Info
                                 {
                                     SharesCode = item.SharesCode,
                                     Market = item.Market,
                                     PlateId = item.PlateId,
                                     Type = item.Type,
                                     IsForce1 = item.IsForce1,
                                     IsForce2 = item.IsForce2
                                 }).ToList();
                return BuildDic(tag_force);
            }
        }

        private static Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> BuildDic(List<Plate_Tag_Force_Session_Info> dataList)
        {
            Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> session_Dic = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            foreach (var item in dataList)
            {
                long key1 = long.Parse(item.SharesCode) * 1000 + item.Market * 100 + item.Type;
                if (!session_Dic.ContainsKey(key1))
                {
                    session_Dic.Add(key1, new Dictionary<long, Plate_Tag_Force_Session_Info>());
                }
                long key2 = item.PlateId;
                session_Dic[key1][key2] = item;
            }
            return session_Dic;
        }

        public static Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> UpdateSessionPart(object newData)
        {
            var newDataResult = newData as List<Plate_Tag_Force_Session_Info>;
            var oldData = Singleton.Instance.sessionHandler.GetPlate_Tag_Force_Session();

            foreach (var item in newDataResult)
            {
                long key1 = long.Parse(item.SharesCode) * 1000 + item.Market*100+item.Type;
                if (!oldData.ContainsKey(key1))
                {
                    oldData.Add(key1, new Dictionary<long, Plate_Tag_Force_Session_Info>());
                }
                long key2 = item.PlateId;
                if (!oldData[key1].ContainsKey(key2))
                {
                    oldData[key1].Add(key2, item);
                }
                else
                {
                    oldData[key1][key2] = item;
                }
            }
            return oldData;
        }

        public static Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>;
            var resultData = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            foreach (var item in data)
            {
                resultData[item.Key] = new Dictionary<long, Plate_Tag_Force_Session_Info>(item.Value);
            }
            return resultData;
        }
    }
}
