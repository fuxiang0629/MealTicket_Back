using FXCommon.Common;
using MealTicket_DBCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Quotes_Today_Session
    {
        public static Shares_Quotes_Today_Session_Obj UpdateSession()
        {
            Shares_Quotes_Today_Session_Obj resultDic = new Shares_Quotes_Today_Session_Obj();
            resultDic.Shares_Quotes_Today = new Dictionary<long, Shares_Quotes_Session_Info>();
            resultDic.Shares_Quotes_Total = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
            using (var db = new meal_ticketEntities())
            {
                string sql = "exec P_GetShares_Quotes_Today_Session";
                var result = db.Database.SqlQuery<Shares_Quotes_Session_Info>(sql).ToList();
                foreach (var item in result)
                {
                    if (resultDic.Shares_Quotes_Today.ContainsKey(item.SharesKey))
                    {
                        continue;
                    }
                    resultDic.Shares_Quotes_Today.Add(item.SharesKey, item);
                }
            }
            var quotes_today = resultDic.Shares_Quotes_Today;
            var quotes_date =Singleton.Instance.sessionHandler.GetShares_Quotes_Date_Session(0, true);
            var shares_base = Singleton.Instance.sessionHandler.GetShares_Base_Session(false);
            foreach (var item in quotes_date)
            {
                long Tradable = 0;
                if (shares_base.ContainsKey(item.Key))
                {
                    Tradable = shares_base[item.Key].CirculatingCapital;
                }
                Dictionary<DateTime, Shares_Quotes_Session_Info> temp = new Dictionary<DateTime, Shares_Quotes_Session_Info>();
                DateTime? maxDate = null;
                if (quotes_today.ContainsKey(item.Key))
                {
                    var todayQuote = quotes_today[item.Key];
                    todayQuote.HandsRate = Tradable == 0 ? 0 : (int)Math.Round(todayQuote.TotalCount * 100 * 1.0 / Tradable * 10000, 0);
                    todayQuote.OrderIndex = 0;
                    temp[DateTime.Now.Date] = todayQuote;
                    maxDate = DateTime.Now.Date;
                }
                int idx = 0;
                foreach (var quote in item.Value)
                {
                    idx++;
                    if (idx == 1 && maxDate == null)
                    {
                        maxDate = quote.Key;
                    }
                    quote.Value.HandsRate = Tradable == 0 ? 0 : (int)Math.Round(quote.Value.TotalCount * 100 * 1.0 / Tradable * 10000, 0);
                    quote.Value.OrderIndex = idx;
                    temp[quote.Key] = quote.Value;
                }

                var sortTemp = new SortedDictionary<DateTime, Shares_Quotes_Session_Info>(temp);
                long lastTotalCount = -1;
                long lastTotalAmount = -1;
                foreach (var sortquote in sortTemp)
                {
                    if (lastTotalCount != -1)
                    {
                        sortquote.Value.BiddingTotalCountRate = (int)Math.Round(sortquote.Value.BiddingTotalCount * 1.0 / lastTotalCount * 100, 0);
                    }
                    lastTotalCount = sortquote.Value.BiddingTotalCount;
                    if (lastTotalAmount != -1)
                    {
                        sortquote.Value.BiddingTotalAmountRate = (int)Math.Round(sortquote.Value.BiddingTotalAmount * 1.0 / lastTotalAmount * 10000, 0);
                    }
                    lastTotalAmount = sortquote.Value.TotalAmount;
                }
                resultDic.Shares_Quotes_Total.Add(item.Key, temp);
            }

            return resultDic;
        }

        public static Dictionary<long, Shares_Quotes_Session_Info> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Shares_Quotes_Session_Info>;
            var resultData = new Dictionary<long, Shares_Quotes_Session_Info>(data);
            return resultData;
        }
    }
}
