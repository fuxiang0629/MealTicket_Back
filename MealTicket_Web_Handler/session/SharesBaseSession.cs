using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.session
{
    public class SharesBaseSession:Session<List<SharesBaseInfo>>
    {
        public override List<SharesBaseInfo> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.v_shares_baseinfo
                              select new SharesBaseInfo
                              {
                                  SharesCode = item.SharesCode,
                                  SharesName = item.SharesName,
                                  SharesPyjc = item.SharesPyjc,
                                  Area = item.Area,
                                  Business = item.Business,
                                  SharesHandCount = item.SharesHandCount,
                                  CirculatingCapital = item.CirculatingCapital ?? 0,
                                  Idea = item.Idea,
                                  Industry = item.Industry,
                                  Market = item.Market,
                                  TotalCapital = item.TotalCapital ?? 0,
                                  ClosedPrice=item.ClosedPrice,
                                  MarketStatus=item.MarketStatus
                              }).ToList();
                return result;
            }
        }
    }
}
