using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FXCommon.Common.Session_New;

namespace MealTicket_Web_Handler
{
    public class Shares_Energy_Table_Session
    {
        public static Dictionary<long, Shares_Energy_Table_Session_Info> UpdateSession()
        {
            GET_DATA_CXT gdc = new GET_DATA_CXT(SessionHandler.GET_SHARES_ENERGY_TABLE_SESSION_INFO, null);
            var result = (Singleton.Instance.sessionHandler.GetDataWithLock("", gdc)) as Dictionary<long, Shares_Energy_Table_Session_Info>;
            return result;
        }

        public static Dictionary<long, Shares_Energy_Table_Session_Info> Cal_Shares_Energy_Table_bak() 
        {
            Dictionary<long, Shares_Energy_Table_Session_Info> result = new Dictionary<long, Shares_Energy_Table_Session_Info>();
            var oldData = Singleton.Instance.sessionHandler.GetShares_Energy_Table_Session(false);

            List<long> leaderShares = Singleton.Instance.sessionHandler.GetLeaderShares_ByAllShares(false);
            var shares_quotes_last = Singleton.Instance.sessionHandler.GetShares_Quotes_Last_Session(true, false);

            var limit_fundmultiple=Singleton.Instance.sessionHandler.GetShares_Limit_Fundmultiple_Session();

            List<long> oldIdList = oldData.Keys.ToList();
            List<long> totalIdList = new List<long>();
            totalIdList = leaderShares.Union(oldIdList).ToList();

            DateTime timeNow = DateTime.Now;
            foreach (var share in totalIdList)
            {
                if (!limit_fundmultiple.ContainsKey(share))
                {
                    continue;
                }
                int minRiseRate = limit_fundmultiple[share].NearLimitRange;
                if (!shares_quotes_last.ContainsKey(share))
                {
                    continue;
                }
                var shares_last = shares_quotes_last[share];
                var lastQuotes = shares_last.shares_quotes_info;
                if (lastQuotes == null)
                {
                    continue;
                }
                bool isVaild = false;
                if (lastQuotes.RiseRate >= minRiseRate)
                {
                    isVaild = true;
                }
                Shares_Energy_Table_Session_Info tempItem = new Shares_Energy_Table_Session_Info();
                tempItem.SharesKey = share;
                tempItem.RiseRate = lastQuotes.RiseRate;
                tempItem.ClosedPrice = lastQuotes.YestodayClosedPrice;
                tempItem.RiseAmount = lastQuotes.ClosedPrice - lastQuotes.YestodayClosedPrice;
                tempItem.IsLimitUpBomb = lastQuotes.IsLimitUpBomb;
                tempItem.RateExpect = shares_last.RateExpect;
                tempItem.RateNow = shares_last.RateNow;
                if (!leaderShares.Contains(share) || !isVaild)
                {
                    if (!oldData.ContainsKey(share))
                    {
                        continue;
                    }
                    tempItem.IsVaild = false;
                    tempItem.TriTime = oldData[share].TriTime;
                    tempItem.TriCount = oldData[share].TriCount;
                }
                else if (oldData.ContainsKey(share))
                {
                    var oldData_info = oldData[share];
                    if (oldData_info.IsVaild)
                    {
                        tempItem.TriTime = oldData_info.TriTime;
                        tempItem.TriCount = oldData_info.TriCount;
                    }
                    else
                    {
                        tempItem.TriTime = DateTime.Now;
                        tempItem.TriCount++;
                    }
                    tempItem.IsVaild = true;
                }
                else
                {
                    tempItem.IsVaild = true;
                    tempItem.TriTime = timeNow;
                    tempItem.TriCount = 1;
                }
                result.Add(share, tempItem);
            }
            return result;
        }

        public static Dictionary<long, Shares_Energy_Table_Session_Info> CopySessionData(object objData)
        {
            var data = objData as Dictionary<long, Shares_Energy_Table_Session_Info>;
            Dictionary<long, Shares_Energy_Table_Session_Info> result = new Dictionary<long, Shares_Energy_Table_Session_Info>(data);
            return result;
        }
    }
}
