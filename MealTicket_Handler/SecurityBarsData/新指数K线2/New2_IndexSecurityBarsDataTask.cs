using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Handler.RunnerHandler;
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
    public class New2_IndexSecurityBarsDataTask
    {
        #region===所有板块字典缓存===
        /// <summary>
        /// 板块字典缓存
        /// key=PlateId
        /// </summary>
        private Dictionary<long, SharesPlateInfo_Session> PlateSessionDic = new Dictionary<long, SharesPlateInfo_Session>();

        /// <summary>
        /// 获取某个板块数据
        /// </summary>
        /// <returns></returns>
        private SharesPlateInfo_Session GetPlateSessionValue(long key) 
        {
            if (!PlateSessionDic.ContainsKey(key))
            {
                return null;
            }
            return PlateSessionDic[key];
        }

        /// <summary>
        /// 加载板块字典缓存
        /// </summary>
        public void LoadPlateSession()
        {
            var plateList = GetPlateData();//所有板块列表
            PlateSessionDic = plateList.ToDictionary(k => k.PlateId, v => v);//所有板块字典
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
	where [Time]<convert(varchar(10),dateadd(DAY,1,getdate()),120)
	group by PlateId,WeightType
)t1 on t.PlateId=t1.PlateId and t.WeightType=t1.WeightType and t.GroupTimeKey=t1.GroupTimeKey", tableName);
                    lastData = db.Database.SqlQuery<PlateImportData>(sql).ToList();

                }
                foreach (var item in lastData)
                {
                    long key = item.PlateId * 1000 + item.WeightType * 100 + dataType;
                    SetPlateKlineLastSession(key, item);
                }
            }
        }
        #endregion

        #region===板块计算结果缓存===
        /// <summary>
        /// 板块Id*100+K线类型=>计算结果
        /// GroupTimeKey*10+WeightType
        /// </summary>
        private Dictionary<long, Dictionary<long, PlateKlineSession>> PlateKlineSessionDic = new Dictionary<long, Dictionary<long, PlateKlineSession>>();

        /// <summary>
        /// 获取所有键
        /// </summary>
        /// <returns></returns>
        private List<long> GetPlateKlineSessionAllKey()
        {
            List<long> temp = PlateKlineSessionDic.Keys.ToList();
            return temp;
        }

        /// <summary>
        /// 重置是否更新
        /// </summary>
        private void ResetPlateKlineSessionIsUpdate()
        {
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
        }

        /// <summary>
        /// 获取板块计算结果缓存
        /// </summary>
        /// <returns></returns>
        private Dictionary<long, PlateKlineSession> GetPlateKlineSession(long key)
        {
            Dictionary<long, PlateKlineSession> temp = new Dictionary<long, PlateKlineSession>();
            if (PlateKlineSessionDic.ContainsKey(key))
            {
                temp = PlateKlineSessionDic[key];
            }
            return temp;
        }

        /// <summary>
        /// 设置板块计算结果缓存
        /// </summary>
        private void SetPlateKlineSession(long key, Dictionary<long, PlateKlineSession> data)
        {
            PlateKlineSessionDic[key] = data;
        }

        /// <summary>
        /// 更新板块计算结果缓存
        /// </summary>
        /// <returns></returns>
        public void LoadPlateKlineSession()
        {
            PlateKlineSessionDic.Clear();
            Logger.WriteFileLog("===开始加载板块计算结果缓存===", null);
            List<int> dataTypeList = GetPlateDataType();

            for (int i = 0; i < dataTypeList.Count; i++)
            {
                int dataType = dataTypeList[i];

                string tableName = "";
                if (!ParseTableName(dataType, ref tableName))
                {
                    continue;
                }

                long groupTimeKey = 0;
                if (!ParseTimeGroupKey(DateTime.Now.Date, dataType, ref groupTimeKey))
                {
                    continue;
                }

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
  from {2} t
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode
	  from t_shares_plate_rel_snapshot t with(nolock)
	  where t.[Date]='{1}'
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode
  inner join v_shares_quotes_last t2 on t.Market=t2.Market and t.SharesCode=t2.SharesCode 
  where t.GroupTimeKey>={0}
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
  from {2} t
  inner join
  (
	  select t.PlateId,t.Market,t.SharesCode
	  from t_shares_plate_rel_snapshot t with(nolock)
	  where t.[Date]='{1}'
  )t1 on t.Market=t1.Market and t.SharesCode=t1.SharesCode
  inner join v_shares_quotes_last t2 on t.Market=t2.Market and t.SharesCode=t2.SharesCode 
  where t.GroupTimeKey>={0}
  group by t.GroupTimeKey,t1.PlateId", groupTimeKey, DateTime.Now.Date.ToString("yyyy-MM-dd"), tableName);

                using (var db = new meal_ticketEntities())
                {
                    try
                    {
                        db.Database.CommandTimeout = 600;
                        var resultList = db.Database.SqlQuery<PlateKlineSession>(sql).ToList();

                        Dictionary<long, List<PlateKlineSession>> resultGroup = new Dictionary<long, List<PlateKlineSession>>();
                        foreach (var item in resultList)
                        {
                            var lastData = GetPlateKlineLastSessionValue(item.PlateId * 1000 + item.WeightType * 100 + dataType);
                            if (lastData != null && lastData.GroupTimeKey > item.GroupTimeKey)
                            {
                                item.IsUpdate = false;
                            }
                            if (resultGroup.ContainsKey(item.PlateId))
                            {
                                resultGroup[item.PlateId].Add(item);
                            }
                            else
                            {
                                resultGroup.Add(item.PlateId, new List<PlateKlineSession> { item });
                            }
                        }
                        foreach (var item in resultGroup)
                        {
                            SetPlateKlineSession(item.Key * 100 + dataType, item.Value.ToDictionary(k => k.GroupTimeKey * 10 + k.WeightType, v => v));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("计算结果缓存加载错误,datatype=" + dataType, ex);
                        throw ex;
                    }
                }
            }
            Logger.WriteFileLog("===结束加载板块计算结果缓存===", null);
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
            if (!SharesKlineLastSession.ContainsKey(key))
            {
                SharesKlineLastSession.Add(key, new SharesKlineData());
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
        public void LoadSecurityBarsLastData()
        {
            SharesKlineLastSession.Clear();
            Logger.WriteFileLog("===开始初始化最后一条数据缓存===", null);
            List<int> dataTypeList = Singleton.Instance.SecurityBarsDataTypeList;

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
                using (var db = new meal_ticketEntities())
                {
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
                }
                foreach (var item in lastData)
                {
                    SetSharesKlineLastSession(item.SharesCodeNum * 1000 + item.Market * 100 + dataType, item);
                }
            }
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
                            select item).ToList().GroupBy(e=>new { e.Market,e.SharesCode}).ToDictionary(k=>int.Parse(k.Key.SharesCode)*10+k.Key.Market,v=>v.Select(e=>e.PlateId).ToList());
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
            SharesKlineQueue = new ThreadMsgTemplate<SharesKlineDataContain>();
            SharesKlineQueue.Init();
            DailyInitWaitQueue = new ThreadMsgTemplate<int>();
            DailyInitWaitQueue.Init();
            DoTask();
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
                while (true)
                {
                    try
                    {
                        int obj = 0;
                        if (DailyInitWaitQueue.WaitMessage(ref obj, Singleton.Instance.SecurityBarsIntervalTime))
                        {
                            break;
                        }
                        DoDailyTask(ref TaskSuccessLastTime);
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
        private void DoDailyTask(ref DateTime TaskSuccessLastTime)
        {
            TimeSpan spanNow = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
            if (spanNow < TimeSpan.Parse("00:30:00") || spanNow > TimeSpan.Parse("05:30:00"))
            {
                return;
            }
            if (DateTime.Now.Date <= TaskSuccessLastTime)
            {
                return;
            }
            //更新板块数据
            LoadPlateSession();

            //指令数据执行
            DoExcuteOrder();

            //加载板块最后一条K线数据
            LoadPlateKlineLastSession();

            //补全数据
            DoRealTimeTask_Cal_His();

            //加载其他缓存
            LoadSecurityBarsLastData();
            //加载板块最后一条K线数据
            LoadPlateKlineLastSession();
            UpdateTodayPlateRelSnapshot();
            LoadPlateKlineSession();
            TaskSuccessLastTime = DateTime.Now;
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
                              select item).ToList();
                foreach (var item in result)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            item.IsExcute = true;
                            item.ExcuteTime = DateTime.Now;
                            db.SaveChanges();

                            //重置基准日
                            if (item.Type == 1)
                            {
                                var context = JsonConvert.DeserializeObject<dynamic>(item.Context);
                                DateTime baseDate = Convert.ToDateTime(context.Date);
                                baseDate=DbHelper.GetLastTradeDate2(0, 0, 0, 1, baseDate);
                                var plate=GetPlateSessionValue(item.PlateId);
                                if (plate == null)
                                {
                                    tran.Commit();
                                    continue;
                                }
                                long initIndex = Singleton.Instance.IndexInitValueDic[plate.CalType];

                                #region===删除所有K线数据===
                                //1.1分钟K线
                                string sql = string.Format(@"delete t_shares_plate_securitybarsdata_1min where PlateId={0};",item.PlateId);
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1min
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(1,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd0931")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1min
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(2,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd0931")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                //2.5分钟K线
                                sql = string.Format(@"delete t_shares_plate_securitybarsdata_5min where PlateId={0};", item.PlateId);
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_5min
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(1,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd0935")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_5min
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(2,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd0935")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                //3.15分钟K线
                                sql = string.Format(@"delete t_shares_plate_securitybarsdata_15min where PlateId={0};", item.PlateId);
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_15min
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(1,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd0945")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_15min
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(2,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd0945")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                //4.30分钟K线
                                sql = string.Format(@"delete t_shares_plate_securitybarsdata_30min where PlateId={0};", item.PlateId);
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_30min
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(1,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd1000")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_30min
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(2,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd1000")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                //5.60分钟K线
                                sql = string.Format(@"delete t_shares_plate_securitybarsdata_60min where PlateId={0};", item.PlateId);
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_60min
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(1,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd1030")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_60min
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(2,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd1030")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                //6.日K线
                                sql = string.Format(@"delete t_shares_plate_securitybarsdata_1day where PlateId={0};", item.PlateId);
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1day
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(1,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1day
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(2,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMMdd")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                //7.周K线
                                sql = string.Format(@"delete t_shares_plate_securitybarsdata_1week where PlateId={0};", item.PlateId);
                                db.Database.ExecuteSqlCommand(sql);
                                long groupTimeKey = 0;
                                ParseTimeGroupKey(baseDate, 8, ref groupTimeKey);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1week
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(1,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, groupTimeKey, baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1week
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(2,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, groupTimeKey, baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                //8.月K线
                                sql = string.Format(@"delete t_shares_plate_securitybarsdata_1month where PlateId={0};", item.PlateId);
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1month
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(1,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMM")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1month
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(2,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyyMM")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                //9.季度K线
                                sql = string.Format(@"delete t_shares_plate_securitybarsdata_1quarter where PlateId={0};", item.PlateId);
                                db.Database.ExecuteSqlCommand(sql);
                                groupTimeKey = 0;
                                ParseTimeGroupKey(baseDate, 10, ref groupTimeKey);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1quarter
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(1,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, groupTimeKey, baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1quarter
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(2,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, groupTimeKey, baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                //10.年K线
                                sql = string.Format(@"delete t_shares_plate_securitybarsdata_1year where PlateId={0};", item.PlateId);
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1year
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(1,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyy")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                sql = string.Format(@"insert into t_shares_plate_securitybarsdata_1year
  (WeightType, PlateId, Market, SharesCode, GroupTimeKey,[Time], OpenedPrice, ClosedPrice, PreClosePrice, YestodayClosedPrice, MinPrice, MaxPrice, TradeStock, TradeAmount,
  LastTradeStock, LastTradeAmount, Tradable, TotalCapital, IsLast, LastModified)
  values(2,{0},0,'',{1},'{2}',{3},{3},{3},{3},{3},{3},0,0,0,0,0,0,0,'{4}');", item.PlateId, long.Parse(baseDate.ToString("yyyy")), baseDate.ToString("yyyy-MM-dd 09:31:00"), initIndex, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                db.Database.ExecuteSqlCommand(sql);
                                #endregion
                            }

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("执行指令出错",ex);
                            tran.Rollback();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 指定代码，去券商直接获取板块K线数据
        /// </summary>
        private void DoRealTimeTask_Push() 
        {
            var plateDataType = GetPlateDataType();//K线类型列表
            var plateList = GetPlateData();//板块列表

            Dictionary<int, List<dynamic>> DataDic = new Dictionary<int, List<dynamic>>();
            foreach (var item in plateList)
            {
                if (string.IsNullOrEmpty(item.WeightSharesCode) && string.IsNullOrEmpty(item.NoWeightSharesCode))
                {
                    continue;
                }
                foreach (var dataType in plateDataType)
                {
                    if (!string.IsNullOrEmpty(item.WeightSharesCode))
                    {
                        long key= item.PlateId * 1000 + 200 + dataType;
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
                            PlateId = item.PlateId,
                            SharesCode = item.WeightSharesCode,
                            Market = item.WeightMarket,
                            WeightType = 2,
                            StartTimeKey = plateLast == null ? tempGroupTimeKey : plateLast.GroupTimeKey,
                            EndTimeKey = -1,
                            PreClosePrice = plateLast == null ? 0 : plateLast.PreClosePrice,
                            YestodayClosedPrice = plateLast == null ? 0 : plateLast.YestodayClosedPrice,
                            LastTradeStock = plateLast == null ? 0 : plateLast.LastTradeStock,
                            LastTradeAmount = plateLast == null ? 0 : plateLast.LastTradeAmount
                        });
                    }
                    if (!string.IsNullOrEmpty(item.NoWeightSharesCode))
                    {
                        long key = item.PlateId * 1000 + 100 + dataType;
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
                            PlateId = item.PlateId,
                            SharesCode = item.NoWeightSharesCode,
                            Market = item.NoWeightMarket,
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
            DoRealTimeTask_Push();
            var lastDate=DbHelper.GetLastTradeDate2(0, 0, 0, -1);
            lastDate = lastDate.AddHours(15);

            ThreadMsgTemplate<PlateImportData> data = new ThreadMsgTemplate<PlateImportData>();
            data.Init();
            Dictionary<long, PlateImportData> lastKLineData = new Dictionary<long, PlateImportData>(PlateKlineLastSessionDic);
            foreach (var item in lastKLineData)
            {
                PlateImportData tempData = item.Value;
                if (tempData.Time >= lastDate)
                {
                    continue;
                }
                //PlateId*1000+WeightType*100+DataType
                tempData.DataType = (int)(item.Key % 100);
                if (tempData.DataType > 7)
                {
                    continue;
                }
                data.AddMessage(item.Value);
            }

            int taskCount = 32;
            Task[] taskArr = new Task[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                taskArr[i] = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        do
                        {
                            PlateImportData tempData = new PlateImportData();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }
                            string tableName = "";
                            if (!ParseTableName(tempData.DataType, ref tableName))
                            {
                                break;
                            }

                            var lastTime = tempData.Time.Date;
                            long plateId = tempData.PlateId;

                            long groupTimeKey = tempData.GroupTimeKey;
                            long yesClosePrice = tempData.YestodayClosedPrice;
                            long preClosePrice = tempData.PreClosePrice;
                            using (var db = new meal_ticketEntities())
                            {
                                while (true)
                                {
                                    if (lastTime > lastDate.Date)
                                    {
                                        break;
                                    }
                                    using (var tran = db.Database.BeginTransaction())
                                    {
                                        try
                                        {
                                            List<PlateKlineSession> datalist = new List<PlateKlineSession>();
                                            string sql = "";
                                            if (tempData.WeightType == 1)
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
  group by t.GroupTimeKey,t1.PlateId", tableName, lastTime.ToString("yyyy-MM-dd"), plateId, groupTimeKey, lastTime.AddDays(1).ToString("yyyy-MM-dd"));

                                            }
                                            else if (tempData.WeightType == 2)
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
  group by t.GroupTimeKey,t1.PlateId", tableName, lastTime.ToString("yyyy-MM-dd"), plateId, groupTimeKey, lastTime.AddDays(1).ToString("yyyy-MM-dd"));
                                            }
                                            else
                                            {
                                                break;
                                            }
                                            datalist = db.Database.SqlQuery<PlateKlineSession>(sql).ToList();
                                            //if (datalist.Count() <= 0)
                                            //{
                                            //    break;
                                            //}
                                            datalist = datalist.OrderBy(e => e.GroupTimeKey).ToList();

                                            List<PlateImportData> tempValue = new List<PlateImportData>();
                                            foreach (var x in datalist)
                                            {
                                                //不加权
                                                if (tempData.WeightType == 1)
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
                                                else if (tempData.WeightType == 2)
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
                                            _bulkData(tempData.DataType, tempValue, db);
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
                                    lastTime = DbHelper.GetLastTradeDate2(0, 0, 0, 1, lastTime);
                                    if (!ParseTimeGroupKey(lastTime, tempData.DataType, ref groupTimeKey))
                                    {
                                        break;
                                    }
                                }
                            }
                        } while (true);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("某板块补全数据失败", ex);
                    }
                });
            }
            Task.WaitAll(taskArr);
            data.Release();
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
                            _calToday(data);
                            if (data.IsFinish)
                            {
                                InsertToDatabase();
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

                var lastSharesData = GetSharesKlineLastSession(key);
                //3.板块循环
                foreach (var plateId in plateIdArr)
                {
                    //设置板块内股票最后一条数据缓存
                    CalPlateKlineResult(item, plateId, data.DataType, lastSharesData);
                }
            }
        }

        /// <summary>
        /// 计算板块k线结果
        /// </summary>
        private void CalPlateKlineResult(SharesKlineData data, long plateId, int dataType, SharesKlineData lastSharesData)
        {
            if (lastSharesData != null && lastSharesData.GroupTimeKey > data.GroupTimeKey)
            {
                return;
            }

            //1.查询原板块计算结果
            var calResult = GetPlateKlineSession(plateId * 100 + dataType);
            var plate = GetPlateSessionValue(plateId);
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

            #region===不加权===
            if (isCalWeight)
            {
                if (!calResult.ContainsKey(data.GroupTimeKey * 10 + 1))
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
                            temp.TotalClosedPrice = temp.TotalClosedPrice + last.ClosedPriceRate;
                            temp.TotalMaxPrice = temp.TotalMaxPrice + last.MaxPriceRate;
                            temp.TotalMinPrice = temp.TotalMinPrice + last.MinPriceRate;
                            temp.TotalOpenedPrice = temp.TotalOpenedPrice + last.OpenedPriceRate;
                            temp.Tradable = temp.Tradable + last.Tradable;
                            temp.TotalCapital = temp.TotalCapital + last.TotalCapital;
                            temp.IsUpdate = true;

                            int key = int.Parse(last.SharesCode) * 10 + last.Market;
                            temp.LastTempData.Add(key, new SharesKlineData
                            {
                                LastTradeStock = last.LastTradeStock,
                                LastTradeAmount = last.LastTradeAmount,
                                TradeStock = last.TradeStock,
                                TradeAmount = last.TradeAmount,
                                ClosedPrice = last.ClosedPrice,
                                MaxPrice = last.MaxPrice,
                                MinPrice = last.MinPrice,
                                OpenedPrice = last.OpenedPrice,
                                PreClosePrice = last.PreClosePrice,
                                YestodayClosedPrice = last.YestodayClosedPrice,
                                Tradable = last.Tradable,
                                TotalCapital = last.TotalCapital,
                                GroupTimeKey = last.GroupTimeKey,
                                Time = last.Time,
                                WeightType = last.WeightType,
                                PlateId = plateId,
                                Market = last.Market,
                                SharesCode = last.SharesCode
                            });
                        }
                        else
                        {
                            temp.CalCount = temp.CalCount + 1;
                        }
                    }
                    calResult.Add(data.GroupTimeKey * 10 + 1, temp);
                }
                else
                {
                    PlateKlineSession noWeightResult = calResult[data.GroupTimeKey * 10 + 1];
                    int key = int.Parse(data.SharesCode) * 10 + data.Market;
                    SharesKlineData tempLast = new SharesKlineData();
                    if (noWeightResult.LastTempData == null)
                    {
                        tempLast = lastSharesData;
                        noWeightResult.LastTempData = new Dictionary<int, SharesKlineData>();
                    }
                    else if (noWeightResult.LastTempData.ContainsKey(key))
                    {
                        tempLast = noWeightResult.LastTempData[key];
                    }
                    else
                    {
                        tempLast = lastSharesData;
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

                    noWeightResult.LastTempData[key] = new SharesKlineData
                    {
                        LastTradeStock = data.LastTradeStock,
                        LastTradeAmount = data.LastTradeAmount,
                        TradeStock = data.TradeStock,
                        TradeAmount = data.TradeAmount,
                        ClosedPrice = data.ClosedPrice,
                        MaxPrice = data.MaxPrice,
                        MinPrice = data.MinPrice,
                        OpenedPrice = data.OpenedPrice,
                        PreClosePrice = data.PreClosePrice,
                        YestodayClosedPrice = data.YestodayClosedPrice,
                        Tradable = data.Tradable,
                        TotalCapital = data.TotalCapital,
                        GroupTimeKey = data.GroupTimeKey,
                        Time = data.Time,
                        WeightType = data.WeightType,
                        PlateId = data.PlateId,
                        Market = data.Market,
                        SharesCode = data.SharesCode
                    };
                }
            }
            #endregion

            #region===加权===
            if (isCalNoWeight)
            {
                if (!calResult.ContainsKey(data.GroupTimeKey * 10 + 2))
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
                            temp.Tradable = temp.Tradable + last.Tradable;
                            temp.TotalCapital = temp.TotalCapital + last.TotalCapital;
                            temp.IsUpdate = true;

                            int key = int.Parse(last.SharesCode) * 10 + last.Market;
                            temp.LastTempData.Add(key, new SharesKlineData
                            {
                                LastTradeStock = last.LastTradeStock,
                                LastTradeAmount = last.LastTradeAmount,
                                TradeStock = last.TradeStock,
                                TradeAmount = last.TradeAmount,
                                ClosedPrice = last.ClosedPrice,
                                MaxPrice = last.MaxPrice,
                                MinPrice = last.MinPrice,
                                OpenedPrice = last.OpenedPrice,
                                PreClosePrice = last.PreClosePrice,
                                YestodayClosedPrice = last.YestodayClosedPrice,
                                Tradable = last.Tradable,
                                TotalCapital = last.TotalCapital,
                                GroupTimeKey = last.GroupTimeKey,
                                Time = last.Time,
                                WeightType = last.WeightType,
                                PlateId = plateId,
                                Market = last.Market,
                                SharesCode = last.SharesCode
                            });
                        }
                        else
                        {
                            temp.CalCount = temp.CalCount + 1;
                        }
                    }
                    calResult.Add(data.GroupTimeKey * 10 + 2, temp);
                }
                else
                {
                    var weightResult = calResult[data.GroupTimeKey * 10 + 2];
                    int key = int.Parse(data.SharesCode) * 10 + data.Market;
                    SharesKlineData tempLast = new SharesKlineData();
                    if (weightResult.LastTempData == null)
                    {
                        tempLast = lastSharesData;
                        weightResult.LastTempData = new Dictionary<int, SharesKlineData>();
                    }
                    else if (weightResult.LastTempData.ContainsKey(key))
                    {
                        tempLast = weightResult.LastTempData[key];
                    }
                    else
                    {
                        tempLast = lastSharesData;
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

                    weightResult.LastTempData[key] = new SharesKlineData
                    {
                        LastTradeStock = data.LastTradeStock,
                        LastTradeAmount = data.LastTradeAmount,
                        TradeStock = data.TradeStock,
                        TradeAmount = data.TradeAmount,
                        ClosedPrice = data.ClosedPrice,
                        MaxPrice = data.MaxPrice,
                        MinPrice = data.MinPrice,
                        OpenedPrice = data.OpenedPrice,
                        PreClosePrice = data.PreClosePrice,
                        YestodayClosedPrice = data.YestodayClosedPrice,
                        Tradable = data.Tradable,
                        TotalCapital = data.TotalCapital,
                        GroupTimeKey = data.GroupTimeKey,
                        Time = data.Time,
                        WeightType = data.WeightType,
                        PlateId = data.PlateId,
                        Market = data.Market,
                        SharesCode = data.SharesCode
                    };
                }
            }
            #endregion

            //设置结果集
            SetPlateKlineSession(plateId * 100 + dataType, calResult);
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
                var tempData = GetPlateKlineSession(key).Values.Where(e=>e.IsUpdate==true).ToList();

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
                                    ClosedPrice = (long)((x.TotalClosedPrice * 1.0/x.CalCount / 10000 + 1) * yesClosePrice + 0.5),
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
                                    SharesCode="",
                                    Market=0
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
            var tempList=list.GroupBy(e => new { e.PlateId, e.WeightType }).ToList();
            foreach (var item in tempList)
            {
                long key = item.Key.PlateId * 1000 + item.Key.WeightType * 100 + dataType;
                SetPlateKlineLastSession(key,item.OrderByDescending(e=>e.GroupTimeKey).First());
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
        /// 获取所有板块列表
        /// </summary>
        /// <returns></returns>
        private List<SharesPlateInfo_Session> GetPlateData()
        {
            return Singleton.Instance._SharesPlateSession.GetSessionData();
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
        /// 执行重置操作
        /// </summary>
        public void ToExecuteRetry(int dataType) 
        { 

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
            if (DailyInitThread != null)
            {
                DailyInitWaitQueue.AddMessage(0);
                DailyInitThread.Join();
                DailyInitWaitQueue.Release();
            }
        }
    }
}
