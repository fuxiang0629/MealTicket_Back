using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityBarsDataUpdate
{
    public class SharesQuotesSessionData 
    {
        public int Date { get; set; }

        public Dictionary<int, t_shares_quotes_date> QuotesDic { get; set; }
    }

    public class SharesQuotesSession : Session<SharesQuotesSessionData>
    {
        public SharesQuotesSession()
        {
            Name = "SharesQuotesSession";
        }
        public override SharesQuotesSessionData UpdateSession()
        {
            var tradeDate = DbHelper.GetLastTradeDate2(-9);//获取当前交易日日期
            using (var db = new meal_ticketEntities())
            {
                string tradeDateStr = tradeDate.ToString("yyyy-MM-dd");
                var result = (from item in db.t_shares_quotes_date
                              where item.Date == tradeDateStr
                              select item).ToList().ToDictionary(k => int.Parse(k.SharesCode) * 10 + k.Market, v => v);
            
                return new SharesQuotesSessionData
                {
                    Date = int.Parse(tradeDate.ToString("yyyyMMdd")),
                    QuotesDic= result
                };
            }
        }
    }
}
