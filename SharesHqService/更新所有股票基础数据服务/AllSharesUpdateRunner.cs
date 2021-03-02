using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class AllSharesUpdateRunner : Runner
    {
        static DateTime LastExecuteDate = DateTime.Now.Date.AddDays(-1);

        public AllSharesUpdateRunner()
        {
            SleepTime = 1800000;
            Name = "AllSharesUpdateRunner";
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
                    //每天凌晨1-4点执行
                    if (DateTime.Now.Hour < Singleton.Instance.AllSharesStartHour || DateTime.Now.Hour > Singleton.Instance.AllSharesEndHour)
                    {
                        //return false;
                    }
                    var checkDate = Helper.CheckTradeDate();
                    if (!checkDate)
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
                var list=ShareHelper.TdxHq_GetSecurityList(0);
                if (list != null && list.Count()>0)
                {
                    DataBaseHelper.UpdateAllShares(list, 0);
                }
                list = ShareHelper.TdxHq_GetSecurityList(1);
                if (list != null && list.Count() > 0)
                {
                    DataBaseHelper.UpdateAllShares(list, 1);
                }

                LastExecuteDate = DateTime.Now.Date;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新所有股票数据出错",ex);
            }
        }
    }
}
