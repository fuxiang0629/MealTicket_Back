using FXCommon.Common;
using FXCommon.Database;
using MealTicket_DBCommon;
using MealTicket_Handler.RunnerHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_Handler.SecurityBarsData
{
    public class MinutetimeToDataBaseInfo 
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public string Date { get; set; }
    }

    public class SecurityBarsDataTask
    {
        /// <summary>
        /// 任务线程
        /// </summary>
        Thread TaskThread;       

        /// <summary>
        /// K线数据回调队列
        /// </summary>
        public ThreadMsgTemplate<QueueMsgObj> SecurityBarsDataQueue;

        private SecurityBarsDataTaskQueueInfo dataObj;

        private int SecurityBarsIntervalTime;

        /// <summary>
        /// 任务初始化
        /// </summary>
        public void Init(int dataType, int securityBarsIntervalTime)
        {
            SecurityBarsIntervalTime = securityBarsIntervalTime;
            SecurityBarsDataQueue = new ThreadMsgTemplate<QueueMsgObj>();
            SecurityBarsDataQueue.Init();
            dataObj = new SecurityBarsDataTaskQueueInfo
            {
                StartTime = null,
                DataIndex = 0,
                TotalPacketCount = 0,
                CallBackPacketCount = 0,
                DataList = new List<SecurityBarsDataInfo>(),
                TaskGuid = null,
                TaskTimeOut = Singleton.Instance.SecurityBarsTaskTimeout,
                DataType = dataType,
            };
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        public void DoTask()
        {
            TaskThread = new Thread(() =>
            {
                int waitTimeout = CalcIntervalTime(null, 0);
                do
                {
                    DateTime waitTime = DateTime.Now;
                    QueueMsgObj msgObj = new QueueMsgObj();
                    if (!SecurityBarsDataQueue.WaitMessage(ref msgObj, waitTimeout))
                    {
                        waitTimeout = CalcIntervalTime(null, 0);
                        if (CheckTaskTimeout(dataObj))
                        {
                            ClearTaskInfo(dataObj);//超时则清空任务
                            PushToDataUpDate(dataObj);
                        }
                        continue;
                    }

                    if (msgObj.MsgId == -1)
                    {
                        break;
                    }

                    var resultData = msgObj.MsgObj as SecurityBarsDataTaskQueueInfo;
                    DoBusiness(resultData, msgObj.MsgId);

                    waitTimeout = CalcIntervalTime(waitTime, waitTimeout);
                } while (true);
            });
            TaskThread.Start();
        }

        /// <summary>
        /// 计算间隔时间
        /// </summary>
        /// <param name="dtTime"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private int CalcIntervalTime(DateTime? dtTime, int timeout)
        {
            if (dtTime == null)
            {
                return SecurityBarsIntervalTime;
            }
            long elapsedTime = (long)(DateTime.Now - dtTime.Value).TotalMilliseconds;
            return (elapsedTime >= timeout ? 0 : timeout - (int)elapsedTime);
        }

        /// <summary>
        /// 判断任务是否超时
        /// </summary>
        /// <param name="dataObj"></param>
        /// <returns></returns>
        private bool CheckTaskTimeout(SecurityBarsDataTaskQueueInfo dataObj)
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

        /// <summary>
        /// 清空任务信息
        /// </summary>
        /// <param name="dataObj"></param>
        private void ClearTaskInfo(SecurityBarsDataTaskQueueInfo dataObj)
        {
            dataObj.TaskGuid = null;
            dataObj.StartTime = null;
            dataObj.TotalPacketCount = 0;
            dataObj.CallBackPacketCount = 0;
            dataObj.DataIndex = 0;
            dataObj.DataList = new List<SecurityBarsDataInfo>();
        }

        /// <summary>
        /// 需要更新K线的股票数据扔到队列
        /// </summary>
        private bool PushToDataUpDate(SecurityBarsDataTaskQueueInfo dataObj)
        {
            try
            {
                DateTime timeNow = DateTime.Now;
                if (!RunnerHelper.CheckTradeTime2(timeNow, false, true, false) && !RunnerHelper.CheckTradeTime2(timeNow.AddSeconds(-600), false, true, false))
                {
                    return false;
                }

                var tempGuid = Guid.NewGuid().ToString("N");
                dataObj.TotalPacketCount = SendTransactionShares(tempGuid, dataObj.DataType);
                dataObj.TaskGuid = tempGuid;
                dataObj.StartTime = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("需要更新K线的股票数据扔到队列失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 执行业务
        /// </summary>
        /// <param name="dataObj"></param>
        /// <returns></returns>
        private void DoBusiness(SecurityBarsDataTaskQueueInfo receivedObj, int msgId)
        {
            if (msgId == 1 && receivedObj.HandlerType == 1)
            {
                if (dataObj.TaskGuid != receivedObj.TaskGuid)
                {
                    return;
                }
                dataObj.DataList.AddRange(receivedObj.DataList);
                dataObj.CallBackPacketCount++;
                if (dataObj.CallBackPacketCount < dataObj.TotalPacketCount)//接受包数量不正确
                {
                    return;
                }
            }
            else if (receivedObj.HandlerType == 2)
            {
                dataObj.TaskTimeOut = -1;
                dataObj.HandlerType = receivedObj.HandlerType;
                dataObj.DataList = receivedObj.DataList;
                dataObj.DataType = receivedObj.DataType;
                dataObj.DataIndex = receivedObj.DataIndex;
            }

            bool isFinish = false;

            if (dataObj.DataType == 2)
            {
                Logger.WriteFileLog("====开始导入1分钟k线数据,处理类型" + receivedObj.HandlerType + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "====", null);
            }
            if (UpdateToDataBase(ref isFinish))
            {
                if (dataObj.DataType == 2)
                {
                    Logger.WriteFileLog("====结束导入1分钟k线数据,处理类型" + receivedObj.HandlerType + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "====", null);
                }
                dataObj.TaskTimeOut = -1;

                if (!isFinish)
                {
                    dataObj.HandlerType = receivedObj.HandlerType;
                    SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                    {
                        MsgId = 2,
                        MsgObj = dataObj
                    });
                }
                else
                {
                    if (dataObj.DataType == 2)
                    {
                        Logger.WriteFileLog("====开始导入其他k线数据,处理类型" + receivedObj.HandlerType + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "====", null);
                        //UpdateMinutetimeToDataBase(receivedObj.HandlerType);
                        Logger.WriteFileLog("====结束导入其他k线数据" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "====", null);
                    }
                }
            }

            if (isFinish)
            {
                ClearTaskInfo(dataObj);//超时则清空任务
                dataObj.TaskTimeOut = Singleton.Instance.SecurityBarsTaskTimeout;
            }
        }

        /// <summary>
        /// 数据更新到数据库
        /// </summary>
        /// <returns></returns>
        private bool UpdateToDataBase(ref bool isFinish)
        {
            isFinish = false;
            string proName = "";
            switch (dataObj.DataType)
            {
                case 2:
                    proName = "P_Shares_SecurityBarsData_Update";
                    break;
                case 3:
                    proName = "P_Shares_SecurityBarsData_5min_Update";
                    break;
                case 4:
                    proName = "P_Shares_SecurityBarsData_15min_Update";
                    break;
                case 5:
                    proName = "P_Shares_SecurityBarsData_30min_Update";
                    break;
                case 6:
                    proName = "P_Shares_SecurityBarsData_60min_Update";
                    break;
                case 7:
                    proName = "P_Shares_SecurityBarsData_1day_Update";
                    break;
                case 8:
                    proName = "P_Shares_SecurityBarsData_1week_Update";
                    break;
                case 9:
                    proName = "P_Shares_SecurityBarsData_1month_Update";
                    break;
                case 10:
                    proName = "P_Shares_SecurityBarsData_1quarter_Update";
                    break;
                case 11:
                    proName = "P_Shares_SecurityBarsData_1year_Update";
                    break;
                default:
                    break;
            }
            if (string.IsNullOrEmpty(proName))
            {
                isFinish = true;
                return false;
            }
            var disList = dataObj.DataList.Skip(dataObj.DataIndex).Take(Singleton.Instance.SecurityBarsUpdateCountOnce).ToList();
            int disCount = disList.Count();
            int totalCount = dataObj.DataList.Count();
            if (disCount <= 0)
            {
                isFinish = true;
                return true;
            }

            bool isSuccess = false;

            var disResultList = (from item in disList
                                 join item2 in Singleton.Instance._sharesBaseSession.GetSessionData() on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                                 from ai in a.DefaultIfEmpty()
                                 select new { item, ai }).ToList();
            try
            {
                DataTable table = new DataTable();
                table.Columns.Add("Market", typeof(int));
                table.Columns.Add("SharesCode", typeof(string));
                table.Columns.Add("GroupTimeKey", typeof(long));
                table.Columns.Add("Time", typeof(DateTime));
                table.Columns.Add("TimeStr", typeof(string));
                table.Columns.Add("OpenedPrice", typeof(long));
                table.Columns.Add("ClosedPrice", typeof(long));
                table.Columns.Add("PreClosePrice", typeof(long));
                table.Columns.Add("MinPrice", typeof(long));
                table.Columns.Add("MaxPrice", typeof(long));
                table.Columns.Add("TradeStock", typeof(long));
                table.Columns.Add("TradeAmount", typeof(long));
                table.Columns.Add("LastTradeStock", typeof(long));
                table.Columns.Add("LastTradeAmount", typeof(long));
                table.Columns.Add("Tradable", typeof(long));
                table.Columns.Add("TotalCapital", typeof(long));
                table.Columns.Add("HandCount", typeof(int));
                table.Columns.Add("LastModified", typeof(DateTime));
                foreach (var item in disResultList)
                {
                    DataRow row = table.NewRow();
                    row["Market"] = item.item.Market;
                    row["SharesCode"] = item.item.SharesCode;
                    row["GroupTimeKey"] = item.item.GroupTimeKey;
                    row["Time"] = item.item.Time;
                    row["TimeStr"] = item.item.TimeStr;
                    row["OpenedPrice"] = item.item.OpenedPrice;
                    row["ClosedPrice"] = item.item.ClosedPrice;
                    row["PreClosePrice"] = item.item.PreClosePrice;
                    row["MinPrice"] = item.item.MinPrice;
                    row["MaxPrice"] = item.item.MaxPrice;
                    row["TradeStock"] = item.item.TradeStock;
                    row["TradeAmount"] = item.item.TradeAmount;
                    row["LastTradeStock"] = 0;
                    row["LastTradeAmount"] = 0;
                    row["Tradable"] = item.ai == null ? 0 : item.ai.CirculatingCapital;
                    row["TotalCapital"] = item.ai == null ? 0 : item.ai.TotalCapital;
                    row["HandCount"] = item.ai == null ? 0 : item.ai.SharesHandCount;
                    row["LastModified"] = DateTime.Now;
                    table.Rows.Add(row);
                }

                using (var db = new meal_ticketEntities())
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        //关键是类型
                        SqlParameter parameter = new SqlParameter("@sharesSecurityBarsData", SqlDbType.Structured);
                        //必须指定表类型名
                        parameter.TypeName = "dbo.SharesSecurityBarsData";
                        //赋值
                        parameter.Value = table;
                        db.Database.ExecuteSqlCommand(string.Format("exec {0} @sharesSecurityBarsData", proName), parameter);
                        tran.Commit();
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("更新K线数据出错", ex);
                        tran.Rollback();
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新K线数据失败", ex);
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
        /// 数据更新到数据库(通过temp表)
        /// </summary>
        /// <returns></returns>
        private bool UpdateToDataBaseByTempTable(ref bool isFinish)
        {
            isFinish = false;
            string tableName = "";
            string proName = "";
            switch (dataObj.DataType)
            {
                case 2:
                    tableName = "t_shares_securitybarsdata_1min_temp";
                    proName = "P_Shares_SecurityBarsData_1min_Update_Temp";
                    break;
                case 3:
                    tableName = "t_shares_securitybarsdata_5min_temp";
                    proName = "P_Shares_SecurityBarsData_5min_Update_Temp";
                    break;
                case 4:
                    tableName = "t_shares_securitybarsdata_15min_temp";
                    proName = "P_Shares_SecurityBarsData_15min_Update_Temp";
                    break;
                case 5:
                    tableName = "t_shares_securitybarsdata_30min_temp";
                    proName = "P_Shares_SecurityBarsData_30min_Update_Temp";
                    break;
                case 6:
                    tableName = "t_shares_securitybarsdata_60min_temp";
                    proName = "P_Shares_SecurityBarsData_60min_Update_Temp";
                    break;
                case 7:
                    tableName = "t_shares_securitybarsdata_1day_temp";
                    proName = "P_Shares_SecurityBarsData_1day_Update_Temp";
                    break;
                case 8:
                    tableName = "t_shares_securitybarsdata_1week_temp";
                    proName = "P_Shares_SecurityBarsData_1week_Update_Temp";
                    break;
                case 9:
                    tableName = "t_shares_securitybarsdata_1month_temp";
                    proName = "P_Shares_SecurityBarsData_1month_Update_Temp";
                    break;
                case 10:
                    tableName = "t_shares_securitybarsdata_1quarter_temp";
                    proName = "P_Shares_SecurityBarsData_1quarter_Update_Temp";
                    break;
                case 11:
                    tableName = "t_shares_securitybarsdata_1year_temp";
                    proName = "P_Shares_SecurityBarsData_1year_Update_Temp";
                    break;
                default:
                    break;
            }
            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(proName))
            {
                isFinish = true;
                return false;
            }
            var disList = dataObj.DataList.Skip(dataObj.DataIndex).Take(Singleton.Instance.SecurityBarsUpdateCountOnce).ToList();
            int disCount = disList.Count();
            int totalCount = dataObj.DataList.Count();
            if (disCount <= 0)
            {
                isFinish = true;
                return true;
            }

            bool isSuccess = false;

            var disResultList = (from item in disList
                                 join item2 in Singleton.Instance._sharesBaseSession.GetSessionData() on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                                 from ai in a.DefaultIfEmpty()
                                 select new { item, ai }).ToList();
            try
            {
                var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);

                DataTable table = new DataTable();
                table.Columns.Add("Market", typeof(int));
                table.Columns.Add("SharesCode", typeof(string));
                table.Columns.Add("GroupTimeKey", typeof(long));
                table.Columns.Add("Time", typeof(DateTime));
                table.Columns.Add("TimeStr", typeof(string));
                table.Columns.Add("OpenedPrice", typeof(long));
                table.Columns.Add("ClosedPrice", typeof(long));
                table.Columns.Add("PreClosePrice", typeof(long));
                table.Columns.Add("MinPrice", typeof(long));
                table.Columns.Add("MaxPrice", typeof(long));
                table.Columns.Add("TradeStock", typeof(long));
                table.Columns.Add("TradeAmount", typeof(long));
                table.Columns.Add("LastTradeStock", typeof(long));
                table.Columns.Add("LastTradeAmount", typeof(long));
                table.Columns.Add("Tradable", typeof(long));
                table.Columns.Add("TotalCapital", typeof(long));
                table.Columns.Add("HandCount", typeof(int));
                table.Columns.Add("LastModified", typeof(DateTime));
                foreach (var item in disResultList)
                {
                    DataRow row = table.NewRow();
                    row["Market"] = item.item.Market;
                    row["SharesCode"] = item.item.SharesCode;
                    row["GroupTimeKey"] = item.item.GroupTimeKey;
                    row["Time"] = item.item.Time;
                    row["TimeStr"] = item.item.TimeStr;
                    row["OpenedPrice"] = item.item.OpenedPrice;
                    row["ClosedPrice"] = item.item.ClosedPrice;
                    row["PreClosePrice"] = item.item.PreClosePrice;
                    row["MinPrice"] = item.item.MinPrice;
                    row["MaxPrice"] = item.item.MaxPrice;
                    row["TradeStock"] = item.item.TradeStock;
                    row["TradeAmount"] = item.item.TradeAmount;
                    row["LastTradeStock"] = 0;
                    row["LastTradeAmount"] = 0;
                    row["Tradable"] = item.ai == null ? 0 : item.ai.CirculatingCapital;
                    row["TotalCapital"] = item.ai == null ? 0 : item.ai.TotalCapital;
                    row["HandCount"] = item.ai == null ? 0 : item.ai.SharesHandCount;
                    row["LastModified"] = DateTime.Now;
                    table.Rows.Add(row);
                }

                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("Market", "Market");
                dic.Add("SharesCode", "SharesCode");
                dic.Add("GroupTimeKey", "GroupTimeKey");
                dic.Add("Time", "Time");
                dic.Add("TimeStr", "TimeStr");
                dic.Add("OpenedPrice", "OpenedPrice");
                dic.Add("ClosedPrice", "ClosedPrice");
                dic.Add("PreClosePrice", "PreClosePrice");
                dic.Add("MinPrice", "MinPrice");
                dic.Add("MaxPrice", "MaxPrice");
                dic.Add("TradeStock", "TradeStock");
                dic.Add("TradeAmount", "TradeAmount");
                dic.Add("LastTradeStock", "LastTradeStock");
                dic.Add("LastTradeAmount", "LastTradeAmount");
                dic.Add("Tradable", "Tradable");
                dic.Add("TotalCapital", "TotalCapital");
                dic.Add("HandCount", "HandCount");
                dic.Add("LastModified", "LastModified");
                bulk.ColumnMappings = dic;
                bulk.BatchSize = Singleton.Instance.SecurityBarsUpdateCountOnce;

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    string sql = string.Format("truncate table {0}", tableName);
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;   //sql语句
                        cmd.ExecuteNonQuery();
                    }
                    using (SqlTransaction tran = conn.BeginTransaction())//开启事务
                    {
                        try
                        {
                            bulk.BulkWriteToServer(conn, table, tableName, tran);
                            sql = string.Format("exec {0}",proName);
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = sql;   //sql语句
                                cmd.Transaction = tran;
                                cmd.ExecuteNonQuery();
                            }
                            tran.Commit();
                            isSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("更新K线数据出错", ex);
                            tran.Rollback();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新K线数据失败", ex);
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
        /// 更新分时数据
        /// </summary>
        /// <param name="dataList"></param>
        private void UpdateMinutetimeToDataBasebak(int handlerType)
        {
            try
            {
                var tempData = (from item in dataObj.DataList
                                group item by new { item.Market, item.SharesCode, item.Date } into g
                                select g.Key).ToList();
                DataTable table = new DataTable();
                table.Columns.Add("Market", typeof(int));
                table.Columns.Add("SharesCode", typeof(string));
                table.Columns.Add("Date", typeof(DateTime));
                table.Columns.Add("HandlerType", typeof(int));
                foreach (var item in tempData)
                {
                    DataRow row = table.NewRow();
                    row["Market"] = item.Market;
                    row["SharesCode"] = item.SharesCode;
                    row["Date"] = item.Date;
                    row["HandlerType"] = handlerType;
                    table.Rows.Add(row);
                }

                using (var db = new meal_ticketEntities())
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        //关键是类型
                        SqlParameter parameter = new SqlParameter("@sharesMinuteTimeDataBatch", SqlDbType.Structured);
                        //必须指定表类型名
                        parameter.TypeName = "dbo.SharesMinuteTimeDataBatch";
                        //赋值
                        parameter.Value = table;
                        db.Database.ExecuteSqlCommand("exec P_Shares_MinuteTimeData_Update_batch @sharesMinuteTimeDataBatch", parameter);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("更新分时数据出错", ex);
                        tran.Rollback();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新分时数据失败", ex);
            }
        }

        /// <summary>
        /// 更新分时数据
        /// </summary>
        /// <param name="dataList"></param>
        private void UpdateMinutetimeToDataBase(int handlerType)
        {
            try
            {
                var tempData = (from item in dataObj.DataList
                                group item by new { item.Market, item.SharesCode, item.Date } into g
                                select new MinutetimeToDataBaseInfo 
                                {
                                    SharesCode=g.Key.SharesCode,
                                    Date=g.Key.Date.Value.ToString("yyyy-MM-dd"),
                                    Market=g.Key.Market
                                }).ToList();
                string sql = "exec P_Shares_MinuteTimeData_Update {0},'{1}','{2}',{3} ";

                int defaultTaskCount = 32;
                Task[] taskArr = new Task[defaultTaskCount];

                ThreadMsgTemplate<MinutetimeToDataBaseInfo> data = new ThreadMsgTemplate<MinutetimeToDataBaseInfo>();
                data.Init();
                foreach (var item in tempData)
                {
                    data.AddMessage(item);
                }

                for (int i = 0; i < defaultTaskCount; i++)
                {
                    taskArr[i] = new Task(()=>
                    {
                        do
                        {
                            MinutetimeToDataBaseInfo tempObj = new MinutetimeToDataBaseInfo();
                            if (!data.GetMessage(ref tempObj, true))
                            {
                                break;
                            }

                            using (var db = new meal_ticketEntities())
                            using (var tran = db.Database.BeginTransaction())
                            {
                                int market = tempObj.Market;
                                string sharesCode = tempObj.SharesCode;
                                string date = tempObj.Date;
                                try
                                {
                                    db.Database.ExecuteSqlCommand(string.Format(sql, market, sharesCode, date, handlerType));
                                    tran.Commit();
                                }
                                catch (Exception ex)
                                {
                                    tran.Rollback();
                                }
                            }
                        } while (true);
                    },TaskCreationOptions.LongRunning);
                    taskArr[i].Start();
                }
                Task.WaitAll(taskArr);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新分时数据失败", ex);
            }
        }

        /// <summary>
        /// 推送需要获取K线的股票到队列
        /// </summary>
        /// <param name="taskGuid"></param>
        /// <returns></returns>
        private static int SendTransactionShares(string taskGuid,int dataType)
        {
            DateTime dateNow = DateTime.Now.Date;
            string tableName = "";
            long groupTimeKey = 0;
            switch (dataType)
            {
                case 2:
                    tableName = "t_shares_securitybarsdata_1min";
                    groupTimeKey = long.Parse(dateNow.ToString("yyyyMMdd0000"));
                    break;
                case 3:
                    tableName = "t_shares_securitybarsdata_5min";
                    groupTimeKey = long.Parse(dateNow.ToString("yyyyMMdd0000"));
                    break;
                case 4:
                    tableName = "t_shares_securitybarsdata_15min";
                    groupTimeKey = long.Parse(dateNow.ToString("yyyyMMdd0000"));
                    break;
                case 5:
                    tableName = "t_shares_securitybarsdata_30min";
                    groupTimeKey = long.Parse(dateNow.ToString("yyyyMMdd0000"));
                    break;
                case 6:
                    tableName = "t_shares_securitybarsdata_60min";
                    groupTimeKey = long.Parse(dateNow.ToString("yyyyMMdd0000"));
                    break;
                case 7:
                    tableName = "t_shares_securitybarsdata_1day";
                    groupTimeKey = long.Parse(dateNow.ToString("yyyyMMdd"));
                    break;
                case 8:
                    tableName = "t_shares_securitybarsdata_1week";
                    groupTimeKey = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                    where item.the_date == int.Parse(dateNow.ToString("yyyyMMdd"))
                                    select item.the_week).FirstOrDefault() ?? 0;
                    break;
                case 9:
                    tableName = "t_shares_securitybarsdata_1month";
                    groupTimeKey = long.Parse(dateNow.ToString("yyyyMM"));
                    break;
                case 10:
                    tableName = "t_shares_securitybarsdata_1quarter";
                    groupTimeKey = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                    where item.the_date == int.Parse(dateNow.ToString("yyyyMMdd"))
                                    select item.the_quarter).FirstOrDefault() ?? 0;
                    break;
                case 11:
                    tableName = "t_shares_securitybarsdata_1year";
                    groupTimeKey = long.Parse(dateNow.ToString("yyyy"));
                    break;
                default:
                    break;

            }
            if (string.IsNullOrEmpty(tableName) || groupTimeKey==0)
            {
                return 0;
            }

            var allSharesList = (from item in Singleton.Instance._sharesBaseSession.GetSessionData()
                                 select new
                                 {
                                     Market = item.Market,
                                     SharesCode = item.SharesCode,
                                     PreClosePrice = item.ClosedPrice
                                 }).ToList();
            List<SecurityBarsDataInfo> securitybarsdata = new List<SecurityBarsDataInfo>();
            using (var db = new meal_ticketEntities())
            {
                string sql = string.Format(@"select Market,SharesCode,GroupTimeKey,PreClosePrice
  from
  (
	  select Market,SharesCode,GroupTimeKey,PreClosePrice,ROW_NUMBER()OVER(partition by Market,SharesCode order by [Time] desc) num
	  from {0} with(nolock)
	  where GroupTimeKey >= {1}
  )t
  where t.num=1;", tableName, groupTimeKey);
                securitybarsdata = db.Database.SqlQuery<SecurityBarsDataInfo>(sql).ToList();
            }
            var sharesList = (from item in allSharesList
                              join item2 in securitybarsdata on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                              from ai in a.DefaultIfEmpty()
                              select new
                              {
                                  SharesCode = item.SharesCode,
                                  Market = item.Market,
                                  StartTimeKey = ai == null ? groupTimeKey : ai.GroupTimeKey,
                                  EndTimeKey = -1,
                                  PreClosePrice = ai == null ? item.PreClosePrice : ai.PreClosePrice
                              }).ToList();

            int totalCount = sharesList.Count();
            //批次数
            int HandlerCount = Singleton.Instance.SecurityBarsBatchCount;
            int batchSize = totalCount / HandlerCount;
            if (totalCount % HandlerCount != 0)
            {
                batchSize = batchSize + 1;
            }

            for (int size = 0; size < batchSize; size++)
            {
                var batchList = sharesList.Skip(size * HandlerCount).Take(HandlerCount).ToList();
                Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new
                {
                    TaskGuid = taskGuid,
                    HandlerType=1,
                    DataType= dataType,
                    DataList = batchList
                })), "SecurityBars", "1min");
            }
            return batchSize;
        }

        /// <summary>
        /// 资源回收
        /// </summary>
        public void Dispose()
        {
            if (TaskThread != null)
            {
                SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                {
                    MsgId = -1,
                    MsgObj = null
                }, true, 0);
                TaskThread.Join();
            }
            if (SecurityBarsDataQueue != null)
            {
                SecurityBarsDataQueue.Release();
            }
        }
    }
}
