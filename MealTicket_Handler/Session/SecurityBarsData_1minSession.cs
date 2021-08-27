using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Handler.SecurityBarsData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class SecurityBarsData_1minSession : Session<Dictionary<string, List<SecurityBarsDataInfo>>>
    {
        public SecurityBarsData_1minSession()
        {
            Name = "SecurityBarsData_1minSession";
        }
        public override Dictionary<string, List<SecurityBarsDataInfo>> UpdateSession()
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                db.Database.CommandTimeout = 600;
                var result = (from item in db.t_shares_securitybarsdata_1min
                              where item.Date == dateNow
                              group item by new { item.Market, item.SharesCode } into g
                              select g).ToDictionary(e => e.Key.SharesCode + e.Key.Market, v => (v.Select(x => new SecurityBarsDataInfo
                              {
                                  SharesCode = x.SharesCode,
                                  TimeStr = x.TimeStr,
                                  TradeStock = x.TradeStock,
                                  ClosedPrice = x.ClosedPrice,
                                  Date = x.Date,
                                  Market = x.Market,
                                  MaxPrice = x.MaxPrice,
                                  MinPrice = x.MinPrice,
                                  OpenedPrice = x.OpenedPrice,
                                  PreClosePrice = x.PreClosePrice,
                                  Time = x.Time,
                                  TradeAmount = x.TradeAmount
                              })).ToList());
                return result;
            }
        }

        public bool LoadMoreSession(List<SecurityBarsDataInfo> list)
        {
            _readWriteLock.AcquireWriterLock(-1);
            bool isSuccess = _LoadMoreSession(list);
            _readWriteLock.ReleaseWriterLock();
            return isSuccess;
        }
        private bool _LoadMoreSession(List<SecurityBarsDataInfo> list)
        {
            try
            {
                var tempData = (from item in list
                                group item by new { item.Market, item.SharesCode } into g
                                select g).ToList();
                foreach (var item in tempData)
                {
                    List<SecurityBarsDataInfo> tempList = new List<SecurityBarsDataInfo>();
                    if (!SessionData.TryGetValue(item.Key.SharesCode + item.Key.Market, out tempList))
                    {
                        SessionData.Add(item.Key.SharesCode + item.Key.Market, item.ToList());
                    }
                    else
                    {
                        if (tempList == null)
                        {
                            tempList = new List<SecurityBarsDataInfo>();
                        }
                        if (tempList.Count() > 0)
                        {
                            tempList.RemoveAt(tempList.Count() - 1);
                        }
                        tempList.AddRange(item.ToList());
                        SessionData[item.Key.SharesCode + item.Key.Market] = tempList;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("SecurityBarsData_1minSession缓存Load失败", ex);
                return false;
            }
        }
    }
}
