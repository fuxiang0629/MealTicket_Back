using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class SharesQuotesUpdateRunner : Runner
    {
        //是否可以进入行情更新
        object quotesLock = new object();
        public bool QuotesCanEnter = true;

        public bool TryQuotesCanEnter()
        {
            lock (quotesLock)
            {
                if (!QuotesCanEnter) { return false; }

                QuotesCanEnter = false;
            }

            return true;
        }


        public bool TryQuotesCanLeave()
        {
            lock (quotesLock)
            {
                if (QuotesCanEnter) { return false; }

                QuotesCanEnter = true;
            }

            return true;
        }

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
                    if (!Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.RunStartTime)) && !Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.RunEndTime)))
                    {
                        Singleton.Instance.TryToSetIsRun(false);
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Singleton.Instance.TryToSetIsRun(false);
                    return false;
                }
            }
        }

        public override void Execute()
        {
            if (!TryQuotesCanEnter())
            {
                return;
            }
            Task task = new Task(() =>
            {
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    Console.WriteLine("==========开始获取五档行情=================");
                    var list = ShareHelper.TdxHq_GetSecurityQuotes();
                    stopwatch.Stop();
                    Console.WriteLine("=====获取五档行情结束:" + stopwatch.ElapsedMilliseconds + "============");
                    Console.WriteLine("==========开始更新五档行情=================");
                    stopwatch.Restart();
                    if (list != null && list.Count() > 0)
                    {
                        DataBaseHelper.UpdateSharesQuotes(list);
                    }
                    stopwatch.Stop();
                    Console.WriteLine("=====五档行情更新结束:" + stopwatch.ElapsedMilliseconds + "============");
                    Console.WriteLine("");
                    Console.WriteLine("");
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("更新五档数据出错", ex);
                }
                finally
                {
                    TryQuotesCanLeave();
                    Singleton.Instance.TryToSetIsRun(true);
                }
            });
            task.Start();
        }
    }
}
