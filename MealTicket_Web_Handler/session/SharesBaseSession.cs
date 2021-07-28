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
        public SharesBaseSession()
        {
            Name = "SharesBaseSession";
        }
        public override List<SharesBaseInfo> UpdateSession()
        {
            DateTime endTime = Helper.GetLastTradeDate(-9, 0, 0);
            DateTime startTime= Helper.GetLastTradeDate(-9, 0, 0, Singleton.Instance.QuotesDaysShow);
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

                var quotes_date = (from item in db.t_shares_quotes_date
                                   where item.LastModified >= startTime && item.LastModified < endTime
                                   select item).ToList();
                //var DaysAvg = (from item in quotes_date
                //               group item by new { item.Market, item.SharesCode } into g
                //               let temp = g.OrderByDescending(e => e.Date).Select((e, i) => new { Value = e, Index = i }).Where(e => e.Value.PriceType == 1).FirstOrDefault()
                //               select new
                //               {
                //                   Market = g.Key.Market,
                //                   SharesCode = g.Key.SharesCode,
                //                   DaysAvgDealCount = g.Average(e => e.TotalCount),
                //                   DaysAvgDealAmount = g.Average(e => e.TotalAmount),
                //                   LimitUpCount = g.Where(e => e.PriceType == 1).Count(),
                //                   LimitDownCount = g.Where(e => e.PriceType == 2).Count(),
                //                   LimitUpDay = temp == null ? 0 : temp.Index
                //               }).ToList();

                var DaysAvg = (from item in quotes_date
                               group item by new { item.Market, item.SharesCode } into g
                               select new
                               {
                                   Market = g.Key.Market,
                                   SharesCode = g.Key.SharesCode,
                                   DaysAvgDealCount = g.Average(e => e.TotalCount),
                                   DaysAvgDealAmount = g.Average(e => e.TotalAmount),
                                   LimitUpCount = g.Where(e => e.PriceType == 1).Count(),
                                   LimitDownCount = g.Where(e => e.PriceType == 2).Count(),
                                   LimitUpDay = g.Where(e => e.PriceType == 1).OrderByDescending(e => e.Date).Select(e=>e.Date).FirstOrDefault()
                               }).ToList();

                var PreDay = (from item in quotes_date
                               group item by new { item.Market, item.SharesCode} into g
                               select new
                               {
                                   Market = g.Key.Market,
                                   SharesCode = g.Key.SharesCode,
                                   PreDayDealCount = g.OrderByDescending(e=>e.Date).Select(e=>e.TotalCount).FirstOrDefault(),
                                   PreDayDealAmount = g.OrderByDescending(e => e.Date).Select(e => e.TotalAmount).FirstOrDefault()
                               }).ToList();
                result = (from item in result
                          join item2 in DaysAvg on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                          join item3 in PreDay on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode }
                          select new SharesBaseInfo
                          {
                              SharesCode=item.SharesCode,
                              SharesName = item.SharesName,
                              SharesPyjc = item.SharesPyjc,
                              Area = item.Area,
                              Business = item.Business,
                              SharesHandCount = item.SharesHandCount,
                              CirculatingCapital = item.CirculatingCapital,
                              Idea = item.Idea,
                              Industry = item.Industry,
                              Market = item.Market,
                              TotalCapital = item.TotalCapital,
                              ClosedPrice = item.ClosedPrice,
                              MarketStatus = item.MarketStatus,
                              DaysAvgDealAmount=(long)item2.DaysAvgDealAmount,
                              DaysAvgDealCount=(int)item2.DaysAvgDealCount,
                              LimitDownCount=item2.LimitDownCount,
                              LimitUpCount=item2.LimitUpCount,
                              LimitUpDay=item2.LimitUpDay,
                              PreDayDealAmount=item3.PreDayDealAmount,
                              PreDayDealCount=item3.PreDayDealCount
                          }).ToList();
                return result;
            }
        }
    }
}
