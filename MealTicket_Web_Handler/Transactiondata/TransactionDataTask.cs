using FXCommon.Common;
using FXCommon.Database;
using MealTicket_Web_Handler.Model;
using MealTicket_Web_Handler.Runner;
using Newtonsoft.Json;
using stock_db_core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static StockTrendMonitor.Define.StockMonitorDefine;

namespace MealTicket_Web_Handler.Transactiondata
{
    /// <summary>
    /// 开盘收盘价
    /// </summary>
    public struct BASE_STOCK_TRADE_INFO
    {
        public int iBiddingTradePrice;
        public int iPreDayTradePrice;
        public DateTime dtRefreshTime;
    }

    public class TransactionDataTask
    {
        /// <summary>
        /// 开盘收盘价格缓存
        /// </summary>
        Dictionary<string, BASE_STOCK_TRADE_INFO> m_dicBaseInfo = new Dictionary<string, BASE_STOCK_TRADE_INFO>();

        /// <summary>
        /// 任务线程
        /// </summary>
        Thread TaskThread;

        /// <summary>
        /// task唯一Id
        /// </summary>
        public string TaskGuid;

        /// <summary>
        /// 任务开始时间
        /// </summary>
        public DateTime? StartTime;

        /// <summary>
        /// 股票分包数量
        /// </summary>
        public int TotalPacketCount;

        /// <summary>
        /// 目前回调包数量
        /// </summary>
        public int CallBackPacketCount;

        /// <summary>
        /// 目前收到所有数据包列表
        /// </summary>
        public List<TransactiondataAnalyseInfo> DataList;

        /// <summary>
        /// 分笔数据回调队列
        /// </summary>
        public ThreadMsgTemplate<TransactiondataTaskQueueInfo> TransactiondataQueue;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TransactionDataTask()
        {
            TransactiondataQueue = new ThreadMsgTemplate<TransactiondataTaskQueueInfo>();
            TransactiondataQueue.Init();
            InitLoadToMonitor();
            ClearTaskInfo();
            DoTask();
        }

        /// <summary>
        /// 销毁对象资源
        /// </summary>
        public void Dispose()
        {
            if (TaskThread != null)
            {
                TransactiondataQueue.AddMessage(new TransactiondataTaskQueueInfo
                {
                    TaskGuid = "-1"
                });
                TaskThread.Join();
            }
            if (TransactiondataQueue != null)
            {
                TransactiondataQueue.Release();
            }
        }

        /// <summary>
        /// 清除task信息
        /// </summary>
        public void ClearTaskInfo() 
        {
            TaskGuid = null;
            StartTime = null;
            TotalPacketCount = 0;
            CallBackPacketCount = 0;
            DataList = new List<TransactiondataAnalyseInfo>();
        }

        /// <summary>
        /// 需要更新分笔的股票数据扔到队列
        /// </summary>
        public bool PushToDataUpDate() 
        {
            try
            {
                DateTime timeNow = DateTime.Now;
                if (!Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.NewTransactionDataRunStartTime)) && !Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.NewTransactionDataRunEndTime)))
                {
                    return false;
                }
                var tempGuid = Guid.NewGuid().ToString("N");
                TotalPacketCount =RunnerHelper.SendTransactionShares(tempGuid);
                TaskGuid = tempGuid;
                StartTime = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("需要更新分笔的股票数据扔到队列失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        private void DoTask() 
        {
            TaskThread = new Thread(()=> 
            {
                int timeout = Singleton.Instance.NewTransactionDataSendPeriodTime;
                int taskTimeOut = Singleton.Instance.NewTransactiondataTaskTimeOut;
                do 
                {
                    DateTime waitTime = DateTime.Now;
                    TransactiondataTaskQueueInfo temp = new TransactiondataTaskQueueInfo();
                    if (!TransactiondataQueue.WaitMessage(ref temp, timeout))
                    {
                        timeout = Singleton.Instance.NewTransactionDataSendPeriodTime;

                        if (TaskGuid != null)//存在正在执行的任务
                        {
                            //判断当前任务是否超时
                            if ((DateTime.Now - StartTime.Value).TotalMilliseconds < taskTimeOut)//未超时
                            {
                                continue;
                            }
                            taskTimeOut = taskTimeOut * 2;
                            ClearTaskInfo();//超时则清空任务
                        }

                        PushToDataUpDate();
                        continue;
                    }



                    if (temp.TaskGuid == "-1")
                    {
                        break;
                    }

                    if (temp.TaskGuid != TaskGuid)
                    {
                        timeout = CalcElapsedTime(waitTime, timeout);
                        continue;
                    }

                    DataList.AddRange(temp.DataList);
                    CallBackPacketCount++;
                    if (CallBackPacketCount < TotalPacketCount)//接受包数量不正确
                    {
                        timeout = CalcElapsedTime(waitTime, timeout);
                        continue;
                    }
                    if (UpdateToDataBase())
                    {
                        taskTimeOut = Singleton.Instance.NewTransactiondataTaskTimeOut;
                        if (LoadToMonitor())
                        {
                            DataAnalyse();
                        }
                    }

                    ClearTaskInfo();
                    timeout = CalcElapsedTime(waitTime, timeout);
                } while (true);
            });
            TaskThread.Start();
        }

        private int CalcElapsedTime(DateTime dtTime, int timeout)
        {
            long elapsedTime = (long)(DateTime.Now - dtTime).TotalMilliseconds;
            return (elapsedTime >= timeout ? 0 : timeout - (int)elapsedTime);
        }

        /// <summary>
        /// 数据更新到数据库
        /// </summary>
        private bool UpdateToDataBase() 
        {
            try
            {
                string connectionString = Singleton.Instance.ConnectionString_meal_ticket_shares_transactiondata;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            string sql = string.Format("truncate table t_shares_transactiondata_time_analyse_temp");
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                            using (SqlTransaction tran = conn.BeginTransaction())
                            {
                                cmd.Transaction = tran;
                                try
                                {
                                    var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                                    using (DataTable table = new DataTable())
                                    {
                                        #region====定义表字段数据类型====
                                        table.Columns.Add("Id", typeof(long));
                                        table.Columns.Add("Market", typeof(int));
                                        table.Columns.Add("SharesCode", typeof(string));
                                        table.Columns.Add("SharesInfoNum", typeof(int));
                                        table.Columns.Add("Date", typeof(DateTime)); 
                                        table.Columns.Add("Time", typeof(DateTime));
                                        table.Columns.Add("iTradeStock", typeof(int));
                                        table.Columns.Add("lTradeAmount", typeof(long));
                                        table.Columns.Add("iOpeningPrice", typeof(long));
                                        table.Columns.Add("iLastestPrice", typeof(long));
                                        table.Columns.Add("iMinPrice", typeof(long));
                                        table.Columns.Add("iMaxPrice", typeof(long));
                                        table.Columns.Add("LastModified", typeof(DateTime));
                                        #endregion

                                        #region====绑定数据====
                                        foreach (var item in DataList)
                                        {
                                            DataRow row = table.NewRow();
                                            row["Id"] = 0;
                                            row["Market"] = item.Market;
                                            row["SharesCode"] = item.SharesCode;
                                            row["SharesInfoNum"] = item.SharesInfoNum;
                                            row["Date"] = item.Date;
                                            row["Time"] = item.Time;
                                            row["iTradeStock"] = item.iTradeStock;
                                            row["lTradeAmount"] = item.lTradeAmount;
                                            row["iOpeningPrice"] = item.iOpeningPrice;
                                            row["iLastestPrice"] = item.iLastestPrice;
                                            row["iMinPrice"] = item.iMinPrice;
                                            row["iMaxPrice"] = item.iMaxPrice;
                                            row["LastModified"] = DateTime.Now;
                                            table.Rows.Add(row);
                                        }
                                        #endregion

                                        #region====绑定结构字段====
                                        Dictionary<string, string> dic = new Dictionary<string, string>();
                                        dic.Add("Id", "Id");
                                        dic.Add("Market", "Market");
                                        dic.Add("SharesCode", "SharesCode");
                                        dic.Add("SharesInfoNum", "SharesInfoNum");
                                        dic.Add("Date", "Date");
                                        dic.Add("Time", "Time");
                                        dic.Add("iTradeStock", "iTradeStock");
                                        dic.Add("lTradeAmount", "lTradeAmount");
                                        dic.Add("iOpeningPrice", "iOpeningPrice");
                                        dic.Add("iLastestPrice", "iLastestPrice");
                                        dic.Add("iMinPrice", "iMinPrice");
                                        dic.Add("iMaxPrice", "iMaxPrice");
                                        dic.Add("LastModified", "LastModified");
                                        #endregion

                                        bulk.BulkCopyTimeout = 600;
                                        bulk.ColumnMappings = dic;
                                        bulk.BatchSize = 10000;

                                        bulk.BulkWriteToServer(conn, table, "t_shares_transactiondata_time_analyse_temp", tran);
                                    }


                                    //更新数据到分笔统计表
                                    sql = string.Format(@"merge into t_shares_transactiondata_time_analyse as t
using (select * from t_shares_transactiondata_time_analyse_temp) as t1
ON t.SharesInfoNum = t1.SharesInfoNum and t.[Time]=t1.[Time]
when matched
then update set t.iTradeStock=t1.iTradeStock,t.lTradeAmount=t1.lTradeAmount,t.iOpeningPrice=t1.iOpeningPrice,
t.iLastestPrice=t1.iLastestPrice,t.iMinPrice=t1.iMinPrice,t.iMaxPrice=t1.iMaxPrice,t.LastModified=t1.LastModified
when not matched by target
then insert(Market,SharesCode,SharesInfoNum,[Date],[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,iMinPrice,iMaxPrice,LastModified) 
values(t1.Market,t1.SharesCode,t1.SharesInfoNum,t1.[Date],t1.[Time],t1.iTradeStock,t1.lTradeAmount,t1.iOpeningPrice,t1.iLastestPrice,t1.iMinPrice,t1.iMaxPrice,t1.LastModified);");
                                    cmd.CommandText = sql;
                                    cmd.CommandTimeout = 600;
                                    cmd.ExecuteNonQuery();

                                    tran.Commit();
                                    return true;
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteFileLog("更新分笔统计数据意外出错", ex);
                                    tran.Rollback();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("更新分笔统计数据数据库连接出错", ex);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新分笔统计数据库失败", ex);
            }
            return false;
        }

        /// <summary>
        /// 走势分笔数据内存加载
        /// </summary>
        /// <returns></returns>
        private bool LoadToMonitor(List<TransactiondataAnalyseInfo> list=null)
        {
            try
            {
                var parList = new List<TransactiondataAnalyseInfo>();
                if (list == null)
                {
                    parList = JsonConvert.DeserializeObject<List<TransactiondataAnalyseInfo>>(JsonConvert.SerializeObject(DataList));
                }
                else
                {
                    parList = JsonConvert.DeserializeObject<List<TransactiondataAnalyseInfo>>(JsonConvert.SerializeObject(list));
                }
                var sharesList = (from item in parList
                                  group item by new { item.Market, item.SharesCode } into g
                                  select new SharesInfo
                                  {
                                      Market = g.Key.Market,
                                      SharesCode = g.Key.SharesCode
                                  }).ToList();
                var priceDic = GetStockPriceBatch(sharesList);
                if (priceDic.Count == 0)
                {
                    return false;
                }
                var parDataList = (from item in parList
                                   group item by new { item.Market, item.SharesCode } into g
                                   select g).ToDictionary(e => e.Key.SharesCode + "," + e.Key.Market, x => x.Select(t => new DB_TIME_OF_USE_PRICE
                                   {
                                       dtTradeTime = t.Time,
                                       iLastestPrice = (int)t.iLastestPrice,
                                       iLowestPrice = (int)t.iMinPrice,
                                       iOpeningPrice = (int)t.iOpeningPrice,
                                       iTopPrice = (int)t.iMaxPrice,
                                       iTradeAmount = t.iTradeStock,
                                       lTradePrice = t.lTradeAmount,
                                       iBalancePrice = t.iTradeStock == 0 ? 0 : (int)(t.lTradeAmount / t.iTradeStock)
                                   }).ToList());

                DateTime lastTime = DateTime.Now;
                if (lastTime > DateTime.Parse(lastTime.ToString("yyyy-MM-dd 15:00:00")))
                {
                    lastTime = DateTime.Parse(lastTime.ToString("yyyy-MM-dd 15:00:00"));
                }
                foreach (var result in parDataList)
                {
                    var tempTime = DateTime.Parse(lastTime.ToString("yyyy-MM-dd HH:mm:00"));
                    var startTime = DateTime.Parse(result.Value.Min(e=>e.dtTradeTime).ToString("yyyy-MM-dd HH:mm:00"));
                    DB_TIME_OF_USE_PRICE lastest = null;
                    while (startTime <= tempTime)
                    {
                        if (startTime == DateTime.Parse(lastTime.ToString("yyyy-MM-dd 09:26:00")))
                        {
                            startTime = DateTime.Parse(lastTime.ToString("yyyy-MM-dd 09:30:00"));
                            continue;
                        }
                        if (startTime == DateTime.Parse(lastTime.ToString("yyyy-MM-dd 11:31:00")))
                        {
                            startTime = DateTime.Parse(lastTime.ToString("yyyy-MM-dd 13:00:00"));
                            continue;
                        }
                        var sourceItem = result.Value.Where(e => e.dtTradeTime == startTime).FirstOrDefault();
                        if (sourceItem == null)
                        {
                            result.Value.Add(new DB_TIME_OF_USE_PRICE
                            {
                                iTradeAmount = 0,
                                dtTradeTime = startTime,
                                iBalancePrice = lastest == null ? 0 : lastest.iBalancePrice,
                                iLastestPrice = lastest == null ? 0 : lastest.iLastestPrice,
                                iOpeningPrice = lastest == null ? 0 : lastest.iLastestPrice,
                                lTradePrice = 0,
                                iLowestPrice = lastest == null ? 0 : lastest.iLastestPrice,
                                iTopPrice = lastest == null ? 0 : lastest.iLastestPrice,
                            });
                        }
                        else
                        {
                            lastest = sourceItem;
                        }
                        startTime = startTime.AddMinutes(1);
                    }
                }

                foreach (var item in priceDic)
                {
                    item.Value.openingInfo = parDataList[item.Key].Where(e => e.dtTradeTime == DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 09:25:00"))).FirstOrDefault();
                    item.Value.lastestInfo = parDataList[item.Key].OrderByDescending(e => e.dtTradeTime).FirstOrDefault();
                    item.Value.topInfo = parDataList[item.Key].OrderByDescending(e => e.iTopPrice).ThenBy(e => e.dtTradeTime).FirstOrDefault();
                    item.Value.lowestInfo = parDataList[item.Key].OrderBy(e => e.iLowestPrice).ThenBy(e => e.dtTradeTime).FirstOrDefault();
                    item.Value.topInfo2 = parDataList[item.Key].OrderByDescending(e => e.iLastestPrice).ThenBy(e => e.dtTradeTime).FirstOrDefault();
                    item.Value.lowestInfo2 = parDataList[item.Key].OrderBy(e => e.iLastestPrice).ThenBy(e => e.dtTradeTime).FirstOrDefault();
                }
                List<LOAD_TRADE_PRICE_INFO> tempList = new List<LOAD_TRADE_PRICE_INFO>();
                foreach (var item in sharesList)
                {
                    tempList.Add(new LOAD_TRADE_PRICE_INFO
                    {
                        strStockCode = item.SharesCode + "," + item.Market,
                        priceInfo = priceDic[item.SharesCode + "," + item.Market],
                        lstTimeOfUsePrice = parDataList[item.SharesCode + "," + item.Market],
                        dtMergeTime = parDataList[item.SharesCode + "," + item.Market].Min(e => e.dtTradeTime)
                    });
                }


                var error = Singleton.Instance.m_stockMonitor.LoadTradePriceInfoBat(tempList);
                return error > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 初始化载入
        /// </summary>
        private void InitLoadToMonitor()
        {
            List<TransactiondataAnalyseInfo> resultList = new List<TransactiondataAnalyseInfo>();
            string sqlQuery = string.Format(@"select SharesCode,Market,[Time],iTradeStock,iOpeningPrice,iLastestPrice,lTradeAmount,iMinPrice,iMaxPrice
from t_shares_transactiondata_time_analyse with(nolock)
where [Date]='{0}'
order by SharesInfoNum,[Time]", DateTime.Now.ToString("yyyy-MM-dd"));

            using (SqlConnection conn = new SqlConnection(Singleton.Instance.ConnectionString_meal_ticket_shares_transactiondata))
            {
                conn.Open();

                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 300;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            int market = int.Parse(reader["Market"].ToString());
                            string sharesCode = reader["SharesCode"].ToString();
                            TransactiondataAnalyseInfo result = new TransactiondataAnalyseInfo();
                            result.Time = DateTime.Parse(reader["Time"].ToString());
                            result.iTradeStock = int.Parse(reader["iTradeStock"].ToString());
                            result.iOpeningPrice = int.Parse(reader["iOpeningPrice"].ToString());
                            result.iLastestPrice = int.Parse(reader["iLastestPrice"].ToString());
                            result.lTradeAmount = long.Parse(reader["lTradeAmount"].ToString());
                            result.iMinPrice = int.Parse(reader["iMinPrice"].ToString());
                            result.iMaxPrice = int.Parse(reader["iMaxPrice"].ToString());
                            result.SharesInfoNum = int.Parse(sharesCode) * 10 + market;
                            result.Market = market;
                            result.SharesCode = sharesCode;
                            result.Date = DateTime.Now.Date; 
                            result.iBalancePrice = result.iTradeStock == 0 ? 0 : (int)(result.lTradeAmount / result.iTradeStock);
                            resultList.Add(result);
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
            LoadToMonitor(resultList);
        }

        /// <summary>
        /// 走势分析
        /// </summary>
        /// <returns></returns>
        private void DataAnalyse()
        {
            var sharesList = (from item in DataList
                              group item by new { item.Market, item.SharesCode } into g
                              select new SharesInfo
                              {
                                  Market = g.Key.Market,
                                  SharesCode = g.Key.SharesCode
                              }).ToList();
            DataHelper.ToAnalyse(sharesList);
        }

        /// <summary>
        /// 批量查询股票当天价格信息
        /// </summary>
        /// <param name="market">市场0深圳 1上海</param>
        /// <param name="sharesCode">股票代码</param>
        /// <param name="dataTime">时间</param>
        private Dictionary<string, DB_TRADE_PRICE_INFO> GetStockPriceBatch(List<SharesInfo> stockList)
        {
            List<string> lstStockCode = new List<string>();
            Dictionary<string, BASE_STOCK_TRADE_INFO> dicBaseInfo = GetStockBaseTradeInfo(stockList, ref lstStockCode);
            if (lstStockCode.Count > 0)
            {
                Dictionary<string, BASE_STOCK_TRADE_INFO> dicNewInfo = GetBaseStockTradeInfoFromDb(lstStockCode);
                if (dicNewInfo.Count == 0)
                {
                    throw new Exception("加载五档行情出错");
                }

                dicBaseInfo=dicBaseInfo.Concat(dicNewInfo).ToDictionary(k => k.Key, v => v.Value);
            }

            Dictionary<string, DB_TRADE_PRICE_INFO> resultDic = new Dictionary<string, DB_TRADE_PRICE_INFO>();
            foreach (var _item in dicBaseInfo)
            {
                DB_TRADE_PRICE_INFO tmpPriceInfo = new DB_TRADE_PRICE_INFO();

                tmpPriceInfo.iBiddingTradePrice = _item.Value.iBiddingTradePrice;
                tmpPriceInfo.iPreDayTradePrice = _item.Value.iPreDayTradePrice;
                resultDic.Add(_item.Key, tmpPriceInfo);
            }

            return resultDic;
        }
        private Dictionary<string, BASE_STOCK_TRADE_INFO> GetStockBaseTradeInfo(List<SharesInfo> stockList, ref List<string> ref_stockList)
        {
            DateTime timeNow = DateTime.Now;
            Dictionary<string, BASE_STOCK_TRADE_INFO> result = new Dictionary<string, BASE_STOCK_TRADE_INFO>();
            foreach (var _item in stockList)
            {
                string key = _item.SharesCode + "," + _item.Market;
                if (m_dicBaseInfo.ContainsKey(key))
                {
                    TimeSpan timeNowTime = TimeSpan.Parse(timeNow.ToString("HH:mm:ss"));
                    TimeSpan oldTime= TimeSpan.Parse(m_dicBaseInfo[key].dtRefreshTime.ToString("HH:mm:ss"));
                    if (timeNowTime < Singleton.Instance._tradeTime.MinTime)
                    {
                        result.Add(key, m_dicBaseInfo[key]);
                    }
                    else if (oldTime > Singleton.Instance._tradeTime.MinTime)
                    {
                        result.Add(key, m_dicBaseInfo[key]);
                    }
                    else
                    {
                        ref_stockList.Add(key);
                    }
                }
                else
                {
                    ref_stockList.Add(key);
                }
            }
            return result;
        }

        private Dictionary<string, BASE_STOCK_TRADE_INFO>  GetBaseStockTradeInfoFromDb(List<string> lstStockCode)
        {
            StringBuilder strStockCode = new StringBuilder(); 
            Dictionary<string, BASE_STOCK_TRADE_INFO> resultDic = new Dictionary<string, BASE_STOCK_TRADE_INFO>();
            strStockCode.Append("(");
            for (int i = 0; i < lstStockCode.Count(); i++)
            {
                resultDic.Add(lstStockCode[i], new BASE_STOCK_TRADE_INFO
                {
                    iBiddingTradePrice = 0,
                    iPreDayTradePrice = 0
                });
                strStockCode.Append("'");
                strStockCode.Append(lstStockCode[i]);
                strStockCode.Append("'");
                if (i < lstStockCode.Count() - 1)
                {
                    strStockCode.Append(",");
                }
            }
            strStockCode.Append(")");

            using (SqlConnection conn = new SqlConnection(Singleton.Instance.ConnectionString_meal_ticket))
            {
                conn.Open();
                try
                {
                    //查询股票当天开盘价和收盘价
                    string sqlQuery = string.Format("select SharesCode,Market,ClosedPrice,OpenedPrice from t_shares_quotes_date with(nolock) where SharesInfo in {0} and Date='{1}'", strStockCode, DateTime.Now.ToString("yyyy-MM-dd"));
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            int market = int.Parse(reader["Market"].ToString());
                            string sharesCode = reader["SharesCode"].ToString();
                            int closedPrice = int.Parse(reader["ClosedPrice"].ToString());
                            int openedPrice = int.Parse(reader["OpenedPrice"].ToString());

                            BASE_STOCK_TRADE_INFO temp = new BASE_STOCK_TRADE_INFO 
                            {
                                iBiddingTradePrice= openedPrice,
                                iPreDayTradePrice= closedPrice,
                                dtRefreshTime=DateTime.Now
                            };
                            resultDic[sharesCode + "," + market] = temp;
                            m_dicBaseInfo.Add(sharesCode + "," + market, temp);
                        }
                        reader.Close();
                    }
                    return resultDic;
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
    }
}
