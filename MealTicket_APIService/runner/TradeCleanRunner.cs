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
    /// 每日清除当天购买委托数据
    /// </summary>
    public class TradeCleanRunner:Runner
    {
        static DateTime LastExecuteDate = DateTime.Now.Date.AddDays(-1);

        public TradeCleanRunner()
        {
            Name = "TradeCleanRunner";
            SleepTime = Singleton.Instance.TradeCleanSleepTime;
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
                    //每天21-23点执行
                    if (DateTime.Now.Hour < Singleton.Instance.TradeCleanStartHour || DateTime.Now.Hour >= Singleton.Instance.TradeCleanEndHour)
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
                RunnerHelper.TradeClean();//清理
                RunnerHelper.JoinCanSold();//可售
                LastExecuteDate = DateTime.Now.Date;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每日清除当天委托数据出错", ex);
            }
        }
    }
}
