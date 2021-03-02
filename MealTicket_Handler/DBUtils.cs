using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class DBUtils
    {
        /// <summary>
        /// 计算其他成本
        /// </summary>
        /// <param name="IdList">持仓Id/委托Id</param>
        /// <param name="Type">1.持仓Id 2.委托Id</param>
        /// <returns></returns>
        public static long CalculateOtherCost(List<long> IdList,int Type) 
        {
            using (var db = new meal_ticketEntities())
            {
                //统计其他成本的项
                var par = (from item in db.t_system_param
                           where item.ParamName == "OtherCostRules"
                           select item).FirstOrDefault();
                if (par == null)
                {
                    return 0;
                }
                var parValue = JsonConvert.DeserializeObject<List<string>>(par.ParamValue);

                long cost = 0;
                List<t_account_shares_entrust> result = new List<t_account_shares_entrust>();
                if (Type == 1)
                {
                    var hold = (from item in db.t_account_shares_hold
                                where IdList.Contains(item.Id)
                                select item).ToList(); 
                    if (hold.Count()<=0)
                    {
                        return 0;
                    }
                    if (parValue.Contains("HoldServiceRules"))
                    {
                        cost = cost + hold.Sum(e=>e.ServiceFee);
                    }
                    result = (from item in db.t_account_shares_entrust
                              where IdList.Contains(item.HoldId)
                              select item).ToList();
                }
                if (Type == 2)
                {
                    result = (from item in db.t_account_shares_entrust
                              where IdList.Contains(item.Id)
                              select item).ToList();
                }
                if (result.Count() > 0)
                {
                    if (parValue.Contains("HandlingFeeRules"))
                    {
                        cost = cost + result.Sum(e=>e.HandlingFee);
                    }
                    if (parValue.Contains("BuyServiceRules"))
                    {
                        cost = cost + result.Sum(e => e.BuyService);
                    }
                    if (parValue.Contains("CommissionRules"))
                    {
                        cost = cost + result.Sum(e => e.Commission);
                    }
                    if (parValue.Contains("AdministrationFeeRules"))
                    {
                        cost = cost + result.Sum(e => e.AdministrationFee);
                    }
                    if (parValue.Contains("StampFeeRules"))
                    {
                        cost = cost + result.Sum(e => e.StampFee);
                    }
                    if (parValue.Contains("ProfitTaxRules"))
                    {
                        cost = cost + result.Sum(e => e.ProfitService);
                    }
                    if (parValue.Contains("TransferFeeRules"))
                    {
                        cost = cost + result.Sum(e => e.TransferFee);
                    }
                    if (parValue.Contains("SettlementFeeRules"))
                    {
                        cost = cost + result.Sum(e => e.SettlementFee);
                    }
                }
                return cost;
            }
        }
    }
}
