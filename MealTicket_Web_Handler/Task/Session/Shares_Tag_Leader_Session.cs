﻿using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Tag_Leader_Session
    {
        //PlateId*100+Type
        //SharesCode*10+Market
        public static Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                DateTime dateNow = DateTime.Now.Date;
                string sql = string.Format(@"merge into t_plate_shares_rel_tag_leader_date as t
using (select * from t_plate_shares_rel_tag_leader) as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode and t.PlateId=t1.PlateId and t.[Type]=t1.[Type] and t.[Date]='{0}'
when matched
then update set t.LeaderType = t1.LeaderType
when not matched by target
then insert([Type],Market,SharesCode,PlateId,LeaderType,[Date]) values(t1.[Type],t1.Market,t1.SharesCode,t1.PlateId,t1.LeaderType,'{0}');", dateNow.ToString("yyyy-MM-dd"));
                db.Database.ExecuteSqlCommand(sql);

                var dataList = (from item in db.t_plate_shares_rel_tag_leader
                                 select new Shares_Tag_Leader_Session_Info
                                 {
                                     SharesCode = item.SharesCode,
                                     Market = item.Market,
                                     PlateId = item.PlateId,
                                     Type = item.Type,
                                     LeaderType = item.LeaderType
                                 }).ToList();
                return BuildDic(dataList);
            }
        }

        private static Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>> BuildDic(List<Shares_Tag_Leader_Session_Info> dataList)
        {
            Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>> session_Dic = new Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>>();
            foreach (var item in dataList)
            {
                long key1 = item.PlateId * 100 + item.Type;
                if (!session_Dic.ContainsKey(key1))
                {
                    session_Dic.Add(key1, new Dictionary<long, Shares_Tag_Leader_Session_Info>());
                }
                long key2 = long.Parse(item.SharesCode) * 10 + item.Market;
                session_Dic[key1][key2] = item;
            }
            return session_Dic;
        }

        public static Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>> UpdateSessionPart(object newData)
        {
            var newDataResult = newData as List<Shares_Tag_Leader_Session_Info>;

            var disData = new Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>>();

            foreach (var item in newDataResult)
            {
                long key1 = item.PlateId * 100 + item.Type;
                if (!disData.ContainsKey(key1))
                {
                    disData.Add(key1, new Dictionary<long, Shares_Tag_Leader_Session_Info>());
                }
                long key2 = long.Parse(item.SharesCode) * 10 + item.Market;
                if (!disData[key1].ContainsKey(key2))
                {
                    disData[key1].Add(key2, item);
                }
                else
                {
                    disData[key1][key2] = item;
                }
            }
            return disData;
        }

        public static Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>>;
            var resultData = new Dictionary<long, Dictionary<long, Shares_Tag_Leader_Session_Info>>();
            foreach (var item in data)
            {
                resultData[item.Key] = new Dictionary<long, Shares_Tag_Leader_Session_Info>(item.Value);
            }
            return resultData;
        }
    }
}
