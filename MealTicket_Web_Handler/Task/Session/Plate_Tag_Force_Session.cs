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
            List<Plate_Tag_Force_Session_Info> force_List = new List<Plate_Tag_Force_Session_Info>();
            Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> session_Dic1 = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>> session_Dic2 = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            foreach (var item in dataList)
            {
                force_List.Add(item);

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
                Shares_Plate_Force_Session= session_Dic1,
                Force_List= force_List
            };
        }

        public static Plate_Tag_Force_Session_Obj UpdateSessionPart(object newData)
        {
            var newDataResult = newData as List<Plate_Tag_Force_Session_Info>;

            var isAuto_shares = Singleton.Instance.sessionHandler.GetShares_PlateTag_IsAuto_Session(false);
            var oldData = Singleton.Instance.sessionHandler.GetPlate_Tag_Force_Session_List(false);

            List<Plate_Tag_Force_Session_Info> resultSource = new List<Plate_Tag_Force_Session_Info>();
            foreach (var item in oldData)
            {
                long key = long.Parse(item.SharesCode) * 10 + item.Market;
                if (isAuto_shares.ContainsKey(key))
                {
                    resultSource.Add(item);
                }
            }
            foreach (var item in newDataResult)
            {
                long key = long.Parse(item.SharesCode) * 10 + item.Market;
                if (isAuto_shares.ContainsKey(key))
                {
                    continue;
                }
                resultSource.Add(item);
            }
            var result = new Plate_Tag_Force_Session_Obj();
            result.Shares_Plate_Force_Session = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            result.Plate_Shares_Force_Session = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            result.Force_List = new List<Plate_Tag_Force_Session_Info>();
            foreach (var item in resultSource)
            {
                if (!item.IsForce1 && !item.IsForce2)
                {
                    continue;
                }
                result.Force_List.Add(item);

                long key1 = long.Parse(item.SharesCode) * 1000 + item.Market*100+ item.Type;
                long key2 = item.PlateId;
                if (!result.Shares_Plate_Force_Session.ContainsKey(key1))
                {
                    result.Shares_Plate_Force_Session.Add(key1, new Dictionary<long, Plate_Tag_Force_Session_Info>());
                }
                if (!result.Shares_Plate_Force_Session[key1].ContainsKey(key2))
                {
                    result.Shares_Plate_Force_Session[key1].Add(key2, new Plate_Tag_Force_Session_Info());
                }
                result.Shares_Plate_Force_Session[key1][key2] = item;


                if (!result.Plate_Shares_Force_Session.ContainsKey(key2))
                {
                    result.Plate_Shares_Force_Session.Add(key2, new Dictionary<long, Plate_Tag_Force_Session_Info>());
                }
                if (!result.Plate_Shares_Force_Session[key2].ContainsKey(key1))
                {
                    result.Plate_Shares_Force_Session[key2].Add(key1, new Plate_Tag_Force_Session_Info());
                }
                result.Plate_Shares_Force_Session[key2][key1] = item;
            }
            return result;
        }

        public static Plate_Tag_Force_Session_Obj CopySessionData(object objData)
        {
            var data = objData as Plate_Tag_Force_Session_Obj;
            var resultData = new Plate_Tag_Force_Session_Obj();
            resultData.Shares_Plate_Force_Session = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();
            resultData.Plate_Shares_Force_Session = new Dictionary<long, Dictionary<long, Plate_Tag_Force_Session_Info>>();

            resultData.Force_List = new List<Plate_Tag_Force_Session_Info>(data.Force_List);

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
