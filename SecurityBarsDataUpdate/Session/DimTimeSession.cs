using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityBarsDataUpdate
{
    public class DimTimeSession : Session<List<t_dim_time>>
    {
        public DimTimeSession()
        {
            Name = "DimTimeSession";
        }
        public override List<t_dim_time> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"declare @currTime datetime;
set @currTime=getdate();
select * from t_dim_time with(nolock)
where the_year<=datepart(YEAR,@currTime) and the_year>=datepart(YEAR,@currTime)-1;";
                var result = db.Database.SqlQuery<t_dim_time>(sql).ToList();
                return result;
            }
        }
    }
}
