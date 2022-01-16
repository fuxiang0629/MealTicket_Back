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
    public class PlateTagCalRunner : Runner
    {
        public PlateTagCalRunner()
        {
            Name = "PlateTagCalRunner";
            SleepTime = Singleton.Instance.PlateTagCalIntervalTime;
        }

        public override bool Check
        {
            get
            {
                SleepTime = Singleton.Instance.PlateTagCalIntervalTime;
                try
                {
                    //if (!DbHelper.CheckTradeTime2())
                    //{
                    //    return false;
                    //}
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
                PlateTagCalHelper.Calculate();
                SharesTagCalHelper.Calculate();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("板块标记计算出错", ex);
            }
        }
    }
}
