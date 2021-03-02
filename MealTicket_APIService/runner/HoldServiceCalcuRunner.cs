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
    /// 服务费计算
    /// </summary>
    public class HoldServiceCalcuRunner : Runner
    {
        static DateTime LastExecuteDate = DateTime.Now.Date.AddDays(-1);

        public HoldServiceCalcuRunner()
        {
            Name = "HoldServiceCalcuRunner";
            SleepTime = Singleton.Instance.HoldServiceCalcuSleepTime;
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
                    //每天1-8点执行
                    if (DateTime.Now.Hour < Singleton.Instance.HoldServiceCalcuStartHour || DateTime.Now.Hour >= Singleton.Instance.HoldServiceCalcuEndHour)
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
                RunnerHelper.HoldClose();//关闭持仓
                RunnerHelper.JoinService();//加入计算服务费金额
                RunnerHelper.ServiceFeeRecharge();//计算服务费
                RunnerHelper.SharesAllot();//执行派息
                LastExecuteDate = DateTime.Now.Date;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("服务费计算出错", ex);
            }
        }
    }
}
