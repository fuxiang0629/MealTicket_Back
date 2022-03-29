using MealTicket_DBCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Runner
{
    public class HotSpotHisHelper
    {
        public static void Cal_HotSpotHis() 
        {
            cal_HotSpotHis_auto();
            cal_HotSpotHis_custom();
        }

        private static void cal_HotSpotHis_auto()
        {
            DateTime Date = DbHelper.GetLastTradeDate2(0,0,0,0,DateTime.Now.AddDays(-1).Date);
            TrendHandler handler = new TrendHandler();
            List<int> daysTypeList = new List<int> { 0, 1, 2, 3, 4 };

            List<t_shares_hotspot_auto> hotpostautoList = new List<t_shares_hotspot_auto>();
            foreach (var dayType in daysTypeList)
            {
                var content = handler.GetSharesAllLimitUpList(new Model.GetSharesHotSpotListRequest
                {
                    DaysType = dayType
                }, null);
                hotpostautoList.Add(new t_shares_hotspot_auto
                {
                    Date = Date,
                    DaysType = dayType,
                    ContentJson = JsonConvert.SerializeObject(content)
                });
            }
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var temp = (from item in db.t_shares_hotspot_auto
                                where item.Date == Date
                                select item).ToList();
                    if (temp != null)
                    {
                        db.t_shares_hotspot_auto.RemoveRange(temp);
                        db.SaveChanges();
                    }

                    db.t_shares_hotspot_auto.AddRange(hotpostautoList);
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

        private static void cal_HotSpotHis_custom()
        {
            DateTime Date = DbHelper.GetLastTradeDate2(0, 0, 0, 0, DateTime.Now.AddDays(-1).Date);
            TrendHandler handler = new TrendHandler();
            List<int> daysTypeList = new List<int> { 0, 1, 2, 3, 4 };
            using (var db = new meal_ticketEntities())
            {
                var account_hotsopt = (from item in db.t_shares_hotspot
                                       join item2 in db.t_shares_hotspot_group_rel on item.Id equals item2.HostId
                                       join item3 in db.t_shares_hotspot_group on item2.GroupId equals item3.Id
                                       where item.DataType == 0
                                       select new
                                       {
                                           AccountId = item.AccountId,
                                           GroupId = item3.Id,
                                           GroupName = item3.GroupName
                                       }).ToList();

                List<t_shares_hotspot_his> hotpostList = new List<t_shares_hotspot_his>();
                foreach (var dayType in daysTypeList)
                {
                    foreach (var item in account_hotsopt)
                    {
                        var content = handler.GetSharesHotSpotList(new Model.GetSharesHotSpotListRequest
                        {
                            DaysType=dayType,
                            GroupId=item.GroupId
                        }, new Model.HeadBase
                        {
                            AccountId = item.AccountId
                        });
                        hotpostList.Add(new t_shares_hotspot_his
                        {
                            Date = Date,
                            DaysType = dayType,
                            AccountId= item.AccountId,
                            GroupId= item.GroupId,
                            GroupName=item.GroupName,
                            ContentJson = JsonConvert.SerializeObject(content)
                        });
                    }
                }
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var temp = (from item in db.t_shares_hotspot_his
                                    where item.Date == Date
                                    select item).ToList();
                        if (temp != null)
                        {
                            db.t_shares_hotspot_his.RemoveRange(temp);
                            db.SaveChanges();
                        }

                        db.t_shares_hotspot_his.AddRange(hotpostList);
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
}
