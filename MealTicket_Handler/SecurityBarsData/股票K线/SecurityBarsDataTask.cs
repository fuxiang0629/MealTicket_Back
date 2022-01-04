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
        Thread TaskHandle;

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
                PackageList = new Dictionary<int, SecurityBarsDataType>(),
                SuccessPackageList = new List<SecurityBarsDataParList>(),
                FailPackageList = new List<SecurityBarsDataParList>()
            };
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        public void DoTask()
        {
            TaskHandle = new Thread(() =>
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
            TaskHandle.Start();
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
            if (dataObj.PackageList != null)
            {
                dataObj.PackageList.Clear();
            }
            if (dataObj.SuccessPackageList != null)
            {
                dataObj.SuccessPackageList.Clear();
            }
            if (dataObj.FailPackageList != null)
            {
                dataObj.FailPackageList.Clear();
            }
            dataObj.TaskGuid = null;
            dataObj.StartTime = null;
            dataObj.TotalPacketCount = 0;
            dataObj.CallBackPacketCount = 0;
            dataObj.HandlerType = 1;
        }

        /// <summary>
        /// 需要更新K线的股票数据扔到队列
        /// </summary>
        private bool PushToDataUpDate(SecurityBarsDataTaskQueueInfo dataObj)
        {
            try
            {
                DateTime timeNow = DateTime.Now;
                if (!DbHelper.CheckTradeDate(timeNow))
                {
                    dataObj.TaskTimeOut = Singleton.Instance.SecurityBarsTaskTimeout;
                    return false;
                }
                if (!DbHelper.CheckTradeTime7(timeNow))
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

            else if (receivedObj.HandlerType == 2 || receivedObj.HandlerType == 21)
            {
                dataObj.TaskGuid = receivedObj.TaskGuid;
                dataObj.TaskTimeOut = -1;
                dataObj.HandlerType = receivedObj.HandlerType;
                dataObj.PackageList = receivedObj.PackageList;
                dataObj.SuccessPackageList = receivedObj.SuccessPackageList;
                dataObj.FailPackageList = receivedObj.FailPackageList;
                dataObj.RetryCount = receivedObj.RetryCount;
                dataObj.TotalRetryCount = receivedObj.TotalRetryCount;
                //需要重新获取的重新获取
                if (receivedObj.FailPackageList.Count() > 0 && receivedObj.RetryCount <= receivedObj.TotalRetryCount)
                {
                    Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new
                    {
                        TaskGuid = receivedObj.TaskGuid,
                        HandlerType = receivedObj.HandlerType,
                        RetryCount = receivedObj.RetryCount + 1,
                        TotalRetryCount = receivedObj.TotalRetryCount,
                        PackageList = receivedObj.FailPackageList
                    })), "SecurityBars", "1min");
                }
            }

            UpdateToDataBase();

            //通知板块
            if (dataObj.HandlerType == 2 || dataObj.HandlerType == 21)
            {
                pushNoticeToPlate(dataObj.HandlerType, dataObj.SuccessPackageList);
            }
            ClearTaskInfo(dataObj);//超时则清空任务
            dataObj.TaskTimeOut = Singleton.Instance.SecurityBarsTaskTimeout;
        }

        /// <summary>
        /// 通知板块
        /// </summary>
        private void pushNoticeToPlate(int HandlerType,List<SecurityBarsDataParList> SuccessPackageList) 
        {
            List<SecurityBarsDataParList> list = new List<SecurityBarsDataParList>(SuccessPackageList); 
            Singleton.Instance._newIindexSecurityBarsDataTask.ToReceiveSharesRetry(HandlerType, list);
        }

        /// <summary>
        /// 数据更新到数据库
        /// </summary>
        /// <returns></returns>
        private void UpdateToDataBase()
        {
            int taskCount = dataObj.PackageList.Count();
            Task[] taskArr = new Task[taskCount];
            int i = 0;
            foreach (var dataDic in dataObj.PackageList)
            {
                int dataType = dataDic.Key;
                var dataList = dataDic.Value.DataList;
                int haldlerType = dataObj.HandlerType;
                taskArr[i] = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Dictionary<long, SecurityBarsImportData> successData = _updateToDataBase(dataList, dataType);
                        if (successData.Count()==0)
                        {
                            return;
                        }
                        if (haldlerType == 1)
                        {
                            List<SharesKlineData> sessionDataList = new List<SharesKlineData>();
                            foreach (var item in successData)
                            {
                                sessionDataList.Add(new SharesKlineData {
                                    SharesCode = item.Value.SharesCode,
                                    LastTradeStock = item.Value.LastTradeStock,
                                    ClosedPrice = item.Value.ClosedPrice,
                                    GroupTimeKey = item.Value.GroupTimeKey,
                                    TradeStock = item.Value.TradeStock,
                                    LastTradeAmount = item.Value.LastTradeAmount,
                                    Market = item.Value.Market,
                                    MaxPrice = item.Value.MaxPrice,
                                    MinPrice = item.Value.MinPrice,
                                    OpenedPrice = item.Value.OpenedPrice,
                                    PreClosePrice = item.Value.PreClosePrice,
                                    Time = item.Value.Time,
                                    TotalCapital = item.Value.TotalCapital,
                                    Tradable = item.Value.Tradable,
                                    TradeAmount = item.Value.TradeAmount,
                                    YestodayClosedPrice = item.Value.YestodayClosedPrice
                                });
                            }
                            Singleton.Instance._newIindexSecurityBarsDataTask.ToPushData(new SharesKlineDataContain
                            {
                                DataType = dataType,
                                IsFinish=true,
                                SharesKlineData = sessionDataList
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("导入K线数据出错",ex);
                    }
                });
                i++;
            }
            Task.WaitAll(taskArr);
        }

        private Dictionary<long, SecurityBarsImportData> _updateToDataBase(List<SecurityBarsDataInfo> datalist, int dataType)
        {
            int totalCount = datalist.Count;
            if (totalCount <= 0)
            {
                return new Dictionary<long, SecurityBarsImportData>();
            }

            int dataIndex = 0;
            var sharesBaseDic = Singleton.Instance._sharesBaseSession.GetSessionData().ToDictionary(k => int.Parse(k.SharesCode) * 10 + k.Market, v => v);
            Dictionary<long, SecurityBarsImportData> totalImportDic = new Dictionary<long, SecurityBarsImportData>();
            List<SecurityBarsDataInfo> tempImportList = new List<SecurityBarsDataInfo>();
            foreach (var data in datalist)
            {
                tempImportList.Add(data);
                dataIndex++;
                if (tempImportList.Count() < Singleton.Instance.SecurityBarsUpdateCountOnce && dataIndex < totalCount)
                {
                    continue;
                }

                Dictionary<long, SecurityBarsImportData> importDic = new Dictionary<long, SecurityBarsImportData>();
                foreach (var item in tempImportList)
                {
                    int sharesKey = int.Parse(item.SharesCode) * 10 + item.Market;
                    if (!sharesBaseDic.ContainsKey(sharesKey))
                    {
                        continue;
                    }
                    var sharesInfo = sharesBaseDic[sharesKey];
                    long disKey = item.GroupTimeKey * 10000000 + long.Parse(item.SharesCode) * 10 + item.Market;
                    importDic[disKey] = new SecurityBarsImportData
                    {
                        SharesCode = item.SharesCode,
                        LastTradeStock = item.LastTradeStock,
                        TimeStr = item.TimeStr,
                        TradeStock = item.TradeStock,
                        ClosedPrice = item.ClosedPrice,
                        GroupTimeKey = item.GroupTimeKey,
                        HandCount = sharesInfo.SharesHandCount,
                        IsLast = item.IsLast,
                        IsVaild = item.IsVaild,
                        LastTradeAmount = item.LastTradeAmount,
                        Market = item.Market,
                        MaxPrice = item.MaxPrice,
                        MinPrice = item.MinPrice,
                        OpenedPrice = item.OpenedPrice,
                        PreClosePrice = item.PreClosePrice,
                        Time = item.Time.Value,
                        TotalCapital = sharesInfo.TotalCapital,
                        Tradable = sharesInfo.CirculatingCapital,
                        TradeAmount = item.TradeAmount,
                        YestodayClosedPrice = item.YestodayClosedPrice
                    };
                }

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
                foreach (var item in importDic)
                {
                    DataRow row = table.NewRow();
                    row["Market"] = item.Value.Market;
                    row["SharesCode"] = item.Value.SharesCode;
                    row["GroupTimeKey"] = item.Value.GroupTimeKey;
                    row["Time"] = item.Value.Time;
                    row["TimeStr"] = item.Value.TimeStr;
                    row["OpenedPrice"] = item.Value.OpenedPrice;
                    row["ClosedPrice"] = item.Value.ClosedPrice;
                    row["PreClosePrice"] = item.Value.PreClosePrice;
                    row["MinPrice"] = item.Value.MinPrice;
                    row["MaxPrice"] = item.Value.MaxPrice;
                    row["TradeStock"] = item.Value.TradeStock;
                    row["TradeAmount"] = item.Value.TradeAmount;
                    row["LastTradeStock"] = item.Value.LastTradeStock;
                    row["LastTradeAmount"] = item.Value.LastTradeAmount;
                    row["Tradable"] = item.Value.Tradable;
                    row["TotalCapital"] = item.Value.TotalCapital;
                    row["HandCount"] = item.Value.HandCount;
                    row["LastModified"] = DateTime.Now;
                    row["IsLast"] = item.Value.IsLast;
                    row["YestodayClosedPrice"] = item.Value.YestodayClosedPrice;
                    row["IsVaild"] = item.Value.IsVaild;
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

                        foreach (var item in importDic)
                        {
                            totalImportDic[item.Key] = item.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("更新K线数据出错", ex);
                        tran.Rollback();
                        break;
                    }
                }

                tempImportList.Clear();
            }

            return totalImportDic;
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

            DateTime dateNow = DateTime.Now.Date;
            List<string> sharesList = new List<string>();
            var allSharesList = (from item in Singleton.Instance._SharesQuotesSession.GetSessionData()
                                 select new
                                 {
                                     Market = item.Market,
                                     SharesCode = item.SharesCode,
                                     SharesCodeNum = long.Parse(item.SharesCode),
                                     PreClosePrice = item.LastModified > dateNow ? item.ClosedPrice : item.PresentPrice
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
                            StartTimeKey = (lastData == null || lastData.GroupTimeKey==0)? item.GroupTimeKey : lastData.GroupTimeKey,
                            EndTimeKey = -1,
                            PreClosePrice = (lastData == null || lastData.PreClosePrice==0) ? x.PreClosePrice : lastData.PreClosePrice,
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
            if (TaskHandle != null)
            {
                SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                {
                    MsgId = -1,
                    MsgObj = null
                }, true, 0);
                TaskHandle.Join();
            }
            if (SecurityBarsDataQueue != null)
            {
                SecurityBarsDataQueue.Release();
            }
        }
    }
}
