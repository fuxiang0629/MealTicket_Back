using FXCommon.Common;
using MealTicket_Web_Handler.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_APIService.runner
{
    public class MarketSentimentScoreCalRunner:Runner
    {
        public MarketSentimentScoreCalRunner()
        {
            Name = "MarketSentimentScoreCalRunner";
            SleepTime = 3600000;
        }

        public override bool Check
        {
            get
            {
                try
                {
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
                MarketSentimentScoreCalHelper.Cal_MarketSentimentScore();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("MarketSentimentScoreCalRunner", ex);
            }
        }
    }
}
