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
        public static Plate_Tag_Force_Session_Obj UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var tag_force = (from item in db.t_shares_plate_rel_tag_force
                                 where item.IsForce1 || item.IsForce2
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

        private static Plate_Tag_Force_Session_Obj BuildDic(List<Plate_Tag_Force_Session_Info> dataList)
        {
            Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> session_Dic1 = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> session_Dic2 = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            foreach (var item in dataList)
            {
                long key1 = long.Parse(item.SharesCode) * 1000 + item.Market * 100 + item.Type;
                long key2 = item.PlateId;
                if (!session_Dic1.ContainsKey(key1))
                {
                    session_Dic1.Add(key1, new Dictionary<long, Plate_Tag_Force_Session_Info>());
                }
                session_Dic1[key1][key2] = item;

                if (!session_Dic2.ContainsKey(key2))
                {
                    session_Dic2.Add(key2, new Dictionary<long, Plate_Tag_Force_Session_Info>());
                }
                session_Dic2[key2][key1] = item;
            }
            return new Plate_Tag_Force_Session_Obj 
            {
                Plate_Shares_Force_Session= session_Dic2,
                Shares_Plate_Force_Session= session_Dic1
            };
        }

        public static Plate_Tag_Force_Session_Obj UpdateSessionPart(object newData)
        {
            var newDataResult = newData as List<Plate_Tag_Force_Session_Info>;
            var resultData = new Plate_Tag_Force_Session_Obj();
            resultData.Shares_Plate_Force_Session = Singleton.Instance.sessionHandler.GetPlate_Tag_Force_Session_ByShares(false);
            resultData.Plate_Shares_Force_Session = Singleton.Instance.sessionHandler.GetPlate_Tag_Force_Session_ByPlate(false);

            foreach (var item in newDataResult)
            {
                if (!item.IsForce1 && !item.IsForce2)
                {
                    continue;
                }
                long key1 = long.Parse(item.SharesCode) * 1000 + item.Market*100+item.Type;
                long key2 = item.PlateId;
                if (!resultData.Shares_Plate_Force_Session.ContainsKey(key1))
                {
                    resultData.Shares_Plate_Force_Session.Add(key1, new Dictionary<long, Plate_Tag_Force_Session_Info>());
                }

                if (!resultData.Shares_Plate_Force_Session[key1].ContainsKey(key2))
                {
                    resultData.Shares_Plate_Force_Session[key1].Add(key2, item);
                }
                else
                {
                    resultData.Shares_Plate_Force_Session[key1][key2] = item;
                }

                if (!resultData.Plate_Shares_Force_Session.ContainsKey(key2))
                {
                    resultData.Plate_Shares_Force_Session.Add(key2, new Dictionary<long, Plate_Tag_Force_Session_Info>());
                }

                if (!resultData.Plate_Shares_Force_Session[key2].ContainsKey(key1))
                {
                    resultData.Plate_Shares_Force_Session[key2].Add(key1, item);
                }
                else
                {
                    resultData.Plate_Shares_Force_Session[key2][key1] = item;
                }
            }
            return resultData;
        }

        public static Plate_Tag_Force_Session_Obj CopySessionData(object objData)
        {
            var data = objData as Plate_Tag_Force_Session_Obj;
            var resultData = new Plate_Tag_Force_Session_Obj();
            resultData.Shares_Plate_Force_Session = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            resultData.Plate_Shares_Force_Session = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            foreach (var item in data.Shares_Plate_Force_Session)
            {
                if (!resultData.Shares_Plate_Force_Session.ContainsKey(item.Key))
                {
                    resultData.Shares_Plate_Force_Session.Add(item.Key, new Dictionary<long, Plate_Tag_Force_Session_Info>());
                }

                foreach (var item2 in item.Value)
                {
                    if (!item2.Value.IsForce1 && !item2.Value.IsForce2)
                    {
                        continue;
                    }
                
                    resultData.Shares_Plate_Force_Session[item.Key].Add(item2.Key,item2.Value);
                }
            }

            foreach (var item in data.Plate_Shares_Force_Session)
            {
                if (!resultData.Plate_Shares_Force_Session.ContainsKey(item.Key))
                {
                    resultData.Plate_Shares_Force_Session.Add(item.Key, new Dictionary<long, Plate_Tag_Force_Session_Info>());
                }

                foreach (var item2 in item.Value)
                {
                    if (!item2.Value.IsForce1 && !item2.Value.IsForce2)
                    {
                        continue;
                    }

                    resultData.Plate_Shares_Force_Session[item.Key].Add(item2.Key, item2.Value);
                }
            }
            return resultData;
        }
    }
}
