using FXCommon.Common;
using MealTicket_Handler.RunnerHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_APIService.runner
{
    public class RechargeRefundRunner : Runner
    {
        public RechargeRefundRunner()
        {
            Name = "RechargeRefundRunner";
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
                RunnerHelper.RechargeRefund();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("充值订单关闭退款出错", ex);
            }
        }
    }
}
