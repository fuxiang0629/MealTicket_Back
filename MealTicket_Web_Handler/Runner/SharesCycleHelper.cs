using System;
using MealTicket_DBCommon;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FXCommon.Common;
using Newtonsoft.Json;

namespace MealTicket_Web_Handler.Runner
{
    public class SharesCycleHelper
    {
        public static void Cal_SharesCycle() 
        {
            var shares_quotes_last_session = Singleton.Instance.sessionHandler.GetShares_Quotes_Last_Session(false,false);
            var plate_shares_real_session = Singleton.Instance.sessionHandler.GetPlate_Real_Shares_Session(60, false);
            using (var db = new meal_ticketEntities())
            {
                var sharesCycle = (from item in db.t_shares_cycle_manager
                                   select item).ToList();
                Dictionary<long, List<long>> cycleSharesList = new Dictionary<long, List<long>>();
                foreach (var item in sharesCycle)
                {
                    List<long> tempPlateList = JsonConvert.DeserializeObject<List<long>>(item.PlateId);
                    List<long> sharesList = new List<long>();
                    foreach (var plate in tempPlateList)
                    {
                        if (!plate_shares_real_session.ContainsKey(plate))
                        {
                            continue;
                        }
                        sharesList.AddRange(plate_shares_real_session[plate]);
                    }
                    sharesList = sharesList.Distinct().ToList();
                    var disList = new List<long>();
                    foreach (var share in sharesList)
                    {
                        if (!shares_quotes_last_session.ContainsKey(share))
                        {
                            continue;
                        }
                        var sharesInfo = shares_quotes_last_session[share];
                        if (sharesInfo.shares_quotes_info.PriceType != 1)
                        {
                            continue;
                        }
                        disList.Add(share);
                    }
                    if (disList.Count() <= 0)
                    {
                        continue;
                    }
                    if (!cycleSharesList.ContainsKey(item.Id))
                    {
                        cycleSharesList.Add(item.Id, new List<long>());
                    }
                    cycleSharesList[item.Id].AddRange(disList);
                }
                foreach (var item in cycleSharesList)
                {
                    var shares_cycle_stock = (from x in db.t_shares_cycle_manager_stock
                                              where x.CycleId == item.Key
                                              select x).ToList();
                    foreach (var share in item.Value)
                    {
                        var tempInfo = (from x in shares_cycle_stock
                                        where x.SharesKey == share
                                        select x).FirstOrDefault();
                        if (tempInfo == null)
                        {
                            db.t_shares_cycle_manager_stock.Add(new t_shares_cycle_manager_stock
                            {
                                SharesKey = share,
                                Status = 1,
                                CreateTime = DateTime.Now,
                                CycleId = item.Key,
                                Type = 2
                            });
                        }
                        else if (tempInfo.Status == 2)
                        {
                            tempInfo.Status = 1;
                        }
                    }
                    db.SaveChanges();
                }
            }
        }

        public static void ReCal_SharesCycle(long cycleId)
        {
            var shares_quote_date_session = Singleton.Instance.sessionHandler.GetShares_Quotes_Date_Session(60, false);
            var plate_shares_session = Singleton.Instance.sessionHandler.GetPlate_Real_Shares_Session(0,false);
            List<t_dim_time> dim_time_session = new List<t_dim_time>();
            List<t_shares_limit_date_group> shares_limit_date_group_session = new List<t_shares_limit_date_group>();
            List<t_shares_limit_date> shares_limit_date_session = new List<t_shares_limit_date>();
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    dim_time_session = (from item in db.t_dim_time
                                        select item).ToList();
                    shares_limit_date_group_session = (from item in db.t_shares_limit_date_group
                                                       select item).ToList();
                    shares_limit_date_session = (from item in db.t_shares_limit_date
                                                 select item).ToList();

                    var cycle = (from item in db.t_shares_cycle_manager
                                 where item.Id == cycleId
                                 select item).FirstOrDefault();
                    if (cycle == null)
                    {
                        return;
                    }
                    var cycleRel = (from item in db.t_shares_cycle_manager_stock
                                    where item.CycleId == cycleId && item.Type == 2
                                    select item).ToList();
                    db.t_shares_cycle_manager_stock.RemoveRange(cycleRel);
                    db.SaveChanges();

                    List<long> tempPlateList = JsonConvert.DeserializeObject<List<long>>(cycle.PlateId);
                    List<long> sharesList = new List<long>();
                    foreach (var plate in tempPlateList)
                    {
                        if (!plate_shares_session.ContainsKey(plate))
                        {
                            continue;
                        }
                        sharesList.AddRange(plate_shares_session[plate]);
                    }
                    sharesList = sharesList.Distinct().ToList();

                    List<long> sharesKeyList = new List<long>();
                    foreach (var share in sharesList)
                    {
                        if (!shares_quote_date_session.ContainsKey(share))
                        {
                            continue;
                        }
                        var quotesInfo = shares_quote_date_session[share];

                        bool isVaild = false;
                        DateTime endDate = cycle.EndDate == null || cycle.EndDate > DateTime.Now.Date ? DateTime.Now.Date : cycle.EndDate.Value;
                        for (var minDate = cycle.StartDate; minDate <= endDate; minDate = minDate.AddDays(1))
                        {
                            if (!DbHelper.CheckTradeDate(dim_time_session, shares_limit_date_group_session, shares_limit_date_session, minDate))
                            {
                                continue;
                            }
                            if (!quotesInfo.ContainsKey(minDate))
                            {
                                continue;
                            }
                            if (quotesInfo[minDate].PriceType == 1)
                            {
                                isVaild = true;
                                break;
                            }
                        }
                        if (isVaild)
                        {
                            sharesKeyList.Add(share);
                        }
                    }

                    var shares_cycle_stock = (from x in db.t_shares_cycle_manager_stock
                                              where x.CycleId == cycleId && x.Type == 1
                                              select x).ToList();

                    foreach (var share in sharesKeyList)
                    {
                        var tempInfo = (from x in shares_cycle_stock
                                        where x.SharesKey == share
                                        select x).FirstOrDefault();
                        if (tempInfo == null)
                        {
                            db.t_shares_cycle_manager_stock.Add(new t_shares_cycle_manager_stock
                            {
                                SharesKey = share,
                                Status = 1,
                                CreateTime = DateTime.Now,
                                CycleId = cycleId,
                                Type = 2
                            });
                        }
                        else if (tempInfo.Status == 2)
                        {
                            tempInfo.Status = 1;
                        }
                    }
                    db.SaveChanges();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }
    }
}
