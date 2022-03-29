using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Shares_Rel_Session
    {
        public static Plate_Shares_Rel_Session_Obj UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select PlateId,Market,SharesCode from v_shares_plate_rel";
                var result = db.Database.SqlQuery<Plate_Shares_Rel_Session_Info>(sql).ToList();
                return BuildDic(result);
            }
        }

        private static Plate_Shares_Rel_Session_Obj BuildDic(List<Plate_Shares_Rel_Session_Info> dataList)
        {
            Plate_Shares_Rel_Session_Obj result = new Plate_Shares_Rel_Session_Obj();
            result.Plate_Shares_Rel_Session = new Dictionary<long, List<Plate_Shares_Rel_Session_Info>>();
            result.Shares_Plate_Rel_Session = new Dictionary<long, List<Plate_Shares_Rel_Session_Info>>();
            result.Plate_Shares_Rel_Dic_Session = new Dictionary<long, Dictionary<long, Plate_Shares_Rel_Session_Info>>();
            result.Shares_Plate_Rel_Dic_Session = new Dictionary<long, Dictionary<long, Plate_Shares_Rel_Session_Info>>();
            foreach (var item in dataList)
            {
                long key1 = long.Parse(item.SharesCode) * 10 + item.Market;
                long key2 = item.PlateId;
                if (!result.Shares_Plate_Rel_Session.ContainsKey(key1))
                {
                    result.Shares_Plate_Rel_Session.Add(key1, new List<Plate_Shares_Rel_Session_Info>());
                }
                result.Shares_Plate_Rel_Session[key1].Add(item);


                if (!result.Plate_Shares_Rel_Session.ContainsKey(key2))
                {
                    result.Plate_Shares_Rel_Session.Add(key2, new List<Plate_Shares_Rel_Session_Info>());
                }
                result.Plate_Shares_Rel_Session[key2].Add(item);

                if (!result.Plate_Shares_Rel_Dic_Session.ContainsKey(key2))
                {
                    result.Plate_Shares_Rel_Dic_Session.Add(key2, new Dictionary<long, Plate_Shares_Rel_Session_Info>());
                }
                if (!result.Plate_Shares_Rel_Dic_Session[key2].ContainsKey(key1))
                {
                    result.Plate_Shares_Rel_Dic_Session[key2].Add(key1, new Plate_Shares_Rel_Session_Info());
                }
                result.Plate_Shares_Rel_Dic_Session[key2][key1] = item;


                if (!result.Shares_Plate_Rel_Dic_Session.ContainsKey(key1))
                {
                    result.Shares_Plate_Rel_Dic_Session.Add(key1, new Dictionary<long, Plate_Shares_Rel_Session_Info>());
                }
                if (!result.Shares_Plate_Rel_Dic_Session[key1].ContainsKey(key2))
                {
                    result.Shares_Plate_Rel_Dic_Session[key1].Add(key2, new Plate_Shares_Rel_Session_Info());
                }
                result.Shares_Plate_Rel_Dic_Session[key1][key2] = item;
            }
            return result;
        }

        public static Plate_Shares_Rel_Session_Obj CopySessionData(object objData)
        {
            var data = objData as Plate_Shares_Rel_Session_Obj;
            var resultData = new Plate_Shares_Rel_Session_Obj();
            resultData.Plate_Shares_Rel_Session = new Dictionary<long, List<Plate_Shares_Rel_Session_Info>>(data.Plate_Shares_Rel_Session);
            resultData.Shares_Plate_Rel_Session = new Dictionary<long, List<Plate_Shares_Rel_Session_Info>>(data.Shares_Plate_Rel_Session);
            resultData.Plate_Shares_Rel_Dic_Session = new Dictionary<long, Dictionary<long, Plate_Shares_Rel_Session_Info>>(data.Plate_Shares_Rel_Dic_Session);
            resultData.Shares_Plate_Rel_Dic_Session = new Dictionary<long, Dictionary<long, Plate_Shares_Rel_Session_Info>>(data.Shares_Plate_Rel_Dic_Session);
            //foreach (var item in data.Plate_Shares_Rel_Session)
            //{
            //    resultData.Plate_Shares_Rel_Session[item.Key] = new List<Plate_Shares_Rel_Session_Info>(item.Value);
            //}
            //foreach (var item in data.Shares_Plate_Rel_Session)
            //{
            //    resultData.Shares_Plate_Rel_Session[item.Key] = new List<Plate_Shares_Rel_Session_Info>(item.Value);
            //}
            return resultData;
        }
    }
}
