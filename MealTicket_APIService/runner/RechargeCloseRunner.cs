using FXCommon.Common;
using MealTicket_Handler.RunnerHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_APIService.runner
{
    public class RechargeCloseRunner : Runner
    {
        public RechargeCloseRunner()
        {
            Name = "RechargeCloseRunner";
            SleepTime = 60000;
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
                RunnerHelper.RechargeClose();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("充值订单关闭出错", ex);
            }
        }
    }
}
