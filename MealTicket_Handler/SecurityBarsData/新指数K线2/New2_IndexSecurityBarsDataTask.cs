using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Handler.RunnerHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_Handler.SecurityBarsData
{
    /// <summary>
    /// 板块/指数任务
    /// </summary>
    public class New2_IndexSecurityBarsDataTask
    {
        string connString_meal_ticket = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        DateTime MINTIME = DateTime.Parse("1990-01-01 00:00:00");

        #region===所有板块字典缓存===
        /// <summary>
        /// 板块字典缓存
        /// key=PlateId
        /// </summary>
        private Dictionary<long, Plate_Base_Session_Info> PlateSessionDic = new Dictionary<long, Plate_Base_Session_Info>();
        ReaderWriterLock PlateSessionDicLock = new ReaderWriterLock();

        public Dictionary<long, Plate_Base_Session_Info> GetPlateBaseSession()
        {
            Dictionary<long, Plate_Base_Session_Info> result;
            PlateSessionDicLock.AcquireReaderLock(Timeout.Infinite);
            result = new Dictionary<long, Plate_Base_Session_Info>(PlateSessionDic);
            PlateSessionDicLock.ReleaseReaderLock();
            return result;
        }
        public Plate_Base_Session_Info GetPlateBaseSessionByKey(long key)
        {
            Plate_Base_Session_Info result = null;
            PlateSessionDicLock.AcquireReaderLock(Timeout.Infinite);
            if (PlateSessionDic.ContainsKey(key))
            {
                result = PlateSessionDic[key];
            }
            PlateSessionDicLock.ReleaseReaderLock();
            return result;
        }

        /// <summary>
        /// 加载板块字典缓存
        /// </summary>
        public void LoadPlateSession()
        {
            PlateSessionDicLock.AcquireWriterLock(Timeout.Infinite);
            PlateSessionDic = Singleton.Instance.sessionHandler.GetPlate_Base_Session();
            PlateSessionDicLock.ReleaseWriterLock();
        }
        #endregion

        #region===板块K线最后一条数据缓存===
        /// <summary>
        /// PlateId*1000+WeightType*100+DataType=>K线信息
        /// </summary>
        private Dictionary<long, PlateImportData> PlateKlineLastSessionDic = new Dictionary<long, PlateImportData>();

        /// <summary>
        /// 获取板块最后一条数据缓存
        /// </summary>
        /// <returns></returns>
        public PlateImportData GetPlateKlineLastSessionValue(long key)
        {
            PlateImportData temp = null;
            if (PlateKlineLastSessionDic.ContainsKey(key))
            {
                temp = PlateKlineLastSessionDic[key];
            }
            return temp;
        }

        /// <summary>
        /// 设置板块最后一条数据缓存
        /// </summary>
        public void SetPlateKlineLastSession(long key, PlateImportData data)
        {
            PlateKlineLastSessionDic[key] = data;
        }

        /// <summary>
        /// 初始化最后一条数据缓存
        /// </summary>
        public void LoadPlateKlineLastSession()
        {
            PlateKlineLastSessionDic.Clear();
            DateTime date = DateTime.Now.AddDays(1).Date;
            List<int> dataTypeList = GetPlateDataType();

            for (int i = 0; i < dataTypeList.Count(); i++)
            {
                int dataType = dataTypeList[i];
                string tableName = "";
                if (!ParsePlateTableName(dataType, ref tableName))
                {
                    continue;
                }

                long groupTimeKey = 0;
                if (!ParseTimeGroupKey(DateTime.Now.Date, dataType, ref groupTimeKey))
                {
                    continue;
                }

                List<PlateImportData> lastData = new List<PlateImportData>();
                using (var db = new meal_ticketEntities())
                {
                    db.Database.CommandTimeout = 600;
                    string sql = string.Format(@"select t.PlateId,t.WeightType,t.Market,t.SharesCode,t.GroupTimeKey,t.PreClosePrice,t.LastTradeStock,t.LastTradeAmount,t.ClosedPrice,t.TradeStock,t.MaxPrice,t.MinPrice,t.OpenedPrice,
  t.[Time],t.TotalCapital,t.Tradable,t.TradeAmount,t.YestodayClosedPrice
from {0} t with(nolock)
inner join 
(
	select PlateId,WeightType,Max(GroupTimeKey)GroupTimeKey
	from {0} with(nolock)
	where [Time]<'{1}'
	group by PlateId,WeightType
)t1 on t.PlateId=t1.PlateId and t.WeightType=t1.WeightType and t.GroupTimeKey=t1.GroupTimeKey", tableName, date.ToString("yyyy-MM-dd"));
                    lastData = db.Database.SqlQuery<PlateImportData>(sql).ToList();

                }
                foreach (var item in lastData)
                {
                    long key = item.PlateId * 1000 + item.WeightType * 100 + dataType;
                    item.DataType = dataType;
                    SetPlateKlineLastSession(key, item);
                }
            }
        }
        #endregion

        #region===板块计算结果缓存===
        ReaderWriterLock SetPlateKlineSessionLock = new ReaderWriterLock();
        /// <summary>
        /// 板块Id*100+K线类型=>计算结果
        /// </summary>
        private Dictionary<long, Dictionary<long, PlateKlineSession>> PlateKlineSessionDic = new Dictionary<long, Dictionary<long, PlateKlineSession>>(14000);

        /// <summary>
        /// 获取所有键
        /// </summary>
        /// <returns></returns>
        private List<long> GetPlateKlineSessionAllKey()
        {
            SetPlateKlineSessionLock.AcquireReaderLock(Timeout.Infinite);
            List<long> temp = PlateKlineSessionDic.Keys.ToList();
            SetPlateKlineSessionLock.ReleaseReaderLock();
            return temp;
        }

        /// <summary>
        /// 重置是否更新
        /// </summary>
        private void ResetPlateKlineSessionIsUpdate()
        {
            SetPlateKlineSessionLock.AcquireWriterLock(Timeout.Infinite);
            foreach (var item in PlateKlineSessionDic)
            {
                foreach (var item2 in item.Value)
                {
                    if (item2.Value.IsUpdate == true)
                    {
                        item2.Value.IsUpdate = false;
                    }
                }
            }
            SetPlateKlineSessionLock.ReleaseWriterLock();
        }

        private Dictionary<long, Dictionary<long, PlateKlineSession>> GetAllData()
        {
            SetPlateKlineSessionLock.AcquireReaderLock(Timeout.Infinite);
            Dictionary<long, Dictionary<long, PlateKlineSession>> tempDic = PlateKlineSessionDic;
            SetPlateKlineSessionLock.ReleaseReaderLock();
            return tempDic;
        }

        /// <summary>
        /// 获取板块计算结果缓存
        /// </summary>
        /// <returns></returns>
        private Dictionary<long, PlateKlineSession> GetPlateKlineSession(long key)
        {
            SetPlateKlineSessionLock.AcquireReaderLock(Timeout.Infinite);
            if (!PlateKlineSessionDic.ContainsKey(key))
            {
                PlateKlineSessionDic.Add(key, new Dictionary<long, PlateKlineSession>(500));
            }
            Dictionary<long, PlateKlineSession> temp = PlateKlineSessionDic[key];
            SetPlateKlineSessionLock.ReleaseReaderLock();
            return temp;
        }

        private void SetPlateKlineSession(long keyMain, Dictionary<long, PlateKlineSession> dicValue)
        {
            SetPlateKlineSessionLock.AcquireWriterLock(Timeout.Infinite);
            if (!PlateKlineSessionDic.ContainsKey(keyMain))
            {
                PlateKlineSessionDic.Add(keyMain, new Dictionary<long, PlateKlineSession>(500));
            }
            foreach (var item in dicValue)
            {
                PlateKlineSessionDic[keyMain][item.Key] = item.Value;
            }
            SetPlateKlineSessionLock.ReleaseWriterLock();
        }

        /// <summary>
        /// 设置板块计算结果缓存
        /// </summary>
        private void SetPlateKlineSession(Dictionary<long, Dictionary<long, PlateKlineSession>> refDicSession)
        {
            SetPlateKlineSessionLock.AcquireWriterLock(Timeout.Infinite);
            foreach (var item in refDicSession)
            {
                PlateKlineSessionDic.Add(item.Key, new Dictionary<long, PlateKlineSession>(500));
                foreach (var item2 in item.Value)
                {
                    PlateKlineSessionDic[item.Key].Add(item2.Key, item2.Value);
                }
            }
            SetPlateKlineSessionLock.ReleaseWriterLock();
        }

        /// <summary>
        /// 更新板块计算结果缓存(多线程)
        /// </summary>
        /// <returns></returns>
        public void LoadPlateKlineSession()
        {
            PlateKlineSessionDic.Clear();

            DateTime date = DateTime.Now.Date;
            List<int> dataTypeList = GetPlateDataType();

            int taskCount = dataTypeList.Count();
            WaitHandle[] taskArr = new WaitHandle[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                int dataType = dataTypeList[i];

                taskArr[i] = TaskThread.CreateTask((e) =>
                {
                    string tableName = "";
                    if (!ParseTableName(dataType, ref tableName))
                    {
                        return;
                    }

                    long groupTimeKey = 0;
                    if (!ParseTimeGroupKey(date.Date, dataType, ref groupTimeKey))
                    {
                        return;
                    }

                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connString_meal_ticket))
                        {
                            conn.Open();
                            try
                            {
                                string sql = string.Format(@"select t1.PlateId,1 WeightType,t.GroupTimeKey,MAX(t.[Time])[Time],
  SUM(case when t2.ClosedPrice=0 then 0 else cast(round((t.OpenedPrice-t2.ClosedPrice)*1.0/t2.ClosedPrice*10000,0) as bigint) end) TotalOpenedPrice,
  SUM(case when t2.ClosedPrice=0 then 0 else cast(round((t.ClosedPrice-t2.ClosedPrice)*1.0/t2.ClosedPrice*10000,0) as bigint) end) TotalClosedPrice,
  SUM(case when t2.ClosedPrice=0 then 0 else cast(round((t.MinPrice-t2.ClosedPrice)*1.0/t2.ClosedPrice*10000,0) as bigint) end) TotalMinPrice,
  SUM(case when t2.ClosedPrice=0 then 0 else cast(round((t.MaxPrice-t2.ClosedPrice)*1.0/t2.ClosedPrice*10000,0) as bigint) end) TotalMaxPrice,
  convert(bigint,0) TotalPreClosePrice,
  SUM(t.TradeStock) TotalTradeStock,
  SUM(t.TradeAmount) TotalTradeAmount,
  SUM(t.LastTradeStock)TotalLastTradeStock,
  SUM(t.LastTradeAmount)TotalLastTradeAmount,
  SUM(t.Tradable)Tradable,
  SUM(t.TotalCapital)TotalCapital,
  count(*)CalCount,
  convert(bit,1)IsUpdate
  from {2} t with(index=index_grouptimekey_only)
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode
	  from t_shares_plate_rel_snapshot t with(nolock)
	  where t.[Date] in 
	  (
		  select Min([Date])
		  from t_shares_plate_rel_snapshot
		  where [Date]>='{1}'
	  )
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode
  inner join t_shares_quotes_date t2 on t.Market=t2.Market and t.SharesCode=t2.SharesCode and t2.[Date]='{1}'
  where t.GroupTimeKey>={0} and [Time]<'{3}'
  group by t.GroupTimeKey,t1.PlateId
  union all
  select t1.PlateId,2 WeightType,t.GroupTimeKey,MAX(t.[Time])[Time],
  SUM(t.OpenedPrice*t.TotalCapital) TotalOpenedPrice,
  SUM(t.ClosedPrice*t.TotalCapital) TotalClosedPrice,
  SUM(t.MinPrice*t.TotalCapital) TotalMinPrice,
  SUM(t.MaxPrice*t.TotalCapital) TotalMaxPrice,
  SUM(t2.ClosedPrice*t.TotalCapital) TotalPreClosePrice,
  SUM(t.TradeStock) TotalTradeStock,
  SUM(t.TradeAmount) TotalTradeAmount,
  SUM(t.LastTradeStock)TotalLastTradeStock,
  SUM(t.LastTradeAmount)TotalLastTradeAmount,
  SUM(t.Tradable)Tradable,
  SUM(t.TotalCapital)TotalCapital,
  count(*)CalCount,
  convert(bit,1)IsUpdate
  from {2} t with(index=index_grouptimekey_only)
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode
	  from t_shares_plate_rel_snapshot t with(nolock)
	  where t.[Date] in 
	  (
		  select Min([Date])
		  from t_shares_plate_rel_snapshot
		  where [Date]>='{1}'
	  )
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode
  inner join t_shares_quotes_date t2 on t.Market=t2.Market and t.SharesCode=t2.SharesCode and t2.[Date]='{1}'
  where t.GroupTimeKey>={0} and [Time]<'{3}'
  group by t.GroupTimeKey,t1.PlateId", groupTimeKey, date.ToString("yyyy-MM-dd"), tableName, date.AddDays(1).ToString("yyyy-MM-dd"));

                                Dictionary<long, Dictionary<long, PlateKlineSession>> resultGroup = new Dictionary<long, Dictionary<long, PlateKlineSession>>();
                                using (var cmd = conn.CreateCommand())
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = sql;   //sql语句
                                    cmd.CommandTimeout = 1200;

                                    SqlDataReader reader = cmd.ExecuteReader();
                                    while (reader.Read())
                                    {
                                        long plateId = Convert.ToInt64(reader["PlateId"]);
                                        int weightType = Convert.ToInt32(reader["WeightType"]);
                                        long this_groupTimeKey = Convert.ToInt64(reader["GroupTimeKey"]);
                                        bool isUpdate = true;
                                        var lastData = GetPlateKlineLastSessionValue(plateId * 1000 + weightType * 100 + dataType);
                                        if (lastData != null && lastData.GroupTimeKey > this_groupTimeKey)
                                        {
                                            isUpdate = false;
                                        }

                                        PlateKlineSession temp = new PlateKlineSession
                                        {
                                            TotalLastTradeStock = Convert.ToInt64(reader["TotalLastTradeStock"]),
                                            TotalTradeStock = Convert.ToInt64(reader["TotalTradeStock"]),
                                            CalCount = Convert.ToInt32(reader["CalCount"]),
                                            GroupTimeKey = this_groupTimeKey,
                                            IsUpdate = isUpdate,
                                            LastTempData = new Dictionary<int, SharesKlineData>(),
                                            PlateId = plateId,
                                            Time = Convert.ToDateTime(reader["Time"]),
                                            TotalCapital = Convert.ToInt64(reader["TotalCapital"]),
                                            TotalClosedPrice = Convert.ToInt64(reader["TotalClosedPrice"]),
                                            TotalLastTradeAmount = Convert.ToInt64(reader["TotalLastTradeAmount"]),
                                            TotalMaxPrice = Convert.ToInt64(reader["TotalMaxPrice"]),
                                            TotalMinPrice = Convert.ToInt64(reader["TotalMinPrice"]),
                                            TotalOpenedPrice = Convert.ToInt64(reader["TotalOpenedPrice"]),
                                            TotalPreClosePrice = Convert.ToInt64(reader["TotalPreClosePrice"]),
                                            TotalTradeAmount = Convert.ToInt64(reader["TotalTradeAmount"]),
                                            Tradable = Convert.ToInt64(reader["Tradable"]),
                                            WeightType = weightType,
                                        };

                                        if (resultGroup.ContainsKey(plateId * 100 + dataType))
                                        {
                                            resultGroup[plateId * 100 + dataType].Add(this_groupTimeKey * 10 + weightType, temp);
                                        }
                                        else
                                        {
                                            var tempDic = new Dictionary<long, PlateKlineSession>();
                                            tempDic.Add(this_groupTimeKey * 10 + weightType, temp);
                                            resultGroup.Add(plateId * 100 + dataType, tempDic);
                                        }

                                    }
                                    reader.Close();
                                }

                                SetPlateKlineSession(resultGroup);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("板块计算结果缓存加载失败", ex);
                    }
                }, null);
            }

            TaskThread.WaitAll(taskArr, Timeout.Infinite);
            TaskThread.CloseAllTasks(taskArr);
            Logger.WriteFileLog("===结束加载板块计算结果缓存===", null);
        }
        #endregion

        #region===股票最后一条数据缓存===
        /// <summary>
        /// 股票SharesCode*1000+Market*100+DataType=>K线信息
        /// </summary>
        Dictionary<long, SharesKlineData> SharesKlineLastSession = new Dictionary<long, SharesKlineData>();

        object SetSharesKlineLastSessionLock = new object();
        /// <summary>
        /// 获取股票最后一条数据缓存
        /// </summary>
        /// <returns></returns>
        public SharesKlineData GetSharesKlineLastSession(long key)
        {
            lock (SetSharesKlineLastSessionLock)
            {
                if (!SharesKlineLastSession.ContainsKey(key))
                {
                    SharesKlineLastSession.Add(key, new SharesKlineData());
                }
            }
            return SharesKlineLastSession[key];
        }

        /// <summary>
        /// 设置股票最后一条数据缓存
        /// </summary>
        public void SetSharesKlineLastSession(long key, SharesKlineData data)
        {
            lock (SetSharesKlineLastSessionLock)
            {
                if (!SharesKlineLastSession.ContainsKey(key))
                {
                    SharesKlineLastSession.Add(key, new SharesKlineData());
                }
            }
            SharesKlineLastSession[key].ClosedPrice = data.ClosedPrice;
            SharesKlineLastSession[key].GroupTimeKey = data.GroupTimeKey;
            SharesKlineLastSession[key].LastTradeAmount = data.LastTradeAmount;
            SharesKlineLastSession[key].LastTradeStock = data.LastTradeStock;
            SharesKlineLastSession[key].Market = data.Market;
            SharesKlineLastSession[key].MaxPrice = data.MaxPrice;
            SharesKlineLastSession[key].MinPrice = data.MinPrice;
            SharesKlineLastSession[key].OpenedPrice = data.OpenedPrice;
            SharesKlineLastSession[key].PlateId = data.PlateId;
            SharesKlineLastSession[key].PreClosePrice = data.PreClosePrice;
            SharesKlineLastSession[key].SharesCode = data.SharesCode;
            SharesKlineLastSession[key].Time = data.Time;
            SharesKlineLastSession[key].TotalCapital = data.TotalCapital;
            SharesKlineLastSession[key].Tradable = data.Tradable;
            SharesKlineLastSession[key].TradeAmount = data.TradeAmount;
            SharesKlineLastSession[key].TradeStock = data.TradeStock;
            SharesKlineLastSession[key].WeightType = data.WeightType;
            SharesKlineLastSession[key].YestodayClosedPrice = data.YestodayClosedPrice;
        }

        /// <summary>
        /// 初始化最后一条数据缓存
        /// </summary>
        public void LoadSecurityBarsLastDataBak()
        {
            SharesKlineLastSession.Clear();
            Logger.WriteFileLog("===开始初始化最后一条数据缓存===", null);
            List<int> dataTypeList = Singleton.Instance.SecurityBarsDataTypeList;
            using (var db = new meal_ticketEntities())
            {
                for (int i = 0; i < dataTypeList.Count(); i++)
                {
                    int dataType = dataTypeList[i];
                    string tableName = "";
                    if (!ParseTableName(dataType, ref tableName))
                    {
                        return;
                    }
                    long groupTimeKey = 0;
                    if (!ParseTimeGroupKey(DateTime.Now.Date, dataType, ref groupTimeKey))
                    {
                        return;
                    }
                    List<SharesKlineData> lastData = new List<SharesKlineData>();

                    db.Database.CommandTimeout = 600;
                    string sql = string.Format(@"select t.Market,t.SharesCode,t.GroupTimeKey,t.PreClosePrice,t.LastTradeStock,t.LastTradeAmount,t.ClosedPrice,t.TradeStock,t.MaxPrice,t.MinPrice,t.OpenedPrice,
  t.[Time],t.TotalCapital,t.Tradable,t.TradeAmount,t2.ClosedPrice YestodayClosedPrice
from {0} t with(nolock)
inner join 
(
	select Market,SharesCode,Max(GroupTimeKey)GroupTimeKey
	from {0} with(nolock)
	where [Time]<convert(varchar(10),dateadd(DAY,1,getdate()),120) and [Time]>convert(varchar(10),getdate(),120)
	group by Market,SharesCode
)t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode and t.GroupTimeKey=t1.GroupTimeKey
inner join v_shares_quotes_last t2 on t.Market=t2.Market and t.SharesCode=t2.SharesCode", tableName);
                    lastData = db.Database.SqlQuery<SharesKlineData>(sql).ToList();
                    foreach (var item in lastData)
                    {
                        SetSharesKlineLastSession(item.SharesCodeNum * 1000 + item.Market * 100 + dataType, item);
                    }
                }
            }
            Logger.WriteFileLog("===结束初始化最后一条数据缓存===", null);
        }

        /// <summary>
        /// 初始化最后一条数据缓存(多线程)
        /// </summary>
        public void LoadSecurityBarsLastData()
        {
            SharesKlineLastSession.Clear();
            Logger.WriteFileLog("===开始初始化最后一条数据缓存===", null);
            List<int> dataTypeList = Singleton.Instance.SecurityBarsDataTypeList;

            int taskCount = dataTypeList.Count();
            WaitHandle[] taskArr = new WaitHandle[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                int dataType = dataTypeList[i];

                taskArr[i] = TaskThread.CreateTask((e) =>
                {
                    string tableName = "";
                    if (!ParseTableName(dataType, ref tableName))
                    {
                        return;
                    }

                    long groupTimeKey = 0;
                    if (!ParseTimeGroupKey(DateTime.Now.Date, dataType, ref groupTimeKey))
                    {
                        return;
                    }
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connString_meal_ticket))
                        {
                            conn.Open();
                            try
                            {
                                string sql = string.Format(@"select t.Market,t.SharesCode,t.GroupTimeKey,t.PreClosePrice,t.LastTradeStock,t.LastTradeAmount,t.ClosedPrice,t.TradeStock,t.MaxPrice,t.MinPrice,t.OpenedPrice,
  t.[Time],t.TotalCapital,t.Tradable,t.TradeAmount,t2.ClosedPrice YestodayClosedPrice
from {0} t with(nolock)
inner join 
(
	select Market,SharesCode,Max(GroupTimeKey)GroupTimeKey
	from {0} with(nolock)
	where [Time]<convert(varchar(10),dateadd(DAY,1,getdate()),120) and [Time]>convert(varchar(10),getdate(),120)
	group by Market,SharesCode
)t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode and t.GroupTimeKey=t1.GroupTimeKey
inner join v_shares_quotes_last t2 on t.Market=t2.Market and t.SharesCode=t2.SharesCode", tableName);

                                using (var cmd = conn.CreateCommand())
                                {
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = sql;   //sql语句
                                    cmd.CommandTimeout = 600;

                                    SqlDataReader reader = cmd.ExecuteReader();
                                    while (reader.Read())
                                    {
                                        long sharesCodeNum = Convert.ToInt64(reader["SharesCode"]);
                                        int market = Convert.ToInt32(reader["Market"]);
                                        SharesKlineData item = new SharesKlineData
                                        {
                                            ClosedPrice = Convert.ToInt64(reader["ClosedPrice"]),
                                            GroupTimeKey = Convert.ToInt64(reader["GroupTimeKey"]),
                                            SharesCode = Convert.ToString(reader["SharesCode"]),
                                            LastTradeStock = Convert.ToInt64(reader["LastTradeStock"]),
                                            TradeStock = Convert.ToInt64(reader["TradeStock"]),
                                            LastTradeAmount = Convert.ToInt64(reader["LastTradeAmount"]),
                                            Market = Convert.ToInt32(reader["Market"]),
                                            MaxPrice = Convert.ToInt64(reader["MaxPrice"]),
                                            MinPrice = Convert.ToInt64(reader["MinPrice"]),
                                            OpenedPrice = Convert.ToInt64(reader["OpenedPrice"]),
                                            PreClosePrice = Convert.ToInt64(reader["PreClosePrice"]),
                                            Time = Convert.ToDateTime(reader["Time"]),
                                            Tradable = Convert.ToInt64(reader["Tradable"]),
                                            TotalCapital = Convert.ToInt64(reader["TotalCapital"]),
                                            TradeAmount = Convert.ToInt64(reader["TradeAmount"]),
                                            YestodayClosedPrice = Convert.ToInt64(reader["YestodayClosedPrice"])
                                        };

                                        SetSharesKlineLastSession(sharesCodeNum * 1000 + market * 100 + dataType, item);
                                    }
                                    reader.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("最后一条数据缓存加载失败", ex);
                    }
                }, null);
            }

            TaskThread.WaitAll(taskArr, Timeout.Infinite);
            TaskThread.CloseAllTasks(taskArr);
            Logger.WriteFileLog("===结束初始化最后一条数据缓存===", null);
        }
        #endregion

        #region===今日板块股票快照缓存===
        /// <summary>
        /// PlateId*100+DataType=>股票最后一条k先数据
        /// SharesCode*1000+Market*100+datatype
        /// </summary>
        Dictionary<long, Dictionary<long, SharesKlineData>> SharesByPlateSession = new Dictionary<long, Dictionary<long, SharesKlineData>>();

        /// <summary>
        /// 获取今日快照缓存(根据板块)
        /// </summary>
        /// <returns></returns>
        private Dictionary<long, SharesKlineData> GetSharesByPlateSession(long key)
        {
            Dictionary<long, SharesKlineData> result = new Dictionary<long, SharesKlineData>();
            if (SharesByPlateSession.ContainsKey(key))
            {
                result = SharesByPlateSession[key];
            }
            return result;
        }

        /// <summary>
        /// 设置今日快照缓存(根据板块)
        /// </summary>
        private void SetSharesByPlateSession(long key, Dictionary<long, SharesKlineData> sharesInfo)
        {
            SharesByPlateSession[key] = sharesInfo;
        }

        /// <summary>
        /// SharesCode*10+Market=>板块Id
        /// </summary>
        Dictionary<long, List<long>> SharesPlateSession = new Dictionary<long, List<long>>();

        /// <summary>
        /// 获取今日快照缓存
        /// </summary>
        /// <returns></returns>
        private List<long> GetSharesPlateSession(long key)
        {
            List<long> plateId = new List<long>();
            if (SharesPlateSession.ContainsKey(key))
            {
                plateId = SharesPlateSession[key];
            }
            return plateId;
        }

        /// <summary>
        /// 设置今日快照缓存
        /// </summary>
        private void SetSharesPlateSession(long key, List<long> plateId)
        {
            SharesPlateSession[key] = plateId;
        }

        /// <summary>
        /// 更新快照缓存
        /// </summary>
        /// <returns></returns>
        public void LoadSharesPlateSession()
        {
            SharesByPlateSession.Clear();
            SharesPlateSession.Clear();
            Logger.WriteFileLog("===加载快照缓存===", null);
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var list = (from item in db.t_shares_plate_rel_snapshot
                            where item.Date == dateNow
                            select item).ToList().GroupBy(e => new { e.Market, e.SharesCode }).ToDictionary(k => int.Parse(k.Key.SharesCode) * 10 + k.Key.Market, v => v.Select(e => e.PlateId).ToList());
                Logger.WriteFileLog("===加载快照缓存1===", null);
                foreach (var item in list)
                {
                    SetSharesPlateSession(item.Key, item.Value);
                }
                Logger.WriteFileLog("===加载快照缓存2===", null);
                var list2 = (from item in db.t_shares_plate_rel_snapshot
                             where item.Date == dateNow
                             select item).ToList().GroupBy(e => e.PlateId).ToDictionary(k => k.Key, v => v.ToList());
                Logger.WriteFileLog("===加载快照缓存3===", null);
                List<int> dataTypeList = Singleton.Instance.SecurityBarsDataTypeList;
                foreach (var item in list2)
                {
                    foreach (int dataType in dataTypeList)
                    {
                        Dictionary<long, SharesKlineData> dic = new Dictionary<long, SharesKlineData>();
                        foreach (var share in item.Value)
                        {
                            long key = long.Parse(share.SharesCode) * 1000 + share.Market * 100 + dataType;
                            dic.Add(key, GetSharesKlineLastSession(key));
                        }
                        SetSharesByPlateSession(item.Key * 100 + dataType, dic);
                    }
                }
                Logger.WriteFileLog("===加载快照缓存4===", null);
            }
        }

        /// <summary>
        /// 今日快照是否更新
        /// </summary>
        private DateTime PlateRelSnapshotUpdateLastTime = DateTime.Parse("1990-01-01");

        /// <summary>
        /// 更新当天板块内股票快照
        /// </summary>
        /// <returns></returns>
        public void UpdateTodayPlateRelSnapshot()
        {
            Logger.WriteFileLog("===开始当天板块内股票快照缓存===", null);
            DateTime dateNow = DateTime.Now.Date;
            if (dateNow <= PlateRelSnapshotUpdateLastTime)
            {
                return;
            }
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //判断今日快照是否已经更新
                    var catalogue = (from item in db.t_shares_plate_rel_snapshot_catalogue
                                     where item.Date == dateNow
                                     select item).FirstOrDefault();
                    if (catalogue != null)
                    {
                        //更新快照缓存
                        LoadSharesPlateSession();
                        PlateRelSnapshotUpdateLastTime = DateTime.Now;
                        throw new Exception("今日快照已更新");
                    }
                    string sql = string.Format("insert into t_shares_plate_rel_snapshot_catalogue([Date],CreateTime)values('{0}','{1}')", dateNow, DateTime.Now);
                    db.Database.ExecuteSqlCommand(sql);

                    sql = string.Format(@"declare @marketTime datetime=dbo.f_getTradeDate(getdate(),4);
insert into t_shares_plate_rel_snapshot
([Date], PlateId, Market, SharesCode)
select '{0}',t.PlateId,t.Market,t.SharesCode
from t_shares_plate_rel t with(nolock)
inner join 
(
	select Market,SharesCode from t_shares_markettime with(nolock)
	where MarketTime is not null and MarketTime<@marketTime
) t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode", dateNow);
                    db.Database.ExecuteSqlCommand(sql);

                    tran.Commit();
                    //更新快照缓存
                    LoadSharesPlateSession();
                    PlateRelSnapshotUpdateLastTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("更新当天板块内股票快照失败", ex);
                    tran.Rollback();
                }
            }
            Logger.WriteFileLog("===结束当天板块内股票快照缓存===", null);
        }
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            LoadPlateSession();
            LoadAllSession();

            SharesKlineQueue = new ThreadMsgTemplate<SharesKlineDataContain>();
            SharesKlineQueue.Init();
            DailyInitWaitQueue = new ThreadMsgTemplate<int>();
            DailyInitWaitQueue.Init();
            DoTask();
            plateRetryThreadStart();
            RetryThreadStart();
        }

        public void LoadAllSession()
        {
            //获取股票最后一条数据缓存
            LoadSecurityBarsLastData();
            //加载板块最新缓存
            LoadPlateKlineLastSession();
            //更新当天股票快照
            UpdateTodayPlateRelSnapshot();
            //更新当天计算结果缓存
            LoadPlateKlineSession();
        }

        private void DoTask()
        {
            DoRealTimeTask_Cal_Today();
            DoRealTimeTask();
        }

        /// <summary>
        /// 实时任务线程
        /// </summary>
        public Thread DailyInitThread;

        /// <summary>
        /// 每天初始化等待队列
        /// </summary>
        private ThreadMsgTemplate<int> DailyInitWaitQueue;

        /// <summary>
        /// 实时任务
        /// 1.指定代码，去券商直接获取板块K线数据
        /// 2.查找需要计算的板块
        /// </summary>
        private void DoRealTimeTask()
        {
            DailyInitThread = new Thread(() =>
            {
                DateTime TaskSuccessLastTime = DateTime.Now.Date;
                bool TaskMiddleExcute = true;
                while (true)
                {
                    try
                    {
                        int obj = 0;
                        if (DailyInitWaitQueue.WaitMessage(ref obj, Singleton.Instance.SecurityBarsIntervalTime))
                        {
                            break;
                        }
                        if (!RunnerHelper.CheckTradeDate())
                        {
                            continue;
                        }
                        DoDailyTask(ref TaskSuccessLastTime,ref TaskMiddleExcute);
                        if (!RunnerHelper.CheckTradeTime2(DateTime.Now.AddSeconds(-180)) && !RunnerHelper.CheckTradeTime2(DateTime.Now.AddSeconds(180)))
                        {
                            continue;
                        }
                        DoRealTimeTask_Push();
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("每天任务意外出错,继续执行", ex);
                    }
                }
            });
            DailyInitThread.Start();
        }

        /// <summary>
        /// 每日任务
        /// 1.更新所有板块缓存
        /// </summary>
        private void DoDailyTask(ref DateTime TaskSuccessLastTime,ref bool TaskMiddleExcute)
        {
            TimeSpan spanNow = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
            //if (spanNow > TimeSpan.Parse("11:45:00") && spanNow < TimeSpan.Parse("12:30:00") && TaskMiddleExcute)
            //{
            //    //更新当天计算结果缓存
            //    LoadPlateKlineSession();
            //    TaskMiddleExcute = false;
            //    return;
            //}
            if (spanNow < TimeSpan.Parse("00:30:00") || spanNow > TimeSpan.Parse("05:30:00"))
            {
                return;
            }
            if (DateTime.Now.Date <= TaskSuccessLastTime)
            {
                return;
            }
            TaskSuccessLastTime = DateTime.Now;
            TaskMiddleExcute = true;
            LoadPlateSession();
            //指令数据执行
            DoExcuteOrder();
            //扔入重算队列
            plateRetryQueue.AddMessage(0);
        }

        /// <summary>
        /// 指令执行
        /// </summary>
        private void DoExcuteOrder()
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_plate_rel_snapshot_instructions
                              where item.IsExcute == false
                              orderby item.Type descending
                              select item).ToList();
                Dictionary<long, Plate_Base_Session_Info> plate_base_session = new Dictionary<long, Plate_Base_Session_Info>();
                foreach (var item in result)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            item.IsExcute = true;
                            item.ExcuteTime = DateTime.Now;
                            db.SaveChanges();

                            _doExcuteOrder(item, plate_base_session, db);
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("执行指令出错", ex);
                            tran.Rollback();
                        }
                    }
                }
            }
        }

        private void _doExcuteOrder(t_shares_plate_rel_snapshot_instructions item, Dictionary<long, Plate_Base_Session_Info> plate_base_session, meal_ticketEntities db)
        {
            //重置基准日
            if (item.Type == 1)
            {
                _doExcuteOrder1(item.PlateId, plate_base_session, db);
            }
            //重置K线
            else if (item.Type == 2)
            {
                _doExcuteOrder2(item.PlateId, item.DataType, item.Context, db);
            }
            //批量重置K线
            else if (item.Type == 3)
            {
                _doExcuteOrder3(item.DataType, item.Context, db);
            }
        }

        /// <summary>
        /// 重置基准日指令
        /// </summary>
        private void _doExcuteOrder1(long plateId, Dictionary<long, Plate_Base_Session_Info> plate_base_session, meal_ticketEntities db)
        {
            if (plate_base_session.Count() == 0)
            {
                plate_base_session = GetPlateBaseSession();
            }
            if (!plate_base_session.ContainsKey(plateId))
            {
                throw new Exception("板块Id不存在");
            }
            var baseInfo = plate_base_session[plateId];
            if (baseInfo.BaseDate == null)
            {
                throw new Exception("基准日不存在");
            }
            long initIndex = Singleton.Instance.IndexInitValueDic[baseInfo.CalType];

            #region===删除所有K线数据===
            //1.1分钟K线
            ToDeleteResult(plateId, 2, MINTIME, db);
            ToInsertBaseData(plateId, 2, long.Parse(baseInfo.BaseDate.Value.ToString("yyyyMMdd0930")), baseInfo.BaseDate.Value.ToString("yyyy-MM-dd 09:30:00"), initIndex, db);
            //2.5分钟K线
            ToDeleteResult(plateId, 3, MINTIME, db);
            ToInsertBaseData(plateId, 3, long.Parse(baseInfo.BaseDate.Value.ToString("yyyyMMdd0935")), baseInfo.BaseDate.Value.ToString("yyyy-MM-dd 09:30:00"), initIndex, db);
            //3.15分钟K线
            ToDeleteResult(plateId, 4, MINTIME, db);
            ToInsertBaseData(plateId, 4, long.Parse(baseInfo.BaseDate.Value.ToString("yyyyMMdd0945")), baseInfo.BaseDate.Value.ToString("yyyy-MM-dd 09:30:00"), initIndex, db);
            //4.30分钟K线
            ToDeleteResult(plateId, 5, MINTIME, db);
            ToInsertBaseData(plateId, 5, long.Parse(baseInfo.BaseDate.Value.ToString("yyyyMMdd1000")), baseInfo.BaseDate.Value.ToString("yyyy-MM-dd 09:30:00"), initIndex, db);
            //5.60分钟K线
            ToDeleteResult(plateId, 6, MINTIME, db);
            ToInsertBaseData(plateId, 6, long.Parse(baseInfo.BaseDate.Value.ToString("yyyyMMdd1030")), baseInfo.BaseDate.Value.ToString("yyyy-MM-dd 09:30:00"), initIndex, db);
            //6.日K线
            ToDeleteResult(plateId, 7, MINTIME, db);
            ToInsertBaseData(plateId, 7, long.Parse(baseInfo.BaseDate.Value.ToString("yyyyMMdd")), baseInfo.BaseDate.Value.ToString("yyyy-MM-dd 09:30:00"), initIndex, db);
            //7.周K线
            ToDeleteResult(plateId, 8, MINTIME, db);
            long groupTimeKey = 0;
            ParseTimeGroupKey(baseInfo.BaseDate.Value, 8, ref groupTimeKey);
            ToInsertBaseData(plateId, 8, groupTimeKey, baseInfo.BaseDate.Value.ToString("yyyy-MM-dd 09:30:00"), initIndex, db);
            //8.月K线
            ToDeleteResult(plateId, 9, MINTIME, db);
            ToInsertBaseData(plateId, 9, long.Parse(baseInfo.BaseDate.Value.ToString("yyyyMM")), baseInfo.BaseDate.Value.ToString("yyyy-MM-dd 09:30:00"), initIndex, db);
            //9.季度K线
            ToDeleteResult(plateId, 10, MINTIME, db);
            groupTimeKey = 0;
            ParseTimeGroupKey(baseInfo.BaseDate.Value, 10, ref groupTimeKey);
            ToInsertBaseData(plateId, 10, groupTimeKey, baseInfo.BaseDate.Value.ToString("yyyy-MM-dd 09:30:00"), initIndex, db);
            //10.年K线
            ToDeleteResult(plateId, 11, MINTIME, db);
            ToInsertBaseData(plateId, 11, long.Parse(baseInfo.BaseDate.Value.ToString("yyyy")), baseInfo.BaseDate.Value.ToString("yyyy-MM-dd 09:30:00"), initIndex, db);
            #endregion
        }

        /// <summary>
        /// 重置K线
        /// </summary>
        private void _doExcuteOrder2(long plateId, int dataType, string contextStr, meal_ticketEntities db)
        {
            var context = JsonConvert.DeserializeObject<dynamic>(contextStr);
            long groupTimeKey = Convert.ToInt64(context.GroupTimeKey);
            ToDeleteResult(plateId, dataType, groupTimeKey, db);
        }

        /// <summary>
        /// 批量重置K线
        /// </summary>
        private void _doExcuteOrder3(int dataType, string contextStr, meal_ticketEntities db)
        {
            var context = JsonConvert.DeserializeObject<dynamic>(contextStr);
            long groupTimeKey = Convert.ToInt64(context.GroupTimeKey);
            ToDeleteResult(0, dataType, groupTimeKey, db);
        }

        //删除某个板块K线数据
        private bool ToDeleteResult(long plateId, int dataType, DateTime startTime, meal_ticketEntities db)
        {
            db.Database.CommandTimeout = 900;
            string tableName = "";
            if (!ParsePlateTableName(dataType, ref tableName))
            {
                return false;
            }
            string sql = "";
            if (plateId == 0)
            {
                sql = string.Format("delete {0} where [Time]>='{1}'", tableName, startTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                sql = string.Format("delete {0} where PlateId={1} and [Time]>='{2}'", tableName, plateId, startTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            db.Database.ExecuteSqlCommand(sql);
            return true;
        }

        //删除某个板块K线数据
        private bool ToDeleteResult(long plateId, int dataType, long groupTimeKey, meal_ticketEntities db)
        {
            db.Database.CommandTimeout = 900;
            string tableName = "";
            if (!ParsePlateTableName(dataType, ref tableName))
            {
                return false;
            }
            string sql = "";
            if (plateId == 0)
            {
                sql = string.Format("delete {0} where GroupTimeKey>={1}", tableName, groupTimeKey);
            }
            else
            {
                sql = string.Format("delete {0} where PlateId={1} and GroupTimeKey>={2}", tableName, plateId, groupTimeKey);
            }
            db.Database.ExecuteSqlCommand(sql);
            return true;
        }

        //添加基准日基准数据
        private bool ToInsertBaseData(long plateId, int dataType, long groupTimeKey, string time, long initIndex, meal_ticketEntities db)
        {
            string tableName = "";
            if (!ParsePlateTableName(dataType, ref tableName))
            {
                return false;
            }
            string sql = string.Format(@"insert into {0}
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(1,{1},0,'',{2},'{3}',{4},{4},{4},{4},{4},{4},0,0,0,0,0,0,0,'{5}');", tableName, plateId, groupTimeKey, time, initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); ;
            db.Database.ExecuteSqlCommand(sql);
            sql = string.Format(@"insert into {0}
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(2,{1},0,'',{2},'{3}',{4},{4},{4},{4},{4},{4},0,0,0,0,0,0,0,'{5}');", tableName, plateId, groupTimeKey, time, initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); ;
            db.Database.ExecuteSqlCommand(sql);
            return true;
        }

        /// <summary>
        /// 指定代码，去券商直接获取板块K线数据
        /// </summary>
        private void DoRealTimeTask_Push()
        {
            var plateDataType = GetPlateDataType();//K线类型列表
            var plateList = GetPlateBaseSession();//板块列表

            Dictionary<int, List<dynamic>> DataDic = new Dictionary<int, List<dynamic>>();
            foreach (var item in plateList)
            {
                if (string.IsNullOrEmpty(item.Value.WeightSharesCode) && string.IsNullOrEmpty(item.Value.NoWeightSharesCode))
                {
                    continue;
                }
                foreach (var dataType in plateDataType)
                {
                    if (!string.IsNullOrEmpty(item.Value.WeightSharesCode))
                    {
                        long key = item.Value.PlateId * 1000 + 200 + dataType;
                        var plateLast = GetPlateKlineLastSessionValue(key);
                        long tempGroupTimeKey = 0;
                        if (plateLast == null)
                        {
                            ParseTimeGroupKey(DateTime.Now.Date, dataType, ref tempGroupTimeKey);
                        }
                        var temp = new List<dynamic>();
                        if (!DataDic.ContainsKey(dataType))
                        {
                            DataDic[dataType] = temp;
                        }
                        else
                        {
                            temp = DataDic[dataType];
                        }
                        temp.Add(new
                        {
                            PlateId = item.Value.PlateId,
                            SharesCode = item.Value.WeightSharesCode,
                            Market = item.Value.WeightMarket,
                            WeightType = 2,
                            StartTimeKey = plateLast == null ? tempGroupTimeKey : plateLast.GroupTimeKey,
                            EndTimeKey = -1,
                            PreClosePrice = plateLast == null ? 0 : plateLast.PreClosePrice,
                            YestodayClosedPrice = plateLast == null ? 0 : plateLast.YestodayClosedPrice,
                            LastTradeStock = plateLast == null ? 0 : plateLast.LastTradeStock,
                            LastTradeAmount = plateLast == null ? 0 : plateLast.LastTradeAmount
                        });
                    }
                    if (!string.IsNullOrEmpty(item.Value.NoWeightSharesCode))
                    {
                        long key = item.Value.PlateId * 1000 + 100 + dataType;
                        var plateLast = GetPlateKlineLastSessionValue(key);
                        long tempGroupTimeKey = 0;
                        if (plateLast == null)
                        {
                            ParseTimeGroupKey(DateTime.Now.Date, dataType, ref tempGroupTimeKey);
                        }
                        var temp = new List<dynamic>();
                        if (!DataDic.ContainsKey(dataType))
                        {
                            DataDic[dataType] = temp;
                        }
                        else
                        {
                            temp = DataDic[dataType];
                        }
                        temp.Add(new
                        {
                            PlateId = item.Value.PlateId,
                            SharesCode = item.Value.NoWeightSharesCode,
                            Market = item.Value.NoWeightMarket,
                            WeightType = 1,
                            StartTimeKey = plateLast == null ? tempGroupTimeKey : plateLast.GroupTimeKey,
                            EndTimeKey = -1,
                            PreClosePrice = plateLast == null ? 0 : plateLast.PreClosePrice,
                            YestodayClosedPrice = plateLast == null ? 0 : plateLast.YestodayClosedPrice,
                            LastTradeStock = plateLast == null ? 0 : plateLast.LastTradeStock,
                            LastTradeAmount = plateLast == null ? 0 : plateLast.LastTradeAmount
                        });
                    }
                }
            }

            List<dynamic> sendList = new List<dynamic>();
            foreach (var item in DataDic)
            {
                sendList.Add(new
                {
                    DataType = item.Key,
                    SecurityBarsGetCount = 0,
                    DataList = item.Value
                });
            }
            Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new
            {
                TaskGuid = "",
                HandlerType = 3,
                RetryCount = 1,
                TotalRetryCount = Singleton.Instance.TotalRetryCount,
                PackageList = sendList
            })), "SecurityBars", "1min");
        }

        /// <summary>
        /// 计算历史补全K线
        /// </summary>
        private void DoRealTimeTask_Cal_His()
        {
            ThreadMsgTemplate<PlateImportData> data = new ThreadMsgTemplate<PlateImportData>();
            data.Init();
            Dictionary<long, PlateImportData> lastKLineData = new Dictionary<long, PlateImportData>(PlateKlineLastSessionDic);
            foreach (var item in lastKLineData)
            {
                if (item.Value.DataType > 7)
                {
                    continue;
                }
                data.AddMessage(item.Value);
            }

            int taskCount = 16;
            WaitHandle[] taskArr = new WaitHandle[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                taskArr[i] = TaskThread.CreateTask(_doRealTimeTask_Cal_His, data);
            }
            TaskThread.WaitAll(taskArr, Timeout.Infinite);
            TaskThread.CloseAllTasks(taskArr);
            data.Release();
        }

        private void _doRealTimeTask_Cal_His(object obj)
        {
            var data = obj as ThreadMsgTemplate<PlateImportData>;
            var lastDate = DbHelper.GetLastTradeDate2();
            do
            {
                PlateImportData tempData = new PlateImportData();
                if (!data.GetMessage(ref tempData, true))
                {
                    break;
                }
                try
                {
                    var lastTime = tempData.Time.Date;
                    long plateId = tempData.PlateId;
                    int weightType = tempData.WeightType;
                    int dataType = tempData.DataType;

                    long groupTimeKey = tempData.GroupTimeKey;
                    long yesClosePrice = tempData.YestodayClosedPrice;
                    long preClosePrice = tempData.PreClosePrice;

                    using (var db = new meal_ticketEntities())
                    {
                        db.Database.CommandTimeout = 600;
                        while (true)
                        {
                            if (lastTime > lastDate)
                            {
                                break;
                            }
                            //获取K线数据
                            var datalist = GetPlateKlineData(plateId, weightType, dataType, lastTime, groupTimeKey, db);
                            if (datalist == null || datalist.Count() <= 0)
                            {
                                lastTime = lastTime.AddDays(1);
                                if (!ParseTimeGroupKey(lastTime, dataType, ref groupTimeKey))
                                {
                                    break;
                                }
                                continue;
                            }

                            using (var tran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    ToRetryKLineData(datalist, ref preClosePrice, yesClosePrice, weightType, dataType, db);
                                    tran.Commit();
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteFileLog("执行板块K线数据补全出错", ex);
                                    tran.Rollback();
                                    break;
                                }
                            }
                            yesClosePrice = preClosePrice;
                            lastTime = lastTime.AddDays(1);
                            if (!ParseTimeGroupKey(lastTime, dataType, ref groupTimeKey))
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("重算板块K线有误", ex);
                }
            } while (true);
        }

        /// <summary>
        /// 重新计算K线数据（某个板块某一天）
        /// </summary>
        /// <param name="datalist"></param>
        /// <param name="preClosePrice"></param>
        /// <param name="yesClosePrice"></param>
        /// <param name="weightType"></param>
        /// <param name="dataType"></param>
        /// <param name="db"></param>
        private void ToRetryKLineData(List<PlateKlineSession> datalist, ref long preClosePrice, long yesClosePrice, long weightType, int dataType, meal_ticketEntities db)
        {
            List<PlateImportData> tempValue = new List<PlateImportData>();
            foreach (var x in datalist)
            {
                //不加权
                if (weightType == 1)
                {
                    tempValue.Add(new PlateImportData
                    {
                        LastTradeStock = x.TotalLastTradeStock,
                        TradeStock = x.TotalTradeStock,
                        ClosedPrice = (long)((x.TotalClosedPrice * 1.0 / x.CalCount / 10000 + 1) * yesClosePrice + 0.5),
                        GroupTimeKey = x.GroupTimeKey,
                        LastTradeAmount = x.TotalLastTradeAmount,
                        MaxPrice = (long)((x.TotalMaxPrice * 1.0 / x.CalCount / 10000 + 1) * yesClosePrice + 0.5),
                        MinPrice = (long)((x.TotalMinPrice * 1.0 / x.CalCount / 10000 + 1) * yesClosePrice + 0.5),
                        OpenedPrice = (long)((x.TotalOpenedPrice * 1.0 / x.CalCount / 10000 + 1) * yesClosePrice + 0.5),
                        PlateId = x.PlateId,
                        PreClosePrice = preClosePrice,
                        TotalCapital = x.TotalCapital,
                        Time = x.Time,
                        Tradable = x.Tradable,
                        TradeAmount = x.TotalTradeAmount,
                        YestodayClosedPrice = yesClosePrice,
                        WeightType = x.WeightType,
                        SharesCode = "",
                        Market = 0
                    });
                    preClosePrice = (long)((x.TotalClosedPrice * 1.0 / x.CalCount / 10000 + 1) * yesClosePrice + 0.5);
                }
                //加权
                else if (weightType == 2)
                {
                    tempValue.Add(new PlateImportData
                    {
                        LastTradeStock = x.TotalLastTradeStock,
                        TradeStock = x.TotalTradeStock,
                        ClosedPrice = x.TotalPreClosePrice == 0 ? preClosePrice : (long)(((x.TotalClosedPrice - x.TotalPreClosePrice) * 1.0 / x.TotalPreClosePrice + 1) * yesClosePrice + 0.5),
                        GroupTimeKey = x.GroupTimeKey,
                        LastTradeAmount = x.TotalLastTradeAmount,
                        MaxPrice = x.TotalPreClosePrice == 0 ? preClosePrice : (long)(((x.TotalMaxPrice - x.TotalPreClosePrice) * 1.0 / x.TotalPreClosePrice + 1) * yesClosePrice + 0.5),
                        MinPrice = x.TotalPreClosePrice == 0 ? preClosePrice : (long)(((x.TotalMinPrice - x.TotalPreClosePrice) * 1.0 / x.TotalPreClosePrice + 1) * yesClosePrice + 0.5),
                        OpenedPrice = x.TotalPreClosePrice == 0 ? preClosePrice : (long)(((x.TotalOpenedPrice - x.TotalPreClosePrice) * 1.0 / x.TotalPreClosePrice + 1) * yesClosePrice + 0.5),
                        PlateId = x.PlateId,
                        PreClosePrice = preClosePrice,
                        TotalCapital = x.TotalCapital,
                        Time = x.Time,
                        Tradable = x.Tradable,
                        TradeAmount = x.TotalTradeAmount,
                        YestodayClosedPrice = yesClosePrice,
                        WeightType = x.WeightType,
                        SharesCode = "",
                        Market = 0
                    });
                    preClosePrice = x.TotalPreClosePrice == 0 ? preClosePrice : (long)(((x.TotalClosedPrice - x.TotalPreClosePrice) * 1.0 / x.TotalPreClosePrice + 1) * yesClosePrice + 0.5);
                }
            }
            _bulkData(dataType, tempValue, db);
        }

        /// <summary>
        /// 获取某个板块（加权/不加权）某一天的K线数据
        /// </summary>
        /// <returns></returns>
        private List<PlateKlineSession> GetPlateKlineData(long plateId, int weightType, int dataType, DateTime date, long groupTimeKey, meal_ticketEntities db)
        {
            string tableName = "";
            if (!ParseTableName(dataType, ref tableName))
            {
                return null;
            }

            string sql = "";
            if (weightType == 1)
            {
                sql = string.Format(@"select t1.PlateId,1 WeightType,t.GroupTimeKey,MAX(t.[Time])[Time],
  SUM(case when t2.ClosedPrice=0 then 0 else cast(round((t.OpenedPrice-t2.ClosedPrice)*1.0/t2.ClosedPrice*10000,0) as bigint) end) TotalOpenedPrice,
  SUM(case when t2.ClosedPrice=0 then 0 else cast(round((t.ClosedPrice-t2.ClosedPrice)*1.0/t2.ClosedPrice*10000,0) as bigint) end) TotalClosedPrice,
  SUM(case when t2.ClosedPrice=0 then 0 else cast(round((t.MinPrice-t2.ClosedPrice)*1.0/t2.ClosedPrice*10000,0) as bigint) end) TotalMinPrice,
  SUM(case when t2.ClosedPrice=0 then 0 else cast(round((t.MaxPrice-t2.ClosedPrice)*1.0/t2.ClosedPrice*10000,0) as bigint) end) TotalMaxPrice,
  convert(bigint,0) TotalPreClosePrice,
  SUM(t.TradeStock) TotalTradeStock,
  SUM(t.TradeAmount) TotalTradeAmount,
  SUM(t.LastTradeStock)TotalLastTradeStock,
  SUM(t.LastTradeAmount)TotalLastTradeAmount,
  SUM(t.Tradable)Tradable,
  SUM(t.TotalCapital)TotalCapital,
  count(*)CalCount,
  convert(bit,1)IsUpdate
  from {0} t
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode
	  from t_shares_plate_rel_snapshot t with(nolock)
	  where t.PlateId={2} and t.[Date] in 
	  (
		  select Min([Date])
		  from t_shares_plate_rel_snapshot
		  where PlateId={2} and [Date]>='{1}'
	  )
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode
  inner join t_shares_quotes_date t2 on t.Market=t2.Market and t.SharesCode=t2.SharesCode and t2.[Date]='{1}'
  where t.GroupTimeKey>={3} and [Time]<'{4}'
  group by t.GroupTimeKey,t1.PlateId", tableName, date.ToString("yyyy-MM-dd"), plateId, groupTimeKey, date.AddDays(1).ToString("yyyy-MM-dd"));

            }
            else if (weightType == 2)
            {
                sql = string.Format(@"select t1.PlateId,2 WeightType,t.GroupTimeKey,MAX(t.[Time])[Time],
  SUM(t.OpenedPrice*t.TotalCapital) TotalOpenedPrice,
  SUM(t.ClosedPrice*t.TotalCapital) TotalClosedPrice,
  SUM(t.MinPrice*t.TotalCapital) TotalMinPrice,
  SUM(t.MaxPrice*t.TotalCapital) TotalMaxPrice,
  SUM(t2.ClosedPrice*t.TotalCapital) TotalPreClosePrice,
  SUM(t.TradeStock) TotalTradeStock,
  SUM(t.TradeAmount) TotalTradeAmount,
  SUM(t.LastTradeStock)TotalLastTradeStock,
  SUM(t.LastTradeAmount)TotalLastTradeAmount,
  SUM(t.Tradable)Tradable,
  SUM(t.TotalCapital)TotalCapital,
  count(*)CalCount,
  convert(bit,1)IsUpdate
  from {0} t
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode
	  from t_shares_plate_rel_snapshot t with(nolock)
	  where t.PlateId={2} and t.[Date] in 
	  (
		  select Min([Date])
		  from t_shares_plate_rel_snapshot
		  where PlateId={2} and [Date]>='{1}'
	  )
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode
  inner join t_shares_quotes_date t2 on t.Market=t2.Market and t.SharesCode=t2.SharesCode and t2.[Date]='{1}'
  where t.GroupTimeKey>={3} and [Time]<'{4}'
  group by t.GroupTimeKey,t1.PlateId", tableName, date.ToString("yyyy-MM-dd"), plateId, groupTimeKey, date.AddDays(1).ToString("yyyy-MM-dd"));
            }
            else
            {
                return null;
            }
            var datalist = db.Database.SqlQuery<PlateKlineSession>(sql).ToList();
            return datalist.OrderBy(e => e.GroupTimeKey).ToList();
        }

        /// <summary>
        /// 股票K线数据队列
        /// </summary>
        private ThreadMsgTemplate<SharesKlineDataContain> SharesKlineQueue;

        /// <summary>
        /// k线计算线程
        /// </summary>
        public Thread KlineThread;

        /// <summary>
        /// 计算今日K线
        /// </summary>
        private void DoRealTimeTask_Cal_Today()
        {
            KlineThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        SharesKlineDataContain data = new SharesKlineDataContain();
                        SharesKlineQueue.WaitMessage(ref data);
                        if (data == null)
                        {
                            break;
                        }
                        if (data.CalType == 1)
                        {
                            var list = (from item in data.SharesKlineData
                                        where item.PlateId > 0
                                        select new PlateImportData
                                        {
                                            LastTradeStock = item.LastTradeStock,
                                            TradeStock = item.TradeStock,
                                            GroupTimeKey = item.GroupTimeKey,
                                            PlateId = item.PlateId,
                                            Time = item.Time.Value,
                                            ClosedPrice = item.ClosedPrice,
                                            MaxPrice = item.MaxPrice,
                                            LastTradeAmount = item.LastTradeAmount,
                                            MinPrice = item.MinPrice,
                                            OpenedPrice = item.OpenedPrice,
                                            PreClosePrice = item.PreClosePrice,
                                            TradeAmount = item.TradeAmount,
                                            YestodayClosedPrice = item.YestodayClosedPrice,
                                            WeightType = item.WeightType,
                                            SharesCode = item.SharesCode,
                                            Market = item.Market,
                                            TotalCapital = item.TotalCapital,
                                            Tradable = item.Tradable
                                        }).ToList();
                            using (var db = new meal_ticketEntities())
                            {
                                _bulkData(data.DataType, list, db);
                            }
                        }
                        else
                        {
                            Logger.WriteFileLog("开始计算单包板块K线" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                            _calToday(data);
                            Logger.WriteFileLog("单包板块K线计算完成"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                            if (data.IsFinish)
                            {
                                Logger.WriteFileLog("===整体板块K线计算完成" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                                InsertToDatabase();
                                Logger.WriteFileLog("===整体板块K线导入完成" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("业务处理意外出错,继续执行", ex);
                    }
                }
            });
            KlineThread.Start();
        }

        /// <summary>
        /// 计算今日K线数据
        /// </summary>
        private void _calToday(SharesKlineDataContain data)
        {
            foreach (var item in data.SharesKlineData)
            {
                //2查询原最后一条股票数据缓存
                long key = item.SharesCodeNum * 1000 + item.Market * 100 + data.DataType;
                //设置股票最后一调数据缓存
                SetSharesKlineLastSession(key, item);


                //1.查询股票所属板块
                List<long> plateIdArr = new List<long>();
                plateIdArr = GetSharesPlateSession(item.SharesCodeNum * 10 + item.Market);
                if (plateIdArr.Count() <= 0)
                {
                    continue;
                }

                //3.板块循环
                foreach (var plateId in plateIdArr)
                {
                    //设置板块内股票最后一条数据缓存
                    CalPlateKlineResult(item, plateId, data.DataType);
                }
            }
        }

        /// <summary>
        /// 计算板块k线结果
        /// </summary>
        private void CalPlateKlineResult(SharesKlineData data, long plateId, int dataType)
        {
            var plate = GetPlateBaseSessionByKey(plateId);
            if (plate == null)
            {
                return;
            }
            bool isCalWeight = true;
            bool isCalNoWeight = true;
            if (!string.IsNullOrEmpty(plate.WeightSharesCode))
            {
                isCalWeight = false;
            }
            if (!string.IsNullOrEmpty(plate.NoWeightSharesCode))
            {
                isCalNoWeight = false;
            }


            var sharesDic = GetSharesByPlateSession(plateId * 100 + dataType);

            //1.查询原板块计算结果
            var calResult = GetPlateKlineSession(plateId * 100 + dataType);
            //int shareKey = (int)data.SharesCodeNum * 10 + data.Market;
            //foreach (var item in calResult)
            //{
            //    if (item.Value.GroupTimeKey < data.GroupTimeKey && item.Value.LastTempData.ContainsKey(shareKey))
            //    {
            //        item.Value.LastTempData.Remove(shareKey);
            //    }
            //}

            #region===不加权===
            if (isCalWeight)
            {
                _calPlateKlineResult_NoWeight(ref calResult, sharesDic, data, plateId, dataType);
            }
            #endregion

            #region===加权===
            if (isCalNoWeight)
            {
                _calPlateKlineResult_Weight(ref calResult, sharesDic, data, plateId, dataType);
            }
            #endregion

            //设置结果集
            //SetPlateKlineSession(plateId * 100 + dataType, calResult);
        }

        private void _calPlateKlineResult_NoWeight(ref Dictionary<long, PlateKlineSession> calResult, Dictionary<long, SharesKlineData> sharesDic, SharesKlineData data, long plateId, int dataType)
        {
            long resultKey = data.GroupTimeKey * 10 + 1;
            bool exists = calResult.ContainsKey(resultKey);
            if (exists)
            {
                var tempResult = calResult[resultKey];
                if (tempResult.CalCount < sharesDic.Count())//计算数量不足
                {
                    calResult.Remove(resultKey);
                    exists = false;
                }
            }

            if (!exists)
            {
                var temp = new PlateKlineSession
                {
                    PlateId = plateId,
                    CalCount = 0,
                    TotalLastTradeStock = 0,
                    TotalLastTradeAmount = 0,
                    TotalTradeStock = 0,
                    TotalTradeAmount = 0,
                    GroupTimeKey = data.GroupTimeKey,
                    Time = data.Time.Value,
                    TotalClosedPrice = 0,
                    TotalMaxPrice = 0,
                    TotalMinPrice = 0,
                    TotalOpenedPrice = 0,
                    TotalPreClosePrice = 0,
                    TotalYestodayClosedPrice = 0,
                    WeightType = 1,
                    IsUpdate = true,
                    Tradable = 0,
                    TotalCapital = 0,
                    LastTempData = new Dictionary<int, SharesKlineData>()
                };
                //计算该板块所有股票
                foreach (var item in sharesDic)
                {
                    int tempMarket = (int)(item.Key % 1000) / 100;
                    string tempCode = (item.Key / 1000).ToString("000000");
                    SharesKlineData tempData = new SharesKlineData
                    {
                        SharesCode = tempCode,
                        Market = tempMarket,
                        LastTradeStock = 0,
                        TradeStock = 0,
                        ClosedPrice = 0,
                        GroupTimeKey = data.GroupTimeKey,
                        LastTradeAmount = 0,
                        MaxPrice = 0,
                        MinPrice = 0,
                        OpenedPrice = 0,
                        PlateId = plateId,
                        PreClosePrice = 0,
                        Time = data.Time,
                        TotalCapital = 0,
                        Tradable = 0,
                        TradeAmount = 0,
                        WeightType = data.WeightType,
                        YestodayClosedPrice = 0
                    };
                    if (item.Key == data.SharesCodeNum * 1000 + data.Market * 100 + dataType)
                    {
                        tempData.LastTradeStock = data.LastTradeStock;
                        tempData.TradeStock = data.TradeStock;
                        tempData.ClosedPrice = data.ClosedPrice;
                        tempData.LastTradeAmount = data.LastTradeAmount;
                        tempData.MaxPrice = data.MaxPrice;
                        tempData.MinPrice = data.MinPrice;
                        tempData.OpenedPrice = data.OpenedPrice;
                        tempData.PreClosePrice = data.PreClosePrice;
                        tempData.TotalCapital = data.TotalCapital;
                        tempData.Tradable = data.Tradable;
                        tempData.TradeAmount = data.TradeAmount;
                        tempData.YestodayClosedPrice = data.YestodayClosedPrice;
                    }
                    else if (item.Value != null)
                    {
                        tempData.LastTradeStock = item.Value.LastTradeStock;
                        tempData.TradeStock = item.Value.TradeStock;
                        tempData.ClosedPrice = item.Value.ClosedPrice;
                        tempData.LastTradeAmount = item.Value.LastTradeAmount;
                        tempData.MaxPrice = item.Value.MaxPrice;
                        tempData.MinPrice = item.Value.MinPrice;
                        tempData.OpenedPrice = item.Value.OpenedPrice;
                        tempData.PreClosePrice = item.Value.PreClosePrice;
                        tempData.TotalCapital = item.Value.TotalCapital;
                        tempData.Tradable = item.Value.Tradable;
                        tempData.TradeAmount = item.Value.TradeAmount;
                        tempData.YestodayClosedPrice = item.Value.YestodayClosedPrice;
                    }

                    temp.TotalLastTradeStock = temp.TotalLastTradeStock + tempData.LastTradeStock;
                    temp.TotalLastTradeAmount = temp.TotalLastTradeAmount + tempData.LastTradeAmount;
                    temp.TotalTradeStock = temp.TotalTradeStock + tempData.TradeStock;
                    temp.TotalTradeAmount = temp.TotalTradeAmount + tempData.TradeAmount;
                    temp.TotalClosedPrice = temp.TotalClosedPrice + tempData.ClosedPriceRate;
                    temp.TotalMaxPrice = temp.TotalMaxPrice + tempData.MaxPriceRate;
                    temp.TotalMinPrice = temp.TotalMinPrice + tempData.MinPriceRate;
                    temp.TotalOpenedPrice = temp.TotalOpenedPrice + tempData.OpenedPriceRate;
                    temp.Tradable = temp.Tradable + tempData.Tradable;
                    temp.TotalCapital = temp.TotalCapital + tempData.TotalCapital;
                    temp.CalCount = temp.CalCount + 1;

                    int key = int.Parse(tempCode) * 10 + tempMarket;
                    temp.LastTempData.Add(key, tempData);
                }
                calResult.Add(resultKey, temp);
            }
            else
            {
                PlateKlineSession noWeightResult = calResult[resultKey];
                int key = int.Parse(data.SharesCode) * 10 + data.Market;
                SharesKlineData tempLast = new SharesKlineData();
                if (noWeightResult.LastTempData == null)
                {
                    tempLast = data;
                    noWeightResult.LastTempData = new Dictionary<int, SharesKlineData>();
                }
                else if (noWeightResult.LastTempData.ContainsKey(key))
                {
                    tempLast = noWeightResult.LastTempData[key];
                }
                else
                {
                    tempLast = data;
                }
                noWeightResult.TotalClosedPrice = noWeightResult.TotalClosedPrice + data.ClosedPriceRate - tempLast.ClosedPriceRate;
                noWeightResult.TotalLastTradeAmount = noWeightResult.TotalLastTradeAmount + data.LastTradeAmount - tempLast.LastTradeAmount;
                noWeightResult.TotalLastTradeStock = noWeightResult.TotalLastTradeStock + data.LastTradeStock - tempLast.LastTradeStock;
                noWeightResult.TotalMaxPrice = noWeightResult.TotalMaxPrice + data.MaxPriceRate - tempLast.MaxPriceRate;
                noWeightResult.TotalMinPrice = noWeightResult.TotalMinPrice + data.MinPriceRate - tempLast.MinPriceRate;
                noWeightResult.TotalOpenedPrice = noWeightResult.TotalOpenedPrice + data.OpenedPriceRate - tempLast.OpenedPriceRate;
                noWeightResult.TotalTradeAmount = noWeightResult.TotalTradeAmount + data.TradeAmount - tempLast.TradeAmount;
                noWeightResult.TotalTradeStock = noWeightResult.TotalTradeStock + data.TradeStock - tempLast.TradeStock;
                noWeightResult.Tradable = noWeightResult.Tradable + data.Tradable - tempLast.Tradable;
                noWeightResult.TotalCapital = noWeightResult.TotalCapital + data.TotalCapital - tempLast.TotalCapital;
                noWeightResult.IsUpdate = true;

                noWeightResult.LastTempData[key] = data;
            }
        }

        private void _calPlateKlineResult_Weight(ref Dictionary<long, PlateKlineSession> calResult, Dictionary<long, SharesKlineData> sharesDic, SharesKlineData data, long plateId, int dataType)
        {
            long resultKey = data.GroupTimeKey * 10 + 2;
            bool exists = calResult.ContainsKey(resultKey);
            if (exists)
            {
                var tempResult = calResult[resultKey];
                if (tempResult.CalCount < sharesDic.Count())//计算数量不足
                {
                    calResult.Remove(resultKey);
                    exists = false;
                }
            }

            if (!exists)
            {
                var temp = new PlateKlineSession
                {
                    PlateId = plateId,
                    CalCount = 0,
                    TotalLastTradeStock = 0,
                    TotalLastTradeAmount = 0,
                    TotalTradeStock = 0,
                    TotalTradeAmount = 0,
                    GroupTimeKey = data.GroupTimeKey,
                    Time = data.Time.Value,
                    TotalClosedPrice = 0,
                    TotalMaxPrice = 0,
                    TotalMinPrice = 0,
                    TotalOpenedPrice = 0,
                    TotalPreClosePrice = 0,
                    TotalYestodayClosedPrice = 0,
                    WeightType = 2,
                    IsUpdate = true,
                    Tradable = 0,
                    TotalCapital = 0,
                    LastTempData = new Dictionary<int, SharesKlineData>()
                };
                //计算该板块所有股票
                foreach (var item in sharesDic)
                {
                    int tempMarket = (int)(item.Key % 1000) / 100;
                    string tempCode = (item.Key / 1000).ToString("000000");
                    SharesKlineData tempData = new SharesKlineData
                    {
                        SharesCode = tempCode,
                        Market = tempMarket,
                        LastTradeStock = 0,
                        TradeStock = 0,
                        ClosedPrice = 0,
                        GroupTimeKey = data.GroupTimeKey,
                        LastTradeAmount = 0,
                        MaxPrice = 0,
                        MinPrice = 0,
                        OpenedPrice = 0,
                        PlateId = plateId,
                        PreClosePrice = 0,
                        Time = data.Time,
                        TotalCapital = 0,
                        Tradable = 0,
                        TradeAmount = 0,
                        WeightType = data.WeightType,
                        YestodayClosedPrice = 0
                    };
                    if (item.Key == data.SharesCodeNum * 1000 + data.Market * 100 + dataType)
                    {
                        tempData.LastTradeStock = data.LastTradeStock;
                        tempData.TradeStock = data.TradeStock;
                        tempData.ClosedPrice = data.ClosedPrice;
                        tempData.LastTradeAmount = data.LastTradeAmount;
                        tempData.MaxPrice = data.MaxPrice;
                        tempData.MinPrice = data.MinPrice;
                        tempData.OpenedPrice = data.OpenedPrice;
                        tempData.PreClosePrice = data.PreClosePrice;
                        tempData.TotalCapital = data.TotalCapital;
                        tempData.Tradable = data.Tradable;
                        tempData.TradeAmount = data.TradeAmount;
                        tempData.YestodayClosedPrice = data.YestodayClosedPrice;
                    }
                    else if (item.Value != null)
                    {
                        tempData.LastTradeStock = item.Value.LastTradeStock;
                        tempData.TradeStock = item.Value.TradeStock;
                        tempData.ClosedPrice = item.Value.ClosedPrice;
                        tempData.LastTradeAmount = item.Value.LastTradeAmount;
                        tempData.MaxPrice = item.Value.MaxPrice;
                        tempData.MinPrice = item.Value.MinPrice;
                        tempData.OpenedPrice = item.Value.OpenedPrice;
                        tempData.PreClosePrice = item.Value.PreClosePrice;
                        tempData.TotalCapital = item.Value.TotalCapital;
                        tempData.Tradable = item.Value.Tradable;
                        tempData.TradeAmount = item.Value.TradeAmount;
                        tempData.YestodayClosedPrice = item.Value.YestodayClosedPrice;
                    }
                    temp.TotalLastTradeStock = temp.TotalLastTradeStock + tempData.LastTradeStock;
                    temp.TotalLastTradeAmount = temp.TotalLastTradeAmount + tempData.LastTradeAmount;
                    temp.TotalTradeStock = temp.TotalTradeStock + tempData.TradeStock;
                    temp.TotalTradeAmount = temp.TotalTradeAmount + tempData.TradeAmount;
                    temp.TotalClosedPrice = temp.TotalClosedPrice + tempData.ClosedPrice * tempData.TotalCapital;
                    temp.TotalMaxPrice = temp.TotalMaxPrice + tempData.MaxPrice * tempData.TotalCapital;
                    temp.TotalMinPrice = temp.TotalMinPrice + tempData.MinPrice * tempData.TotalCapital;
                    temp.TotalOpenedPrice = temp.TotalOpenedPrice + tempData.OpenedPrice * tempData.TotalCapital;
                    temp.TotalPreClosePrice = temp.TotalPreClosePrice + tempData.PreClosePrice * tempData.TotalCapital;
                    temp.Tradable = temp.Tradable + tempData.Tradable;
                    temp.TotalCapital = temp.TotalCapital + tempData.TotalCapital;
                    temp.CalCount = temp.CalCount + 1;

                    int key = int.Parse(tempCode) * 10 + tempMarket;
                    temp.LastTempData.Add(key, tempData);
                }
                calResult.Add(data.GroupTimeKey * 10 + 2, temp);
            }
            else
            {
                PlateKlineSession weightResult = calResult[data.GroupTimeKey * 10 + 2];
                int key = int.Parse(data.SharesCode) * 10 + data.Market;
                SharesKlineData tempLast = new SharesKlineData();
                if (weightResult.LastTempData == null)
                {
                    tempLast = data;
                    weightResult.LastTempData = new Dictionary<int, SharesKlineData>();
                }
                else if (weightResult.LastTempData.ContainsKey(key))
                {
                    tempLast = weightResult.LastTempData[key];
                }
                else
                {
                    tempLast = data;
                }
                weightResult.TotalClosedPrice = weightResult.TotalClosedPrice + data.ClosedPrice * data.TotalCapital - tempLast.ClosedPrice * tempLast.TotalCapital;
                weightResult.TotalLastTradeAmount = weightResult.TotalLastTradeAmount + data.LastTradeAmount - tempLast.LastTradeAmount;
                weightResult.TotalLastTradeStock = weightResult.TotalLastTradeStock + data.LastTradeStock - tempLast.LastTradeStock;
                weightResult.TotalMaxPrice = weightResult.TotalMaxPrice + data.MaxPrice * data.TotalCapital - tempLast.MaxPrice * tempLast.TotalCapital;
                weightResult.TotalMinPrice = weightResult.TotalMinPrice + data.MinPrice * data.TotalCapital - tempLast.MinPrice * tempLast.TotalCapital;
                weightResult.TotalOpenedPrice = weightResult.TotalOpenedPrice + data.OpenedPrice * data.TotalCapital - tempLast.OpenedPrice * tempLast.TotalCapital;
                weightResult.TotalPreClosePrice = weightResult.TotalPreClosePrice + data.PreClosePrice * data.TotalCapital - tempLast.PreClosePrice * tempLast.TotalCapital;
                weightResult.TotalTradeAmount = weightResult.TotalTradeAmount + data.TradeAmount - tempLast.TradeAmount;
                weightResult.TotalTradeStock = weightResult.TotalTradeStock + data.TradeStock - tempLast.TradeStock;
                weightResult.Tradable = weightResult.Tradable + data.Tradable - tempLast.Tradable;
                weightResult.TotalCapital = weightResult.TotalCapital + data.TotalCapital - tempLast.TotalCapital;
                weightResult.IsUpdate = true;

                weightResult.LastTempData[key] = data;
            }
        }

        /// <summary>
        /// 导入数据业务
        /// </summary>
        private void InsertToDatabase()
        {
            List<long> allKeys = GetPlateKlineSessionAllKey();

            Dictionary<int, List<PlateKlineSession>> dataDic = new Dictionary<int, List<PlateKlineSession>>();
            foreach (long key in allKeys)
            {
                //解析板块Id和K线类型
                long plateId = key / 100;
                int dataType = (int)(key % 100);
                var tempData = GetPlateKlineSession(key).Values.Where(e => e.IsUpdate == true).ToList();

                List<PlateKlineSession> dataList = new List<PlateKlineSession>();
                if (!dataDic.TryGetValue(dataType, out dataList))
                {
                    dataDic.Add(dataType, tempData);
                }
                else
                {
                    dataList.AddRange(tempData);
                    dataDic[dataType] = dataList;
                }
            }
            if (_InsertToDatabase(dataDic))
            {
                ResetPlateKlineSessionIsUpdate();
            }
        }

        /// <summary>
        /// 导数数据
        /// </summary>
        /// <returns></returns>
        private bool _InsertToDatabase(Dictionary<int, List<PlateKlineSession>> dataDic)
        {
            DateTime minCalTime = DbHelper.GetLastTradeDate2(0, 0, 0, -1);
            minCalTime = minCalTime.AddHours(15);
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in dataDic)
                    {
                        long plateId = 0;
                        int weightType = 0;
                        long key = 0;
                        long preClosePrice = 0;
                        long yesClosePrice = 0;
                        List<PlateImportData> tempValue = new List<PlateImportData>();
                        var resultList = item.Value.OrderBy(e => e.WeightType).ThenBy(e => e.GroupTimeKey).ToList();
                        foreach (var x in resultList)
                        {
                            if (plateId != x.PlateId || weightType != x.WeightType)
                            {
                                key = x.PlateId * 1000 + x.WeightType * 100 + item.Key;
                                var lastPlateKLine = GetPlateKlineLastSessionValue(key);
                                if (lastPlateKLine == null)
                                {
                                    continue;
                                }
                                if (lastPlateKLine.Time < minCalTime)
                                {
                                    continue;
                                }
                                if (lastPlateKLine.GroupTimeKey > x.GroupTimeKey)
                                {
                                    continue;
                                }
                                if (lastPlateKLine.Time == minCalTime)
                                {
                                    yesClosePrice = lastPlateKLine.ClosedPrice;
                                    preClosePrice = lastPlateKLine.ClosedPrice;
                                }
                                else if (lastPlateKLine.GroupTimeKey == x.GroupTimeKey)
                                {
                                    yesClosePrice = lastPlateKLine.YestodayClosedPrice;
                                    preClosePrice = lastPlateKLine.PreClosePrice;
                                }
                                else
                                {
                                    yesClosePrice = lastPlateKLine.YestodayClosedPrice;
                                    preClosePrice = lastPlateKLine.ClosedPrice;
                                }
                                plateId = x.PlateId;
                                weightType = x.WeightType;
                            }
                            //不加权
                            if (weightType == 1)
                            {
                                tempValue.Add(new PlateImportData
                                {
                                    LastTradeStock = x.TotalLastTradeStock,
                                    TradeStock = x.TotalTradeStock,
                                    ClosedPrice = (long)((x.TotalClosedPrice * 1.0 / x.CalCount / 10000 + 1) * yesClosePrice + 0.5),
                                    GroupTimeKey = x.GroupTimeKey,
                                    LastTradeAmount = x.TotalLastTradeAmount,
                                    MaxPrice = (long)((x.TotalMaxPrice * 1.0 / x.CalCount / 10000 + 1) * yesClosePrice + 0.5),
                                    MinPrice = (long)((x.TotalMinPrice * 1.0 / x.CalCount / 10000 + 1) * yesClosePrice + 0.5),
                                    OpenedPrice = (long)((x.TotalOpenedPrice * 1.0 / x.CalCount / 10000 + 1) * yesClosePrice + 0.5),
                                    PlateId = x.PlateId,
                                    PreClosePrice = preClosePrice,
                                    TotalCapital = x.TotalCapital,
                                    Time = x.Time,
                                    Tradable = x.Tradable,
                                    TradeAmount = x.TotalTradeAmount,
                                    YestodayClosedPrice = yesClosePrice,
                                    WeightType = x.WeightType,
                                    SharesCode = "",
                                    Market = 0
                                });
                                preClosePrice = (long)((x.TotalClosedPrice * 1.0 / x.CalCount / 10000 + 1) * yesClosePrice + 0.5);
                            }
                            //加权
                            else if (weightType == 2)
                            {
                                tempValue.Add(new PlateImportData
                                {
                                    LastTradeStock = x.TotalPreClosePrice == 0 ? preClosePrice : (long)(((x.TotalLastTradeStock - x.TotalPreClosePrice) * 1.0 / x.TotalPreClosePrice + 1) * yesClosePrice + 0.5),
                                    TradeStock = x.TotalTradeStock,
                                    ClosedPrice = x.TotalPreClosePrice == 0 ? preClosePrice : (long)(((x.TotalClosedPrice - x.TotalPreClosePrice) * 1.0 / x.TotalPreClosePrice + 1) * yesClosePrice + 0.5),
                                    GroupTimeKey = x.GroupTimeKey,
                                    LastTradeAmount = x.TotalLastTradeAmount,
                                    MaxPrice = x.TotalPreClosePrice == 0 ? preClosePrice : (long)(((x.TotalMaxPrice - x.TotalPreClosePrice) * 1.0 / x.TotalPreClosePrice + 1) * yesClosePrice + 0.5),
                                    MinPrice = x.TotalPreClosePrice == 0 ? preClosePrice : (long)(((x.TotalMinPrice - x.TotalPreClosePrice) * 1.0 / x.TotalPreClosePrice + 1) * yesClosePrice + 0.5),
                                    OpenedPrice = x.TotalPreClosePrice == 0 ? preClosePrice : (long)(((x.TotalOpenedPrice - x.TotalPreClosePrice) * 1.0 / x.TotalPreClosePrice + 1) * yesClosePrice + 0.5),
                                    PlateId = x.PlateId,
                                    PreClosePrice = preClosePrice,
                                    TotalCapital = x.TotalCapital,
                                    Time = x.Time,
                                    Tradable = x.Tradable,
                                    TradeAmount = x.TotalTradeAmount,
                                    YestodayClosedPrice = yesClosePrice,
                                    WeightType = x.WeightType,
                                    SharesCode = "",
                                    Market = 0
                                });
                                preClosePrice = x.TotalPreClosePrice == 0 ? preClosePrice : (long)(((x.TotalClosedPrice - x.TotalPreClosePrice) * 1.0 / x.TotalPreClosePrice + 1) * yesClosePrice + 0.5);
                            }
                        }
                        if (tempValue.Count() <= 0)
                        {
                            continue;
                        }
                        try
                        {
                            var importData = new List<PlateImportData>();
                            for (int i = 0; i < tempValue.Count(); i++)
                            {
                                importData.Add(tempValue[i]);
                                if (i % 5000 == 0 || i == tempValue.Count() - 1)
                                {
                                    _bulkData(item.Key, importData, db);
                                    importData = new List<PlateImportData>();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("数据导入数据库失败,datatype=" + item.Key, ex);
                            throw ex;
                        }
                    }
                    tran.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("板块K线导入数据库失败", ex);
                    tran.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        private void _bulkData(int dataType, List<PlateImportData> list, meal_ticketEntities db)
        {
            DataTable table = new DataTable();
            table.Columns.Add("PlateId", typeof(long));
            table.Columns.Add("WeightType", typeof(int));
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("GroupTimeKey", typeof(long));
            table.Columns.Add("Time", typeof(DateTime));
            table.Columns.Add("OpenedPrice", typeof(long));
            table.Columns.Add("ClosedPrice", typeof(long));
            table.Columns.Add("PreClosePrice", typeof(long));
            table.Columns.Add("YestodayClosedPrice", typeof(long));
            table.Columns.Add("MinPrice", typeof(long));
            table.Columns.Add("MaxPrice", typeof(long));
            table.Columns.Add("TradeStock", typeof(long));
            table.Columns.Add("TradeAmount", typeof(long));
            table.Columns.Add("LastTradeStock", typeof(long));
            table.Columns.Add("LastTradeAmount", typeof(long));
            table.Columns.Add("Tradable", typeof(long));
            table.Columns.Add("TotalCapital", typeof(long));
            table.Columns.Add("IsLast", typeof(bool));
            table.Columns.Add("LastModified", typeof(DateTime));

            foreach (var item in list)
            {
                DataRow row = table.NewRow();
                row["PlateId"] = item.PlateId;
                row["WeightType"] = item.WeightType;
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["GroupTimeKey"] = item.GroupTimeKey;
                row["Time"] = item.Time;
                row["OpenedPrice"] = item.OpenedPrice;
                row["ClosedPrice"] = item.ClosedPrice;
                row["PreClosePrice"] = item.PreClosePrice;
                row["YestodayClosedPrice"] = item.YestodayClosedPrice;
                row["MinPrice"] = item.MinPrice;
                row["MaxPrice"] = item.MaxPrice;
                row["TradeStock"] = item.TradeStock;
                row["TradeAmount"] = item.TradeAmount;
                row["LastTradeStock"] = item.LastTradeStock;
                row["LastTradeAmount"] = item.LastTradeAmount;
                row["Tradable"] = item.Tradable;
                row["TotalCapital"] = item.TotalCapital;
                row["IsLast"] = 0;
                row["LastModified"] = DateTime.Now;
                table.Rows.Add(row);
            }

            //关键是类型
            SqlParameter parameter = new SqlParameter("@plateKLineData", SqlDbType.Structured);
            //必须指定表类型名
            parameter.TypeName = "dbo.PlateKLineData";
            //赋值
            parameter.Value = table;

            SqlParameter dataType_parameter = new SqlParameter("@dataType", SqlDbType.Int);
            dataType_parameter.Value = dataType;
            db.Database.ExecuteSqlCommand("exec P_Plate_KLineData_Update @plateKLineData,@dataType", parameter, dataType_parameter);

            //设置板块最后一条K线数据
            var tempList = list.GroupBy(e => new { e.PlateId, e.WeightType }).ToList();
            foreach (var item in tempList)
            {
                long key = item.Key.PlateId * 1000 + item.Key.WeightType * 100 + dataType;
                var temp = item.OrderByDescending(e => e.GroupTimeKey).First();
                temp.DataType = dataType;
                SetPlateKlineLastSession(key, temp);
            }
        }

        /// <summary>
        /// 获取需要的板块K线类型
        /// </summary>
        private List<int> GetPlateDataType()
        {
            return Singleton.Instance.SecurityBarsDataTypeList;
        }

        /// <summary>
        /// 解析时间值
        /// </summary>
        /// <returns></returns>
        private bool ParseTimeGroupKey(DateTime date, int type, ref long groupTimeKey)
        {
            switch (type)
            {
                case 2:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMddHHmm"));
                    break;
                case 3:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMddHHmm"));
                    break;
                case 4:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMddHHmm"));
                    break;
                case 5:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMddHHmm"));
                    break;
                case 6:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMddHHmm"));
                    break;
                case 7:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMdd"));
                    break;
                case 8:
                    groupTimeKey = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                    where item.the_date == int.Parse(date.ToString("yyyyMMdd"))
                                    select item.the_week).FirstOrDefault() ?? 0;
                    break;
                case 9:
                    groupTimeKey = long.Parse(date.ToString("yyyyMM"));
                    break;
                case 10:
                    groupTimeKey = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                    where item.the_date == int.Parse(date.ToString("yyyyMMdd"))
                                    select item.the_quarter).FirstOrDefault() ?? 0;
                    break;
                case 11:
                    groupTimeKey = long.Parse(date.ToString("yyyy"));
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 获取板块K线数据表名称
        /// </summary>
        /// <returns></returns>
        private bool ParsePlateTableName(int type, ref string tableName)
        {
            switch (type)
            {
                case 2:
                    tableName = "t_shares_plate_securitybarsdata_1min";
                    break;
                case 3:
                    tableName = "t_shares_plate_securitybarsdata_5min";
                    break;
                case 4:
                    tableName = "t_shares_plate_securitybarsdata_15min";
                    break;
                case 5:
                    tableName = "t_shares_plate_securitybarsdata_30min";
                    break;
                case 6:
                    tableName = "t_shares_plate_securitybarsdata_60min";
                    break;
                case 7:
                    tableName = "t_shares_plate_securitybarsdata_1day";
                    break;
                case 8:
                    tableName = "t_shares_plate_securitybarsdata_1week";
                    break;
                case 9:
                    tableName = "t_shares_plate_securitybarsdata_1month";
                    break;
                case 10:
                    tableName = "t_shares_plate_securitybarsdata_1quarter";
                    break;
                case 11:
                    tableName = "t_shares_plate_securitybarsdata_1year";
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 股票k线数据传送
        /// </summary>
        public void ToPushData(SharesKlineDataContain data)
        {
            SharesKlineQueue.AddMessage(data);
        }

        /// <summary>
        /// 板块K线重置队列
        /// </summary>
        private ThreadMsgTemplate<int> plateRetryQueue;

        /// <summary>
        /// 板块K线重置执行线程
        /// </summary>
        private Thread plateRetryThread;

        private void plateRetryThreadStart()
        {
            plateRetryQueue = new ThreadMsgTemplate<int>();
            plateRetryQueue.Init();
            plateRetryThread = new Thread(() =>
            {
                do
                {
                    int tempData = 0;
                    plateRetryQueue.WaitMessage(ref tempData);
                    if (tempData == -1)
                    {
                        break;
                    }
                    try
                    {
                        //加载板块最新缓存
                        LoadPlateKlineLastSession();

                        //执行指数板块补全

                        Logger.WriteFileLog("开始执行指令" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                        DoRealTimeTask_Push();
                        Logger.WriteFileLog("执行指令结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);

                        Logger.WriteFileLog("开始重算" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                        DoRealTimeTask_Cal_His();
                        Logger.WriteFileLog("重算结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);

                        Logger.WriteFileLog("开始加载缓存" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                        LoadAllSession();
                        Logger.WriteFileLog("加载缓存结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("重算操作出错", ex);
                    }
                } while (true);
            });
            plateRetryThread.Start();
        }



        struct retryParData
        {
            public int HandlerType { get; set; }

            public List<SecurityBarsDataParList> PackageList { get; set; }
        }

        /// <summary>
        /// 股票K线重置触发队列
        /// </summary>
        private ThreadMsgTemplate<retryParData> retryQueue;

        /// <summary>
        /// 股票K线重置触发执行线程
        /// </summary>
        private Thread retryThread;

        public void ToReceiveSharesRetry(int handlerType, List<SecurityBarsDataParList> packageList)
        {
            retryQueue.AddMessage(new retryParData
            {
                HandlerType = handlerType,
                PackageList = packageList
            });
        }

        private void RetryThreadStart()
        {
            retryQueue = new ThreadMsgTemplate<retryParData>();
            retryQueue.Init();
            retryThread = new Thread(() =>
            {
                do
                {
                    retryParData tempData = new retryParData();
                    retryQueue.WaitMessage(ref tempData);
                    if (tempData.HandlerType == -1)
                    {
                        break;
                    }
                    try
                    {
                        if (!ToExecuteRetry(tempData.HandlerType, tempData.PackageList))
                        {
                            continue;
                        }

                        //重新计算板块
                        if (tempData.HandlerType == 21)
                        {
                            plateRetryQueue.AddMessage(0);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("重置操作出错", ex);
                    }
                } while (true);
            });
            retryThread.Start();
        }

        /// <summary>
        /// 执行重置操作
        /// </summary>
        public bool ToExecuteRetry(int handlerType, List<SecurityBarsDataParList> packageList)
        {
            if (handlerType == 2)
            {
                return false;
            }
            StringBuilder sql_instructions = new StringBuilder();
            DateTime dateNow = DbHelper.GetLastTradeDate2();
            var shares_plate_rel_session = Singleton.Instance.sessionHandler.GetShares_Plate_Rel_Session();
            foreach (var package in packageList)
            {
                if (package.DataList.Count() <= 0)
                {
                    continue;
                }
                long minGroupTimeKey = long.MaxValue;
                List<long> plateIdList = new List<long>();
                foreach (var shares in package.DataList)
                {
                    if (minGroupTimeKey > shares.StartTimeKey)
                    {
                        minGroupTimeKey = shares.StartTimeKey;
                    }
                    long key = long.Parse(shares.SharesCode) * 10 + shares.Market;
                    if (!shares_plate_rel_session.ContainsKey(key))
                    {
                        continue;
                    }
                    plateIdList.AddRange(shares_plate_rel_session[key].Keys.ToList());

                }
                plateIdList = plateIdList.Distinct().ToList();

                foreach (long plateId in plateIdList)
                {
                    if (handlerType == 2)//生成待执行指令
                    {
                        string Context = JsonConvert.SerializeObject(new
                        {
                            GroupTimeKey = minGroupTimeKey
                        });
                        sql_instructions.AppendLine(string.Format("insert into t_shares_plate_rel_snapshot_instructions([Date],[Type],PlateId,PlateType,Market,SharesCode,DataType,Context,IsExcute,ExcuteTime,CreateTime,LastModified) values('{0}',{1},{2},{3},{4},'{5}',{6},'{7}',{8},'{9}','{10}','{10}');", DateTime.Now.ToString("yyyy-MM-dd"), 2, plateId, 0, 0, "", package.DataType, Context, 0, null, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    }
                    else if (handlerType == 21)//立即执行
                    {
                        string tableName = "";
                        if (!ParsePlateTableName(package.DataType, ref tableName))
                        {
                            continue;
                        }
                        sql_instructions.AppendLine(string.Format("delete {0} where PlateId={1} and [Time]>='{2}'", tableName, plateId, dateNow.ToString("yyyy-MM-dd HH:mm:ss")));
                    }
                }
            }

            if (sql_instructions.Length <= 0)
            {
                return false;
            }
            using (var db = new meal_ticketEntities())
            {
                db.Database.CommandTimeout = 900;
                db.Database.ExecuteSqlCommand(sql_instructions.ToString());
            }
            return true;
        }

        /// <summary>
        /// 获取股票K线数据表名称
        /// </summary>
        /// <returns></returns>
        private bool ParseTableName(int type, ref string tableName)
        {
            switch (type)
            {
                case 2:
                    tableName = "t_shares_securitybarsdata_1min";
                    break;
                case 3:
                    tableName = "t_shares_securitybarsdata_5min";
                    break;
                case 4:
                    tableName = "t_shares_securitybarsdata_15min";
                    break;
                case 5:
                    tableName = "t_shares_securitybarsdata_30min";
                    break;
                case 6:
                    tableName = "t_shares_securitybarsdata_60min";
                    break;
                case 7:
                    tableName = "t_shares_securitybarsdata_1day";
                    break;
                case 8:
                    tableName = "t_shares_securitybarsdata_1week";
                    break;
                case 9:
                    tableName = "t_shares_securitybarsdata_1month";
                    break;
                case 10:
                    tableName = "t_shares_securitybarsdata_1quarter";
                    break;
                case 11:
                    tableName = "t_shares_securitybarsdata_1year";
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            if (KlineThread != null)
            {
                SharesKlineQueue.AddMessage(null, true, 0);
                KlineThread.Join();
                SharesKlineQueue.Release();
            }
            if (retryThread != null)
            {
                retryQueue.AddMessage(new retryParData
                {
                    HandlerType = -1
                }, true, 0);
                retryThread.Join();
                retryQueue.Release();
            }
            if (plateRetryThread != null)
            {
                plateRetryQueue.AddMessage(-1, true, 0);
                plateRetryThread.Join();
                plateRetryQueue.Release();
            }
            if (DailyInitThread != null)
            {
                DailyInitWaitQueue.AddMessage(0);
                DailyInitThread.Join();
                DailyInitWaitQueue.Release();
            }
        }
    }
}
