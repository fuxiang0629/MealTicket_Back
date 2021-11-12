using FXCommon.Common;
using MealTicket_Web_Handler;
using MealTicket_Web_Handler.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_APIService.runner
{
    /// <summary>
    /// 监控搜索
    /// </summary>
    public class SearchMonitorRunner:Runner
    {
        public SearchMonitorRunner()
        {
            Name = "SearchMonitorRunner";
            SleepTime = Singleton.Instance.SearchInterval;
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
                RunnerHelper.SearchMonitor();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("监控搜索出错", ex);
            }
        }
    }
}
