using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Runner
{
    public class RunnerHelper
    {
        public static void ClearTransactiondata()
        {
            string sql = "";
            using (var db = new meal_ticketEntities(ConfigurationManager.ConnectionStrings["meal_ticketEntities2"].ConnectionString))
            {
                db.Database.CommandTimeout = 7200;
                try
                {
                    sql = @"declare @currTime datetime=getdate()
                    insert into t_shares_transactiondata_history
                    ([Market],[SharesCode],[Time],[TimeStr],[Price],[Volume],[Stock],[Type],[OrderIndex],[LastModified],[SharesInfo],[SharesInfoNum])
                    select [Market],[SharesCode],[Time],[TimeStr],[Price],[Volume],[Stock],[Type],[OrderIndex],[LastModified],[SharesInfo],[SharesInfoNum]
                    from t_shares_transactiondata with(xlock)
                    where [Time]<convert(varchar(10),@currTime,120);";
                    db.Database.ExecuteSqlCommand(sql);

                    sql = "truncate table t_shares_transactiondata";
                    db.Database.ExecuteSqlCommand(sql);
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("执行出错", ex);
                }
            }
        }
    }
}
