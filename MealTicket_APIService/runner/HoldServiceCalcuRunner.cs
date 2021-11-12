﻿using FXCommon.Common;
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
        public HoldServiceCalcuRunner()
        {
            Name = "HoldServiceCalcuRunner";
            SleepTime = 600000;
        }

        public override bool Check
        {
            get
            {
                try
                {
                    //每天1-8点执行
                    TimeSpan tpNow = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                    if (tpNow  < Singleton.Instance.HoldServiceCalcuStartHour || tpNow > Singleton.Instance.HoldServiceCalcuEndHour)
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
                RunnerHelper.CalLimit();//计算连板数量
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("服务费计算出错", ex);
            }
        }
    }
}
