using FXCommon.Common;
using MealTicket_DBCommon;
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
    public struct BASE_STOCK_TRADE_INFO
    {
        public int iBiddingTradePrice;
        public int iPreDayTradePrice;
        public DateTime dtRefreshTime;
    }

    public class QueueMsgObj
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public int MsgId { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public object MsgObj { get; set; }
    }

    public class TransactionDataTask
    {
        [Conditional("DEBUG")]
        public void ConsoleLog(string msg)
        {
            Console.WriteLine(msg);
        }

        /// <summary>
        /// 开盘收盘价格缓存
        /// </summary>
        Dictionary<string, BASE_STOCK_TRADE_INFO> m_dicBaseInfo = new Dictionary<string, BASE_STOCK_TRADE_INFO>();

        /// <summary>
        /// 任务线程
        /// </summary>
        Thread TaskThread;

        /// <summary>
        /// 分笔数据回调队列
        /// </summary>
        public ThreadMsgTemplate<QueueMsgObj> TransactiondataQueue;

        private TransactiondataTaskQueueInfo dataObj0;
        private TransactiondataTaskQueueInfo dataObj1;

        public void Init()
        {
            TransactiondataQueue = new ThreadMsgTemplate<QueueMsgObj>();
            TransactiondataQueue.Init();
            dataObj0 = new TransactiondataTaskQueueInfo
            {
                StartTime = null,
                DataIndex = 0,
                TotalPacketCount = 0,
                CallBackPacketCount = 0,
                DataList = new List<TransactiondataAnalyseInfo>(),
                DataType = 0,
                TaskGuid = null,
                TaskTimeOut = Singleton.Instance.NewTransactiondataTaskTimeOut
            };
            dataObj1 = new TransactiondataTaskQueueInfo
            {
                StartTime = null,
                DataIndex = 0,
                TotalPacketCount = 0,
                CallBackPacketCount = 0,
                DataList = new List<TransactiondataAnalyseInfo>(),
                DataType = 1,
                TaskGuid = null,
                TaskTimeOut = Singleton.Instance.NewTransactiondataTaskTimeOut2
            };
            InitLoadToMonitor();
        }

        public void Dispose()
        {
            if (TaskThread != null)
            {
                TransactiondataQueue.AddMessage(new QueueMsgObj
                {
                    MsgId = -1,
                    MsgObj = null
                }, true, 0);
                TaskThread.Join();
            }
            if (TransactiondataQueue != null)
            {
                TransactiondataQueue.Release();
            }
        }

        private bool CheckTaskTimeout(TransactiondataTaskQueueInfo dataObj)
        {
            if (!string.IsNullOrEmpty(dataObj.TaskGuid))
            {
                if ((DateTime.Now - dataObj.StartTime.Value).TotalMilliseconds < dataObj.TaskTimeOut || dataObj.TaskTimeOut == -1)//未超时
                {
                    return false;
                }
                dataObj.TaskTimeOut = dataObj.TaskTimeOut * 2;
            }
            return true;
        }

        public void ClearTaskInfo(TransactiondataTaskQueueInfo dataObj)
        {
            dataObj.TaskGuid = null;
            dataObj.StartTime = null;
            dataObj.TotalPacketCount = 0;
            dataObj.CallBackPacketCount = 0;
            dataObj.DataIndex = 0;
            dataObj.DataList = new List<TransactiondataAnalyseInfo>();
        }

        private int CalcElapsedWaitTime(DateTime? dtTime, int timeout)
        {
            if (dtTime == null)
            {
                return Singleton.Instance.NewTransactionDataSendPeriodTime;
            }
            long elapsedTime = (long)(DateTime.Now - dtTime.Value).TotalMilliseconds;
            return (elapsedTime >= timeout ? 0 : timeout - (int)elapsedTime);
        }

        /// <summary>
        /// 需要更新分笔的股票数据扔到队列
        /// </summary>
        private bool PushToDataUpDate(TransactiondataTaskQueueInfo dataObj)
        {
            try
            {
                DateTime timeNow = DateTime.Now;
                if (!Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.NewTransactionDataRunStartTime)) && !Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.NewTransactionDataRunEndTime)))
                {
                    dataObj.TaskTimeOut = Singleton.Instance.NewTransactiondataTaskTimeOut;
                    return false;
                }
                var tempGuid = Guid.NewGuid().ToString("N");
                dataObj.TotalPacketCount = RunnerHelper.SendTransactionShares(tempGuid, dataObj.DataType);
                if (dataObj.TotalPacketCount > 0)
                {
                    ConsoleLog("数据发送数据时间:"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+"==="+ tempGuid + "===,类型"+ dataObj.DataType);
                }
                dataObj.TaskGuid = tempGuid;
                dataObj.StartTime = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("需要更新分笔的股票数据扔到队列失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 执行业务
        /// </summary>
        /// <param name="dataObj"></param>
        /// <returns></returns>
        private void DoBusiness(TransactiondataTaskQueueInfo receivedObj,int msgType)
        {
            TransactiondataTaskQueueInfo dataObj = receivedObj.DataType == 0 ? dataObj0 : receivedObj.DataType == 1 ? dataObj1 : null;
            if (dataObj == null)
            {
                return;
            }

            if (msgType == 1)
            {
                if (dataObj.TaskGuid != receivedObj.TaskGuid)
                {
                    return;
                }

                if (LoadToMonitor(receivedObj.DataList))
                {
                    DataAnalyse(receivedObj.DataList);
                }

                dataObj.DataList.AddRange(receivedObj.DataList);
                dataObj.CallBackPacketCount++;
                if (dataObj.CallBackPacketCount < dataObj.TotalPacketCount)//接受包数量不正确
                {
                    return;
                }
            }

            bool isFinish = false;
            if (UpdateToDataBase(dataObj, ref isFinish))
            {
                if (isFinish)
                {
                    UpdateSharesStatus(dataObj);
                }
                else
                {
                    TransactiondataQueue.AddMessage(new QueueMsgObj
                    {
                        MsgId = 2,
                        MsgObj = dataObj
                    });
                    dataObj.TaskTimeOut = -1;
                }
            }

            if(isFinish)
            {
                ClearTaskInfo(dataObj);//超时则清空任务
                dataObj.TaskTimeOut = dataObj.DataType == 0 ? Singleton.Instance.NewTransactiondataTaskTimeOut : Singleton.Instance.NewTransactiondataTaskTimeOut2;
            }
        }

        /// <summary>
        /// 数据更新到数据库
        /// </summary>
        /// <returns></returns>
        private bool UpdateToDataBase(TransactiondataTaskQueueInfo dataObj,ref bool isFinish)
        {
            isFinish = false;
            var disList = dataObj.DataList.Skip(dataObj.DataIndex).Take(Singleton.Instance.NewTransactiondataTaskUpdateCountOnce).ToList();
            int disCount = disList.Count();
            int totalCount = dataObj.DataList.Count();
            if (disCount <= 0)
            {
                isFinish = true;
                return true;
            }

            bool isSuccess = false;
            try
            {
                DataTable table = new DataTable();
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
                foreach (var item in disList)
                {
                    DataRow row = table.NewRow();
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

                string connectionString = Singleton.Instance.ConnectionString_meal_ticket_shares_transactiondata;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand("P_Transactiondata_Time_Analyse_Update", conn))
                            {
                                cmd.Transaction = tran;
                                cmd.CommandType = CommandType.StoredProcedure;
                                //关键是类型
                                SqlParameter parameter = new SqlParameter("@analyseData", SqlDbType.Structured);
                                //必须指定表类型名
                                parameter.TypeName = "dbo.TransactiondataTimeAnalyse";
                                //赋值
                                parameter.Value = table;
                                cmd.Parameters.Add(parameter);
                                cmd.ExecuteNonQuery();
                                tran.Commit();
                                isSuccess = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("更新分笔统计数据数据库连接出错", ex);
                            tran.Rollback();
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新分笔统计数据库失败", ex);
            }

            if (isSuccess)
            {
                dataObj.DataIndex = dataObj.DataIndex + disCount;
                if (totalCount <= dataObj.DataIndex)
                {
                    isFinish = true;
                }
            }
            else
            {
                isFinish = true;
            }
            return isSuccess;
        }

        /// <summary>
        /// 更新股票状态
        /// </summary>
        /// <returns></returns>
        private bool UpdateSharesStatus(TransactiondataTaskQueueInfo dataObj)
        {
            if (dataObj.DataList.Count() == 0)
            {
                return false;
            }

            try
            {
                string sharesInfo = string.Empty;
                var dataGroup = (from item in dataObj.DataList
                                 group item by new { item.Market, item.SharesCode } into g
                                 select new
                                 {
                                     Market = g.Key.Market,
                                     SharesCode = g.Key.SharesCode
                                 }).ToList();
                foreach (var item in dataGroup)
                {
                    sharesInfo = sharesInfo + "'" + item.SharesCode + "," + item.Market + "'" + ",";
                }
                sharesInfo = sharesInfo.TrimEnd(',');
                using (var db = new meal_ticketEntities())
                {
                    string sql = string.Format(@"update t_account_shares_conditiontrade_buy set DataType=0 where DataType=1 and SharesInfo in ({0});
update t_shares_monitor set DataType=0 where DataType=1 and SharesInfo in ({0});", sharesInfo);
                    db.Database.ExecuteSqlCommand(sql);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新数据标记出错", ex);
                return false;
            }
        }

        /// <summary>
        /// 走势分笔数据内存加载
        /// </summary>
        /// <returns></returns>
        private bool LoadToMonitor(List<TransactiondataAnalyseInfo> list)
        {
            Logger.WriteFileLog("开始加载数据====="+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+"=============",null);
            try
            {
                var sharesList = (from item in list
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

                var parDataList = (from item in list
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
                    var startTime = DateTime.Parse(result.Value.Min(e => e.dtTradeTime).ToString("yyyy-MM-dd HH:mm:00"));
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
                if (error <= 0)
                {
                    Logger.WriteFileLog("走势分笔数据内存加载报错" + error, null);
                }
                return error > 0;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("走势分笔数据内存加载失败", ex);
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
        private void DataAnalyse(List<TransactiondataAnalyseInfo> list)
        {
            var sharesList = (from item in list
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

                dicBaseInfo = dicBaseInfo.Concat(dicNewInfo).ToDictionary(k => k.Key, v => v.Value);
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
                    TimeSpan oldTime = TimeSpan.Parse(m_dicBaseInfo[key].dtRefreshTime.ToString("HH:mm:ss"));
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

        private Dictionary<string, BASE_STOCK_TRADE_INFO> GetBaseStockTradeInfoFromDb(List<string> lstStockCode)
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
                                iBiddingTradePrice = openedPrice,
                                iPreDayTradePrice = closedPrice,
                                dtRefreshTime = DateTime.Now
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

        /// <summary>
        /// 执行任务
        /// </summary>
        public void DoTask()
        {
            TaskThread = new Thread(() =>
            {
                int waitTimeout = CalcElapsedWaitTime(null, 0);
                do
                {
                    DateTime waitTime = DateTime.Now;
                    QueueMsgObj msgObj = new QueueMsgObj();
                    if (!TransactiondataQueue.WaitMessage(ref msgObj, waitTimeout))
                    {
                        waitTimeout = CalcElapsedWaitTime(null, 0);
                        if (CheckTaskTimeout(dataObj0))
                        {
                            ClearTaskInfo(dataObj0);//超时则清空任务
                            if (Singleton.Instance.IsOpen == 1)
                            {
                                PushToDataUpDate(dataObj0);
                            }
                        }

                        if (CheckTaskTimeout(dataObj1))
                        {
                            ClearTaskInfo(dataObj1);//超时则清空任务
                            if (Singleton.Instance.IsOpen == 1)
                            {
                                PushToDataUpDate(dataObj1);
                            }
                        }
                        continue;
                    }

                    if (msgObj.MsgId == -1)
                    {
                        break;
                    }
                    
                    if (msgObj.MsgId == 1)//执行队列接受数据包
                    {
                        var resultData = msgObj.MsgObj as TransactiondataTaskQueueInfo;
                        DoBusiness(resultData,1);
                    }
                    else if (msgObj.MsgId == 2)//执行大数据包分割导入
                    {
                        var resultData = msgObj.MsgObj as TransactiondataTaskQueueInfo;
                        DoBusiness(resultData,2);
                    }

                    waitTimeout = CalcElapsedWaitTime(waitTime, waitTimeout);
                } while (true);
            });
            TaskThread.Start();
        }
    }
}
