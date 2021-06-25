using FXCommon.Common;
using MealTicket_Web_Handler;
using MealTicket_Web_Handler.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_Web_APIService
{
    public class TransactiondataRealTimeRunner : Runner
    {
        public TransactiondataRealTimeRunner()
        {
            Name = "RealTimeRunner";
            SleepTime = Singleton.Instance.NewTransactionDataSendPeriodTime;
        }

        public override bool Check
        {
            get
            {
                DateTime timeNow = DateTime.Now;
                try
                {
                    //检查交易时间
                    if (!Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.NewTransactionDataRunStartTime)) && !Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.NewTransactionDataRunEndTime)))
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
