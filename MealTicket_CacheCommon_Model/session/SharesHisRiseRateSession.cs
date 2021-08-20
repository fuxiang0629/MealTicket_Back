using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_CacheCommon_Session.session
{
    public class SharesHisRiseRateSession : Session<List<SharesHisRiseRateInfo_Session>>
    {
        public SharesHisRiseRateSession()
        {
            Name = "SharesHisRiseRateSession";
            dataKey = "SharesHisRiseRateSession";
            username = "mealTicket_baseData";
        }
        public override List<SharesHisRiseRateInfo_Session> UpdateSession()
        {
            DateTime EndTime = Helper.GetLastTradeDate(-9, 0, 0);
            DateTime startTime = Helper.GetLastTradeDate(-9, 0, 0, 20);
            using (var db = new meal_ticketEntities())
            {
                List<SharesHisRiseRateInfo_Session> result = new List<SharesHisRiseRateInfo_Session>();
                var shares_quotes_date = (from item in db.t_shares_quotes_date
                                          where item.LastModified >= startTime && item.LastModified < EndTime
                                          select item).ToList();
                var sharesList = shares_quotes_date.GroupBy(e => new { e.Market, e.SharesCode }).ToList();
                foreach (var item in sharesList)
                {
                    int YestodayRiseRate = 0;
                    int TwoDaysRiseRate = 0;
                    int ThreeDaysRiseRate = 0;

                    int DaysAvgDealCount = 0;
                    long DaysAvgDealAmount = 0;
                    int LimitUpCount = 0;
                    int LimitDownCount = 0;

                    int PreDayDealCount = 0;
                    long PreDayDealAmount = 0;
                    string LimitUpDay = "";

                    try
                    {
                        var temp1 = item.OrderByDescending(e => e.LastModified).FirstOrDefault();
                        if (temp1 == null)
                        {
                            throw new Exception();
                        }
                        YestodayRiseRate = temp1.ClosedPrice == 0 ? 0 : (int)Math.Round(((temp1.PresentPrice - temp1.ClosedPrice) * 1.0 / temp1.ClosedPrice) * 10000, 0);
                        PreDayDealCount = temp1.TotalCount;
                        PreDayDealAmount = temp1.TotalAmount;

                        var temp2 = item.Where(e => e.LastModified < temp1.LastModified).OrderByDescending(e => e.LastModified).FirstOrDefault();
                        if (temp2 == null)
                        {
                            throw new Exception();
                        }
                        TwoDaysRiseRate = temp2.ClosedPrice == 0 ? 0 : (int)Math.Round(((temp1.PresentPrice - temp2.ClosedPrice) * 1.0 / temp2.ClosedPrice) * 10000, 0);
                        var temp3 = item.Where(e => e.LastModified < temp2.LastModified).OrderByDescending(e => e.LastModified).FirstOrDefault();
                        if (temp3 == null)
                        {
                            throw new Exception();
                        }
                        ThreeDaysRiseRate = temp3.ClosedPrice == 0 ? 0 : (int)Math.Round(((temp1.PresentPrice - temp3.ClosedPrice) * 1.0 / temp3.ClosedPrice) * 10000, 0);

                        LimitUpDay = item.Where(e => e.PriceType == 1).OrderByDescending(e => e.Date).Select(e => e.Date).FirstOrDefault();
                        DaysAvgDealCount = (int)item.Average(e => e.TotalCount);
                        DaysAvgDealAmount = (long)item.Average(e => e.TotalAmount);
                        LimitUpCount = item.Where(e => e.PriceType == 1).Count();
                        LimitDownCount = item.Where(e => e.PriceType == 2).Count();

                    }
                    catch (Exception ex)
                    { }
                    finally
                    {
                        result.Add(new SharesHisRiseRateInfo_Session
                        {
                            SharesCode = item.Key.SharesCode,
                            Market = item.Key.Market,
                            ThreeDaysRiseRate = ThreeDaysRiseRate,
                            TwoDaysRiseRate = TwoDaysRiseRate,
                            YestodayRiseRate = YestodayRiseRate,
                            PreDayDealCount=PreDayDealCount,
                            PreDayDealAmount=PreDayDealAmount,
                            DaysAvgDealAmount= DaysAvgDealAmount,
                            DaysAvgDealCount= DaysAvgDealCount,
                            LimitDownCount=LimitDownCount,
                            LimitUpCount=LimitUpCount,
                            LimitUpDay=LimitUpDay
                        });
                    }
                }
                return result;
            }
        }
    }
}
