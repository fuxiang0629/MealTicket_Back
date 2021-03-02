using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class SharesQuotesUpdateRunner:Runner
    {
        static int checkTimes = 0;

        public SharesQuotesUpdateRunner()
        {
            SleepTime = Singleton.Instance.SshqUpdateRate;
            Name = "SharesQuotesUpdateRunner";
        }

        public override bool Check
        {
            get
            {
                DateTime timeNow = DateTime.Now;
                try
                {
                    //如果当前是交易时间，就60秒检查一次
                    if (checkTimes % 60 == 0)
                    {
                        checkTimes = 0;
                        if (!Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.RunStartTime)) && !Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.RunEndTime)))
                        {
                            //如果当前不是交易时间，30秒检查一次
                            SleepTime = 30000;
                            return false;
                        }
                    }
                    SleepTime = Singleton.Instance.SshqUpdateRate;
                    checkTimes++;
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
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Console.WriteLine("==========开始获取五档行情=================");
                var list = ShareHelper.TdxHq_GetSecurityQuotes();
                Console.WriteLine("==========开始更新五档行情=================");
                if (list != null && list.Count() > 0)
                {
                    DataBaseHelper.UpdateSharesQuotes(list);
                }
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
                Console.WriteLine("==========五档行情更新结束=================");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新所有股票数据出错", ex);
            }
        }
    }
}
