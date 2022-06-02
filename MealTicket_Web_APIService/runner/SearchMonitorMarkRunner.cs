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
    public class SearchMonitorMarkRunner : Runner
    {
        public SearchMonitorMarkRunner()
        {
            Name = "SearchMonitorMarkRunner";
            SleepTime = Singleton.Instance.SearchMarkInterval;
        }

        public override bool Check
        {
            get
            {
                SleepTime = Singleton.Instance.SearchMarkInterval;
                return true;
            }
        }

        public override void Execute()
        {
            try
            {
                Logger.WriteFileLog("开始执行搜索"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),null);
                SearchHelper searchHelper = new SearchHelper();
                searchHelper.SearchMonitor(2);
                Logger.WriteFileLog("结束执行搜索" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("监控搜索2出错", ex);
            }
        }
    }
}
