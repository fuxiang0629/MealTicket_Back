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
    public class SharesGroupStatisticMtlineRunner:Runner
    {
        Dictionary<long, DateTime> lastGroupTimeKey1 = new Dictionary<long, DateTime>();
        Dictionary<long, DateTime> lastGroupTimeKey2 = new Dictionary<long, DateTime>();

        DateTime lastExecuteDate = DateTime.Now.Date.AddDays(-1);

        public SharesGroupStatisticMtlineRunner()
        {
            Name = "SharesGroupStatisticMtlineRunner";
            SleepTime = 15000;
        }

        public override bool Check
        {
            get
            {
                if (DbHelper.CheckTradeTime7())
                {
                    return true;
                }
                DateTime timeNow = DateTime.Now;
                //每天21-23点执行
                TimeSpan tpNow = TimeSpan.Parse(timeNow.ToString("HH:mm:ss"));
                if (tpNow < Singleton.Instance.HotspotDataUpdateStartTime || tpNow > Singleton.Instance.HotspotDataUpdateEndTime)
                {
                    return false;
                }
                if (lastExecuteDate >= timeNow.Date)
                {
                    return false;
                }
                lastExecuteDate = DateTime.Now.Date;
                lastGroupTimeKey1 = new Dictionary<long, DateTime>();
                lastGroupTimeKey2 = new Dictionary<long, DateTime>();
                return true;
            }
        }

        public override void Execute()
        {
            try
            {
                //Logger.WriteFileLog("开始重算"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),null);
                Task[] taskArr = new Task[2];
                taskArr[0] = new Task(()=> 
                {
                    SharesGroupStatisticMtlineHelper.Cal_SharesGroupStatisticMtline(DateTime.Now.Date, ref lastGroupTimeKey1, false);
                });
                taskArr[1] = new Task(() =>
                {
                    SharesGroupStatisticMtlineHelper.Cal_SharesGroupStatisticMtline(DateTime.Now.Date, ref lastGroupTimeKey2, true);
                });
                taskArr[0].Start();
                taskArr[1].Start();

                Task.WaitAll(taskArr);
                //Logger.WriteFileLog("结束重算" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);

            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("SharesGroupStatisticMtlineRunner出错", ex);
            }
        }
    }
}
