using FXCommon.Common;
using MealTicket_Handler.RunnerHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_APIService.runner
{
    /// <summary>
    /// 通知短信发送
    /// </summary>
    public class SmsSendRunner:Runner
    {
        public SmsSendRunner()
        {
            Name = "SmsSendRunner";
            SleepTime = 5000;
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
                RunnerHelper.SmsSend();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("短信发送出错", ex);
            }
        }
    }
}
