using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.session
{
    public class PlateRateSession:Session<List<SharesPlateInfo>>
    {
        public override List<SharesPlateInfo> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var plateList = (from x in db.t_shares_plate_rel
                                 join x2 in db.t_shares_plate on x.PlateId equals x2.Id
                                 join x3 in db.t_shares_plate_riserate_last on x.PlateId equals x3.PlateId into a
                                 from ai in a.DefaultIfEmpty()
                                 join x4 in db.t_shares_plate_type_business on x2.Type equals x4.Id
                                 where x2.Status == 1 && (x2.ChooseStatus == 1 || (x4.IsBasePlate == 1 && x2.BaseStatus == 1))
                                 select new SharesPlateInfo
                                 {
                                     Id = x2.Id,
                                     SharesCode = x.SharesCode,
                                     Market = x.Market,
                                     Type = x2.Type,
                                     Name = x2.Name,
                                     SharesCount = ai == null ? 0 : ai.SharesCount,
                                     RiseRate = ai == null ? 0 : ai.RiseRate,
                                     DownLimitCount = ai == null ? 0 : ai.DownLimitCount,
                                     RiseLimitCount = ai == null ? 0 : ai.RiseLimitCount,
                                     SharesInfo=x.SharesInfo
                                 }).ToList();
                return plateList;
            }
        }
    }
}
