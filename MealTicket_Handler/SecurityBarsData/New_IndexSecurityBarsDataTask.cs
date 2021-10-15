using FXCommon.Common;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
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
    public class New_IndexSecurityBarsDataTask
    {
        #region===板块计算结果缓存===
        /// <summary>
        /// 板块Id,K线类型=>计算结果
        /// </summary>
        private Dictionary<string, List<PlateKlineSession>> PlateKlineSession = new Dictionary<string, List<PlateKlineSession>>();
        private ReaderWriterLock PlateKlineSessionLock = new ReaderWriterLock();

        /// <summary>
        /// 获取所有键
        /// </summary>
        /// <returns></returns>
        private List<string> GetPlateKlineSessionAllKey() 
        {
            List<string> temp = new List<string>();
            PlateKlineSessionLock.AcquireReaderLock(Timeout.Infinite);
            temp = PlateKlineSession.Keys.ToList();
            PlateKlineSessionLock.ReleaseReaderLock();
            return temp;
        }

        /// <summary>
        /// 移除
        /// </summary>
        private void RemovePlateKlineSession(Dictionary<string, long> data) 
        {
            PlateKlineSessionLock.AcquireWriterLock(-1);
            foreach (var item in data)
            {
                if (!PlateKlineSession.ContainsKey(item.Key))
                {
                    continue;
                }
                var tempData = PlateKlineSession[item.Key];
                tempData = tempData.Where(e => e.GroupTimeKey >= item.Value).ToList();
                PlateKlineSession[item.Key] = tempData;
            }
            PlateKlineSessionLock.ReleaseWriterLock();
        }

        /// <summary>
        /// 获取板块计算结果缓存
        /// </summary>
        /// <returns></returns>
        private List<PlateKlineSession> GetPlateKlineSession(string key) 
        {
            List<PlateKlineSession> temp = new List<PlateKlineSession>();
            PlateKlineSessionLock.AcquireReaderLock(Timeout.Infinite);
            if (PlateKlineSession.ContainsKey(key))
            {
                temp = PlateKlineSession[key];
            }
            PlateKlineSessionLock.ReleaseReaderLock();
            return temp;
        }

        /// <summary>
        /// 设置板块计算结果缓存
        /// </summary>
        private void SetPlateKlineSession(string key, List<PlateKlineSession> data)
        {
            PlateKlineSessionLock.AcquireWriterLock(-1);
            PlateKlineSession[key] = data;
            PlateKlineSessionLock.ReleaseWriterLock();
        }

        /// <summary>
        /// 更新板块计算结果缓存
        /// </summary>
        /// <returns></returns>
        public void LoadPlateKlineSession()
        {
            List<int> dataTypeList = Singleton.Instance.SecurityBarsDataTypeList;
            Task[] taskArr = new Task[dataTypeList.Count];

            for (int i = 0; i < dataTypeList.Count; i++)
            {
                int dataType = dataTypeList[i];
                taskArr[i] = Task.Factory.StartNew(() => 
                {
                    string tableName = "";
                    if (!ParseTableName(dataType, ref tableName))
                    {
                        return;
                    }

                    string plateTableName = ""; 
                    if (!ParsePlateTableName(dataType, ref plateTableName))
                    {
                        return;
                    }
                    
                    long groupTimeKey = 0;
                    if (!ParseTimeGroupKey(DateTime.Now.Date, dataType, ref groupTimeKey))
                    {
                        return;
                    }

                    string sql = "";
                    if (dataType == 2)
                    {
                        sql = string.Format(@"select t1.PlateId,t1.BaseDate,1 WeightType,t.GroupTimeKey,t.[Time],SUM(t.OpenedPrice)TotalOpenedPrice,SUM(t.ClosedPrice)TotalClosedPrice,SUM(t.PreClosePrice)TotalPreClosePrice,
  SUM(t.YestodayClosedPrice)TotalYestodayClosedPrice,SUM(t.MinPrice)TotalMinPrice,SUM(t.MaxPrice)TotalMaxPrice,SUM(t.TradeStock)TotalTradeStock,SUM(t.TradeAmount)TotalTradeAmount,
  SUM(t.LastTradeStock)TotalLastTradeStock,SUM(t.LastTradeAmount)TotalLastTradeAmount,count(*)CalCount
  from t_shares_securitybarsdata_1min t
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode,isnull(t1.GroupTimeKey,{0})GroupTimeKey,t2.BaseDate
	  from t_shares_plate_rel_snapshot t
	  inner join 
	  (
		  select * from t_shares_plate where BaseDate is not null and BaseDate<convert(varchar(10),getdate(),120)
	  )
	  t2 on t.PlateId=t2.Id
	  left join  
	  (
		  select PlateId,GroupTimeKey 
		  from t_shares_plate_securitybarsdata_1min
		  where GroupTimeKey>{0} and IsLast=1
	  )t1  on t.PlateId=t1.PlateId
	  where t.[Date]='{1}'
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode and t.GroupTimeKey>=t1.GroupTimeKey
  where t.GroupTimeKey>{0}
  group by t.GroupTimeKey,t.[Time],t1.PlateId,t1.BaseDate
  union all
  select t1.PlateId,t1.BaseDate,2 WeightType,t.GroupTimeKey,t.[Time],SUM(t.OpenedPrice*t.TotalCapital)TotalOpenedPrice,SUM(t.ClosedPrice*t.TotalCapital)TotalClosedPrice,SUM(t.PreClosePrice*t.TotalCapital)TotalPreClosePrice,
  SUM(t.YestodayClosedPrice*t.TotalCapital)TotalYestodayClosedPrice,SUM(t.MinPrice*t.TotalCapital)TotalMinPrice,SUM(t.MaxPrice*t.TotalCapital)TotalMaxPrice,SUM(t.TradeStock)TotalTradeStock,SUM(t.TradeAmount)TotalTradeAmount,
  SUM(t.LastTradeStock)TotalLastTradeStock,SUM(t.LastTradeAmount)TotalLastTradeAmount,count(*)CalCount
  from t_shares_securitybarsdata_1min t
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode,isnull(t1.GroupTimeKey,{0})GroupTimeKey,t2.BaseDate
	  from t_shares_plate_rel_snapshot t
	  inner join 
	  (
		  select * from t_shares_plate where BaseDate is not null and BaseDate<convert(varchar(10),getdate(),120)
	  )
	  t2 on t.PlateId=t2.Id
	  left join  
	  (
		  select PlateId,GroupTimeKey 
		  from t_shares_plate_securitybarsdata_1min
		  where GroupTimeKey>{0} and IsLast=1
	  )t1  on t.PlateId=t1.PlateId
	  where t.[Date]='{1}'
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode and t.GroupTimeKey>=t1.GroupTimeKey
  where t.GroupTimeKey>{0}
  group by t.GroupTimeKey,t.[Time],t1.PlateId,t1.BaseDate", groupTimeKey, DateTime.Now.Date);
                    }
                    else
                    {
                        sql = string.Format(@"select t1.PlateId,t1.BaseDate,1 WeightType,t.GroupTimeKey,t.[Time],SUM(t.OpenedPrice)TotalOpenedPrice,SUM(t.ClosedPrice)TotalClosedPrice,SUM(t.PreClosePrice)TotalPreClosePrice,
  convert(bigint,0) TotalYestodayClosedPrice,SUM(t.MinPrice)TotalMinPrice,SUM(t.MaxPrice)TotalMaxPrice,SUM(t.TradeStock)TotalTradeStock,SUM(t.TradeAmount)TotalTradeAmount,
  convert(bigint,0)TotalLastTradeStock,convert(bigint,0)TotalLastTradeAmount,count(*)CalCount
  from {2} t
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode,isnull(t1.GroupTimeKey,{0})GroupTimeKey,t2.BaseDate
	  from t_shares_plate_rel_snapshot t
	  inner join 
	  (
		  select * from t_shares_plate where BaseDate is not null and BaseDate<convert(varchar(10),getdate(),120)
	  )
	  t2 on t.PlateId=t2.Id
	  left join  
	  (
		  select PlateId,GroupTimeKey 
		  from {3}
		  where GroupTimeKey>{0} and IsLast=1
	  )t1  on t.PlateId=t1.PlateId
	  where t.[Date]='{1}'
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode and t.GroupTimeKey>=t1.GroupTimeKey
  where t.GroupTimeKey>{0}
  group by t.GroupTimeKey,t.[Time],t1.PlateId,t1.BaseDate
  union all
  select t1.PlateId,t1.BaseDate,2 WeightType,t.GroupTimeKey,t.[Time],SUM(t.OpenedPrice*t.TotalCapital)TotalOpenedPrice,SUM(t.ClosedPrice*t.TotalCapital)TotalClosedPrice,SUM(t.PreClosePrice*t.TotalCapital)TotalPreClosePrice,
  convert(bigint,0)TotalYestodayClosedPrice,SUM(t.MinPrice*t.TotalCapital)TotalMinPrice,SUM(t.MaxPrice*t.TotalCapital)TotalMaxPrice,SUM(t.TradeStock)TotalTradeStock,SUM(t.TradeAmount)TotalTradeAmount,
  convert(bigint,0)TotalLastTradeStock,convert(bigint,0)TotalLastTradeAmount,count(*)CalCount
  from {2} t
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode,isnull(t1.GroupTimeKey,{0})GroupTimeKey,t2.BaseDate
	  from t_shares_plate_rel_snapshot t
	  inner join 
	  (
		  select * from t_shares_plate where BaseDate is not null and BaseDate<convert(varchar(10),getdate(),120)
	  )
	  t2 on t.PlateId=t2.Id
	  left join  
	  (
		  select PlateId,GroupTimeKey 
		  from {3}
		  where GroupTimeKey>{0} and IsLast=1
	  )t1  on t.PlateId=t1.PlateId
	  where t.[Date]='{1}'
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode and t.GroupTimeKey>=t1.GroupTimeKey
  where t.GroupTimeKey>{0}
  group by t.GroupTimeKey,t.[Time],t1.PlateId,t1.BaseDate", groupTimeKey,DateTime.Now.Date, tableName, plateTableName);
                    }

                    using (var db = new meal_ticketEntities())
                    {
                        var resultList = db.Database.SqlQuery<PlateKlineSession>(sql).ToList();
                        var resultGroup = resultList.GroupBy(e => e.PlateId).ToDictionary(k => k.Key, v => v.ToList());
                        foreach (var item in resultGroup)
                        {
                            SetPlateKlineSession(item.Key + "," + dataType, item.Value);
                        }
                    }
                });
            }
            Task.WaitAll(taskArr);
        }
        #endregion

        #region===今日板块股票快照缓存===
        /// <summary>
        /// Market,SharesCode=>板块Id
        /// </summary>
        Dictionary<string,List<long>> SharesPlateSession=new Dictionary<string, List<long>>();
        ReaderWriterLock SharesPlateSessionLock = new ReaderWriterLock();

        /// <summary>
        /// 获取今日快照缓存
        /// </summary>
        /// <returns></returns>
        private List<long> GetSharesPlateSession(string key)
        {
            List<long> plateId=new List<long>();
            SharesPlateSessionLock.AcquireReaderLock(Timeout.Infinite);
            if (SharesPlateSession.ContainsKey(key))
            {
                plateId = SharesPlateSession[key];
            }
            SharesPlateSessionLock.ReleaseReaderLock();
            return plateId;
        }

        /// <summary>
        /// 设置今日快照缓存
        /// </summary>
        private void SetSharesPlateSession(string key, List<long> plateId)
        {
            SharesPlateSessionLock.AcquireWriterLock(-1);
            SharesPlateSession[key] = plateId;
            SharesPlateSessionLock.ReleaseWriterLock();
        }

        /// <summary>
        /// 更新快照缓存
        /// </summary>
        /// <returns></returns>
        public void LoadSharesPlateSession()
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var list = (from item in db.t_shares_plate_rel_snapshot
                            where item.Date == dateNow
                            group item by new { item.Market, item.SharesCode } into g
                            select g).ToList();
                foreach (var item in list)
                {
                    SetSharesPlateSession(item.Key.Market+","+item.Key.SharesCode,item.Select(e=>e.PlateId).ToList());
                }
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
from t_shares_plate_rel t
inner join 
(
	select Market,SharesCode from t_shares_markettime
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
        }
        #endregion

        #region===股票最后一条数据缓存===
        /// <summary>
        /// 股票Market,SharesCode,DataType=>K线信息
        /// </summary>
        Dictionary<string, SharesKlineData> SharesKlineLastSession = new Dictionary<string, SharesKlineData>();
        ReaderWriterLock SharesKlineLastSessionLock = new ReaderWriterLock();

        /// <summary>
        /// 获取股票最后一条数据缓存
        /// </summary>
        /// <returns></returns>
        public SharesKlineData GetSharesKlineLastSession(string key)
        {
            SharesKlineData temp = null;
            SharesKlineLastSessionLock.AcquireReaderLock(Timeout.Infinite);
            if (SharesKlineLastSession.ContainsKey(key))
            {
                temp = SharesKlineLastSession[key];
            }
            SharesKlineLastSessionLock.ReleaseReaderLock();
            return temp;
        }

        /// <summary>
        /// 设置股票最后一条数据缓存
        /// </summary>
        public void SetSharesKlineLastSession(string key, SharesKlineData data)
        {
            SharesKlineLastSessionLock.AcquireWriterLock(-1);
            SharesKlineLastSession[key] = data;
            SharesKlineLastSessionLock.ReleaseWriterLock();
        }

        /// <summary>
        /// 初始化最后一条数据缓存
        /// </summary>
        public void LoadSecurityBarsLastData()
        {
            List<int> dataTypeList = Singleton.Instance.SecurityBarsDataTypeList;
            int taskCount = dataTypeList.Count();
            Task[] taskArr = new Task[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                int dataType = dataTypeList[i];
                taskArr[i] = Task.Factory.StartNew(() =>
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
                    List<SharesKlineData> lastData = new List<SharesKlineData>();
                    using (var db = new meal_ticketEntities())
                    {
                        db.Database.CommandTimeout = 600;
                        if (dataType == 2)
                        {
                            string sql = string.Format(@" select Market,SharesCode,GroupTimeKey,PreClosePrice,LastTradeStock,LastTradeAmount,ClosedPrice,TradeStock,MaxPrice,MinPrice,OpenedPrice,
  Time,TotalCapital,Tradable,TradeAmount,YestodayClosedPrice
  from t_shares_securitybarsdata_1min with(nolock) where GroupTimeKey >= {0} and IsLast=1", groupTimeKey);
                            lastData = db.Database.SqlQuery<SharesKlineData>(sql).ToList();
                        }
                        else
                        {
                            string sql = string.Format(@"select Market,SharesCode,GroupTimeKey,PreClosePrice,LastTradeStock,LastTradeAmount,ClosedPrice,TradeStock,MaxPrice,MinPrice,OpenedPrice,
  Time,TotalCapital,Tradable,TradeAmount,convert(bigint,0) YestodayClosedPrice
  from {0} with(nolock) where GroupTimeKey >= {1} and IsLast=1", tableName, groupTimeKey);
                            lastData = db.Database.SqlQuery<SharesKlineData>(sql).ToList();
                        }
                    }
                    foreach (var item in lastData)
                    {
                        SetSharesKlineLastSession(item.Market + "," + item.SharesCode + "," + dataType, item);
                    }
                });
            }
            Task.WaitAll(taskArr);
        }
        #endregion

        #region===板块基准日信息缓存===
        /// <summary>
        /// 板块Id=>结果信息
        /// </summary>
        private Dictionary<long, PlateBaseDataSession> PlateBaseDateSession = new Dictionary<long, PlateBaseDataSession>();
        private ReaderWriterLock PlateBaseDateSessionLock = new ReaderWriterLock();

        /// <summary>
        /// 获取板块计算结果缓存
        /// </summary>
        /// <returns></returns>
        private PlateBaseDataSession GetPlateBaseDateSession(long plateId)
        {
            PlateBaseDataSession temp = null;
            PlateBaseDateSessionLock.AcquireReaderLock(Timeout.Infinite);
            if (PlateBaseDateSession.ContainsKey(plateId))
            {
                temp = PlateBaseDateSession[plateId];
            }
            PlateBaseDateSessionLock.ReleaseReaderLock();
            return temp;
        }

        /// <summary>
        /// 设置板块计算结果缓存
        /// </summary>
        private void SetPlateBaseDateSession(long plateId, PlateBaseDataSession data)
        {
            PlateBaseDateSessionLock.AcquireWriterLock(-1);
            PlateBaseDateSession[plateId] = data;
            PlateBaseDateSessionLock.ReleaseWriterLock();
        }

        /// <summary>
        /// 更新板块计算结果缓存
        /// </summary>
        /// <returns></returns>
        public void LoadPlateBaseDateSession()
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select t.PlateId,t.BaseDate,AVG(t2.ClosedPrice*isnull(t4.TotalCapital,1)) WeightPrice,AVG(t2.ClosedPrice) NoWeightPrice,
	  AVG(t3.ClosedPrice*isnull(t4.TotalCapital,1)) CurrWeightPrice,AVG(t3.ClosedPrice) CurrNoWeightPrice
	  from
	  (
		  select Id PlateId,BaseDate
		  from t_shares_plate
		  where BaseDate is not null and BaseDate<convert(varchar(10),getdate(),120)
	  )t
	  inner join t_shares_plate_rel_snapshot t1 on t.PlateId=t1.PlateId and t1.[Date]=t.BaseDate
	  inner join t_shares_quotes_date t2 on t1.Market=t2.Market and t1.SharesCode=t2.SharesCode and t1.[Date]=t2.[Date]
	  inner join t_shares_quotes_date t3 on t1.Market=t2.Market and t1.SharesCode=t2.SharesCode and t3.[Date]=dbo.f_getTradeDate(convert(varchar(10),getdate(),120),-1)
	  left join t_shares_markettime t4 on t1.Market=t4.Market and t1.SharesCode=t4.SharesCode
	  group by t.PlateId,t.BaseDate";
                var result=db.Database.SqlQuery<PlateBaseDataSession>(sql).ToList();
                foreach (var item in result)
                {
                    SetPlateBaseDateSession(item.PlateId, item);
                }
            }
        }
        #endregion

        /// <summary>
        /// 股票K线数据队列
        /// </summary>
        private ThreadMsgTemplate<SharesKlineDataContain> SharesKlineQueue;

        /// <summary>
        /// 日志
        /// </summary>
        public Thread KlineThread;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init() 
        {
            SharesKlineQueue = new ThreadMsgTemplate<SharesKlineDataContain>();
            SharesKlineQueue.Init();

            KlineThread = new Thread(() =>
            {
                while (true)
                {
                    SharesKlineDataContain data = new SharesKlineDataContain();
                    SharesKlineQueue.WaitMessage(ref data);
                    if (data == null)
                    {
                        break;
                    }
                    ToHandler(data);
                }
            });
            KlineThread.Start();
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void ToHandler(SharesKlineDataContain data) 
        {
            foreach (var item in data.SharesKlineData)
            {
                //2查询原最后一条股票数据缓存
                var lastSharesData = GetSharesKlineLastSession(item.Market + "," + item.SharesCode + "," + data.DataType);
                //设置股票最后一调数据缓存
                SetSharesKlineLastSession(item.Market + "," + item.SharesCode + "," + data.DataType, item);

                //日K以上不需要计算
                if (data.DataType > 7)
                {
                    continue;
                }
                //1.查询股票所属板块
                List<long> plateIdArr = new List<long>();
                var _sharesPlateSession = GetSharesPlateSession(item.Market + "," + item.SharesCode);
                if (_sharesPlateSession.Count()<=0)
                {
                    continue;
                }
                //3.板块循环
                foreach (var plateId in plateIdArr)
                {
                    CalPlateKlineResult(item,plateId,data.DataType,lastSharesData);
                }
            }
        }

        /// <summary>
        /// 计算板块k线结果
        /// </summary>
        private void CalPlateKlineResult(SharesKlineData data,long plateId,int dataType,SharesKlineData lastSharesData) 
        {
            //1.查询原板块计算结果
            var calResult = GetPlateKlineSession(plateId + "," + dataType);
            //2.判断当前数据在原结果中是否存在
            var sourceResult = calResult.Where(e =>e.GroupTimeKey == data.GroupTimeKey).ToList();
            if (lastSharesData != null && lastSharesData.GroupTimeKey > data.GroupTimeKey)
            {
                return;
            }

            var plate = (from item in Singleton.Instance._SharesPlateSession.GetSessionData()
                         where item.PlateId == plateId
                         select item).FirstOrDefault();
            if (plate == null)
            {
                return;
            }
            if (plate.BaseDate == null || plate.BaseDate >= DateTime.Now.Date)
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

            #region===不加权===
            if (isCalWeight)
            {
                var noWeightResult = sourceResult.Where(e => e.WeightType == 1).FirstOrDefault();
                if (noWeightResult == null)
                {
                    calResult.Add(new PlateKlineSession
                    {
                        PlateId= plateId,
                        CalCount = 1,
                        TotalLastTradeStock = data.LastTradeStock,
                        TotalLastTradeAmount = data.LastTradeAmount,
                        TotalTradeStock = data.TradeStock,
                        TotalTradeAmount = data.TradeAmount,
                        GroupTimeKey = data.GroupTimeKey,
                        Time = data.Time.Value,
                        TotalClosedPrice = data.ClosedPrice,
                        TotalMaxPrice = data.MaxPrice,
                        TotalMinPrice = data.MinPrice,
                        TotalOpenedPrice = data.OpenedPrice,
                        TotalPreClosePrice = data.PreClosePrice,
                        TotalYestodayClosedPrice = data.YestodayClosedPrice,
                        WeightType = 1,
                        BaseDate= plate.BaseDate
                    });
                }
                else
                {
                    if (lastSharesData == null || lastSharesData.GroupTimeKey != data.GroupTimeKey)
                    {
                        noWeightResult.TotalClosedPrice = noWeightResult.TotalClosedPrice + data.ClosedPrice;
                        noWeightResult.TotalLastTradeAmount = noWeightResult.TotalLastTradeAmount + data.LastTradeAmount;
                        noWeightResult.TotalLastTradeStock = noWeightResult.TotalLastTradeStock + data.LastTradeStock;
                        noWeightResult.TotalMaxPrice = noWeightResult.TotalMaxPrice + data.MaxPrice;
                        noWeightResult.TotalMinPrice = noWeightResult.TotalMinPrice + data.MinPrice;
                        noWeightResult.TotalOpenedPrice = noWeightResult.TotalOpenedPrice + data.OpenedPrice;
                        noWeightResult.TotalPreClosePrice = noWeightResult.TotalPreClosePrice + data.PreClosePrice;
                        noWeightResult.TotalTradeAmount = noWeightResult.TotalTradeAmount + data.TradeAmount;
                        noWeightResult.TotalTradeStock = noWeightResult.TotalTradeStock + data.TradeStock;
                        noWeightResult.TotalYestodayClosedPrice = noWeightResult.TotalYestodayClosedPrice + data.YestodayClosedPrice;
                    }
                    else
                    {
                        noWeightResult.TotalClosedPrice = noWeightResult.TotalClosedPrice + data.ClosedPrice - lastSharesData.ClosedPrice;
                        noWeightResult.TotalLastTradeAmount = noWeightResult.TotalLastTradeAmount + data.LastTradeAmount - lastSharesData.LastTradeAmount;
                        noWeightResult.TotalLastTradeStock = noWeightResult.TotalLastTradeStock + data.LastTradeStock - lastSharesData.LastTradeStock;
                        noWeightResult.TotalMaxPrice = noWeightResult.TotalMaxPrice + data.MaxPrice - lastSharesData.MaxPrice;
                        noWeightResult.TotalMinPrice = noWeightResult.TotalMinPrice + data.MinPrice - lastSharesData.MinPrice;
                        noWeightResult.TotalOpenedPrice = noWeightResult.TotalOpenedPrice + data.OpenedPrice - lastSharesData.OpenedPrice;
                        noWeightResult.TotalPreClosePrice = noWeightResult.TotalPreClosePrice + data.PreClosePrice - lastSharesData.PreClosePrice;
                        noWeightResult.TotalTradeAmount = noWeightResult.TotalTradeAmount + data.TradeAmount - lastSharesData.TradeAmount;
                        noWeightResult.TotalTradeStock = noWeightResult.TotalTradeStock + data.TradeStock - lastSharesData.TradeStock;
                        noWeightResult.TotalYestodayClosedPrice = noWeightResult.TotalYestodayClosedPrice + data.YestodayClosedPrice - lastSharesData.YestodayClosedPrice;
                    }
                }
            }
            #endregion

            #region===加权===
            if (isCalNoWeight)
            {
                var weightResult = sourceResult.Where(e => e.WeightType == 2).FirstOrDefault();
                if (weightResult == null)
                {
                    calResult.Add(new PlateKlineSession
                    {
                        PlateId = plateId,
                        CalCount = 1,
                        TotalLastTradeStock = data.LastTradeStock,
                        TotalLastTradeAmount = data.LastTradeAmount,
                        TotalTradeStock = data.TradeStock,
                        TotalTradeAmount = data.TradeAmount,
                        GroupTimeKey = data.GroupTimeKey,
                        Time = data.Time.Value,
                        TotalClosedPrice = data.ClosedPrice * data.TotalCapital,
                        TotalMaxPrice = data.MaxPrice * data.TotalCapital,
                        TotalMinPrice = data.MinPrice * data.TotalCapital,
                        TotalOpenedPrice = data.OpenedPrice * data.TotalCapital,
                        TotalPreClosePrice = data.PreClosePrice * data.TotalCapital,
                        TotalYestodayClosedPrice = data.YestodayClosedPrice * data.TotalCapital,
                        WeightType = 2,
                        BaseDate = plate.BaseDate
                    });
                }
                else
                {
                    if (lastSharesData == null || lastSharesData.GroupTimeKey != data.GroupTimeKey)
                    {
                        weightResult.TotalClosedPrice = weightResult.TotalClosedPrice + data.ClosedPrice * data.TotalCapital;
                        weightResult.TotalLastTradeAmount = weightResult.TotalLastTradeAmount + data.LastTradeAmount;
                        weightResult.TotalLastTradeStock = weightResult.TotalLastTradeStock + data.LastTradeStock;
                        weightResult.TotalMaxPrice = weightResult.TotalMaxPrice + data.MaxPrice * data.TotalCapital;
                        weightResult.TotalMinPrice = weightResult.TotalMinPrice + data.MinPrice * data.TotalCapital;
                        weightResult.TotalOpenedPrice = weightResult.TotalOpenedPrice + data.OpenedPrice * data.TotalCapital;
                        weightResult.TotalPreClosePrice = weightResult.TotalPreClosePrice + data.PreClosePrice * data.TotalCapital;
                        weightResult.TotalTradeAmount = weightResult.TotalTradeAmount + data.TradeAmount;
                        weightResult.TotalTradeStock = weightResult.TotalTradeStock + data.TradeStock;
                        weightResult.TotalYestodayClosedPrice = weightResult.TotalYestodayClosedPrice + data.YestodayClosedPrice * data.TotalCapital;
                    }
                    else
                    {
                        weightResult.TotalClosedPrice = weightResult.TotalClosedPrice + data.ClosedPrice * data.TotalCapital - lastSharesData.ClosedPrice * data.TotalCapital;
                        weightResult.TotalLastTradeAmount = weightResult.TotalLastTradeAmount + data.LastTradeAmount - lastSharesData.LastTradeAmount;
                        weightResult.TotalLastTradeStock = weightResult.TotalLastTradeStock + data.LastTradeStock - lastSharesData.LastTradeStock;
                        weightResult.TotalMaxPrice = weightResult.TotalMaxPrice + data.MaxPrice * data.TotalCapital - lastSharesData.MaxPrice * data.TotalCapital;
                        weightResult.TotalMinPrice = weightResult.TotalMinPrice + data.MinPrice * data.TotalCapital - lastSharesData.MinPrice * data.TotalCapital;
                        weightResult.TotalOpenedPrice = weightResult.TotalOpenedPrice + data.OpenedPrice * data.TotalCapital - lastSharesData.OpenedPrice * data.TotalCapital;
                        weightResult.TotalPreClosePrice = weightResult.TotalPreClosePrice + data.PreClosePrice * data.TotalCapital - lastSharesData.PreClosePrice * data.TotalCapital;
                        weightResult.TotalTradeAmount = weightResult.TotalTradeAmount + data.TradeAmount - lastSharesData.TradeAmount;
                        weightResult.TotalTradeStock = weightResult.TotalTradeStock + data.TradeStock - lastSharesData.TradeStock;
                        weightResult.TotalYestodayClosedPrice = weightResult.TotalYestodayClosedPrice + data.YestodayClosedPrice * data.TotalCapital - lastSharesData.YestodayClosedPrice * data.TotalCapital;
                    }
                }
            }
            #endregion

            //设置结果集
            SetPlateKlineSession(plateId + "," + dataType, calResult);
        }

        /// <summary>
        /// 结果数据入库
        /// </summary>
        private void InsertToDatabase() 
        {
            List<string> allKeys = GetPlateKlineSessionAllKey();
            Dictionary<string, long> tempDic = new Dictionary<string, long>();

            Dictionary<int, List<PlateKlineSession>> dataDic = new Dictionary<int, List<PlateKlineSession>>();
            foreach (string key in allKeys)
            {
                //解析板块Id和K线类型
                long plateId = long.Parse(key.Split(',')[0]);
                int dataType = int.Parse(key.Split(',')[1]);
                var tempData = GetPlateKlineSession(key);

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
                tempDic[key] = tempData.Max(e => e.GroupTimeKey);
            }

            if (BulkData(dataDic))
            {
                RemovePlateKlineSession(tempDic);
            }
        }

        /// <summary>
        /// 导数数据
        /// </summary>
        /// <returns></returns>
        private bool BulkData(Dictionary<int, List<PlateKlineSession>> dataDic)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in dataDic)
                    {
                        _bulkData(item.Key,item.Value, db);
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
        private void _bulkData(int dataType, List<PlateKlineSession> list, meal_ticketEntities db)
        {
            DataTable table = new DataTable();
            table.Columns.Add("WeightType", typeof(int));
            table.Columns.Add("PlateId", typeof(long));
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
            table.Columns.Add("BaseDate", typeof(DateTime));

            var weightList = list.Where(e => e.WeightType == 2).ToList();
            var noWeightList = list.Where(e => e.WeightType == 1).ToList();
            long weightMaxGroupKey = 0;
            long noWeightMaxGroupKey = 0;
            if (weightList.Count() > 0)
            {
                weightMaxGroupKey = weightList.Max(e => e.GroupTimeKey);
            }
            if (noWeightList.Count() > 0)
            {
                noWeightMaxGroupKey = noWeightList.Max(e => e.GroupTimeKey);
            }
            foreach (var item in list)
            {
                var baseDateInfo=GetPlateBaseDateSession(item.PlateId);
                if (baseDateInfo == null)
                {
                    continue;
                }

                long basePrice = item.WeightType == 1 ? baseDateInfo.NoWeightPrice : baseDateInfo.WeightPrice;
                long currBasePrice= item.WeightType == 1 ? baseDateInfo.CurrNoWeightPrice : baseDateInfo.CurrWeightPrice;

                DataRow row = table.NewRow();
                row["WeightType"] = item.WeightType;
                row["PlateId"] = item.PlateId;
                row["Market"] = 0;
                row["SharesCode"] = "";
                row["GroupTimeKey"] = item.GroupTimeKey;
                row["Time"] = item.Time;
                row["OpenedPrice"] = (item.TotalOpenedPrice / item.CalCount)*1.0/ basePrice * 1000;
                row["ClosedPrice"] = item.TotalClosedPrice / item.CalCount;
                row["PreClosePrice"] = item.TotalPreClosePrice / item.CalCount;
                row["YestodayClosedPrice"] = item.TotalYestodayClosedPrice / item.CalCount;
                row["MinPrice"] = item.TotalYestodayClosedPrice / item.CalCount;
                row["MaxPrice"] = item.TotalMinPrice / item.CalCount;
                row["TradeStock"] = item.TotalTradeStock;
                row["TradeAmount"] = item.TotalTradeAmount;
                row["LastTradeStock"] = item.TotalLastTradeStock;
                row["LastTradeAmount"] = item.TotalLastTradeAmount;
                row["Tradable"] = 0;
                row["TotalCapital"] = 0;
                row["IsLast"] = (item.WeightType == 2 && item.GroupTimeKey == weightMaxGroupKey) || (item.WeightType == 1 && item.GroupTimeKey == noWeightMaxGroupKey) ? true : false;
                row["LastModified"] = DateTime.Now;
                row["BaseDate"] = item.BaseDate;
                table.Rows.Add(row);
            }


            //关键是类型
            SqlParameter parameter = new SqlParameter("@sharesPlateSecurityBarsData", SqlDbType.Structured);
            //必须指定表类型名
            parameter.TypeName = "dbo.SharesPlateSecurityBarsData";
            //赋值
            parameter.Value = table;

            SqlParameter dataType_parameter = new SqlParameter("@dataType", SqlDbType.Int);
            dataType_parameter.Value = dataType;
            db.Database.ExecuteSqlCommand("exec P_Shares_Plate_SecurityBarsData_Update @sharesPlateSecurityBarsData,@dataType", parameter, dataType_parameter);

        }

        /// <summary>
        /// 股票k线数据传送
        /// </summary>
        public void ToPushData(SharesKlineDataContain data) 
        {
            SharesKlineQueue.AddMessage(data);
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
        }
    }
}
