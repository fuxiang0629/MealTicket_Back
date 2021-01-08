using FXCommon.Common;
using MealTicket_Web_Handler.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_APIService.runner
{
    public class SharesAutoJoinRunner : Runner
    {
        object finishLock = new object();

        bool IsFinish = true;

        public bool TryToEnter()
        {
            lock (finishLock)
            {
                if (!IsFinish) { return false; }

                IsFinish = false;
            }

            return true;
        }

        public bool TryToLeave()
        {
            lock (finishLock)
            {
                if (IsFinish) { return false; }

                IsFinish = true;
            }

            return true;
        }

        public SharesAutoJoinRunner()
        {
            Name = "SharesAutoJoinRunner";
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
            if (!TryToEnter())
            {
                return;
            }
            Task task = new Task(() =>
            {
                try
                {
                    RunnerHelper.SharesAutoJoin();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("自动加入股票出错", ex);
                }
                finally
                {
                    TryToLeave();
                }
            });
            task.Start();
        }
    }
}
