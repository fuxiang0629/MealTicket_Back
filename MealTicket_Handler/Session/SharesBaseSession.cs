using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
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
                db.Database.CommandTimeout = 600;
                string sql = @"select Market,SharesCode,SharesName,SharesPyjc,SharesHandCount,Business,isnull(CirculatingCapital,0) CirculatingCapital,isnull(TotalCapital,0) TotalCapital,Industry,Area,Idea,ClosedPrice,MarketStatus,MarketTime from v_shares_baseinfo with(nolock)";
                var result = db.Database.SqlQuery<SharesBaseInfo_Session>(sql).ToList();
                return result;
            }
        }
    }
}
