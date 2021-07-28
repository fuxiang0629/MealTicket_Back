using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.session
{
    public class BasePlateSession : Session<List<BasePlateInfo>>
    {
        public BasePlateSession()
        {
            Name = "BasePlateSession";
        }
        public override List<BasePlateInfo> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var plate = (from item in db.t_shares_plate_riserate_last
                             join item2 in db.t_shares_plate on item.PlateId equals item2.Id
                             where item2.Status == 1 && item2.BaseStatus == 1
                             select new BasePlateInfo
                             {
                                 PlateType=item2.Type,
                                 SharesCount = item.SharesCount,
                                 DownLimitCount = item.DownLimitCount,
                                 PlateId = item.PlateId,
                                 PlateName = item2.Name,
                                 RiseIndex = item.RiseIndex,
                                 RiseLimitCount = item.RiseLimitCount,
                                 RiseRate = item.RiseRate,
                                 WeightRiseIndex = item.WeightRiseIndex,
                                 WeightRiseRate = item.WeightRiseRate
                             }).ToList();
                return plate;
            }
        }
    }
}
