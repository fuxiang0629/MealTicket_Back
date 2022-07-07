using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_APIService.runner
{
    public class SearchBiddingRunner : Runner
    {
        DateTime? lastDate = null;

        public SearchBiddingRunner()
        {
            Name = "SearchBiddingRunner";
            SleepTime = 15000;
        }

        public override bool Check
        {
            get
            {
                if (lastDate == null)
                {
                    using (var db = new meal_ticketEntities())
                    {
                        var biddingSys = (from item in db.t_shares_bidding_sys
                                          orderby item.Date descending
                                          select item).FirstOrDefault();
                        if (biddingSys == null)
                        {
                            lastDate = DateTime.Now.Date.AddDays(-1);
                        }
                        else
                        {
                            lastDate = biddingSys.Date;
                        }              
                    }
                }
                if (lastDate >= DateTime.Now.Date)
                {
                    return false;
                }
                if (!DbHelper.CheckTradeDate())
                {
                    return false;
                }
                TimeSpan spNow = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                if (spNow < TimeSpan.Parse("09:25:30"))
                {
                    return false;
                }
                return true;
            }
        }

        public override void Execute()
        {
            try
            {
                SearchBiddingHelper.Cal_SearchBidding();
                lastDate = DateTime.Now.Date;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("SearchBiddingRunner出错", ex);
            }
        }
    }
}
