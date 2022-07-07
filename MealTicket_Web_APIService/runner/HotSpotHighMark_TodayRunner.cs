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
    public class HotSpotHighMark_TodayRunner : Runner
    {
        bool isInit = false;

        public HotSpotHighMark_TodayRunner()
        {
            Name = "HotSpotHighMark_TodayRunner";
            SleepTime = 3000;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    if (!DbHelper.CheckTradeDate() && isInit)
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

                HotSpotHighMarkHelper.Cal_HotSpotHighMark_Today();
                if (!isInit)
                {
                    isInit = true;
                }

            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("HotSpotHighMarkRunner", ex);
            }
        }
    }
}
