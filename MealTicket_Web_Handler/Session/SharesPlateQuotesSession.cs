using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class SharesPlateQuotesSession:Session<List<SharesPlateQuotesInfo_Session>>
    {
        public SharesPlateQuotesSession()
        {
            Name = "SharesPlateQuotesSession";
        }
        public override List<SharesPlateQuotesInfo_Session> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select PlateId,SharesCount,RiseRate,RiseIndex,WeightRiseRate,WeightRiseIndex,RiseLimitCount,DownLimitCount from t_shares_plate_riserate_last with(nolock)";
                var result = db.Database.SqlQuery<SharesPlateQuotesInfo_Session>(sql).ToList();
                result = result.OrderByDescending(e => e.RiseRate).ToList();

                var baseList = Singleton.Instance._SharesPlateSession.GetSessionData(); 
                if (baseList.Count() <= 0)
                {
                    return result;
                }
                int i = 1;
                foreach (var item in result)
                {
                    if (i > Singleton.Instance.PlateRankShow)
                    {
                        break;
                    }
                    var temp=baseList.Where(e => e.PlateId == item.PlateId).FirstOrDefault();
                    if (temp == null)
                    {
                        continue;
                    }
                    if (temp.ChooseStatus == 1 || (temp.IsBasePlate == 1 && temp.BaseStatus == 1))
                    {
                        item.Rank = i;
                        i++;
                    }
                }
                return result;
            }
        }
    }
}
