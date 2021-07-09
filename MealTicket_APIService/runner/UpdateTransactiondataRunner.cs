using FXCommon.Common;
using MealTicket_Handler;
using MealTicket_Handler.RunnerHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_APIService.runner
{
    /// <summary>
    /// 每天更新股票分笔数据
    /// </summary>
    public class UpdateTransactiondataRunner : Runner
    {
        static DateTime LastExecuteDate = DateTime.Now.Date.AddDays(-1);

        public UpdateTransactiondataRunner()
        {
            Name = "UpdateTransactiondataRunner";
            SleepTime = 600000;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    if (!RunnerHelper.CheckTradeDate())
                    {
                        return false;
                    }
                    //判断当天是否执行过
                    if (LastExecuteDate >= DateTime.Now.Date)
                    {
                        return false;
                    }
                    //每天21-23点执行
                    TimeSpan tpNow = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                    if (tpNow < Singleton.Instance.TransactiondataStartHour || tpNow > Singleton.Instance.TransactiondataEndHour)
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
                RunnerHelper.UpdateTransactiondata();
                LastExecuteDate = DateTime.Now.Date;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每天更新股票分笔数据出错", ex);
            }
        }
    }
}
