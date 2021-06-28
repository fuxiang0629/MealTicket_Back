using MealTicket_Handler;
using MealTicket_Handler.RunnerHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_APIService
{
    public class ServiceFeeRechargeRunner
    {
        /// <summary>
        /// 收取服务费
        /// </summary>
        public static void ServiceFeeRecharge(DateTime timeDate)
        {
            if (!RunnerHelper.CheckTradeDate())
            {
                return;
            }
            using (var db = new meal_ticketEntities())
            {
                //服务费规则
                var holdServiceRules = (from item in db.t_system_param
                                        where item.ParamName == "HoldServiceRules"
                                        select item.ParamValue).FirstOrDefault();
                if (string.IsNullOrEmpty(holdServiceRules))
                {
                    return;
                }
                var rulesJson = JsonConvert.DeserializeObject<dynamic>(holdServiceRules);
                if (rulesJson == null)
                {
                    return;
                }
                if (rulesJson.MinAmount == null || rulesJson.Rate == null)
                {
                    return;
                }

                var hold = (from item in db.t_account_shares_hold
                            where item.Status == 1 && item.ServiceAmount > 0 && item.LastServiceTime < timeDate
                            select item).ToList();
                foreach (var item in hold)
                {
                    long minAmount = rulesJson.MinAmount;
                    int rate = rulesJson.Rate;
                    var accountPar = (from x in db.t_account_par_setting
                                      where x.AccountId == item.AccountId && x.ParamName == "HoldServiceRules" && x.Status == 1
                                      select x).FirstOrDefault();
                    if (accountPar != null)
                    {
                        var temp = JsonConvert.DeserializeObject<dynamic>(accountPar.ParamValue);
                        if (temp != null && temp.MinAmount != null && temp.Rate != null)
                        {
                            minAmount = temp.MinAmount;
                            rate = temp.Rate;
                        }
                    }
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //计算服务费
                            long serviceFee = ((long)Math.Round(item.ServiceAmount * (rate * 1.0 / 100000), 0)) / 100 * 100;
                            item.LastServiceTime = timeDate;
                            item.ServiceFee = item.ServiceFee + serviceFee;
                            db.SaveChanges();
                            db.P_Calculate_DepositChange_Hold_Temp(item.Id, -serviceFee, "收取服务费", timeDate);

                            var remainServiceFee = serviceFee;
                            db.P_Calculate_WalletChange_Platform_Temp("serviceFee", serviceFee, remainServiceFee, item.Id, timeDate);

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
                }
            }
        }
    }
}
