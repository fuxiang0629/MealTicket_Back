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
    public class PlateLeaderTriRunner : Runner
    {
        public PlateLeaderTriRunner()
        {
            Name = "PlateLeaderTriRunner";
            SleepTime = 3000;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    if (!DbHelper.CheckTradeTime2())
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
                PlateLeaderTriHelper.Cal_Shares_Energy_Table();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("PlateLeaderTriRunner计算出错", ex);
            }
        }
    }
}
