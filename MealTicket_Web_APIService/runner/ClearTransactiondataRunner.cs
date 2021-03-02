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
    public class ClearTransactiondataRunner : Runner
    {
        static DateTime LastExecuteDate = DateTime.Now.Date.AddDays(-1);

        public ClearTransactiondataRunner()
        {
            Name = "ClearTransactiondataRunner";
            SleepTime = Singleton.Instance.ClearTransactiondataSleepTime;
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
                    //每天3-8点执行
                    if (DateTime.Now.Hour < Singleton.Instance.ClearTransactiondataStartHour || DateTime.Now.Hour >= Singleton.Instance.ClearTransactiondataEndHour)
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
                RunnerHelper.ClearTransactiondata();//清理
                LastExecuteDate = DateTime.Now.Date;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每日清除分笔数据出错", ex);
            }
        }
    }
}
