using FXCommon.Common;
using MealTicket_DBCommon;
using Newtonsoft.Json;
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
        /// <summary>
        /// 板块缓存
        /// </summary>
        private Dictionary<long, SharesPlateInfo_Session> _SharesPlateSessionDic = new Dictionary<long, SharesPlateInfo_Session>();

        public void LoadPlateSession()
        {
            _SharesPlateSessionDic = Singleton.Instance._SharesPlateSession.GetSessionData().ToDictionary(k => k.PlateId, v => v);
            foreach (var x in Singleton.Instance._SharesPlateSession.GetSessionData())
            {
                if (x.BaseDate != null && x.BaseDate == DateTime.Now.AddDays(-1).Date && x.BaseDateWeightPrice == 0 && x.BaseDateNoWeightPrice == 0)
                {
                    using (var db = new meal_ticketEntities())
                    {
                        var plate = (from item in db.t_shares_plate
                                     where item.Id == x.PlateId
                                     select item).FirstOrDefault();
                        if (plate == null)
                        {
                            continue;
                        }

                        string date = x.BaseDate.Value.ToString("yyyy-MM-dd");
                        var quoteDate = from item in db.t_shares_quotes_date
                                        where item.Date == date
                                        select item;
                        var snapshot = (from item in db.t_shares_plate_rel_snapshot
                                        join item2 in quoteDate on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                        join item3 in db.t_shares_markettime on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode }
                                        where item.PlateId == x.PlateId && item.Date == x.BaseDate
                                        select new { item, item2, item3 }).ToList();

                        long BaseDateWeightPrice = 0;
                        long BaseDateNoWeightPrice = 0;
                        if (snapshot.Count() > 0)
                        {
                            BaseDateWeightPrice = (long)snapshot.Average(e => e.item2.PresentPrice * e.item3.TotalCapital);
                            BaseDateNoWeightPrice = (long)snapshot.Average(e => e.item2.PresentPrice);
                        }

                        plate.BaseDate = x.BaseDate;
                        plate.BaseDateWeightPrice = BaseDateWeightPrice;
                        plate.BaseDateNoWeightPrice = BaseDateNoWeightPrice;
                        db.SaveChanges();
                    }
                }
            }
        }

        #region===板块计算结果缓存===
        /// <summary>
        /// 板块Id*100+K线类型=>计算结果
        /// GroupTimeKey*10+WeightType
        /// </summary>
        private Dictionary<long,Dictionary<long, PlateKlineSession>> PlateKlineSession = new Dictionary<long, Dictionary<long, PlateKlineSession>>();

        /// <summary>
        /// 获取所有键
        /// </summary>
        /// <returns></returns>
        private List<long> GetPlateKlineSessionAllKey() 
        {
            List<long> temp = PlateKlineSession.Keys.ToList();
            return temp;
        }

        /// <summary>
        /// 重置是否更新
        /// </summary>
        private void ResetPlateKlineSessionIsUpdate() 
        {
            foreach (var item in PlateKlineSession)
            {
                foreach (var item2 in item.Value)
                {
                    if (item2.Value.IsUpdate == true)
                    {
                        item2.Value.IsUpdate = false;
                    }
                }
            }
        }

        /// <summary>
        /// 获取板块计算结果缓存
        /// </summary>
        /// <returns></returns>
        private Dictionary<long, PlateKlineSession> GetPlateKlineSession(long key) 
        {
            Dictionary<long, PlateKlineSession> temp = new Dictionary<long, PlateKlineSession>();
            if (PlateKlineSession.ContainsKey(key))
            {
                temp = PlateKlineSession[key];
            }
            return temp;
        }

        /// <summary>
        /// 设置板块计算结果缓存
        /// </summary>
        private void SetPlateKlineSession(long key,Dictionary<long, PlateKlineSession> data)
        {
            PlateKlineSession[key] = data;
        }

        /// <summary>
        /// 更新板块计算结果缓存
        /// </summary>
        /// <returns></returns>
        public void LoadPlateKlineSession()
        {
            Logger.WriteFileLog("===开始加载板块计算结果缓存===", null);
            List<int> dataTypeList = Singleton.Instance.SecurityBarsDataTypeList;
            dataTypeList = dataTypeList.Where(e => e <= 7).ToList();
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

                    string sql = string.Format(@"declare @date datetime,@groupTimeKey bigint;
  set @date='{1}';
  set @groupTimeKey={0};
  select t1.PlateId,1 WeightType,t.GroupTimeKey,t.[Time],SUM(t.OpenedPrice)TotalOpenedPrice,SUM(t.ClosedPrice)TotalClosedPrice,SUM(t.PreClosePrice)TotalPreClosePrice,
  {3}SUM(t.MinPrice)TotalMinPrice,SUM(t.MaxPrice)TotalMaxPrice,SUM(t.TradeStock)TotalTradeStock,SUM(t.TradeAmount)TotalTradeAmount,
  SUM(t.LastTradeStock)TotalLastTradeStock,SUM(t.LastTradeAmount)TotalLastTradeAmount,count(*)CalCount,convert(bit,1) IsUpdate
  from {2} t with(nolock)
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode
	  from t_shares_plate_rel_snapshot t with(nolock)
	  inner join 
	  (
		  select * from t_shares_plate with(nolock) where BaseDate is not null and BaseDate<convert(varchar(10),getdate(),120)
	  )
	  t2 on t.PlateId=t2.Id
	  where t.[Date]=@date
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode
  where t.GroupTimeKey>=@groupTimeKey
  group by t.GroupTimeKey,t.[Time],t1.PlateId
  union all 
  select t1.PlateId,2 WeightType,t.GroupTimeKey,t.[Time],SUM(t.OpenedPrice*t.TotalCapital)TotalOpenedPrice,SUM(t.ClosedPrice*t.TotalCapital)TotalClosedPrice,SUM(t.PreClosePrice*t.TotalCapital)TotalPreClosePrice,
  {4}SUM(t.MinPrice*t.TotalCapital)TotalMinPrice,SUM(t.MaxPrice*t.TotalCapital)TotalMaxPrice,SUM(t.TradeStock)TotalTradeStock,SUM(t.TradeAmount)TotalTradeAmount,
  SUM(t.LastTradeStock)TotalLastTradeStock,SUM(t.LastTradeAmount)TotalLastTradeAmount,count(*)CalCount,convert(bit,1) IsUpdate
  from {2} t with(nolock)
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode
	  from t_shares_plate_rel_snapshot t with(nolock)
	  inner join 
	  (
		  select * from t_shares_plate with(nolock) where BaseDate is not null and BaseDate<convert(varchar(10),getdate(),120)
	  )
	  t2 on t.PlateId=t2.Id
	  where t.[Date]=@date
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode
  where t.GroupTimeKey>=@groupTimeKey
  group by t.GroupTimeKey,t.[Time],t1.PlateId", groupTimeKey, DateTime.Now.Date, tableName, dataType == 2 ? "SUM(t.YestodayClosedPrice) TotalYestodayClosedPrice," : "", dataType == 2 ? "SUM(t.YestodayClosedPrice*t.TotalCapital) TotalYestodayClosedPrice," : "");
               
                    using (var db = new meal_ticketEntities())
                    {
                        try
                        {
                            db.Database.CommandTimeout = 600;
                            var resultList = db.Database.SqlQuery<PlateKlineSession>(sql).ToList();
                            var resultGroup = resultList.GroupBy(e => e.PlateId).ToDictionary(k => k.Key, v => v.ToList());
                            foreach (var item in resultGroup)
                            {
                                SetPlateKlineSession(item.Key*100 + dataType, item.Value.ToDictionary(k=>k.GroupTimeKey*10+k.WeightType,v=>v));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("计算结果缓存加载错误,datatype="+ dataType, ex);
                            throw ex;
                        }
                    }
                });
            }
            Task.WaitAll(taskArr);
            Logger.WriteFileLog("===结束加载板块计算结果缓存===", null);
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
        Dictionary<long, List<long>> SharesPlateSession=new Dictionary<long, List<long>>();

        /// <summary>
        /// 获取今日快照缓存
        /// </summary>
        /// <returns></returns>
        private List<long> GetSharesPlateSession(long key)
        {
            List<long> plateId=new List<long>();
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
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                db.Database.CommandTimeout = 600;
                var list = (from item in db.t_shares_plate_rel_snapshot
                            where item.Date == dateNow
                            group item by new { item.Market, item.SharesCode } into g
                            select g).ToList();
                foreach (var item in list)
                {
                    SetSharesPlateSession(long.Parse(item.Key.SharesCode) * 10 + item.Key.Market, item.Select(e => e.PlateId).ToList());
                }
                var list2 = (from item in db.t_shares_plate_rel_snapshot
                            where item.Date == dateNow
                            group item by item.PlateId into g
                            select g).ToList();
                List<int> dataTypeList = Singleton.Instance.SecurityBarsDataTypeList;
                foreach (var item in list2)
                {
                    foreach (int dataType in dataTypeList)
                    {
                        Dictionary<long, SharesKlineData> dic = new Dictionary<long, SharesKlineData>();
                        foreach (var share in item)
                        {
                            long key = long.Parse(share.SharesCode) * 1000 + share.Market * 100 + dataType;
                            dic.Add(key, GetSharesKlineLastSession(key));
                        }
                        SetSharesByPlateSession(item.Key * 100 + dataType, dic);
                    }
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

        #region===股票最后一条数据缓存===
        /// <summary>
        /// 股票SharesCode*1000+Market*100+DataType=>K线信息
        /// </summary>
        Dictionary<long, SharesKlineData> SharesKlineLastSession = new Dictionary<long, SharesKlineData>();

        /// <summary>
        /// 获取股票最后一条数据缓存
        /// </summary>
        /// <returns></returns>
        public SharesKlineData GetSharesKlineLastSession(long key)
        {
            SharesKlineData temp = null;
            if (SharesKlineLastSession.ContainsKey(key))
            {
                temp = SharesKlineLastSession[key];
            }
            return temp;
        }

        /// <summary>
        /// 设置股票最后一条数据缓存
        /// </summary>
        public void SetSharesKlineLastSession(long key, SharesKlineData data)
        {
            SharesKlineLastSession[key] = data;
        }

        /// <summary>
        /// 初始化最后一条数据缓存
        /// </summary>
        public void LoadSecurityBarsLastData()
        {
            Logger.WriteFileLog("===开始初始化最后一条数据缓存===", null);
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
                            string sql = @"select t.Market,t.SharesCode,t.GroupTimeKey,t.PreClosePrice,t.LastTradeStock,t.LastTradeAmount,t.ClosedPrice,t.TradeStock,t.MaxPrice,t.MinPrice,t.OpenedPrice,
  t.[Time],t.TotalCapital,t.Tradable,t.TradeAmount,t.YestodayClosedPrice 
from t_shares_securitybarsdata_1min t with(nolock)
inner join 
(
	select Market,SharesCode,Max(GroupTimeKey)GroupTimeKey
	from t_shares_securitybarsdata_1min with(nolock)
	where [Time]<convert(varchar(10),dateadd(DAY,1,getdate()),120)
	group by Market,SharesCode
)t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode and t.GroupTimeKey=t1.GroupTimeKey";
                            lastData = db.Database.SqlQuery<SharesKlineData>(sql).ToList();
                        }
                        else
                        {
                            string sql = string.Format(@"select t.Market,t.SharesCode,t.GroupTimeKey,t.PreClosePrice,t.LastTradeStock,t.LastTradeAmount,t.ClosedPrice,t.TradeStock,t.MaxPrice,t.MinPrice,t.OpenedPrice,
  t.[Time],t.TotalCapital,t.Tradable,t.TradeAmount
from {0} t with(nolock)
inner join 
(
	select Market,SharesCode,Max(GroupTimeKey)GroupTimeKey
	from {0} with(nolock)
	where [Time]<convert(varchar(10),dateadd(DAY,1,getdate()),120)
	group by Market,SharesCode
)t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode and t.GroupTimeKey=t1.GroupTimeKey", tableName);
                            lastData = db.Database.SqlQuery<SharesKlineData>(sql).ToList();
                        }
                    }
                    foreach (var item in lastData)
                    {
                        SetSharesKlineLastSession(item.SharesCodeNum * 1000 + item.Market * 100 + dataType, item);
                    }
                });
            }
            Task.WaitAll(taskArr);
            Logger.WriteFileLog("===结束初始化最后一条数据缓存===", null);
        }
        #endregion

        /// <summary>
        /// 股票K线数据队列
        /// </summary>
        private ThreadMsgTemplate<SharesKlineDataContain> SharesKlineQueue;

        /// <summary>
        /// k线计算线程
        /// </summary>
        public Thread KlineThread;

        /// <summary>
        /// 每天初始化等待队列
        /// </summary>
        private ThreadMsgTemplate<int> DailyInitWaitQueue;

        /// <summary>
        /// 每天初始化线程
        /// </summary>
        public Thread DailyInitThread;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init() 
        {
            SharesKlineQueue = new ThreadMsgTemplate<SharesKlineDataContain>();
            SharesKlineQueue.Init();
            DailyInitWaitQueue = new ThreadMsgTemplate<int>();
            DailyInitWaitQueue.Init();

            DoBusiness();
            DoTask();
        }

        /// <summary>
        /// 每天任务
        /// </summary>
        private void DoTask() 
        {
            DailyInitThread = new Thread(()=>
            { 
                DateTime TaskSuccessLastTime = DateTime.Parse("1990-01-01");
                while (true)
                {
                    try 
                    {
                        int obj = 0;
                        if (DailyInitWaitQueue.WaitMessage(ref obj, 600000))
                        {
                            break;
                        }
                        TimeSpan spanNow = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                        if (spanNow < TimeSpan.Parse("00:30:00") || spanNow > TimeSpan.Parse("05:30:00"))
                        {
                            continue;
                        }
                        if (DateTime.Now.Date <= TaskSuccessLastTime)
                        {
                            continue;
                        }
                        LoadPlateSession();
                        UpdateTodayPlateRelSnapshot();
                        LoadPlateKlineSession();
                        TaskSuccessLastTime = DateTime.Now;
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
        /// 业务处理
        /// </summary>
        private void DoBusiness()
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
                        Logger.WriteFileLog("===1.收到数据,数据类型:" + data.DataType + "，开始处理,总数量" + data.SharesKlineData.Count() + "===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);

                        ToHandler(data);

                        Logger.WriteFileLog("===2.处理完成，开始导入===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                        InsertToDatabase();
                        Logger.WriteFileLog("===3.导入完成===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                        Logger.WriteFileLog("============================", null);
                        Logger.WriteFileLog("============================", null);
                        Logger.WriteFileLog("============================", null);
                        Logger.WriteFileLog("============================", null);
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
        /// 处理数据
        /// </summary>
        private void ToHandler(SharesKlineDataContain data)
        {
            foreach (var item in data.SharesKlineData)
            {
                //2查询原最后一条股票数据缓存
                long key = item.SharesCodeNum * 1000 + item.Market * 100 + data.DataType;
                var lastSharesData = GetSharesKlineLastSession(key);
                //设置股票最后一调数据缓存
                SetSharesKlineLastSession(key, item);

                //日K以上不需要计算
                //if (data.DataType > 7)
                //{
                //    continue;
                //}
                //1.查询股票所属板块
                List<long> plateIdArr = new List<long>();
                plateIdArr = GetSharesPlateSession(item.SharesCodeNum * 10 + item.Market);
                if (plateIdArr.Count()<=0)
                {
                    continue;
                }

                //3.板块循环
                foreach (var plateId in plateIdArr)
                {
                    CalPlateKlineResult(item, plateId, data.DataType, lastSharesData);
                }
            }
        }

        /// <summary>
        /// 计算板块k线结果
        /// </summary>
        private void CalPlateKlineResult(SharesKlineData data,long plateId,int dataType,SharesKlineData lastSharesData)
        {
            if (lastSharesData != null && lastSharesData.GroupTimeKey > data.GroupTimeKey)
            {
                return;
            }
            //1.查询原板块计算结果
            var calResult = GetPlateKlineSession(plateId*100+dataType);
            if (!_SharesPlateSessionDic.ContainsKey(plateId))
            {
                return;
            }
            var plate = _SharesPlateSessionDic[plateId];
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

            var sharesDic = GetSharesByPlateSession(plateId * 100 + dataType);

            #region===不加权===
            if (isCalWeight)
            {
                if (!calResult.ContainsKey(data.GroupTimeKey*100+1))
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
                        IsUpdate=true
                    };
                    //计算该板块所有股票
                    foreach (var item in sharesDic)
                    {
                        SharesKlineData last = new SharesKlineData();
                        if (item.Key == data.SharesCodeNum * 1000+ data.Market*100 +dataType)
                        {
                            last = lastSharesData;
                        }
                        else
                        {
                            last = item.Value;
                        }
                        if (last != null)
                        {
                            temp.CalCount = temp.CalCount + 1;
                            temp.TotalLastTradeStock = temp.TotalLastTradeStock + last.LastTradeStock;
                            temp.TotalLastTradeAmount = temp.TotalLastTradeAmount + last.LastTradeAmount;
                            temp.TotalTradeStock = temp.TotalTradeStock + last.TradeStock;
                            temp.TotalTradeAmount = temp.TotalTradeAmount + last.TradeAmount;
                            temp.TotalClosedPrice = temp.TotalClosedPrice + last.ClosedPrice;
                            temp.TotalMaxPrice = temp.TotalMaxPrice + last.MaxPrice;
                            temp.TotalMinPrice = temp.TotalMinPrice + last.MinPrice;
                            temp.TotalOpenedPrice = temp.TotalOpenedPrice + last.OpenedPrice;
                            temp.TotalPreClosePrice = temp.TotalPreClosePrice + last.PreClosePrice;
                            temp.TotalYestodayClosedPrice = temp.TotalYestodayClosedPrice + last.YestodayClosedPrice;
                            temp.IsUpdate = true;
                        }
                    }
                    calResult.Add(data.GroupTimeKey*100+1,temp);
                }
                else
                {
                    PlateKlineSession noWeightResult = calResult[data.GroupTimeKey * 100 + 1];
                    if (lastSharesData == null)
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
                        noWeightResult.CalCount = noWeightResult.CalCount + 1;
                        noWeightResult.IsUpdate = true;
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
                        noWeightResult.IsUpdate = true;
                    }
                }
            }
            #endregion

            #region===加权===
            if (isCalNoWeight)
            {
                if (!calResult.ContainsKey(data.GroupTimeKey*100+2))
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
                        IsUpdate=true
                    };
                    //计算该板块所有股票
                    foreach (var item in sharesDic)
                    {
                        SharesKlineData last = new SharesKlineData();
                        if (item.Key == data.SharesCodeNum * 1000 + data.Market * 100 + dataType)
                        {
                            last = lastSharesData;
                        }
                        else
                        {
                            last = item.Value;
                        }
                        if (last != null)
                        {
                            temp.CalCount = temp.CalCount + 1;
                            temp.TotalLastTradeStock = temp.TotalLastTradeStock + last.LastTradeStock;
                            temp.TotalLastTradeAmount = temp.TotalLastTradeAmount + last.LastTradeAmount;
                            temp.TotalTradeStock = temp.TotalTradeStock + last.TradeStock;
                            temp.TotalTradeAmount = temp.TotalTradeAmount + last.TradeAmount;
                            temp.TotalClosedPrice = temp.TotalClosedPrice + last.ClosedPrice * last.TotalCapital;
                            temp.TotalMaxPrice = temp.TotalMaxPrice + last.MaxPrice * last.TotalCapital;
                            temp.TotalMinPrice = temp.TotalMinPrice + last.MinPrice * last.TotalCapital;
                            temp.TotalOpenedPrice = temp.TotalOpenedPrice + last.OpenedPrice * last.TotalCapital;
                            temp.TotalPreClosePrice = temp.TotalPreClosePrice + last.PreClosePrice * last.TotalCapital;
                            temp.TotalYestodayClosedPrice = temp.TotalYestodayClosedPrice + last.YestodayClosedPrice * last.TotalCapital;
                            temp.IsUpdate = true;
                        }
                    }
                    calResult.Add(data.GroupTimeKey*100+2, temp);
                }
                else
                {
                    var weightResult = calResult[data.GroupTimeKey * 100 + 2];
                    if (lastSharesData == null)
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
                        weightResult.CalCount = weightResult.CalCount + 1;
                        weightResult.IsUpdate = true;
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
                        weightResult.IsUpdate = true;
                    }
                }
            }
            #endregion

            //设置结果集
            SetPlateKlineSession(plateId * 100 + dataType, calResult);
        }

        /// <summary>
        /// 结果数据入库
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
                var tempData = GetPlateKlineSession(key).Values.ToList();

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
            if (BulkData(dataDic))
            {
                //RemovePlateKlineSession(tempDic);
                ResetPlateKlineSessionIsUpdate();
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
                        var tempValue = item.Value.Where(e => e.IsUpdate == true).ToList();
                        if (tempValue.Count() <= 0)
                        {
                            continue;
                        }
                        try
                        {
                            _bulkData(item.Key, tempValue, db);
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
        private void _bulkData(int dataType, List<PlateKlineSession> list, meal_ticketEntities db)
        {
            list = (from item in list
                    group item by new { item.PlateId, item.WeightType, item.GroupTimeKey } into g
                    select g.FirstOrDefault()).ToList();

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
            table.Columns.Add("BasePrice", typeof(long));
            table.Columns.Add("CalCount", typeof(int));

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
                var baseDateInfo = (from x in Singleton.Instance._SharesPlateSession.GetSessionData()
                                    where x.PlateId == item.PlateId && x.BaseDate != null && x.BaseDate < DateTime.Now && x.BaseDateNoWeightPrice>0 && x.BaseDateWeightPrice>0
                                    select new PlateBaseDataSession
                                    {
                                        PlateId = x.PlateId,
                                        BaseDate = x.BaseDate.Value,
                                        NoWeightPrice = x.BaseDateNoWeightPrice,
                                        WeightPrice = x.BaseDateWeightPrice
                                    }).FirstOrDefault();

                if (baseDateInfo == null)
                {
                    continue;
                }

                long basePrice = item.WeightType == 1 ? baseDateInfo.NoWeightPrice : baseDateInfo.WeightPrice;

                DataRow row = table.NewRow();
                row["WeightType"] = item.WeightType;
                row["PlateId"] = item.PlateId;
                row["Market"] = 0;
                row["SharesCode"] = "";
                row["GroupTimeKey"] = item.GroupTimeKey;
                row["Time"] = item.Time;
                row["OpenedPrice"] = item.TotalOpenedPrice;
                row["ClosedPrice"] = item.TotalClosedPrice;
                row["PreClosePrice"] = item.TotalPreClosePrice;
                row["YestodayClosedPrice"] = item.TotalYestodayClosedPrice;
                row["MinPrice"] = item.TotalMinPrice;
                row["MaxPrice"] = item.TotalMaxPrice;
                row["TradeStock"] = item.TotalTradeStock;
                row["TradeAmount"] = item.TotalTradeAmount;
                row["LastTradeStock"] = item.TotalLastTradeStock;
                row["LastTradeAmount"] = item.TotalLastTradeAmount;
                row["Tradable"] = 0;
                row["TotalCapital"] = 0;
                row["IsLast"] = (item.WeightType == 2 && item.GroupTimeKey == weightMaxGroupKey) || (item.WeightType == 1 && item.GroupTimeKey == noWeightMaxGroupKey) ? true : false;
                row["LastModified"] = DateTime.Now;
                row["BaseDate"] = baseDateInfo.BaseDate;
                row["BasePrice"] = basePrice;
                row["CalCount"] = item.CalCount;
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
            if (DailyInitThread != null)
            {
                DailyInitWaitQueue.AddMessage(-1);
                DailyInitThread.Join();
                DailyInitWaitQueue.Release();
            }
        }
    }
}
