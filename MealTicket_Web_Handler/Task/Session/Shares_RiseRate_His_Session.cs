using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_RiseRate_His_Session
    {
        public static Dictionary<DateTime,List<SharesRiseRateStc>> UpdateSession()
        {
            int hisDays = Singleton.Instance.SharesRankHisDays;
            if (hisDays < Singleton.Instance.SharesPlateRankHisDays)
            {
                hisDays = Singleton.Instance.SharesPlateRankHisDays;
            }
            DateTime minDate = DbHelper.GetLastTradeDate2(-9, 0, 0, -hisDays);
            using (var db = new meal_ticketEntities())
            {
                var rankList_temp = (from item in db.t_shares_riserate_rank
                                     where item.Date >= minDate
                                     select item).ToList();
                var rankList=(from item in rankList_temp
                             select new SharesRiseRateStc
                             {
                                 SharesCode = item.SharesCode,
                                 SharesKey = item.SharesKey??0,
                                 DayType = item.DayType,
                                 Market = item.Market,
                                 RealDays = item.RealDays,
                                 RiseRate = item.RiseRate,
                                 TotalRank = item.Rank,
                                 Date = item.Date
                             }).GroupBy(e => e.Date).ToDictionary(k => k.Key, v => v.ToList());
                return rankList;
            }
        }

        public static Dictionary<DateTime,List<SharesRiseRateStc>> UpdateSessionPart(object newData)
        {
            DateTime dateNow = DateTime.Now.Date;
            var newDataResult = newData as List<SharesRiseRateStc>;
            var oldData = Singleton.Instance.sessionHandler.GetShares_RiseRate_His_Session(false);
            if (!oldData.ContainsKey(dateNow))
            {
                oldData.Add(dateNow, new List<SharesRiseRateStc>());
            }
            oldData[dateNow] = newDataResult;
            return oldData;
        }

        public static List<SharesRiseRateStc> CopySessionData(object objData)
        {
            var data = objData as List<SharesRiseRateStc>;
            var resultData = new List<SharesRiseRateStc>(data);
            return resultData;
        }
    }
}
