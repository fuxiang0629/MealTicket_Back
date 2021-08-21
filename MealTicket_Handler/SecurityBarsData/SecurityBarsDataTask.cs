﻿using FXCommon.Common;
using MealTicket_CacheCommon_Session.session;
using MealTicket_DBCommon;
using MealTicket_Handler.RunnerHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_Handler.SecurityBarsData
{
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

        [Conditional("DEBUG")]
        public void ConsoleLog(string msg)
        {
            Console.WriteLine(msg);
        }

        /// <summary>
        /// 任务初始化
        /// </summary>
        public void Init()
        {
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
                TaskTimeOut = Singleton.Instance.SecurityBarsTaskTimeout
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
                return Singleton.Instance.SecurityBarsIntervalTime;
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
                if (!RunnerHelper.CheckTradeTime2(timeNow, false, true, false) && !RunnerHelper.CheckTradeTime2(timeNow.AddSeconds(-60), false, true, false))
                {
                    return false;
                }

                var tempGuid = Guid.NewGuid().ToString("N");
                dataObj.TotalPacketCount = SendTransactionShares(tempGuid);
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
            if (msgId == 1)
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

            bool isFinish = false;
            if (UpdateToDataBase(dataObj, ref isFinish))
            {
                if (!isFinish)
                {
                    SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                    {
                        MsgId = 2,
                        MsgObj = dataObj
                    });
                    dataObj.TaskTimeOut = -1;
                }
                else 
                {
                    int taskCount = 10;
                    Task[] taskArr = new Task[taskCount];
                    for (int i = 0; i < taskCount; i++)
                    {
                        int type = i;
                        taskArr[type] = new Task(() =>
                        {
                            UpdateOtherToDataBase(type);
                        },TaskCreationOptions.LongRunning);
                        taskArr[type].Start();
                    }
                    Task.WaitAll(taskArr);
                    receivedObj.TaskTimeOut = Singleton.Instance.SecurityBarsTaskTimeout;
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
        private bool UpdateToDataBase(SecurityBarsDataTaskQueueInfo dataObj, ref bool isFinish)
        {
            isFinish = false;
            var disList = dataObj.DataList.Skip(dataObj.DataIndex).Take(Singleton.Instance.SecurityBarsUpdateCountOnce).ToList();
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
                table.Columns.Add("Date", typeof(DateTime));
                table.Columns.Add("Time", typeof(DateTime));
                table.Columns.Add("TimeStr", typeof(string));
                table.Columns.Add("OpenedPrice", typeof(long));
                table.Columns.Add("ClosedPrice", typeof(long));
                table.Columns.Add("PreClosePrice", typeof(long));
                table.Columns.Add("MinPrice", typeof(long));
                table.Columns.Add("MaxPrice", typeof(long));
                table.Columns.Add("TradeStock", typeof(int));
                table.Columns.Add("TradeAmount", typeof(long));
                table.Columns.Add("LastModified", typeof(DateTime)); 
                foreach (var item in disList)
                {
                    DataRow row = table.NewRow();
                    row["Market"] = item.Market;
                    row["SharesCode"] = item.SharesCode;
                    row["Date"] = item.Date;
                    row["Time"] = item.Time;
                    row["TimeStr"] = item.TimeStr;
                    row["OpenedPrice"] = item.OpenedPrice;
                    row["ClosedPrice"] = item.ClosedPrice;
                    row["PreClosePrice"] = item.PreClosePrice;
                    row["MinPrice"] = item.MinPrice;
                    row["MaxPrice"] = item.MaxPrice;
                    row["TradeStock"] = item.TradeStock;
                    row["TradeAmount"] = item.TradeAmount;
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
                        db.Database.ExecuteSqlCommand("exec P_Shares_SecurityBarsData_Update @sharesSecurityBarsData", parameter);
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
        /// 更新其他K线数据
        /// </summary>
        /// <param name="type">1.5分钟线 2.15分钟线 3.30分钟线 4.60分钟线 5.日线 6.周线 7.月线 8.季度线 9.年线</param>
        private void UpdateOtherToDataBase(int type)
        {
            try
            {
                string sql;
                switch (type)
                {
                    case 0:
                        sql = "exec P_Shares_MinuteTimeData_Update";
                        break;
                    case 1:
                        sql = "exec P_Shares_SecurityBarsData_5min_Update";
                        break;
                    case 2:
                        sql = "exec P_Shares_SecurityBarsData_15min_Update";
                        break;
                    case 3:
                        sql = "exec P_Shares_SecurityBarsData_30min_Update";
                        break;
                    case 4:
                        sql = "exec P_Shares_SecurityBarsData_60min_Update";
                        break;
                    case 5:
                        sql = "exec P_Shares_SecurityBarsData_1day_Update";
                        break;
                    case 6:
                        sql = "exec P_Shares_SecurityBarsData_1week_Update";
                        break;
                    case 7:
                        sql = "exec P_Shares_SecurityBarsData_1month_Update";
                        break;
                    case 8:
                        sql = "exec P_Shares_SecurityBarsData_1quarter_Update";
                        break;
                    case 9:
                        sql = "exec P_Shares_SecurityBarsData_1year_Update";
                        break;
                    default:
                        sql = string.Empty;
                        break;
                }
                using (var db = new meal_ticketEntities())
                {
                    if (!string.IsNullOrEmpty(sql))
                    {
                        db.Database.ExecuteSqlCommand(sql);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新其他K线数据失败，type="+type, ex);
            }
        }

        /// <summary>
        /// 推送需要获取K线的股票到队列
        /// </summary>
        /// <param name="taskGuid"></param>
        /// <returns></returns>
        private static int SendTransactionShares(string taskGuid)
        {
            int errorCode = 0;
            var sessionData = Singleton.Instance.sessionClient.Get<List<SharesBaseInfo_Session>>(Singleton.Instance.SharesBaseSession, ref errorCode);
            if (errorCode != 0)
            {
                sessionData = new List<SharesBaseInfo_Session>();
            }
            List<SecurityBarsDataInfo> sharesList = (from item in sessionData
                                                     select new SecurityBarsDataInfo
                                                     {
                                                         Market = item.Market,
                                                         SharesCode = item.SharesCode
                                                     }).ToList();
            List<SecurityBarsDataInfo> securitybarsdata = new List<SecurityBarsDataInfo>();
            using (var db = new meal_ticketEntities())
            {
                string sql = @"select Market,SharesCode,[Time],PreClosePrice
  from
  (
	  select Market,SharesCode,[Time],PreClosePrice,ROW_NUMBER()OVER(partition by Market,SharesCode order by [Time] desc) num
	  from t_shares_securitybarsdata_1min with(nolock)
	  where [Date] = convert(varchar(10), getdate(), 120)
  )t
  where t.num=1;";
                securitybarsdata = db.Database.SqlQuery<SecurityBarsDataInfo>(sql).ToList();
            }
            sharesList = (from item in sharesList
                          join item2 in securitybarsdata on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                          from ai in a.DefaultIfEmpty()
                          select new SecurityBarsDataInfo
                          {
                              SharesCode = item.SharesCode,
                              Market = item.Market,
                              Time = ai == null ? null : ai.Time,
                              PreClosePrice = ai == null ? 0 : ai.PreClosePrice
                          }).ToList();

            int totalCount = sharesList.Count();
            //批次数
            int HandlerCount = Singleton.Instance.SecurityBarsBatchCount;
            int batchSize = totalCount / HandlerCount;
            if (totalCount % HandlerCount != 0)
            {
                batchSize = batchSize + 1;
            }
            Singleton.Instance.mqHandler.ClearQueueData("SecurityBars_1min");

            for (int size = 0; size < batchSize; size++)
            {
                var batchList = sharesList.Skip(size * HandlerCount).Take(HandlerCount).ToList();
                Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new SecurityBarsDataTaskQueueInfo
                {
                    TaskGuid = taskGuid,
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
