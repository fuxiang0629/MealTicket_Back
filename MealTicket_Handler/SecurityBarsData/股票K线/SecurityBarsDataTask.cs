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

        private DateTime LastUpdateDate;

        /// <summary>
        /// 任务初始化
        /// </summary>
        public void Init()
        {
            LastUpdateDate = DateTime.Parse("1991-01-01 00:00:00");
            SecurityBarsIntervalTime = Singleton.Instance.SecurityBarsIntervalTime;
            SecurityBarsDataQueue = new ThreadMsgTemplate<QueueMsgObj>();
            SecurityBarsDataQueue.Init();
            dataObj = new SecurityBarsDataTaskQueueInfo
            {
                StartTime = null,
                TotalPacketCount = 0,
                CallBackPacketCount = 0,
                TaskGuid = null,
                TaskTimeOut = Singleton.Instance.SecurityBarsTaskTimeout,
                HandlerType = 1,
                PackageList = new Dictionary<int, SecurityBarsDataType>()
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
                            if (Singleton.Instance.SecurityBarsIsOpen == 1)
                            {
                                PushToDataUpDate(dataObj);
                            }
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
            dataObj.HandlerType = 1;
            dataObj.PackageList = new Dictionary<int, SecurityBarsDataType>();
        }

        /// <summary>
        /// 需要更新K线的股票数据扔到队列
        /// </summary>
        private bool PushToDataUpDate(SecurityBarsDataTaskQueueInfo dataObj)
        {
            try
            {
                DateTime timeNow = DateTime.Now;
                if (!RunnerHelper.CheckTradeDate(timeNow))
                {
                    dataObj.TaskTimeOut = Singleton.Instance.SecurityBarsTaskTimeout;
                    return false;
                }
                if (!RunnerHelper.CheckTradeTime2(timeNow.AddSeconds(-180)) && !RunnerHelper.CheckTradeTime2(timeNow.AddSeconds(180)))
                {
                    dataObj.TaskTimeOut = Singleton.Instance.SecurityBarsTaskTimeout;
                    //每天21-23点执行
                    TimeSpan tpNow = TimeSpan.Parse(timeNow.ToString("HH:mm:ss"));
                    if (tpNow < Singleton.Instance.SecurityBarsDataUpdateStartTime || tpNow > Singleton.Instance.SecurityBarsDataUpdateEndTime)
                    {
                        return false;
                    }
                    //判断当天是否执行过
                    if (LastUpdateDate.Date >= timeNow.Date)
                    {
                        return false;
                    }
                    PushToDataUpDateAll();
                    LastUpdateDate = timeNow;
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
        /// 每天全部股票补充
        /// </summary>
        private void PushToDataUpDateAll() 
        {
            DateTime timeNow = DateTime.Now;
            var dimTime = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                           where item.the_date == int.Parse(timeNow.ToString("yyyyMMdd"))
                           select item).FirstOrDefault();
            string url = string.Format("{0}/tradecenter/shares/kline/reset/batch", ConfigurationManager.AppSettings["mgrurl"]);
            try
            {
                var content10 = new
                {
                    DataType = 11,
                    StartGroupTimeKey = timeNow.Year,
                    EndGroupTimeKey = timeNow.Year
                };
                HtmlSender.Request(JsonConvert.SerializeObject(content10), url, "application/json", Encoding.UTF8, "POST");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每天补充年K有误",ex);
            }

            try
            {
                if (dimTime == null)
                {
                    throw new Exception("时间超出界限");
                }
                var content9 = new
                {
                    DataType = 10,
                    StartGroupTimeKey = dimTime.the_quarter,
                    EndGroupTimeKey = dimTime.the_quarter
                };
                HtmlSender.Request(JsonConvert.SerializeObject(content9), url, "application/json", Encoding.UTF8, "POST");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每天补充季度K有误", ex);
            }

            try
            {
                var content8 = new
                {
                    DataType = 9,
                    StartGroupTimeKey = long.Parse(timeNow.ToString("yyyyMM")),
                    EndGroupTimeKey = long.Parse(timeNow.ToString("yyyyMM"))
                };
                HtmlSender.Request(JsonConvert.SerializeObject(content8), url, "application/json", Encoding.UTF8, "POST");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每天补充月K有误", ex);
            }

            try
            {
                if (dimTime == null)
                {
                    throw new Exception("时间超出界限");
                }
                var content7 = new
                {
                    DataType = 8,
                    StartGroupTimeKey = dimTime.the_week,
                    EndGroupTimeKey = dimTime.the_week
                };
                HtmlSender.Request(JsonConvert.SerializeObject(content7), url, "application/json", Encoding.UTF8, "POST");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每天补充周K有误", ex);
            }

            try
            {
                var content6 = new
                {
                    DataType = 7,
                    StartGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd")),
                    EndGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd"))
                };
                HtmlSender.Request(JsonConvert.SerializeObject(content6), url, "application/json", Encoding.UTF8, "POST");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每天补充日K有误", ex);
            }

            try
            {
                var content5 = new
                {
                    DataType = 6,
                    StartGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd0930")),
                    EndGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd1500"))
                };
                HtmlSender.Request(JsonConvert.SerializeObject(content5), url, "application/json", Encoding.UTF8, "POST");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每天补充60分K有误", ex);
            }

            try
            {
                var content4 = new
                {
                    DataType = 5,
                    StartGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd0930")),
                    EndGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd1500"))
                };
                HtmlSender.Request(JsonConvert.SerializeObject(content4), url, "application/json", Encoding.UTF8, "POST");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每天补充30分K有误", ex);
            }

            try
            {
                var content3 = new
                {
                    DataType = 4,
                    StartGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd0930")),
                    EndGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd1500"))
                };
                HtmlSender.Request(JsonConvert.SerializeObject(content3), url, "application/json", Encoding.UTF8, "POST");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每天补充15分K有误", ex);
            }

            try
            {
                var content2 = new
                {
                    DataType = 3,
                    StartGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd0930")),
                    EndGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd1500"))
                };
                HtmlSender.Request(JsonConvert.SerializeObject(content2), url, "application/json", Encoding.UTF8, "POST");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每天补充5分K有误", ex);
            }

            try
            {
                var content1 = new
                {
                    DataType = 2,
                    StartGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd0930")),
                    EndGroupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd1500"))
                };
                HtmlSender.Request(JsonConvert.SerializeObject(content1), url, "application/json", Encoding.UTF8, "POST");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("每天补充1分K有误", ex);
            }
        }

        /// <summary>
        /// 执行业务
        /// </summary>
        /// <returns></returns>
        private void DoBusiness(SecurityBarsDataTaskQueueInfo receivedObj, int msgId)
        {
            if (msgId == 1 && receivedObj.HandlerType == 1)
            {
                if (dataObj.TaskGuid != receivedObj.TaskGuid)
                {
                    return;
                }
                foreach (var item in receivedObj.PackageList)
                {
                    if (dataObj.PackageList.ContainsKey(item.Key))
                    {
                        dataObj.PackageList[item.Key].DataList.AddRange(item.Value.DataList);
                    }
                    else
                    {
                        dataObj.PackageList.Add(item.Key, new SecurityBarsDataType
                        {
                            DataType = item.Key,
                            DataIndex = 0,
                            DataList = item.Value.DataList
                        });
                    }
                }
                dataObj.CallBackPacketCount++;
                if (dataObj.CallBackPacketCount < dataObj.TotalPacketCount)//接受包数量不正确
                {
                    return;
                }
            }

            else if (receivedObj.HandlerType == 2 || receivedObj.HandlerType==21)
            {
                dataObj.TaskGuid = receivedObj.TaskGuid;
                dataObj.TaskTimeOut = -1;
                dataObj.HandlerType = receivedObj.HandlerType;
                dataObj.PackageList = receivedObj.PackageList;
                dataObj.SuccessPackageList = receivedObj.SuccessPackageList;
                //需要重新获取的重新获取
                if (receivedObj.FailPackageList.Count() > 0 && receivedObj.RetryCount <= receivedObj.TotalRetryCount)
                {
                    Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new
                    {
                        TaskGuid = receivedObj.TaskGuid,
                        HandlerType = receivedObj.HandlerType,
                        RetryCount = receivedObj.RetryCount+1,
                        TotalRetryCount = receivedObj.TotalRetryCount,
                        PackageList = receivedObj.FailPackageList
                    })), "SecurityBars", "1min");
                }
            }

            bool isFinish = false;

            if (UpdateToDataBase(ref isFinish))
            {
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
            }

            if (isFinish)
            {
                //通知板块
                if (dataObj.HandlerType == 21)//当天更新，立即通知板块
                {

                }
                else if (dataObj.HandlerType == 2)//形成通知板块指令
                {

                }
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

            int finishCount = 0;
            int taskCount = dataObj.PackageList.Count();
            Task[] taskArr = new Task[taskCount];
            int i = 0;
            foreach (var dataDic in dataObj.PackageList)
            {
                int dataType = dataDic.Key;
                int dataIndex = dataDic.Value.DataIndex;
                var dataList = dataDic.Value.DataList;
                taskArr[i] = Task.Factory.StartNew(() =>
                {
                    string tableName = "";
                    long groupTimeKey = 0;
                    if (!CheckDataType(dataType, ref tableName, ref groupTimeKey))
                    {
                        finishCount++;
                        return;
                    }
                    var disList = dataList.Skip(dataIndex).Take(Singleton.Instance.SecurityBarsUpdateCountOnce).ToList();
                    int disCount = disList.Count();
                    int totalCount = dataList.Count();
                    if (disCount <= 0)
                    {
                        finishCount++;
                        return;
                    }

                    bool isSuccess = false;
                    var disResultList = (from item in disList
                                         join item2 in Singleton.Instance._sharesBaseSession.GetSessionData() on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                                         from ai in a.DefaultIfEmpty()
                                         select new { item, ai }).ToList();
                    disResultList = (from item in disResultList
                                     group item by new { item.item.Market, item.item.SharesCode, item.item.GroupTimeKey } into g
                                     select g.FirstOrDefault()).ToList();
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
                        table.Columns.Add("IsLast", typeof(bool));
                        table.Columns.Add("YestodayClosedPrice", typeof(long));
                        table.Columns.Add("IsVaild", typeof(bool));
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
                            row["LastTradeStock"] = item.item.LastTradeStock;
                            row["LastTradeAmount"] = item.item.LastTradeAmount;
                            row["Tradable"] = item.ai == null ? 0 : item.ai.CirculatingCapital;
                            row["TotalCapital"] = item.ai == null ? 0 : item.ai.TotalCapital;
                            row["HandCount"] = item.ai == null ? 0 : item.ai.SharesHandCount;
                            row["LastModified"] = DateTime.Now;
                            row["IsLast"] = item.item.IsLast;
                            row["YestodayClosedPrice"] = item.item.YestodayClosedPrice;
                            row["IsVaild"] = item.item.IsVaild;
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

                                SqlParameter dataType_parameter = new SqlParameter("@dataType", SqlDbType.Int);
                                dataType_parameter.Value = dataType;
                                db.Database.ExecuteSqlCommand("exec P_Shares_SecurityBarsData_Update @sharesSecurityBarsData,@dataType", parameter, dataType_parameter);
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

                    bool finish = false;
                    if (isSuccess)
                    {
                        dataObj.PackageList[dataType].DataIndex = dataIndex + disCount;
                        if (totalCount <= dataIndex + disCount)
                        {
                            finishCount++;
                            finish = true;
                        }
                        if (dataObj.HandlerType == 1)
                        {
                            var sessionDataList = (from item in disResultList
                                                   select new SharesKlineData
                                                   {
                                                       SharesCode = item.item.SharesCode,
                                                       LastTradeStock = item.item.LastTradeStock,
                                                       ClosedPrice = item.item.ClosedPrice,
                                                       GroupTimeKey = item.item.GroupTimeKey,
                                                       TradeStock = item.item.TradeStock,
                                                       LastTradeAmount = item.item.LastTradeAmount,
                                                       Market = item.item.Market,
                                                       MaxPrice = item.item.MaxPrice,
                                                       MinPrice = item.item.MinPrice,
                                                       OpenedPrice = item.item.OpenedPrice,
                                                       PreClosePrice = item.item.PreClosePrice,
                                                       Time = item.item.Time,
                                                       TotalCapital = item.ai == null ? 0 : item.ai.TotalCapital,
                                                       Tradable = item.ai == null ? 0 : item.ai.CirculatingCapital,
                                                       TradeAmount = item.item.TradeAmount,
                                                       YestodayClosedPrice = item.item.YestodayClosedPrice
                                                   }).ToList();
                            Singleton.Instance._newIindexSecurityBarsDataTask.ToPushData(new SharesKlineDataContain
                            {
                                IsFinish = finish,
                                DataType = dataType,
                                SharesKlineData = sessionDataList
                            });
                        }
                    }
                    else
                    {
                        finishCount++;
                    }
                });
                i++;
            }
            Task.WaitAll(taskArr);
            if (finishCount >= taskCount)
            {
                isFinish = true;
            }
            return true;
        }

        /// <summary>
        /// 推送需要获取K线的股票到队列
        /// </summary>
        /// <param name="taskGuid"></param>
        /// <returns></returns>
        private int SendTransactionShares(string taskGuid)
        {
            List<SecurityBarsLastDataGroup> list = new List<SecurityBarsLastDataGroup>();
            List<int> dataTypeList = Singleton.Instance.SecurityBarsDataTypeList;
            foreach (int dataType in dataTypeList)
            {
                string tableName = "";
                long groupTimeKey = 0;
                if (!CheckDataType(dataType, ref tableName, ref groupTimeKey))
                {
                    continue;
                }
                list.Add(new SecurityBarsLastDataGroup
                {
                    DataType = dataType,
                    GroupTimeKey= groupTimeKey
                });
            }
            List<string> sharesList = new List<string>();
            //sharesList.Add("002707");
            //sharesList.Add("000796");
            //sharesList.Add("000978");
            //sharesList.Add("600640");
            //sharesList.Add("300005");
            //sharesList.Add("000917");
            //sharesList.Add("000524");
            //sharesList.Add("600138");
            //sharesList.Add("300178");
            //sharesList.Add("002489");
            //sharesList.Add("600258");
            //sharesList.Add("002558");
            //sharesList.Add("603199");
            //sharesList.Add("300133");
            var allSharesList = (from item in Singleton.Instance._sharesBaseSession.GetSessionData()
                                 where item.PresentPrice>0 //&& sharesList.Contains(item.SharesCode)
                                 select new
                                 {
                                     Market = item.Market,
                                     SharesCode = item.SharesCode,
                                     SharesCodeNum=long.Parse(item.SharesCode),
                                     PreClosePrice = item.ClosedPrice
                                 }).ToList();
            int totalCount = allSharesList.Count();
            //批次数
            int HandlerCount = Singleton.Instance.SecurityBarsBatchCount;
            int batchSize = totalCount / HandlerCount;
            if (totalCount % HandlerCount != 0)
            {
                batchSize = batchSize + 1;
            }

            //Singleton.Instance.mqHandler.ClearQueueData("SecurityBars_1min");
            for (int size = 0; size < batchSize; size++)
            {
                var batchList = allSharesList.Skip(size * HandlerCount).Take(HandlerCount).ToList();
                List<dynamic> sendList = new List<dynamic>();
                foreach (var item in list)
                {
                    List<dynamic> dataList = new List<dynamic>();
                    foreach (var x in batchList)
                    {
                        long key = x.SharesCodeNum * 1000 + x.Market * 100 + item.DataType;
                        var lastData=Singleton.Instance._newIindexSecurityBarsDataTask.GetSharesKlineLastSession(key);
                        dataList.Add(new 
                        {
                            SharesCode = x.SharesCode,
                            Market = x.Market,
                            StartTimeKey = lastData == null ? item.GroupTimeKey : lastData.GroupTimeKey,
                            EndTimeKey = -1,
                            PreClosePrice = lastData == null ? x.PreClosePrice : lastData.PreClosePrice,
                            YestodayClosedPrice = x.PreClosePrice,
                            LastTradeStock = lastData == null ? 0 : lastData.LastTradeStock,
                            LastTradeAmount = lastData == null ? 0 : lastData.LastTradeAmount
                        });
                    }
                    sendList.Add(new
                    {
                        DataType = item.DataType,
                        SecurityBarsGetCount = 0,
                        DataList = dataList,
                    });
                }
                Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new
                {
                    TaskGuid = taskGuid,
                    HandlerType = 1,
                    RetryCount=1,
                    TotalRetryCount=1,
                    PackageList = sendList
                })), "SecurityBars", "1min");
            }
            return batchSize;
        }

        private static bool CheckDataType(int dataType, ref string tableName, ref long groupTimeKey)
        {
            DateTime dateNow = DateTime.Now.Date;
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
                    return false;
            }
            return true;
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
