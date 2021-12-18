using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            SleepTime = 600000;
            Name = "AllSharesUpdateRunner";
        }

        public override bool Check 
        {
            get 
            {
                try
                {
                    bool istest = bool.Parse(ConfigurationManager.AppSettings["IsTest"]);
                    if (istest)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                { }
                try
                {
                    //判断当天是否执行过
                    if (LastExecuteDate >= DateTime.Now.Date)
                    {
                        return false;
                    }
                    //每天凌晨1-4点执行
                    TimeSpan tpNow = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                    if (tpNow < Singleton.Instance.session.GetAllSharesStartHour() || tpNow > Singleton.Instance.session.GetAllSharesEndHour())
                    {
                        return false;
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
                var list1=ShareHelper.TdxHq_GetSecurityList(0);
                var list2 = ShareHelper.TdxHq_GetSecurityList(1);

                var list = list1.Concat(list2).ToList();
                if (list.Count()>0)
                {
                    DataBaseHelper.UpdateAllShares(list);
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
