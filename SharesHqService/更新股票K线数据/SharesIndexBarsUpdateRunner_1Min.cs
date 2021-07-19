using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class SharesIndexBarsUpdateRunner_1Min : Runner
    {

        public SharesIndexBarsUpdateRunner_1Min()
        {
            SleepTime = 60000;
            Name = "SharesIndexBarsUpdateRunner_1Min";
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
            var list = ShareHelper.TdxHq_GetSecurityQuotes();
        }
    }
}
