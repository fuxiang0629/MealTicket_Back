using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Tag_TrendLike_Session
    {
        //SharesCode*10+Market
        //PlateId
        public static Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var tag_trendlike = (from item in db.t_shares_plate_rel_tag_trendlike
                                     select new Plate_Tag_TrendLike_Session_Info
                                     {
                                         SharesCode = item.SharesCode,
                                         Market = item.Market,
                                         PlateId = item.PlateId,
                                         IsTrendLike = item.IsTrendLike
                                     }).ToList();
                return BuildDic(tag_trendlike);
            }
        }

        private static Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> BuildDic(List<Plate_Tag_TrendLike_Session_Info> dataList)
        {
            Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> session_Dic = new Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>>();
            foreach (var item in dataList)
            {
                long key1 = long.Parse(item.SharesCode) * 10 + item.Market;
                if (!session_Dic.ContainsKey(key1))
                {
                    session_Dic.Add(key1, new Dictionary<long, Plate_Tag_TrendLike_Session_Info>());
                }
                long key2 = item.PlateId;
                session_Dic[key1][key2] = item;
            }
            return session_Dic;
        }

        public static Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> UpdateSessionPart(object newData)
        {
            var newDataResult = newData as List<Plate_Tag_TrendLike_Session_Info>;
            var oldDic = Singleton.Instance.sessionHandler.GetPlate_Tag_TrendLike_Session();
            //深度拷贝
            var oldData = Utils.DeepCopyWithBinarySerialize<Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>>>(oldDic);

            foreach (var item in newDataResult)
            {
                long key1 = long.Parse(item.SharesCode) * 10 + item.Market;
                if (!oldData.ContainsKey(key1))
                {
                    oldData.Add(key1, new Dictionary<long, Plate_Tag_TrendLike_Session_Info>());
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
    }
}
