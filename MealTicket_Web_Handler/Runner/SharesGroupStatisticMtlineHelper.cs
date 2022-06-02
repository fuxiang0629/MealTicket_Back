using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Runner
{
    public class MtlineInfo
    {
        public DateTime Date { get; set; }

        public long ContextId { get; set; }

        public int ContextType { get; set; }

        public long GroupTimeKey { get; set; }

        public DateTime Time { get; set; }

        public int RiseRate { get; set; }

        public long TradeStock { get; set; }

        public long TradeAmount { get; set; }

        public int LimitUpCount { get; set; }

        public int RiseUpCount { get; set; }
    }

    public class SharesGroupStatisticMtlineHelper
    {
        public static void Cal_SharesGroupStatisticMtline(DateTime date, ref Dictionary<long, DateTime> lastGroupTimeKey, bool getAll = false)
        {
            var plate_shares_real_session = Singleton.Instance.sessionHandler.GetPlate_Real_Shares_Session(0, false);
            var shares_limit_session = Singleton.Instance.sessionHandler.GetShares_Limit_Session(false);
            //所有股票行情
            Dictionary<long, Shares_Quotes_Session_Info> sharesStatistic = getSharesStatistic(date, shares_limit_session);

            //获取题材
            List<ContextObj> hotPlate = getContext();

            List<MtlineInfo> resultData = new List<MtlineInfo>();
            List<MtlineInfo> AlreadyCal = new List<MtlineInfo>();
            foreach (var item in hotPlate)
            {
                if (item.DataType == 1 && AlreadyCal.Count() > 0)
                {
                    foreach (var x in AlreadyCal)
                    {
                        resultData.Add(new MtlineInfo
                        {
                            TradeStock = x.TradeStock,
                            ContextId = item.ContextId,
                            ContextType = 1,
                            Date = x.Date,
                            GroupTimeKey = x.GroupTimeKey,
                            RiseRate = x.RiseRate,
                            Time = x.Time,
                            TradeAmount = x.TradeAmount,
                            LimitUpCount = x.LimitUpCount,
                            RiseUpCount = x.RiseUpCount
                        });
                    }
                }
                else
                {
                    _cal_SharesGroupStatisticMtline(item, date, plate_shares_real_session, sharesStatistic, ref resultData, ref lastGroupTimeKey, ref AlreadyCal, getAll);
                }
            }
            if (resultData.Count() > 0)
            {
                if (getAll)
                {
                    WriteToDataBaseAll(resultData);
                }
                else
                {
                    WriteToDataBase(resultData);
                }
            }
        }

        public static void _cal_SharesGroupStatisticMtline(ContextObj contextInfo, DateTime date,Dictionary<long,List<long>> plate_shares_real_session,Dictionary<long,Shares_Quotes_Session_Info> sharesStatistic, ref List<MtlineInfo> datalist, ref Dictionary<long, DateTime> lastGroupTimeKey, ref List<MtlineInfo> AlreadyCal, bool getAll=false) 
        {
            List<long> resultKey = GetShares(contextInfo.DataType, contextInfo.PlateIdList, plate_shares_real_session, sharesStatistic, getAll);
            if (resultKey.Count() <= 0)
            {
                return;
            }

            DateTime startTimeKey = DateTime.Now.Date;
            if (lastGroupTimeKey!=null && lastGroupTimeKey.ContainsKey(contextInfo.ContextId) && lastGroupTimeKey[contextInfo.ContextId]>DateTime.Now.Date)
            {
                startTimeKey = lastGroupTimeKey[contextInfo.ContextId];
            }
            else 
            {
                startTimeKey = date;
            }
            DateTime endTimeKey = date.AddDays(1);

            DataTable table = new DataTable();
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            foreach (var sharesKey in resultKey)
            {
                int market = (int)(sharesKey % 10);
                string sharesCode = (sharesKey / 10).ToString().PadLeft(6, '0');
                DataRow row = table.NewRow();
                row["Market"] = market;
                row["SharesCode"] = sharesCode;
                table.Rows.Add(row);
            }
            //关键是类型
            SqlParameter parameter = new SqlParameter("@sharesList", SqlDbType.Structured);
            //必须指定表类型名
            parameter.TypeName = "dbo.SharesBase";
            //赋值
            parameter.Value = table;

            SqlParameter par1 = new SqlParameter("@startTime", SqlDbType.DateTime);
            par1.Value = startTimeKey;
            SqlParameter par2 = new SqlParameter("@endTime", SqlDbType.DateTime);
            par2.Value = endTimeKey;

            List<MtlineInfo> queryList = new List<MtlineInfo>();
            using (var db = new meal_ticketEntities())
            {
                db.Database.CommandTimeout = 1800;
                queryList = db.Database.SqlQuery<MtlineInfo>("exec P_GetMtlineInfo_1min @sharesList,@startTime,@endTime", parameter, par1, par2).ToList();
            }
            if (queryList.Count() > 0 && lastGroupTimeKey != null)
            {
                lastGroupTimeKey[contextInfo.ContextId] = queryList.Max(e => e.Time);
            }
            foreach (var item in queryList)
            {
                if (contextInfo.DataType == 1 && AlreadyCal != null)
                {
                    AlreadyCal.Add(new MtlineInfo
                    {
                        TradeStock = item.TradeStock,
                        ContextId = contextInfo.ContextId,
                        ContextType = 1,
                        Date = date,
                        GroupTimeKey = item.GroupTimeKey,
                        RiseRate = item.RiseRate,
                        Time = item.Time,
                        TradeAmount = item.TradeAmount,
                        LimitUpCount = item.LimitUpCount,
                        RiseUpCount = item.RiseUpCount
                    });
                }
                datalist.Add(new MtlineInfo 
                {
                    TradeStock=item.TradeStock,
                    ContextId= contextInfo.ContextId,
                    ContextType=1,
                    Date= date,
                    GroupTimeKey=item.GroupTimeKey,
                    RiseRate=item.RiseRate,
                    Time=item.Time,
                    TradeAmount=item.TradeAmount,
                    LimitUpCount=item.LimitUpCount,
                    RiseUpCount=item.RiseUpCount
                });
            }
        }
        public static void _cal_SharesGroupStatisticMtline_bak(ContextObj contextInfo, DateTime date, Dictionary<long, List<long>> plate_shares_real_session, Dictionary<long, Shares_Quotes_Session_Info> sharesStatistic, ref List<MtlineInfo> datalist, ref Dictionary<long, DateTime> lastGroupTimeKey, ref List<MtlineInfo> AlreadyCal, bool getAll = false)
        {
            List<long> resultKey = GetShares(contextInfo.DataType, contextInfo.PlateIdList, plate_shares_real_session, sharesStatistic, getAll);
            if (resultKey.Count() <= 0)
            {
                return;
            }

            DateTime startTimeKey = DateTime.Now.Date;
            if (lastGroupTimeKey != null && lastGroupTimeKey.ContainsKey(contextInfo.ContextId))
            {
                startTimeKey = lastGroupTimeKey[contextInfo.ContextId];
            }
            else
            {
                startTimeKey = date;
            }
            DateTime endTimeKey = date.AddDays(1);

            string sharesKeyStr = "";
            foreach (var sharesKey in resultKey)
            {
                sharesKeyStr = sharesKeyStr + sharesKey + ",";
            }
            sharesKeyStr = sharesKeyStr.TrimEnd(',');

            List<MtlineInfo> queryList = new List<MtlineInfo>();
            string sql = string.Format(@"select t.GroupTimeKey,t.[Time],cast(round(avg(case when t.ClosedPrice=0 or t.YestodayClosedPrice=0 then 0 else (t.ClosedPrice-t.YestodayClosedPrice)*1.0/t.YestodayClosedPrice end)*10000,0) as int) RiseRate,
	sum(t.TradeStock) TradeStock,sum(t.TradeAmount) TradeAmount,count(case when t.ClosedPrice-t.YestodayClosedPrice>0 then 1 else null end) RiseUpCount,count(case when t.PriceType=1 then 1 else null end)LimitUpCount
	from t_shares_securitybarsdata_1min t with(index(index_time_key))
	where t.[Time]>='{0}' and t.[Time]<'{1}' and SharesNumber in({2})
	group by [Time],GroupTimeKey", startTimeKey.ToString("yyyy-MM-dd HH:mm:ss"), endTimeKey.ToString("yyyy-MM-dd HH:mm:ss"), sharesKeyStr);

            Logger.WriteFileLog(sql,null);
            using (var db = new meal_ticketEntities())
            {
                db.Database.CommandTimeout = 1800;
                queryList = db.Database.SqlQuery<MtlineInfo>(sql).ToList();
            }
            if (queryList.Count() > 0 && lastGroupTimeKey != null)
            {
                lastGroupTimeKey[contextInfo.ContextId] = queryList.Max(e => e.Time);
            }
            foreach (var item in queryList)
            {
                if (contextInfo.DataType == 1 && AlreadyCal != null)
                {
                    AlreadyCal.Add(new MtlineInfo
                    {
                        TradeStock = item.TradeStock,
                        ContextId = contextInfo.ContextId,
                        ContextType = 1,
                        Date = date,
                        GroupTimeKey = item.GroupTimeKey,
                        RiseRate = item.RiseRate,
                        Time = item.Time,
                        TradeAmount = item.TradeAmount,
                        LimitUpCount = item.LimitUpCount,
                        RiseUpCount = item.RiseUpCount
                    });
                }
                datalist.Add(new MtlineInfo
                {
                    TradeStock = item.TradeStock,
                    ContextId = contextInfo.ContextId,
                    ContextType = 1,
                    Date = date,
                    GroupTimeKey = item.GroupTimeKey,
                    RiseRate = item.RiseRate,
                    Time = item.Time,
                    TradeAmount = item.TradeAmount,
                    LimitUpCount = item.LimitUpCount,
                    RiseUpCount = item.RiseUpCount
                });
            }
        }

        public static void WriteToDataBase(List<MtlineInfo> resultData) 
        {
            DataTable table = new DataTable();
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("ContextId", typeof(long));
            table.Columns.Add("ContextType", typeof(int));
            table.Columns.Add("GroupTimeKey", typeof(long));
            table.Columns.Add("Time", typeof(DateTime));
            table.Columns.Add("RiseRate", typeof(int));
            table.Columns.Add("TradeStock", typeof(long));
            table.Columns.Add("TradeAmount", typeof(long));
            table.Columns.Add("LimitUpCount", typeof(int));
            table.Columns.Add("RiseUpCount", typeof(int));

            foreach (var item in resultData)
            {
                DataRow row = table.NewRow();
                row["Date"] = item.Date;
                row["ContextId"] = item.ContextId;
                row["ContextType"] = item.ContextType;
                row["GroupTimeKey"] = item.GroupTimeKey;
                row["Time"] = item.Time;
                row["RiseRate"] = item.RiseRate;
                row["TradeStock"] = item.TradeStock;
                row["TradeAmount"] = item.TradeAmount;
                row["LimitUpCount"] = item.LimitUpCount;
                row["RiseUpCount"] = item.RiseUpCount;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesGroupStatisticMtline", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesGroupStatisticMtline";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Shares_Group_Statistic_Mtline @sharesGroupStatisticMtline", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("昨日涨停指数入库失败", ex);
                    tran.Rollback();
                }
            }
        }

        public static void WriteToDataBaseAll(List<MtlineInfo> resultData)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("ContextId", typeof(long));
            table.Columns.Add("ContextType", typeof(int));
            table.Columns.Add("GroupTimeKey", typeof(long));
            table.Columns.Add("Time", typeof(DateTime));
            table.Columns.Add("RiseRate", typeof(int));
            table.Columns.Add("TradeStock", typeof(long));
            table.Columns.Add("TradeAmount", typeof(long));
            table.Columns.Add("LimitUpCount", typeof(int));
            table.Columns.Add("RiseUpCount", typeof(int));

            foreach (var item in resultData)
            {
                DataRow row = table.NewRow();
                row["Date"] = item.Date;
                row["ContextId"] = item.ContextId;
                row["ContextType"] = item.ContextType;
                row["GroupTimeKey"] = item.GroupTimeKey;
                row["Time"] = item.Time;
                row["RiseRate"] = item.RiseRate;
                row["TradeStock"] = item.TradeStock;
                row["TradeAmount"] = item.TradeAmount;
                row["LimitUpCount"] = item.LimitUpCount;
                row["RiseUpCount"] = item.RiseUpCount;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesGroupStatisticMtline", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesGroupStatisticMtline";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_Shares_Group_Statistic_Mtline_All @sharesGroupStatisticMtline", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("所有股票指数入库失败", ex);
                    tran.Rollback();
                }
            }
        }

        private static List<long> GetShares(int dataType,List<long> plateIdList, Dictionary<long, List<long>> plate_shares_real_session, Dictionary<long, Shares_Quotes_Session_Info> shares_quotes_appointDate_session,bool getAll=false)
        {
            List<long> sharesKey = new List<long>();
            if (dataType == 0)
            { 
                sharesKey = getShares(plateIdList, plate_shares_real_session);
            }
            else
            {
                sharesKey = getAllShares(plate_shares_real_session);
            }
            List<long> resultKey = new List<long>();
            foreach (var key in sharesKey)
            {
                if (!shares_quotes_appointDate_session.ContainsKey(key))
                {
                    continue;
                }
                if (shares_quotes_appointDate_session[key].PriceTypeYestoday == 1 || getAll)
                {
                    resultKey.Add(key);
                }
            }
            return resultKey;
        }

        private static List<long> getShares(List<long> plateIdList, Dictionary<long, List<long>> plate_shares_real_session)
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

        public static Dictionary<long,Shares_Quotes_Session_Info> getSharesStatistic(DateTime date,List<long> shares_limit_session)
        {
            var session = Singleton.Instance.sessionHandler.GetShares_Quotes_AppointDate_Session(date, false);
            Dictionary<long, Shares_Quotes_Session_Info> result = new Dictionary<long, Shares_Quotes_Session_Info>();
            foreach (var item in session)
            {
                if (shares_limit_session.Contains(item.Key))
                {
                    continue;
                }
                result.Add(item.Key,new Shares_Quotes_Session_Info
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
                    PriceTypeYestoday = item.Value.PriceTypeYestoday
                });
            }
            return result;
        }

        private static List<ContextObj> getContext()
        {
            var hotspotSession = Singleton.Instance.sessionHandler.GetShares_Hotspot_Session(false);

            List<ContextObj> result = new List<ContextObj>();
            foreach (var item in hotspotSession)
            {
                result.Add(new ContextObj
                {
                    ContextId = item.Key,
                    ContextType = 1,
                    DataType = item.Value.DataType,
                    PlateIdList = item.Value.PlateIdList
                });
            }
            return result;
        }
    }
}
