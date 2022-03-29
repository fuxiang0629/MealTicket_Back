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
    public class HotSpotHisRunner : Runner
    {
        static DateTime LastExecuteDate = DateTime.Now.Date.AddDays(-1);
        public HotSpotHisRunner()
        {
            Name = "HotSpotHisRunner";
            SleepTime = 60000*10;
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
                    if (spNow > TimeSpan.Parse("04:00:00"))
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
                HotSpotHisHelper.Cal_HotSpotHis();
                LastExecuteDate = DateTime.Now.Date;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("HotSpotHisRunner", ex);
            }
        }
    }
}
