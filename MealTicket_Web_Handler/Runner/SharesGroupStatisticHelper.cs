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
    public class ContextObj
    {
        public long ContextId { get; set; }

        public int DataType { get; set; }

        public int ContextType { get; set; }

        public List<long> PlateIdList { get; set; }
    }

    public class SharesGroupStatisticHelper
    {
        public static void Cal_SharesGroupStatistic(DateTime date)
        {
            var plate_shares_real_session = Singleton.Instance.sessionHandler.GetPlate_Real_Shares_Session(0,false);
            var shares_limit_session = Singleton.Instance.sessionHandler.GetShares_Limit_Session(false);

            //查询指定日股票行情
            List<Shares_Quotes_Session_Info> sharesStatistic = getSharesStatistic(date, shares_limit_session);

            //获取题材
            List<ContextObj> hotPlate = getContext();

            List<t_shares_group_statistic> resultList = new List<t_shares_group_statistic>();
            foreach (var item in hotPlate)
            {
                var temp = _cal_SharesGroupStatistic(item, date, plate_shares_real_session, sharesStatistic);
                resultList.Add(temp);
            }

            if (resultList.Count() > 0)
            {
                writeToDatabase(resultList);
            }
        }

        public static t_shares_group_statistic _cal_SharesGroupStatistic(ContextObj contextInfo, DateTime date,Dictionary<long,List<long>> plate_shares_real_session, List<Shares_Quotes_Session_Info> sharesStatistic) 
        {
            t_shares_group_statistic temp = new t_shares_group_statistic();
            temp.ContextId = contextInfo.ContextId;
            temp.ContextType = 1;
            temp.CreateTime = DateTime.Now;
            temp.Date = date;
            temp.LastModified = DateTime.Now;
            List<long> sharesKeyList = new List<long>();
            if (contextInfo.DataType == 0)
            {
                sharesKeyList = getShares(contextInfo.PlateIdList, plate_shares_real_session);
            }
            else if (contextInfo.DataType == 1)
            {
                sharesKeyList = getAllShares(plate_shares_real_session);
            }
            calStatistic(sharesKeyList, sharesStatistic, ref temp);
            return temp;
        }

        public static List<Shares_Quotes_Session_Info> getSharesStatistic(DateTime date, List<long> shares_limit_session) 
        {
            var session = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(date, false);
            List<Shares_Quotes_Session_Info> result = new List<Shares_Quotes_Session_Info>();
            foreach (var item in session)
            {
                if (shares_limit_session.Contains(item.Key))
                {
                    continue;
                }
                result.Add(new Shares_Quotes_Session_Info
                {
                    SharesCode = item.Value.SharesCode,
                    Market = item.Value.Market,
                    ClosedPrice = item.Value.ClosedPrice,
                    Date = item.Value.Date,
                    LimitDownBombCount = item.Value.LimitDownBombCount,
                    LimitDownPrice = item.Value.LimitDownPrice,
                    LimitUpBombCount = item.Value.LimitUpBombCount,
                    IsSt = item.Value.IsSt,
                    LimitUpPrice = item.Value.LimitUpPrice,
                    MaxPrice = item.Value.MaxPrice,
                    MinPrice = item.Value.MinPrice,
                    OpenedPrice = item.Value.OpenedPrice,
                    YestodayClosedPrice = item.Value.YestodayClosedPrice,
                    PriceType = item.Value.PriceType,
                    PriceTypeYestoday = item.Value.PriceTypeYestoday,
                    TotalAmount=item.Value.TotalAmount,
                    TotalCount=item.Value.TotalCount
                });
            }
            return result;
        }

        private static List<ContextObj> getContext() 
        {
            var hotspotSession=Singleton.Instance.sessionHandler.GetShares_Hotspot_Session(false);

            List<ContextObj> result = new List<ContextObj>();
            foreach(var item in hotspotSession)
            {
                result.Add(new ContextObj 
                {
                    ContextId=item.Key,
                    ContextType=1,
                    DataType=item.Value.DataType,
                    PlateIdList=item.Value.PlateIdList
                });
            }
            return result;
        }

        private static List<long> getShares(List<long> plateIdList,Dictionary<long, List<long>> plate_shares_real_session) 
        {
            List<long> result = new List<long>();
            foreach (long plateId in plateIdList)
            {
                if (!plate_shares_real_session.ContainsKey(plateId))
                {
                    continue;
                }
                result.AddRange(plate_shares_real_session[plateId]);
            }
            result = result.Distinct().ToList();
            return result;
        }

        private static List<long> getAllShares(Dictionary<long, List<long>> plate_shares_real_session)
        {
            List<long> result = new List<long>();
            foreach (var item in plate_shares_real_session)
            {
                result.AddRange(item.Value);
            }
            result = result.Distinct().ToList();
            return result;
        }

        private static void calStatistic(List<long> sharesKeyList,List<Shares_Quotes_Session_Info> sourceList,ref t_shares_group_statistic disData)
        {
            var result = (from item in sourceList
                          where sharesKeyList.Contains(item.SharesKey)
                          select item).ToList();
            int DownCount = 0;//
            int DownCountYestoday = 0;//
            int FlatCount = 0;//
            int FlatCountYestoday = 0;//
            int LimitDownBombCount = 0;//
            int LimitDownBombCountYestoday = 0;//
            int LimitDownCount = 0;//
            int LimitDownCountYestoday = 0;//
            int LimitUpBombCount = 0;//
            int LimitUpBombCountYestoday = 0;//
            int LimitUpCount = 0;//
            int LimitUpCountYestoday = 0;//
            int RiseRateYestoday = 0;
            int RiseRateCountYestoday = 0;
            int UpCount = 0;//
            int UpCountYestoday = 0;//
            int riseRate = 0;
            long totalCount = 0;
            long totalAmount = 0;
            long totalCountYestoday = 0;
            long totalAmountYestoday = 0;
            List<long> sharesKeyListYes = new List<long>();
            foreach (var item in result)
            {
                long PresentPrice = item.ClosedPrice;
                long ClosedPrice = item.YestodayClosedPrice;
                int PriceType = item.PriceType;
                int PriceTypeYestoday = item.PriceTypeYestoday;
                int LimitUpBombCount_source = item.LimitUpBombCount;
                int LimitDownBombCount_source = item.LimitDownBombCount;
                if (PresentPrice == 0 || ClosedPrice == 0)
                {
                    continue;
                }
                int tempriseRate = (int)Math.Round((PresentPrice - ClosedPrice) * 1.0 / ClosedPrice * 10000, 0);
                riseRate = tempriseRate;
                totalCount = totalCount + item.TotalCount;
                totalAmount = totalAmount + item.TotalAmount;
                if (PresentPrice > ClosedPrice)
                {
                    UpCount++;
                    if (PriceTypeYestoday == 1)
                    {
                        UpCountYestoday++;
                    }
                }
                if (PresentPrice < ClosedPrice)
                {
                    DownCount++;
                    if (PriceTypeYestoday == 1)
                    {
                        DownCountYestoday++;
                    }
                }
                if (PresentPrice == ClosedPrice)
                {
                    FlatCount++;
                    if (PriceTypeYestoday == 1)
                    {
                        FlatCountYestoday++;
                    }
                }
                if (PriceType == 1)
                {
                    LimitUpCount++;
                    if (PriceTypeYestoday == 1)
                    {
                        LimitUpCountYestoday++;
                    }
                }
                if (PriceType == 2)
                {
                    LimitDownCount++;
                    if (PriceTypeYestoday == 1)
                    {
                        LimitDownCountYestoday++;
                    }
                }
                if (LimitUpBombCount_source > 0 && PriceType != 1)
                {
                    LimitUpBombCount++;
                    if (PriceTypeYestoday == 1)
                    {
                        LimitUpBombCountYestoday++;
                    }
                }
                if (LimitDownBombCount_source > 0 && PriceType != 2)
                {
                    LimitDownBombCount++;
                    if (PriceTypeYestoday == 1)
                    {
                        LimitDownBombCountYestoday++;
                    }
                }
                if (PriceTypeYestoday == 1)
                {
                    RiseRateYestoday += tempriseRate;
                    RiseRateCountYestoday++;
                    totalAmountYestoday = totalAmountYestoday + item.TotalAmount;
                    totalCountYestoday = totalCountYestoday + item.TotalCount;
                    sharesKeyListYes.Add(item.SharesKey);
                }
            }
            disData.DownCount = DownCount;
            disData.DownCountYestoday = DownCountYestoday;
            disData.FlatCount = FlatCount;
            disData.FlatCountYestoday = FlatCountYestoday;
            disData.LimitDownBombCount = LimitDownBombCount;
            disData.LimitDownBombCountYestoday = LimitDownBombCountYestoday;
            disData.LimitDownCount = LimitDownCount;
            disData.LimitDownCountYestoday = LimitDownCountYestoday;
            disData.LimitUpBombCount = LimitUpBombCount;
            disData.LimitUpBombCountYestoday = LimitUpBombCountYestoday;
            disData.LimitUpCount = LimitUpCount;
            disData.LimitUpCountYestoday = LimitUpCountYestoday;
            disData.RiseRateYestoday = RiseRateCountYestoday == 0 ? 0 : (int)Math.Round(RiseRateYestoday * 1.0 / RiseRateCountYestoday, 0);
            disData.UpCount = UpCount;
            disData.UpCountYestoday = UpCountYestoday;
            disData.RiseRate = riseRate;
            disData.TotalAmount = totalAmount;
            disData.TotalCount = totalCount;
            disData.TotalAmountYestoday = totalAmountYestoday;
            disData.TotalCountYestoday = totalCountYestoday;
            disData.SharesKeyYestodayList = JsonConvert.SerializeObject(sharesKeyListYes);
        }

        public static void writeToDatabase(List<t_shares_group_statistic> datalist)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("ContextId", typeof(long));
            table.Columns.Add("ContextType", typeof(int));
            table.Columns.Add("LimitUpCount", typeof(int));
            table.Columns.Add("LimitDownCount", typeof(int));
            table.Columns.Add("LimitUpBombCount", typeof(int));
            table.Columns.Add("LimitDownBombCount", typeof(int));
            table.Columns.Add("UpCount", typeof(int));
            table.Columns.Add("DownCount", typeof(int));
            table.Columns.Add("FlatCount", typeof(int));
            table.Columns.Add("RiseRate", typeof(int));
            table.Columns.Add("TotalCount", typeof(long));
            table.Columns.Add("TotalAmount", typeof(long));
            table.Columns.Add("TotalCountYestoday", typeof(long));
            table.Columns.Add("TotalAmountYestoday", typeof(long));
            table.Columns.Add("LimitUpCountYestoday", typeof(int));
            table.Columns.Add("LimitDownCountYestoday", typeof(int));
            table.Columns.Add("LimitUpBombCountYestoday", typeof(int));
            table.Columns.Add("LimitDownBombCountYestoday", typeof(int));
            table.Columns.Add("UpCountYestoday", typeof(int));
            table.Columns.Add("DownCountYestoday", typeof(int));
            table.Columns.Add("FlatCountYestoday", typeof(int));
            table.Columns.Add("RiseRateYestoday", typeof(int));
            table.Columns.Add("SharesKeyYestodayList", typeof(string));
            table.Columns.Add("CreateTime", typeof(DateTime));
            table.Columns.Add("LastModified", typeof(DateTime));

            foreach (var item in datalist)
            {
                DataRow row = table.NewRow();
                row["Date"] = item.Date;
                row["ContextId"] = item.ContextId;
                row["ContextType"] = item.ContextType;
                row["LimitUpCount"] = item.LimitUpCount;
                row["LimitDownCount"] = item.LimitDownCount;
                row["LimitUpBombCount"] = item.LimitUpBombCount;
                row["LimitDownBombCount"] = item.LimitDownBombCount;
                row["UpCount"] = item.UpCount;
                row["DownCount"] = item.DownCount;
                row["FlatCount"] = item.FlatCount;
                row["RiseRate"] = item.RiseRate;
                row["TotalCount"] = item.TotalCount*100;
                row["TotalAmount"] = item.TotalAmount;
                row["TotalCountYestoday"] = item.TotalCountYestoday * 100;
                row["TotalAmountYestoday"] = item.TotalAmountYestoday;
                row["LimitUpCountYestoday"] = item.LimitUpCountYestoday;
                row["LimitDownCountYestoday"] = item.LimitDownCountYestoday;
                row["LimitUpBombCountYestoday"] = item.LimitUpBombCountYestoday;
                row["LimitDownBombCountYestoday"] = item.LimitDownBombCountYestoday;
                row["UpCountYestoday"] = item.UpCountYestoday;
                row["DownCountYestoday"] = item.DownCountYestoday;
                row["FlatCountYestoday"] = item.FlatCountYestoday;
                row["RiseRateYestoday"] = item.RiseRateYestoday;
                row["SharesKeyYestodayList"] = item.SharesKeyYestodayList;
                row["CreateTime"] = item.CreateTime;
                row["LastModified"] = item.LastModified;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesGroupStatisticInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesGroupStatisticInfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Calculate_SharesGroupStatistic @sharesGroupStatisticInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("股票分组统计入库失败", ex);
                    tran.Rollback();
                }
            }
        }
    }
}
