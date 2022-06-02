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
    public class SharesGroupStatisticRunner:Runner
    {
        public SharesGroupStatisticRunner()
        {
            Name = "SharesGroupStatisticRunner";
            SleepTime = 3000;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    if (!DbHelper.CheckTradeTime7())
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
                SharesGroupStatisticHelper.Cal_SharesGroupStatistic(DateTime.Now.Date);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("SharesGroupStatisticRunner", ex);
            }
        }
    }
}
