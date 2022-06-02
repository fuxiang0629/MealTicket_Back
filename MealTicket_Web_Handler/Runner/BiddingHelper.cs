using FXCommon.Common;
using MealTicket_DBCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Runner
{
    public class BiddingInfo
    {
        public long Id { get; set; }

        public DateTime Date { get; set; }

        public int RiseRate { get; set; }

        public long TotalCount { get; set; }

        public long TotalAmount { get; set; }
    }

    public class BiddingHelper
    {
        public static void Cal_Bidding()
        {
            DateTime dateNow = DateTime.Now.Date;
            long dateKey = long.Parse(dateNow.ToString("yyyyMMdd"));
            var shares_bidding_dic = GetSharesBiddingToday(dateKey);

            var plate_real_shares_session = Singleton.Instance.sessionHandler.GetPlate_Real_Shares_Session(0, false);
            var shares_limit_session = Singleton.Instance.sessionHandler.GetShares_Limit_Session(false);
            var shares_quotes_date_session = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(dateNow,false);

            var BiddingList_Plate=Cal_Bidding_Plate(dateNow, shares_bidding_dic, plate_real_shares_session, shares_limit_session, shares_quotes_date_session);
            if (BiddingList_Plate.Count() > 0)
            {
                WriteToDatabase_Plate(BiddingList_Plate);
            }
            var BiddingList_Hotspot = Cal_Bidding_Hotspot(dateNow, shares_bidding_dic, plate_real_shares_session, shares_limit_session, shares_quotes_date_session);
            if (BiddingList_Hotspot.Count() > 0)
            {
                WriteToDatabase_Hotspot(BiddingList_Hotspot);
            }
        }

        public static List<BiddingInfo> Cal_Bidding_Plate(DateTime date, Dictionary<long, t_shares_quotes_date_bidding> shares_bidding_dic,  Dictionary<long, List<long>> plate_real_shares_session,List<long> shares_limit_session,Dictionary<long,Shares_Quotes_Session_Info> shares_quotes_date_session)
        {
            List<BiddingInfo> BiddingList = new List<BiddingInfo>();
            foreach (var item in plate_real_shares_session)
            {
                var tempInfo = _cal_Bidding_Plate(item.Key,item.Value, date, shares_bidding_dic, shares_limit_session, shares_quotes_date_session);
                BiddingList.Add(tempInfo);
            }
            return BiddingList;
        }

        public static BiddingInfo _cal_Bidding_Plate(long plateId,List<long> sharesKeyList, DateTime date, Dictionary<long, t_shares_quotes_date_bidding> shares_bidding_dic,List<long> shares_limit_session, Dictionary<long, Shares_Quotes_Session_Info> shares_quotes_date_session)
        {
            long totalCount = 0;
            long totalAmount = 0;
            int riseRate = 0;
            int count = 0;
            foreach (var sharesKey in sharesKeyList)
            {
                if (shares_limit_session.Contains(sharesKey))
                {
                    continue;
                }
                if (!shares_bidding_dic.ContainsKey(sharesKey))
                {
                    continue;
                }
                if (shares_quotes_date_session.ContainsKey(sharesKey))
                {
                    riseRate = riseRate+ shares_quotes_date_session[sharesKey].RiseRate;
                }
                var biddingInfo = shares_bidding_dic[sharesKey];
                totalCount = totalCount + biddingInfo.TotalCount;
                totalAmount = totalAmount + biddingInfo.TotalAmount;
                count++;
            }

            return new BiddingInfo
            {
                Id = plateId,
                Date = date,
                RiseRate = riseRate == 0 ? 0 : riseRate / count,
                TotalCount = totalCount,
                TotalAmount =totalAmount,
            };
        }

        public static void WriteToDatabase_Plate(List<BiddingInfo> BiddingList)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(long));
            table.Columns.Add("Date", typeof(string));
            table.Columns.Add("RiseRate", typeof(long));
            table.Columns.Add("TotalAmount", typeof(long));
            table.Columns.Add("TotalCount", typeof(long));

            foreach (var item in BiddingList)
            {
                DataRow row = table.NewRow();
                row["Id"] = item.Id;
                row["Date"] = item.Date.ToString("yyyy-MM-dd");
                row["RiseRate"] = item.RiseRate;
                row["TotalAmount"] = item.TotalAmount;
                row["TotalCount"] = item.TotalCount;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@biddingInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.BiddingInfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Bidding_Plate_Update @biddingInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("板块竞价入库失败", ex);
                    tran.Rollback();
                }
            }
        }

        public static List<BiddingInfo> Cal_Bidding_Hotspot(DateTime date, Dictionary<long, t_shares_quotes_date_bidding> shares_bidding_dic, Dictionary<long, List<long>> plate_real_shares_session, List<long> shares_limit_session, Dictionary<long, Shares_Quotes_Session_Info> shares_quotes_date_session)
        {
            var hotspot = Singleton.Instance.sessionHandler.GetShares_Hotspot_Session(false);
            List<BiddingInfo> BiddingList = new List<BiddingInfo>();
            foreach (var item in hotspot)
            {
                var hotspotInfo = new ContextObj
                {
                    ContextId=item.Value.Id,
                    ContextType=1,
                    DataType=item.Value.DataType,
                    PlateIdList=item.Value.PlateIdList
                };
                var tempInfo = _cal_Bidding_Hotspot(hotspotInfo, date, shares_bidding_dic, plate_real_shares_session, shares_limit_session, shares_quotes_date_session);
                BiddingList.Add(tempInfo);
            }
            return BiddingList;
        }

        public static BiddingInfo _cal_Bidding_Hotspot(ContextObj hotspotInfo,DateTime date, Dictionary<long, t_shares_quotes_date_bidding> shares_bidding_dic, Dictionary<long, List<long>> plate_real_shares_session, List<long> shares_limit_session, Dictionary<long, Shares_Quotes_Session_Info> shares_quotes_date_session)
        {
            List<long> sharesKeyList = new List<long>();
            if (hotspotInfo.DataType == 0)
            {
                foreach (long plateId in hotspotInfo.PlateIdList)
                {
                    if (plate_real_shares_session.ContainsKey(plateId))
                    {
                        sharesKeyList.AddRange(plate_real_shares_session[plateId]);
                    }
                }
            }
            else if (hotspotInfo.DataType == 1)
            {
                foreach (var x in plate_real_shares_session)
                {
                    sharesKeyList.AddRange(x.Value);
                }
            }
            sharesKeyList = sharesKeyList.Distinct().ToList();

            long totalCount = 0;
            long totalAmount = 0;
            int riseRate = 0;
            int count = 0;
            foreach (var sharesKey in sharesKeyList)
            {
                if (shares_limit_session.Contains(sharesKey))
                {
                    continue;
                }
                if (!shares_bidding_dic.ContainsKey(sharesKey))
                {
                    continue;
                }
                if (shares_quotes_date_session.ContainsKey(sharesKey))
                {
                    riseRate = riseRate + shares_quotes_date_session[sharesKey].RiseRate;
                }
                var biddingInfo = shares_bidding_dic[sharesKey];
                totalCount = totalCount + biddingInfo.TotalCount;
                totalAmount = totalAmount + biddingInfo.TotalAmount;
                count++;
            }

            return new BiddingInfo
            {
                Id = hotspotInfo.ContextId,
                Date = date,
                RiseRate = riseRate == 0 ? 0 : riseRate / count,
                TotalCount = totalCount,
                TotalAmount = totalAmount,
            };
        }

        public static void WriteToDatabase_Hotspot(List<BiddingInfo> BiddingList)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(long));
            table.Columns.Add("Date", typeof(string));
            table.Columns.Add("RiseRate", typeof(long));
            table.Columns.Add("TotalAmount", typeof(long));
            table.Columns.Add("TotalCount", typeof(long));

            foreach (var item in BiddingList)
            {
                DataRow row = table.NewRow();
                row["Id"] = item.Id;
                row["Date"] = item.Date.ToString("yyyy-MM-dd");
                row["RiseRate"] = item.RiseRate;
                row["TotalAmount"] = item.TotalAmount;
                row["TotalCount"] = item.TotalCount;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@biddingInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.BiddingInfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Bidding_Hotspot_Update @biddingInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("题材竞价入库失败", ex);
                    tran.Rollback();
                }
            }
        }

        public static Dictionary<long, t_shares_quotes_date_bidding> GetSharesBiddingToday(long DateKey)
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_quotes_date_bidding
                              where item.DateKey == DateKey
                              select item).ToList().GroupBy(e=>e.SharesKey).ToDictionary(k => k.Key.Value, v => v.FirstOrDefault());
                return result;
            }
        }
    }
}
