using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Runner;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.SecurityBarsData
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
            dataObj.TaskTimeOut = Singleton.Instance.SecurityBarsTaskTimeout;
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
                if (!Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.NewTransactionDataRunStartTime)) && !Helper.CheckTradeTime(timeNow.AddSeconds(-Singleton.Instance.NewTransactionDataRunEndTime)))
                {
                    return false;
                }

                var tempGuid = Guid.NewGuid().ToString("N");
                dataObj.TotalPacketCount = RunnerHelper.SendTransactionShares(tempGuid);
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
                table.Columns.Add("Time", typeof(string));
                table.Columns.Add("OpenedPrice", typeof(long));
                table.Columns.Add("ClosedPrice", typeof(long));
                table.Columns.Add("MinPrice", typeof(long));
                table.Columns.Add("MaxPrice", typeof(long));
                table.Columns.Add("TradeStock", typeof(int));
                table.Columns.Add("TradeAmount", typeof(long));
                table.Columns.Add("RiseCount", typeof(int));
                table.Columns.Add("FallCount", typeof(int));
                table.Columns.Add("LastModified", typeof(DateTime));
                foreach (var item in disList)
                {
                    DataRow row = table.NewRow();
                    row["Market"] = item.Market;
                    row["SharesCode"] = item.SharesCode;
                    row["Date"] = item.Date;
                    row["Time"] = item.Time;
                    row["OpenedPrice"] = item.OpenedPrice;
                    row["ClosedPrice"] = item.ClosedPrice;
                    row["MinPrice"] = item.MinPrice;
                    row["MaxPrice"] = item.MaxPrice;
                    row["TradeStock"] = item.TradeStock;
                    row["TradeAmount"] = item.TradeAmount;
                    row["RiseCount"] = item.RiseCount;
                    row["FallCount"] = item.FallCount;
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
