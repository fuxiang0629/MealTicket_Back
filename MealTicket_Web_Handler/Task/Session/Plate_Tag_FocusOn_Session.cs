using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Tag_FocusOn_Session
    {
        //SharesCode*10+Market
        //PlateId
        public static Plate_Tag_FocusOn_Session_Obj UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var tag_focuson = (from item in db.t_shares_plate_rel_tag_focuson
                                   where item.IsFocusOn
                                   select new Plate_Tag_FocusOn_Session_Info
                                   {
                                       SharesCode = item.SharesCode,
                                       Market = item.Market,
                                       PlateId = item.PlateId,
                                       IsFocusOn = item.IsFocusOn
                                   }).ToList();
                return BuildDic(tag_focuson);
            }
        }

        private static Plate_Tag_FocusOn_Session_Obj BuildDic(List<Plate_Tag_FocusOn_Session_Info> dataList)
        {
            Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> session_Dic1 = new Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>>();
            Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> session_Dic2 = new Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>>();
            foreach (var item in dataList)
            {
                long key1 = long.Parse(item.SharesCode) * 10 + item.Market;
                long key2 = item.PlateId;
                if (!session_Dic1.ContainsKey(key1))
                {
                    session_Dic1.Add(key1, new Dictionary<long, Plate_Tag_FocusOn_Session_Info>());
                }
                session_Dic1[key1][key2] = item;

                if (!session_Dic2.ContainsKey(key2)) 
                {
                    session_Dic2.Add(key2, new Dictionary<long, Plate_Tag_FocusOn_Session_Info>());
                }
                session_Dic2[key2][key1] = item;
            }
            return new Plate_Tag_FocusOn_Session_Obj 
            {
                Plate_Shares_FocusOn_Session= session_Dic2,
                Shares_Plate_FocusOn_Session= session_Dic1
            };
        }

        public static Plate_Tag_FocusOn_Session_Obj UpdateSessionPart(object newData)
        {
            var newDataResult = newData as List<Plate_Tag_FocusOn_Session_Info>;
            var resultData = new Plate_Tag_FocusOn_Session_Obj();
            resultData.Shares_Plate_FocusOn_Session = Singleton.Instance.sessionHandler.GetPlate_Tag_FocusOn_Session_ByShares(false);
            resultData.Plate_Shares_FocusOn_Session = Singleton.Instance.sessionHandler.GetPlate_Tag_FocusOn_Session_ByPlate(false);

            foreach (var item in newDataResult)
            {
                if (!item.IsFocusOn)
                {
                    continue;
                }
                long key1 = long.Parse(item.SharesCode) * 10 + item.Market;
                long key2 = item.PlateId;
                if (!resultData.Shares_Plate_FocusOn_Session.ContainsKey(key1))
                {
                    resultData.Shares_Plate_FocusOn_Session.Add(key1, new Dictionary<long, Plate_Tag_FocusOn_Session_Info>());
                }

                if (!resultData.Shares_Plate_FocusOn_Session[key1].ContainsKey(key2))
                {
                    resultData.Shares_Plate_FocusOn_Session[key1].Add(key2, item);
                }
                else
                {
                    resultData.Shares_Plate_FocusOn_Session[key1][key2] = item;
                }

                if (!resultData.Plate_Shares_FocusOn_Session.ContainsKey(key2))
                {
                    resultData.Plate_Shares_FocusOn_Session.Add(key2, new Dictionary<long, Plate_Tag_FocusOn_Session_Info>());
                }

                if (!resultData.Plate_Shares_FocusOn_Session[key2].ContainsKey(key1))
                {
                    resultData.Plate_Shares_FocusOn_Session[key2].Add(key1, item);
                }
                else
                {
                    resultData.Plate_Shares_FocusOn_Session[key2][key1] = item;
                }

            }
            return resultData;
        }

        public static Plate_Tag_FocusOn_Session_Obj CopySessionData(object objData)
        {
            var data = objData as Plate_Tag_FocusOn_Session_Obj;
            var resultData = new Plate_Tag_FocusOn_Session_Obj();
            resultData.Shares_Plate_FocusOn_Session = new Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>>();
            resultData.Plate_Shares_FocusOn_Session = new Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>>();
            foreach (var item in data.Plate_Shares_FocusOn_Session)
            {
                if (!resultData.Plate_Shares_FocusOn_Session.ContainsKey(item.Key))
                {
                    resultData.Plate_Shares_FocusOn_Session.Add(item.Key, new Dictionary<long, Plate_Tag_FocusOn_Session_Info>());
                }
                foreach (var item2 in item.Value)
                {
                    if (!item2.Value.IsFocusOn)
                    {
                        continue;
                    }
                    resultData.Plate_Shares_FocusOn_Session[item.Key].Add(item2.Key, item2.Value);
                }
            }
            foreach (var item in data.Shares_Plate_FocusOn_Session)
            {
                if (!resultData.Shares_Plate_FocusOn_Session.ContainsKey(item.Key))
                {
                    resultData.Shares_Plate_FocusOn_Session.Add(item.Key, new Dictionary<long, Plate_Tag_FocusOn_Session_Info>());
                }
                foreach (var item2 in item.Value)
                {
                    if (!item2.Value.IsFocusOn)
                    {
                        continue;
                    }
                    resultData.Shares_Plate_FocusOn_Session[item.Key].Add(item2.Key, item2.Value);
                }
            }
            return resultData;
        }
    }
}
