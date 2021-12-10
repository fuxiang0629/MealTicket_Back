using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityBarsDataUpdate
{
    /// <summary>
    /// 股票基础数据缓存
    /// </summary>
    public class SharesBaseSession:Session<List<SharesBaseInfo_Session>>
    {
        public SharesBaseSession()
        {
            Name = "SharesBaseSession";
        }
        public override List<SharesBaseInfo_Session> UpdateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select Market,SharesCode,SharesName from v_shares_baseinfo with(nolock)";
                var result = db.Database.SqlQuery<SharesBaseInfo_Session>(sql).ToList();
                return result;
            }
        }
    }
}
