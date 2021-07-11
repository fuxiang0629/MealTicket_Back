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
    /// 强制平仓
    /// </summary>
    public class ClosingForceRunner : Runner
    {
        public ClosingForceRunner()
        {
            Name = "ClosingForceRunner";
            SleepTime = Singleton.Instance.ClosingForceSleepTime_TradeDate;
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
                    if (!RunnerHelper.CheckTradeTime2())
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
                RunnerHelper.ClosingForce();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("强制平仓出错", ex);
            }
        }
    }
}
