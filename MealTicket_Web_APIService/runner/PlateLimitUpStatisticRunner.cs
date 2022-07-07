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
    public class PlateLimitUpStatisticRunner : Runner
    {
        bool isInit = true;

        public PlateLimitUpStatisticRunner()
        {
            Name = "PlateLimitUpStatisticRunner";
            SleepTime = 3000;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    if (!DbHelper.CheckTradeTime7() && !isInit)
                    {
                        return false;
                    }
                    isInit = false;
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
                //DateTime datePar = DbHelper.GetLastTradeDate2(0, 0, 0, 0, DateTime.Now.AddHours(-9));
                //for (int idx = 0; idx < 30; idx++)
                //{
                //    PlateLimitUpStatisticHelper.Cal_PlateLimitUpStatistic(datePar);
                //    datePar = DbHelper.GetLastTradeDate2(0, 0, 0, -1, datePar);
                //}
                DateTime datePar = DbHelper.GetLastTradeDate2(0, 0, 0, 0, DateTime.Now.AddHours(-9));
                PlateLimitUpStatisticHelper.Cal_PlateLimitUpStatistic(datePar);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("PlateLimitUpStatisticRunner", ex);
            }
        }
    }
}
