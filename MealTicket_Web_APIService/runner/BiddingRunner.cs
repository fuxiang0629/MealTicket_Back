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
    public class BiddingRunner:Runner
    {
        public BiddingRunner()
        {
            Name = "BiddingRunner";
            SleepTime = 3000;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    if (!DbHelper.CheckTradeDate())
                    {
                        return false;
                    }
                    TimeSpan spNow = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                    if (spNow < TimeSpan.Parse("09:25:00") || spNow > TimeSpan.Parse("09:29:00"))
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public override void Execute()
        {
            try
            {
                BiddingHelper.Cal_Bidding();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("BiddingRunner", ex);
            }
        }
    }
}
