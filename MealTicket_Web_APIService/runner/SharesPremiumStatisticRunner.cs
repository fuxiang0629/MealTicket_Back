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
    public class SharesPremiumStatisticRunner : Runner
    {
        public SharesPremiumStatisticRunner()
        {
            Name = "SharesPremiumStatisticRunner";
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
                var date = DbHelper.GetLastTradeDate2(0,0,0,0, DateTime.Now.AddHours(-9));
                SharesPremiumStatisticHelper.Cal_SharesPremiumStatistic(date);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("SharesPremiumStatisticRunner", ex);
            }
        }
    }
}
