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
    public class TransactiondataRealTimeRunner : Runner
    {
        public TransactiondataRealTimeRunner()
        {
            Name = "RealTimeRunner";
            SleepTime = Singleton.Instance.NewTransactionDataSendPeriodTime <= 0?3000: Singleton.Instance.NewTransactionDataSendPeriodTime;
        }

        public override bool Check
        {
            get
            {
                DateTime timeNow = DateTime.Now;
                try
                {
                    if (Singleton.Instance.NewTransactionDataSendPeriodTime <= 0)
                    {
                        return false;
                    }
                    //检查交易时间
                    if (!RunnerHelper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.NewTransactionDataRunStartTime)) && !RunnerHelper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.NewTransactionDataRunEndTime)))
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
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("实时更新股票分笔数据出错", ex);
            }
        }
    }
}
