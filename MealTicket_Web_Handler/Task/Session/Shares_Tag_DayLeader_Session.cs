using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Tag_DayLeader_Session
    {
        //PlateId*100+Type
        //SharesCode*10+Market
        public static Shares_Tag_DayLeader_Session_Obj UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var dataList = (from item in db.t_plate_shares_rel_tag_dayleader
                                where item.DayLeaderType>0
                                 select new Shares_Tag_DayLeader_Session_Info
                                 {
                                     SharesCode = item.SharesCode,
                                     Market = item.Market,
                                     PlateId = item.PlateId,
                                     Type = item.Type,
                                     DayLeaderType = item.DayLeaderType
                                 }).ToList();
                return BuildDic(dataList);
            }
        }

        private static Shares_Tag_DayLeader_Session_Obj BuildDic(List<Shares_Tag_DayLeader_Session_Info> dataList)
        {
            Shares_Tag_DayLeader_Session_Obj session_Dic = new Shares_Tag_DayLeader_Session_Obj();
            session_Dic.Shares_Tag_DayLeader_Session_ByPlate = new Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>>();
            session_Dic.Shares_Tag_DayLeader_Session_ByShares = new Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>>();
            foreach (var item in dataList)
            {
                long key1 = item.PlateId * 100 + item.Type;
                long key2 = long.Parse(item.SharesCode) * 10 + item.Market;
                if (!session_Dic.Shares_Tag_DayLeader_Session_ByPlate.ContainsKey(key1))
                {
                    session_Dic.Shares_Tag_DayLeader_Session_ByPlate.Add(key1, new Dictionary<long, Shares_Tag_DayLeader_Session_Info>());
                }
                session_Dic.Shares_Tag_DayLeader_Session_ByPlate[key1][key2] = item;


                if (!session_Dic.Shares_Tag_DayLeader_Session_ByShares.ContainsKey(key2))
                {
                    session_Dic.Shares_Tag_DayLeader_Session_ByShares.Add(key2, new Dictionary<long, Shares_Tag_DayLeader_Session_Info>());
                }
                session_Dic.Shares_Tag_DayLeader_Session_ByShares[key2][key1] = item;
            }
            return session_Dic;
        }

        public static Shares_Tag_DayLeader_Session_Obj UpdateSessionPart(object newData)
        {
            var newDataResult = newData as List<Shares_Tag_DayLeader_Session_Info>;
            var disData = new Shares_Tag_DayLeader_Session_Obj();
            disData.Shares_Tag_DayLeader_Session_ByPlate = new Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>>();
            disData.Shares_Tag_DayLeader_Session_ByShares = new Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>>();

            foreach (var item in newDataResult)
            {
                if (item.DayLeaderType <= 0)
                {
                    continue;
                }
                long key1 = item.PlateId * 100 + item.Type;
                long key2 = long.Parse(item.SharesCode) * 10 + item.Market;
                if (!disData.Shares_Tag_DayLeader_Session_ByPlate.ContainsKey(key1))
                {
                    disData.Shares_Tag_DayLeader_Session_ByPlate.Add(key1, new Dictionary<long, Shares_Tag_DayLeader_Session_Info>());
                }
                if (!disData.Shares_Tag_DayLeader_Session_ByPlate[key1].ContainsKey(key2))
                {
                    disData.Shares_Tag_DayLeader_Session_ByPlate[key1].Add(key2, new Shares_Tag_DayLeader_Session_Info());
                }
                disData.Shares_Tag_DayLeader_Session_ByPlate[key1][key2] = item;

                if (!disData.Shares_Tag_DayLeader_Session_ByShares.ContainsKey(key2))
                {
                    disData.Shares_Tag_DayLeader_Session_ByShares.Add(key2, new Dictionary<long, Shares_Tag_DayLeader_Session_Info>());
                }
                if (!disData.Shares_Tag_DayLeader_Session_ByShares[key2].ContainsKey(key1))
                {
                    disData.Shares_Tag_DayLeader_Session_ByShares[key2].Add(key1, new Shares_Tag_DayLeader_Session_Info());
                }
                disData.Shares_Tag_DayLeader_Session_ByShares[key2][key1] = item;
            }
            return disData;
        }

        public static Shares_Tag_DayLeader_Session_Obj CopySessionData(object objData)
        {
            var data = objData as Shares_Tag_DayLeader_Session_Obj;
            var resultData = new Shares_Tag_DayLeader_Session_Obj();
            resultData.Shares_Tag_DayLeader_Session_ByPlate = new Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>>();
            resultData.Shares_Tag_DayLeader_Session_ByShares = new Dictionary<long, Dictionary<long, Shares_Tag_DayLeader_Session_Info>>();
            foreach (var item in data.Shares_Tag_DayLeader_Session_ByPlate)
            {
                resultData.Shares_Tag_DayLeader_Session_ByPlate[item.Key] = new Dictionary<long, Shares_Tag_DayLeader_Session_Info>(item.Value);
            }
            foreach (var item in data.Shares_Tag_DayLeader_Session_ByShares)
            {
                resultData.Shares_Tag_DayLeader_Session_ByShares[item.Key] = new Dictionary<long, Shares_Tag_DayLeader_Session_Info>(item.Value);
            }
            return resultData;
        }
    }
}
