using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionDataUpdate_App
{
    public class TransactiondataUpdateRunner:Runner
    {
        public TransactiondataUpdateRunner()
        {
            Name = "TransactiondataUpdateRunner";
            SleepTime = Singleton.Instance.SendPeriodTime;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    DateTime timeNow = DateTime.Now;
                    if (!DbHelper.CheckTradeDate())
                    {
                        return false;
                    }
                    if (timeNow < DateTime.Parse(timeNow.ToString("yyyy-MM-dd 09:25:00")))
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
                Logger.WriteFileLog("===开始获取数据===", null);
                var list=DataHelper.TdxHq_GetTransactionData();
                Logger.WriteFileLog("===结束获取数据===", null);
                if (list.Count() > 0)
                {
                    Logger.WriteFileLog("===开始更新数据===", null);
                    DataHelper.UpdateToDataBase(list);
                    Logger.WriteFileLog("===结束更新数据===", null);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新分笔数据出错", ex);
            }
        }
    }
}
