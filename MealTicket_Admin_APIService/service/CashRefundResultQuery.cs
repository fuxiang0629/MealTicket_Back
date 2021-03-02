using FXCommon.Common;
using MealTicket_Admin_Handler;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_APIService.service
{
    public class CashRefundResultQuery:Runner
    {
        private ServiceHandler serviceHandler;

        public CashRefundResultQuery() 
        {
            Name = "CashRefundResultQuery";
            SleepTime = 30000;
            serviceHandler = WebApiManager.Kernel.Get<ServiceHandler>();
        }

        public override bool Check 
        {
            get 
            {
                return true;
            }
        }

        public override void Execute()
        {
            try
            {
                serviceHandler.CashRefundResultQuery();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("RechargeRefundQuery出错", ex);
            }
        }
    }
}
