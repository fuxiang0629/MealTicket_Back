using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Hotspot_Session
    {
        public static Dictionary<long, Shares_Hotspot_Session_Info> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_hotspot
                              join item2 in db.t_shares_hotspot_plate on item.Id equals item2.HotId into a
                              from ai in a.DefaultIfEmpty()
                              join item3 in db.t_shares_hotspot_group_rel on item.Id equals item3.HostId into b from bi in b.DefaultIfEmpty()
                              where bi==null || bi.Status==1
                              group new { item, ai } by item into g
                              orderby g.Key.OrderIndex, g.Key.CreateTime descending
                              select new Shares_Hotspot_Session_Info
                              {
                                  Id = g.Key.Id,
                                  Name = g.Key.Name,
                                  AccountId=g.Key.AccountId,
                                  DataType = g.Key.DataType,
                                  BgColor=g.Key.BgColor,
                                  ShowBgColor=g.Key.ShowBgColor,
                                  PlateIdList = (from x in g
                                                 where x.ai != null
                                                 select x.ai.PlateId).Distinct().ToList()
                              }).ToList().ToDictionary(k => k.Id, v => v);
                return result;
            }
        }

        public static Dictionary<long, Shares_Hotspot_Session_Info> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Shares_Hotspot_Session_Info>;
            var resultData = new Dictionary<long, Shares_Hotspot_Session_Info>(data);
            return resultData;
        }
    }
}
