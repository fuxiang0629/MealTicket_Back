using FXCommon.Common;
using MealTicket_Handler.RunnerHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_APIService.runner
{
    public class TradeAutoBuyRunner : Runner
    {
        public TradeAutoBuyRunner()
        {
            Name = "TradeAutoBuyRunner";
            SleepTime = 1000;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    if (!RunnerHelper.CheckTradeDate())
                    {
                        return false;
                    }
                    if (!RunnerHelper.CheckTradeTime2(null, false, true, false))
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
                RunnerHelper.TradeAutoBuy();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("自动买入交易出错", ex);
            }
        }
    }
}
