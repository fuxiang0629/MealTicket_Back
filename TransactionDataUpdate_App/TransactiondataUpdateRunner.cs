﻿using FXCommon.Common;
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
                var list=DataHelper.TdxHq_GetTransactionData();
                if (list.Count() > 0)
                {
                    DataHelper.UpdateToDataBase(list);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新分笔数据出错", ex);
            }
        }
    }
}
