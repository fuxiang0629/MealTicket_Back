using FXCommon.Common;
using MealTicket_Handler.RunnerHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_APIService.runner
{
    public class RechargeRefundQueryRunner:Runner
    {
        public RechargeRefundQueryRunner()
        {
            Name = "RechargeRefundQueryRunner";
            SleepTime = 30000;
        }

        public override bool Check
        {
            get
            {
                return true;
            }
        }

        public override void Execute()
        {
            try
            {
                RunnerHelper.RechargeRefundQuery();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("RechargeRefundQuery出错", ex);
            }
        }
    }
}
