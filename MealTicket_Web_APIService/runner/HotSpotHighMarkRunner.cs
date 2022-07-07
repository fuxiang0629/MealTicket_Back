using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler;
using MealTicket_Web_Handler.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_APIService.runner
{
    public class HotSpotHighMarkRunner : Runner
    {
        static DateTime LastExecuteDate = DateTime.Now.Date.AddDays(-1);

        public HotSpotHighMarkRunner()
        {
            Name = "HotSpotHighMarkRunner";
            SleepTime = 60000 * 10;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    //判断当天是否执行过
                    if (LastExecuteDate >= DateTime.Now.Date)
                    {
                        return false;
                    }
                    if (!DbHelper.CheckTradeDate())
                    {
                        return false;
                    }
                    TimeSpan spNow = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                    if (spNow > TimeSpan.Parse("04:00:00") || spNow < TimeSpan.Parse("02:00:00"))
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
                var lastTradeDate = DbHelper.GetLastTradeDate2(0, 0, 0, -1, DateTime.Now.Date);
                HotSpotHighMarkHelper.Cal_HotSpotHighMark(lastTradeDate);
                LastExecuteDate = DateTime.Now.Date;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("HotSpotHighMarkRunner", ex);
            }
        }
    }
}
