using FXCommon.Common;
using MealTicket_Handler;
using MealTicket_Handler.RunnerHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_APIService
{
    public class TransactiondataDateRunner : Runner
    {
        static DateTime LastExecuteDate = DateTime.Now.Date.AddDays(-1);

        public TransactiondataDateRunner()
        {
            Name = "DateRunner";
            SleepTime = Singleton.Instance.NewTransactiondataSleepTime;
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
                    //每天21 - 23点执行
                    if (DateTime.Now.Hour < Singleton.Instance.NewTransactiondataStartHour || DateTime.Now.Hour >= Singleton.Instance.NewTransactiondataEndHour)
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public override void Execute()
        {
            try
            {
                RunnerHelper.SendTransactionShares();
                LastExecuteDate = DateTime.Now.Date;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("实时更新股票分笔数据出错", ex);
            }
        }
    }
}
