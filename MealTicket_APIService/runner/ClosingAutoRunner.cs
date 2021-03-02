using FXCommon.Common;
using MealTicket_Handler;
using MealTicket_Handler.RunnerHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_APIService.runner
{
    /// <summary>
    /// 自动平仓
    /// </summary>
    public class ClosingAutoRunner : Runner
    {
        public ClosingAutoRunner()
        {
            Name = "ClosingAutoRunner";
            SleepTime = Singleton.Instance.ClosingAutoSleepTime_TradeDate;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    if (!RunnerHelper.CheckTradeDate())
                    {
                        SleepTime = Singleton.Instance.ClosingAutoSleepTime_NoTradeDate;
                        return false;
                    }
                    if (!RunnerHelper.CheckTradeTime2())
                    {
                        SleepTime = Singleton.Instance.ClosingAutoSleepTime_TradeDate;
                        return false;
                    }
                    SleepTime = Singleton.Instance.ClosingAutoSleepTime_TradeDate;
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
                RunnerHelper.ClosingAuto();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("自动平仓出错", ex);
            }
        }
    }
}
