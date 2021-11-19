using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Handler.RunnerHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_Handler.SecurityBarsData
{
    public class IndexSecurityBarsDataTask
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
                HandlerType = 3,
                PackageList = new Dictionary<int, SecurityBarsDataType>()
            };
            UpdateTodayPlateRelSnapshot();
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
                                Logger.WriteFileLog("===开始计算==="+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                                PushToDataUpDate(dataObj);
                                Logger.WriteFileLog("===结束计算===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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
            dataObj.HandlerType = 3;
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
                if (!RunnerHelper.CheckTradeTime2(timeNow, false, true, false) && !RunnerHelper.CheckTradeTime2(timeNow.AddSeconds(-600), false, true, false))
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
                    //调用后台接口发送补全命令
                    //======未完成=======


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
                    DataList = GetSecurityBarsLastData_Session(dataType),
                    GroupTimeKey = groupTimeKey
                });
            }

            DateTime timeNow = DateTime.Now;
            var allSharesList = (from item in Singleton.Instance._SharesPlateSession.GetSessionData()
                                 where item.BaseDate != null && timeNow > item.BaseDate
                                 select new
                                 {
                                     PlateId = item.PlateId,
                                     BaseDate = item.BaseDate,
                                     WeightMarket = item.WeightMarket,
                                     WeightSharesCode = item.WeightSharesCode,
                                     NoWeightMarket = item.NoWeightMarket,
                                     NoWeightSharesCode = item.NoWeightSharesCode,
                                 }).ToList();

            List<dynamic> calList = new List<dynamic>();

            List<dynamic> sendList = new List<dynamic>();
            foreach (var par in list)
            {
                List<dynamic> DataList = new List<dynamic>();
                foreach (var item in allSharesList)
                {
                    var ai2 = (from x in par.DataList
                               where x.PlateId == item.PlateId && x.WeightType == 2
                               select x).FirstOrDefault();
                    if (!string.IsNullOrEmpty(item.WeightSharesCode))
                    {
                        DataList.Add(new
                        {
                            PlateId = item.PlateId,
                            SharesCode = item.WeightSharesCode,
                            Market = item.WeightMarket,
                            WeightType = 2,
                            StartTimeKey = ai2 == null ? par.GroupTimeKey : ai2.GroupTimeKey,
                            EndTimeKey = -1,
                            PreClosePrice = ai2 == null ? 0 : ai2.PreClosePrice,
                            YestodayClosedPrice = ai2 == null ? 0 : ai2.YestodayClosedPrice,
                            LastTradeStock = ai2 == null ? 0 : ai2.LastTradeStock,
                            LastTradeAmount = ai2 == null ? 0 : ai2.LastTradeAmount
                        });
                    }
                    else
                    {
                        calList.Add(new
                        {
                            PlateId = item.PlateId,
                            WeightType = 2,
                            DataType = par.DataType,
                            StartDate = ai2 == null ? par.GroupTimeKey : ai2.GroupTimeKey,
                            EndDate = -1,
                        });
                    }
                    var ai1 = (from x in par.DataList
                               where x.PlateId == item.PlateId && x.WeightType == 1
                               select x).FirstOrDefault();
                    long NoWeightPreClosePrice = 1000;
                    if (!string.IsNullOrEmpty(item.NoWeightSharesCode))
                    {
                        DataList.Add(new
                        {
                            PlateId = item.PlateId,
                            SharesCode = item.NoWeightSharesCode,
                            Market = item.NoWeightMarket,
                            WeightType = 1,
                            StartTimeKey = ai1 == null ? par.GroupTimeKey : ai1.GroupTimeKey,
                            EndTimeKey = -1,
                            PreClosePrice = ai1 == null ? 0 : ai1.PreClosePrice,
                            YestodayClosedPrice = ai1 == null ? 0 : ai1.YestodayClosedPrice,
                            LastTradeStock = ai1 == null ? 0 : ai1.LastTradeStock,
                            LastTradeAmount = ai1 == null ? 0 : ai1.LastTradeAmount
                        });
                    }
                    else
                    {
                        calList.Add(new
                        {
                            PlateId = item.PlateId,
                            WeightType = 1,
                            DataType = par.DataType,
                            StartDate = ai1 == null ? par.GroupTimeKey : ai1.GroupTimeKey,
                            EndDate = -1,
                        });
                    }
                }
                sendList.Add(new
                {
                    DataType = par.DataType,
                    SecurityBarsGetCount = 0,
                    DataList = DataList
                });
            }

            SendToGetPlateKLine(sendList, 3, taskGuid);

            using (var db = new meal_ticketEntities())
            {
                Logger.WriteFileLog("===2.开始计算板块===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
                foreach (var item in calList)
                {
                    CalculateKLine(item.PlateId, item.WeightType, item.DataType, item.StartDate, item.EndDate, db, true);
                }
                Logger.WriteFileLog("===2.结束计算板块===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            }
            return 1;
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
                Logger.WriteFileLog("每天补充年K有误", ex);
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
            Logger.WriteFileLog("===3.收到结果数据开始入库===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
            if (msgId == 1 && receivedObj.HandlerType ==3)
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

            else if (receivedObj.HandlerType == 4)
            {
                dataObj.TaskTimeOut = -1;
                dataObj.HandlerType = receivedObj.HandlerType;
                dataObj.PackageList = receivedObj.PackageList;
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
                ClearTaskInfo(dataObj);//超时则清空任务
                dataObj.TaskTimeOut = Singleton.Instance.SecurityBarsTaskTimeout;
            }
            Logger.WriteFileLog("===3.处理入库完成===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), null);
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
                                     group item by new {item.PlateId,item.WeightType,item.GroupTimeKey } into g
                                     select g.FirstOrDefault()).ToList();
                    try
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
                        foreach (var item in disResultList)
                        {
                            DataRow row = table.NewRow();
                            row["WeightType"] = item.WeightType;
                            row["PlateId"] = item.PlateId;
                            row["Market"] = item.Market;
                            row["SharesCode"] = item.SharesCode??"";
                            row["GroupTimeKey"] = item.GroupTimeKey;
                            row["Time"] = item.Time;
                            row["OpenedPrice"] = item.OpenedPrice;
                            row["ClosedPrice"] = item.ClosedPrice;
                            row["PreClosePrice"] = item.PreClosePrice;
                            row["MinPrice"] = item.MinPrice;
                            row["MaxPrice"] = item.MaxPrice;
                            row["TradeStock"] = item.TradeStock;
                            row["TradeAmount"] = item.TradeAmount;
                            row["LastTradeStock"] = item.LastTradeStock;
                            row["LastTradeAmount"] = item.LastTradeAmount;
                            row["Tradable"] = 0;
                            row["TotalCapital"] = 0;
                            row["HandCount"] = 100;
                            row["LastModified"] = DateTime.Now;
                            row["IsLast"] = item.IsLast;
                            row["YestodayClosedPrice"] = item.YestodayClosedPrice;
                            table.Rows.Add(row);
                        }

                        using (var db = new meal_ticketEntities())
                        using (var tran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                //关键是类型
                                SqlParameter parameter = new SqlParameter("@sharesPlateSecurityBarsData", SqlDbType.Structured);
                                //必须指定表类型名
                                parameter.TypeName = "dbo.SharesPlateSecurityBarsData";
                                //赋值
                                parameter.Value = table;

                                SqlParameter dataType_parameter = new SqlParameter("@dataType", SqlDbType.Int);
                                dataType_parameter.Value = dataType;
                                db.Database.ExecuteSqlCommand("exec P_Shares_Plate_SecurityBarsData_Update @sharesPlateSecurityBarsData,@dataType", parameter, dataType_parameter);
                                tran.Commit();
                                isSuccess = true;
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteFileLog("更新指数K线数据出错", ex);
                                tran.Rollback();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("更新指数K线数据失败", ex);
                    }

                    if (isSuccess)
                    {
                        dataObj.PackageList[dataType].DataIndex = dataIndex + disCount;
                        if (totalCount <= dataIndex + disCount)
                        {
                            finishCount++;
                        }
                        var lastData = (from x in disList
                                        group x by new { x.PlateId,x.WeightType } into g
                                        let temp = g.OrderByDescending(e => e.GroupTimeKey).FirstOrDefault()
                                        select new SecurityBarsLastData
                                        {
                                            PlateId=g.Key.PlateId,
                                            WeightType=g.Key.WeightType,
                                            SharesCode = temp.SharesCode,
                                            Market = temp.Market,
                                            GroupTimeKey = temp.GroupTimeKey,
                                            LastTradeStock = temp.LastTradeStock,
                                            LastTradeAmount = temp.LastTradeAmount,
                                            PreClosePrice = temp.PreClosePrice
                                        }).ToList();
                        SetSecurityBarsLastData_Session(dataType, lastData);
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

        private static bool CheckDataType(int dataType, ref string tableName, ref long groupTimeKey,DateTime? dateNow=null)
        {
            if (dateNow == null)
            {
                dateNow = DateTime.Now.Date;
            }
            switch (dataType)
            {
                case 2:
                    tableName = "t_shares_plate_securitybarsdata_1min";
                    groupTimeKey = long.Parse(dateNow.Value.ToString("yyyyMMdd0000"));
                    break;
                case 3:
                    tableName = "t_shares_plate_securitybarsdata_5min";
                    groupTimeKey = long.Parse(dateNow.Value.ToString("yyyyMMdd0000"));
                    break;
                case 4:
                    tableName = "t_shares_plate_securitybarsdata_15min";
                    groupTimeKey = long.Parse(dateNow.Value.ToString("yyyyMMdd0000"));
                    break;
                case 5:
                    tableName = "t_shares_plate_securitybarsdata_30min";
                    groupTimeKey = long.Parse(dateNow.Value.ToString("yyyyMMdd0000"));
                    break;
                case 6:
                    tableName = "t_shares_plate_securitybarsdata_60min";
                    groupTimeKey = long.Parse(dateNow.Value.ToString("yyyyMMdd0000"));
                    break;
                case 7:
                    tableName = "t_shares_plate_securitybarsdata_1day";
                    groupTimeKey = long.Parse(dateNow.Value.ToString("yyyyMMdd"));
                    break;
                case 8:
                    tableName = "t_shares_plate_securitybarsdata_1week";
                    groupTimeKey = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                    where item.the_date == int.Parse(dateNow.Value.ToString("yyyyMMdd"))
                                    select item.the_week).FirstOrDefault() ?? 0;
                    break;
                case 9:
                    tableName = "t_shares_plate_securitybarsdata_1month";
                    groupTimeKey = long.Parse(dateNow.Value.ToString("yyyyMM"));
                    break;
                case 10:
                    tableName = "t_shares_plate_securitybarsdata_1quarter";
                    groupTimeKey = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                    where item.the_date == int.Parse(dateNow.Value.ToString("yyyyMMdd"))
                                    select item.the_quarter).FirstOrDefault() ?? 0;
                    break;
                case 11:
                    tableName = "t_shares_plate_securitybarsdata_1year";
                    groupTimeKey = long.Parse(dateNow.Value.ToString("yyyy"));
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



        /// <summary>
        /// 今日快照缓存
        /// </summary>
        List<PlateRelSnapshotInfo> _plateRelSnapshotSession;
        ReaderWriterLock _plateRelSnapshotSessionLock = new ReaderWriterLock();
        List<PlateRelSnapshotInfo> PlateRelSnapshotSession 
        {
            get 
            {
                List<PlateRelSnapshotInfo> temp;
                _plateRelSnapshotSessionLock.AcquireReaderLock(-1);
                temp=_plateRelSnapshotSession;
                _plateRelSnapshotSessionLock.ReleaseReaderLock();
                return temp;
            }
            set 
            {
                _plateRelSnapshotSessionLock.AcquireWriterLock(-1);
                _plateRelSnapshotSession = value;
                _plateRelSnapshotSessionLock.ReleaseWriterLock();
            }
        }

        #region===各个类型最后一条K线数据缓存===
        /// <summary>
        /// K线数据缓存
        /// </summary>
        private Dictionary<int, List<SecurityBarsLastData>> securityBarsLastData_Session = new Dictionary<int, List<SecurityBarsLastData>>();
        private ReaderWriterLock _securityBarsLastData_SessionLock = new ReaderWriterLock();
        private List<SecurityBarsLastData> GetSecurityBarsLastData_Session(int dataType)
        {
            List<SecurityBarsLastData> tempData = new List<SecurityBarsLastData>();
            _securityBarsLastData_SessionLock.AcquireReaderLock(Timeout.Infinite);
            if (!securityBarsLastData_Session.TryGetValue(dataType, out tempData))
            {
                tempData = new List<SecurityBarsLastData>();
            }
            _securityBarsLastData_SessionLock.ReleaseReaderLock();
            return tempData;
        }

        private void SetSecurityBarsLastData_Session(int dataType, List<SecurityBarsLastData> dataList)
        {
            _securityBarsLastData_SessionLock.AcquireWriterLock(Timeout.Infinite);
            if (!securityBarsLastData_Session.ContainsKey(dataType))
            {
                securityBarsLastData_Session[dataType] = new List<SecurityBarsLastData>();
            }
            var tempData = securityBarsLastData_Session[dataType];
            var newList = dataList.Union(tempData).ToList();
            securityBarsLastData_Session[dataType] = newList;
            _securityBarsLastData_SessionLock.ReleaseWriterLock();
        }

        //加载当天最后一条数据缓存
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
                    List<SecurityBarsLastData> lastData = new List<SecurityBarsLastData>();
                    using (var db = new meal_ticketEntities())
                    {
                        string sql = string.Format(@"select PlateId,WeightType,GroupTimeKey,
case when GroupTimeKey>{1} then PreClosePrice else ClosedPrice end PreClosePrice,
case when GroupTimeKey>{1} then LastTradeStock else 0 end LastTradeStock,
case when GroupTimeKey>{1} then LastTradeAmount else 0 end LastTradeAmount,
case when GroupTimeKey>{1} then {2} else ClosedPrice end YestodayClosedPrice
from {0} with(nolock) where Id in
(
	select Max(Id) maxId
	from {0} with(nolock)
	group by PlateId,WeightType
)", tableName, groupTimeKey, dataType==2?"YestodayClosedPrice":"convert(bigint,0)");
                        lastData = db.Database.SqlQuery<SecurityBarsLastData>(sql).ToList();
                    }

                    SetSecurityBarsLastData_Session(dataType, lastData);
                });
            }
            Task.WaitAll(taskArr);
        }
        #endregion

        /// <summary>
        /// 当天股票K线数据缓存
        /// </summary>
        private Dictionary<int, List<SecurityBarsDataInfo>> sharesKline_Session = new Dictionary<int, List<SecurityBarsDataInfo>>();
        private ReaderWriterLock _sharesKline_SessionLock = new ReaderWriterLock();
        private List<SecurityBarsDataInfo> GetSharesKline_Session(int dataType)
        {
            List<SecurityBarsDataInfo> tempData = new List<SecurityBarsDataInfo>();
            _sharesKline_SessionLock.AcquireReaderLock(Timeout.Infinite);
            if (!sharesKline_Session.TryGetValue(dataType, out tempData))
            {
                tempData = new List<SecurityBarsDataInfo>();
            }
            _sharesKline_SessionLock.ReleaseReaderLock();
            return tempData;
        }
        public void SetSharesKline_Session(int dataType, List<SecurityBarsDataInfo> dataList)
        {
            _sharesKline_SessionLock.AcquireWriterLock(Timeout.Infinite);
            if (!sharesKline_Session.ContainsKey(dataType))
            {
                sharesKline_Session[dataType] = new List<SecurityBarsDataInfo>();
            }
            var tempData = sharesKline_Session[dataType];
            var newList = dataList.Union(tempData).ToList();

            //var newList = (from item in tempData
            //               join item2 in dataList on new { item.Market, item.SharesCode, item.GroupTimeKey } equals new { item2.Market, item2.SharesCode, item2.GroupTimeKey } into a
            //               from ai in a.DefaultIfEmpty()
            //               select new SecurityBarsDataInfo
            //               {
            //                   SharesCode = item.SharesCode,
            //                   Market = item.Market,
            //                   GroupTimeKey = item.GroupTimeKey,
            //                   Time = item.Time,
            //                   TotalCapital = item.TotalCapital,
            //                   Tradable = item.Tradable,
            //                   LastTradeStock = ai == null ? item.LastTradeStock : ai.LastTradeStock,
            //                   TradeStock = ai == null ? item.TradeStock : ai.TradeStock,
            //                   ClosedPrice = ai == null ? item.ClosedPrice : ai.ClosedPrice,
            //                   LastTradeAmount = ai == null ? item.LastTradeAmount : ai.LastTradeAmount,
            //                   MaxPrice = ai == null ? item.MaxPrice : ai.MaxPrice,
            //                   MinPrice = ai == null ? item.MinPrice : ai.MinPrice,
            //                   OpenedPrice = ai == null ? item.OpenedPrice : ai.OpenedPrice,
            //                   PreClosePrice = ai == null ? item.PreClosePrice : ai.PreClosePrice,
            //                   TradeAmount = ai == null ? item.TradeAmount : ai.TradeAmount,
            //                   YestodayClosedPrice = ai == null ? item.YestodayClosedPrice : ai.YestodayClosedPrice,
            //               }).ToList();
            sharesKline_Session[dataType] = newList;
            _sharesKline_SessionLock.ReleaseWriterLock();
        }

        Thread PlateRelSnapshotUpdateTask;
        ThreadMsgTemplate<int> PlateRelSnapshotUpdateQueue = new ThreadMsgTemplate<int>();

        /// <summary>
        /// 启动快照更新任务
        /// 每天凌晨执行一次
        /// </summary>
        public void DoPlateRelSnapshotUpdateTask()
        {
            PlateRelSnapshotUpdateTask = new Thread(() =>
            {
                do
                {
                    int queueData = 0;
                    if (PlateRelSnapshotUpdateQueue.WaitMessage(ref queueData,60000))
                    {
                        break;
                    }
                    //判断当前时间是否交易日
                    if (!RunnerHelper.CheckTradeDate())
                    {
                        continue;
                    }
                    //判断当前时间是执行时间
                    DateTime timeNow = DateTime.Now;
                    TimeSpan timeNowSpan = TimeSpan.Parse(timeNow.ToString("HH:mm:ss"));
                    if (timeNowSpan < TimeSpan.Parse("00:30:00") || timeNowSpan > TimeSpan.Parse("02:30:00"))
                    {
                        continue;
                    }
                    //更新今日快照
                    UpdateTodayPlateRelSnapshot();
                    //执行更新历史快照指令
                    if (!UpdateHisPlateRelSnapshot())
                    {
                        continue;
                    }
                    //解析指令命令
                    var instructions = GetInstructions();
                    foreach (var item in instructions)
                    {
                        //执行计算操作
                        if (!CalculateInstructions(item))
                        {
                            break;
                        }

                    }
                } while (true);
            });
            PlateRelSnapshotUpdateTask.Start();
        }

        /// <summary>
        /// 更新快照缓存(1.启动时调用 2.更新今日快照时调用)
        /// </summary>
        /// <returns></returns>
        public void LoadPlateRelSnapshotSession()
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var list = (from item in db.t_shares_plate_rel_snapshot
                            where item.Date == dateNow
                            select new PlateRelSnapshotInfo
                            {
                                SharesCode = item.SharesCode,
                                Market = item.Market,
                                PlateId = item.PlateId
                            }).ToList();
                PlateRelSnapshotSession = list;
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
            if (dateNow<= PlateRelSnapshotUpdateLastTime)
            {
                return;
            }
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
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
                        LoadPlateRelSnapshotSession();
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
                    LoadPlateRelSnapshotSession();
                    PlateRelSnapshotUpdateLastTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("更新当天板块内股票快照失败", ex);
                    tran.Rollback();
                }
            }
        }

        /// <summary>
        /// 更新历史板块内股票快照
        /// </summary>
        private bool UpdateHisPlateRelSnapshot() 
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var instructions = (from item in db.t_shares_plate_rel_snapshot_instructions
                                    where item.Type == 1 && item.IsExcute == false && item.Date <= dateNow
                                    orderby item.Date
                                    select item).ToList();
                foreach (var item in instructions)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //解析指令内容
                            var Context=JsonConvert.DeserializeObject<dynamic>(item.Context);
                            //删除该股票所在板块的所有历史快照
                            string sql = string.Format("delete t_shares_plate_rel_snapshot where Date<'{0}' and PlateId={1} and Market={2} and SharesCode='{3}'", dateNow, item.PlateId, item.Market, item.SharesCode);
                            db.Database.ExecuteSqlCommand(sql);
                            //重新添加该股票所在板块的所有快照
                            DateTime StartDate = Context.StartDate;
                            DateTime EndDate = Context.EndDate;
                            //查询股票上市时间
                            var sharesInfo = (from x in Singleton.Instance._sharesBaseSession.GetSessionData()
                                              where x.Market == item.Market && x.SharesCode == item.SharesCode
                                              select x).FirstOrDefault();
                            if (sharesInfo == null || sharesInfo.MarketTime == null)
                            {
                                StartDate = dateNow;
                            }
                            else
                            {
                                DateTime marketTime = sharesInfo.MarketTime.Value;
                                RunnerHelper.GetTradeDate(ref marketTime, 4);
                                if (marketTime.Date > StartDate)
                                {
                                    StartDate = marketTime.Date;
                                }
                            }
                            if (EndDate >= dateNow)
                            {
                                EndDate = dateNow.AddDays(-1);
                            }
                            while (StartDate <= EndDate)
                            {
                                //判断是否交易日
                                if (!RunnerHelper.CheckTradeDate(StartDate))
                                {
                                    StartDate = StartDate.AddDays(1);
                                    continue;
                                }
                                sql = string.Format("insert into t_shares_plate_rel_snapshot([Date],PlateId,Market,SharesCode) values('{0}',{1},{2},'{3}')", StartDate, item.PlateId, item.Market, item.SharesCode);
                                db.Database.ExecuteSqlCommand(sql);

                                StartDate = StartDate.AddDays(1);
                            }

                            //foreach (int type in Singleton.Instance.SecurityBarsDataTypeList)
                            //{
                            //    //自动生成重置命令
                            //    sql = string.Format("merge into t_shares_plate_rel_snapshot_instructions t using (select '{0}' [Date],{1} [Type],'{2}' Context, {3} DataType,{4} PlateType) t1 on t.[Date] = t1.[Date] and t.[Type]= t1.[Type] and t.DataType=t1.DataType and t.PlateType=t1.PlateType when matched then update set Context = t1.Context, IsExcute = 0, ExcuteTime = null, LastModified = getdate() when not matched then insert([Date],[Type],PlateId,Market,SharesCode,DataType,Context,PlateType,IsExcute,ExcuteTime,CreateTime,LastModified) values(t1.[Date], t1.[Type], 0, 0, '',t1.DataType, t1.Context,t1.PlateType, 0, null, getdate(), getdate()); ", dateNow, type, context, dataType, plateType);
                            //    db.Database.ExecuteSqlCommand(sql);
                            //}

                            //更新执行状态
                            item.IsExcute = true;
                            item.ExcuteTime = DateTime.Now;
                            db.SaveChanges();
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("更新历史板块内股票快照出错", ex);
                            tran.Rollback();
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 获取重置指令数据
        /// </summary>
        private List<t_shares_plate_rel_snapshot_instructions> GetInstructions()
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var Instructions = (from item in db.t_shares_plate_rel_snapshot_instructions
                                    where item.IsExcute == false && (item.Type == 2 || item.Type == 3) && item.Date <= dateNow
                                    orderby item.Date
                                    select item).ToList();
                return Instructions;
            }
        }

        /// <summary>
        /// 执行指令
        /// </summary>
        /// <returns></returns>
        private bool CalculateInstructions(t_shares_plate_rel_snapshot_instructions instructions) 
        {
            DateTime date = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    var Context = JsonConvert.DeserializeObject<dynamic>(instructions.Context);
                    long startDate = Context.StartDate;
                    long endDate = Context.EndDate;

                    var plate = (from item in Singleton.Instance._SharesPlateSession.GetSessionData()
                                 where item.BaseDate != null && date >= item.BaseDate
                                 select item).ToList();


                    List<dynamic> list = new List<dynamic>();
                    if (instructions.Type == 2)//重置整个板块
                    {
                        plate = (from item in plate
                                 where item.PlateType == instructions.PlateType
                                 select item).ToList();
                        list=ResetAllPlate(plate, instructions.DataType, startDate, endDate, db);
                    }
                    else if (instructions.Type == 3)//重置指定板块
                    {
                        var calPlate = (from item in plate
                                        where item.PlateId == instructions.PlateId
                                        select item).FirstOrDefault();
                        if (calPlate != null)
                        {
                            list = ResetPlate(calPlate, instructions.DataType, startDate, endDate, db);
                        }
                    }

                    string sql = string.Format("update t_shares_plate_rel_snapshot_instructions set IsExcute=1,ExcuteTime=getdate() where Id={0}", instructions.Id);
                    db.Database.ExecuteSqlCommand(sql);

                    if (list.Count() > 0)
                    {
                        var sendList = new List<dynamic>
                        {
                            new
                            {
                                DataType = instructions.DataType,
                                SecurityBarsGetCount = 0,
                                DataList = list
                            }
                        };
                        SendToGetPlateKLine(sendList, 4, "");
                    }

                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("执行指令失败",ex);
                    tran.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// 重置所有板块
        /// </summary>
        private List<dynamic> ResetAllPlate(List<SharesPlateInfo_Session> plate,int dataType,long startDate,long endDate,meal_ticketEntities db)
        {
            List<long> plateIdList = plate.Select(e => e.PlateId).ToList();
            var plateCloseList = GetPlateClosePrice(dataType, plateIdList, startDate);
            if (plateCloseList == null)
            {
                return new List<dynamic>();
            }
            List<dynamic> list = new List<dynamic>();
            foreach (var item in plate)
            {
                long timeGroupKey = 0;
                if (!ParseTimeGroupKey(item.BaseDate.Value, dataType, ref timeGroupKey))
                {
                    continue;
                }
                if (startDate < timeGroupKey)
                {
                    startDate = timeGroupKey;
                }

                long weightClosePrice = plateCloseList.Where(e => e.PlateId == item.PlateId && e.WeightType == 2).Select(e => e.ClosePrice).FirstOrDefault();
                if (weightClosePrice == 0)
                {
                    weightClosePrice = 1000;
                }
                if (!string.IsNullOrEmpty(item.WeightSharesCode))
                {
                    list.Add(new
                    {
                        PlateId = item.PlateId,
                        SharesCode = item.WeightSharesCode,
                        Market = item.WeightMarket,
                        WeightType = 2,
                        StartTimeKey = startDate,
                        EndTimeKey = endDate,
                        PreClosePrice = weightClosePrice,
                        YestodayClosedPrice = weightClosePrice,
                        LastTradeStock = 0,
                        LastTradeAmount = 0
                    });
                }
                else
                {
                    CalculateKLine(item.PlateId, 2, dataType, startDate, endDate,db,false);
                }

                long noWeightClosePrice = plateCloseList.Where(e => e.PlateId == item.PlateId && e.WeightType == 1).Select(e => e.ClosePrice).FirstOrDefault();
                if (noWeightClosePrice == 0)
                {
                    noWeightClosePrice = 1000;
                }
                if (!string.IsNullOrEmpty(item.NoWeightSharesCode))
                {
                    list.Add(new
                    {
                        PlateId = item.PlateId,
                        SharesCode = item.NoWeightSharesCode,
                        Market = item.NoWeightMarket,
                        WeightType = 1,
                        StartTimeKey = startDate,
                        EndTimeKey = endDate,
                        PreClosePrice = noWeightClosePrice,
                        YestodayClosedPrice = noWeightClosePrice,
                        LastTradeStock = 0,
                        LastTradeAmount = 0
                    });
                }
                else
                {
                    CalculateKLine(item.PlateId, 1, dataType, startDate, endDate, db, false);
                }
            }
            return list;
        }

        /// <summary>
        /// 重置某个板块
        /// </summary>
        /// <returns></returns>
        private List<dynamic> ResetPlate(SharesPlateInfo_Session plate, int dataType, long startDate, long endDate, meal_ticketEntities db)
        {
            List<long> plateIdList = new List<long> 
            {
                plate.PlateId
            };
            var plateCloseList = GetPlateClosePrice(dataType, plateIdList, startDate);
            if (plateCloseList == null)
            {
                return new List<dynamic>();
            }

            List<dynamic> list = new List<dynamic>();

            long timeGroupKey = 0;
            if (!ParseTimeGroupKey(plate.BaseDate.Value, dataType, ref timeGroupKey))
            {
                return new List<dynamic>();
            }

            if (startDate < timeGroupKey)
            {
                startDate = timeGroupKey;
            }

            long weightClosePrice = plateCloseList.Where(e => e.PlateId == plate.PlateId && e.WeightType == 2).Select(e => e.ClosePrice).FirstOrDefault();
            if (weightClosePrice == 0)
            {
                weightClosePrice = 1000;
            }
            if (!string.IsNullOrEmpty(plate.WeightSharesCode))
            {
                list.Add(new
                {
                    PlateId = plate.PlateId,
                    SharesCode = plate.NoWeightSharesCode,
                    Market = plate.NoWeightMarket,
                    WeightType = 2,
                    StartTimeKey = startDate,
                    EndTimeKey = endDate,
                    PreClosePrice = weightClosePrice,
                    YestodayClosedPrice = weightClosePrice,
                    LastTradeStock = 0,
                    LastTradeAmount = 0
                });
            }
            else
            {
                CalculateKLine(plate.PlateId,  2, dataType, startDate, endDate, db, false);
            }

            long noWeightClosePrice = plateCloseList.Where(e => e.PlateId == plate.PlateId && e.WeightType == 1).Select(e => e.ClosePrice).FirstOrDefault();
            if (noWeightClosePrice == 0)
            {
                noWeightClosePrice = 1000;
            }
            if (!string.IsNullOrEmpty(plate.NoWeightSharesCode))
            {
                list.Add(new
                {
                    PlateId = plate.PlateId,
                    SharesCode = plate.NoWeightSharesCode,
                    Market = plate.NoWeightMarket,
                    WeightType = 1,
                    StartTimeKey = startDate,
                    EndTimeKey = endDate,
                    PreClosePrice = noWeightClosePrice,
                    YestodayClosedPrice = noWeightClosePrice,
                    LastTradeStock = 0,
                    LastTradeAmount = 0
                });
            }
            else
            {
                CalculateKLine(plate.PlateId, 1, dataType, startDate, endDate, db, false);
            }

            return list;
        }

        /// <summary>
        /// 查询板块昨日收盘价
        /// </summary>
        /// <returns></returns>
        private List<PlateCloseInfo> GetPlateClosePrice(int dataType,List<long> plateId,long startDate) 
        {
            List<PlateCloseInfo> list = new List<PlateCloseInfo>();
            if (startDate == -1)
            {
                foreach (var item in plateId)
                {
                    list.Add(new PlateCloseInfo
                    {
                        ClosePrice = 1000,
                        PlateId = item,
                        WeightType = 1
                    });
                    list.Add(new PlateCloseInfo
                    {
                        ClosePrice = 1000,
                        PlateId = item,
                        WeightType = 2
                    });
                }
            }
            else
            {
                string tableName = "";
                if (!ParseTableName(dataType, ref tableName))
                {
                    return null;
                }

                string plateStr = string.Join(",", plateId.ToArray());
                using (var db = new meal_ticketEntities())
                {
                    string sql = string.Format("select ClosedPrice,PlateId,WeightType from {0} where PlateId in ({1}) and GroupTimeKey<{2}", tableName, plateStr, startDate);
                    list=db.Database.SqlQuery<PlateCloseInfo>(sql).ToList();
                }
            }
            return list;
        }

        /// <summary>
        /// 计算K线数据
        /// </summary>
        private void CalculateKLine(long plateId,int weightType,int dataType,long startDate,long endDate,meal_ticketEntities db,bool isReal)
        {
            if (isReal)
            {
                CalculateKLine_real(plateId, weightType, dataType, db);
            }
            //if (dataType == 2)
            //{
               
            //    else
            //    {
            //        CalculateKLine_1min(plateId,weightType, startDate, endDate, db);
            //    }
            //}
        }

        /// <summary>
        /// 实时计算一分钟K线
        /// </summary>
        private void CalculateKLine_real(long plateId, int weightType,int dataType, meal_ticketEntities db)
        {
            //1.获得今日快照数据
            var instructions = _plateRelSnapshotSession;
            //2.获取板块最后一笔K线数据
            var lastList = GetSecurityBarsLastData_Session(dataType);
            var last = (from item in lastList
                        where item.PlateId == plateId && item.WeightType == weightType
                        select item).FirstOrDefault();
            if (last == null)
            {
                long GroupTimeKey = 0;
                ParseTimeGroupKey(DateTime.Now.Date, dataType, ref GroupTimeKey);
                last = new SecurityBarsLastData
                {
                    LastTradeStock = 0,
                    PlateId = plateId,
                    GroupTimeKey = GroupTimeKey,
                    LastTradeAmount = 0,
                    PreClosePrice = 0,
                    WeightType = weightType
                };
            }
            //3.查询股票1分钟线，获取平均价格/市值
            var result = (from item in GetSharesKline_Session(dataType)
                          join item2 in instructions on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                          where item.GroupTimeKey >= last.GroupTimeKey && item2.PlateId== plateId
                          group item by new { item.GroupTimeKey, item.Time } into g
                          select new SecurityBarsDataInfo
                          {
                              GroupTimeKey = g.Key.GroupTimeKey,
                              Time = g.Key.Time,
                              LastTradeStock = g.Sum(e => e.LastTradeStock),
                              TradeStock = g.Sum(e => e.TradeStock),
                              LastTradeAmount = g.Sum(e => e.LastTradeAmount),
                              IsLast = g.First().IsLast,
                              PlateId = plateId,
                              WeightType = weightType,
                              TradeAmount = g.Sum(e => e.TradeAmount),
                              ClosedPrice = (long)(g.Average(e => weightType == 1 ? e.ClosedPrice : (e.ClosedPrice * e.TotalCapital))),
                              YestodayClosedPrice = (long)(g.Average(e => weightType == 1 ? e.YestodayClosedPrice : (e.YestodayClosedPrice * e.TotalCapital))),
                              MaxPrice = (long)(g.Average(e => weightType == 1 ? e.MaxPrice : (e.MaxPrice * e.TotalCapital))),
                              MinPrice = (long)(g.Average(e => weightType == 1 ? e.MinPrice : (e.MinPrice * e.TotalCapital))),
                              OpenedPrice = (long)(g.Average(e => weightType == 1 ? e.OpenedPrice : (e.OpenedPrice * e.TotalCapital))),
                              PreClosePrice = (long)(g.Average(e => weightType == 1 ? e.PreClosePrice : (e.PreClosePrice * e.TotalCapital)))
                          }).ToList();
            Dictionary<int, SecurityBarsDataType> dataTypeDic = new Dictionary<int, SecurityBarsDataType>();
            dataTypeDic.Add(dataType, new SecurityBarsDataType
            {
                DataType = dataType,
                DataList = result
            });
            SecurityBarsDataTaskQueueInfo sendList = new SecurityBarsDataTaskQueueInfo
            {
                PackageList = dataTypeDic,
                TaskGuid = "",
                HandlerType = 4,
            };
            SecurityBarsDataQueue.AddMessage(new QueueMsgObj
            {
                MsgId = 1,
                MsgObj = sendList
            });
        }

        /// <summary>
        /// 计算板块1分钟K线
        /// </summary>
        private void CalculateKLine_1min(long plateId, DateTime baseDate, int weightType, long startDate, long endDate, meal_ticketEntities db)
        {
            List<PlateRelSnapshotInfo> relList = new List<PlateRelSnapshotInfo>();
            long tempStart = startDate / 10000;
            long tempEnd = endDate / 10000;
            long todayDate = long.Parse(DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
            if (tempEnd < 0 || tempEnd > todayDate)
            {
                tempEnd = todayDate;
            }
            DateTimeFormatInfo dtfi = new CultureInfo("zh-CN", false).DateTimeFormat;
            DateTime startDateTm;
            if (!DateTime.TryParseExact(tempStart.ToString(), "yyyyMMdd", dtfi, DateTimeStyles.None, out startDateTm))
            {
                Logger.WriteFileLog("startDate有误", null);
                return;
            }
            DateTime endDateTm;
            if (!DateTime.TryParseExact(tempEnd.ToString(), "yyyyMMdd", dtfi, DateTimeStyles.None, out endDateTm))
            {
                Logger.WriteFileLog("endDate有误", null);
                return;
            }

            while (startDateTm <= endDateTm)
            {
                //查询所属股票代码
                relList = (from item in db.t_shares_plate_rel_snapshot
                           where item.PlateId == plateId && item.Date == startDateTm
                           select new PlateRelSnapshotInfo
                           {
                               SharesCode = item.SharesCode,
                               Market = item.Market,
                               PlateId = item.PlateId
                           }).ToList();

                DataTable table = new DataTable();
                table.Columns.Add("Market", typeof(int));
                table.Columns.Add("SharesCode", typeof(string));
                foreach (var item in relList)
                {
                    DataRow row = table.NewRow();
                    row["Market"] = item.Market;
                    row["SharesCode"] = item.SharesCode;
                    table.Rows.Add(row);
                }
                //关键是类型
                SqlParameter parameter = new SqlParameter("@sharesList", SqlDbType.Structured);
                //必须指定表类型名
                parameter.TypeName = "dbo.SharesBase";
                //赋值
                parameter.Value = table;

                SqlParameter par1 = new SqlParameter("@startTimeKey", SqlDbType.BigInt);
                par1.Value = long.Parse(startDateTm.ToString("yyyyMMdd0930"));
                SqlParameter par2 = new SqlParameter("@endTimeKey", SqlDbType.BigInt);
                par2.Value = long.Parse(startDateTm.ToString("yyyyMMdd1500"));
                SqlParameter par3 = new SqlParameter("@weightType", SqlDbType.Int);
                par3.Value = weightType;
                SqlParameter par4 = new SqlParameter("@baseDate", SqlDbType.Int);
                par4.Value = int.Parse(baseDate.ToString("yyyyMMdd"));

                var result = db.Database.SqlQuery<dynamic>("exec P_GetPlateKLine_1min @sharesList,@startTimeKey,@endTimeKey,@weightType,@baseDate", parameter, par1, par2, par3, par4).ToList();

                Dictionary<int, SecurityBarsDataType> dataTypeDic = new Dictionary<int, SecurityBarsDataType>();
                List<SecurityBarsDataInfo> dataList = new List<SecurityBarsDataInfo>();
                foreach (var item in result)
                {
                    long GroupTimeKey = item.GroupTimeKey;
                    DateTime Time = item.Time;
                    long OpenedPrice = item.OpenedPrice;
                    long ClosedPrice = item.ClosedPrice;
                    long MinPrice = item.MinPrice;
                    long MaxPrice = item.MaxPrice;
                    long TradeStock = item.TradeStock;
                    long TradeAmount = item.TradeAmount;
                    long PreClosePrice = item.PreClosePrice;
                    bool IsLast = item.IsLast;
                    long YestodayClosedPrice = item.YestodayClosedPrice;
                    long LastTradeStock = item.LastTradeStock;
                    long LastTradeAmount = item.LastTradeAmount;
                    long BasePrice = item.BasePrice;
                    dataList.Add(new SecurityBarsDataInfo
                    {
                        GroupTimeKey = GroupTimeKey,
                        LastTradeStock = LastTradeStock,
                        TradeStock = TradeStock,
                        LastTradeAmount = LastTradeAmount,
                        IsLast = IsLast,
                        PlateId = plateId,
                        WeightType = weightType,
                        TradeAmount = TradeAmount,
                        Time = Time,
                        ClosedPrice = ClosedPrice,
                        YestodayClosedPrice = YestodayClosedPrice,
                        MaxPrice = MaxPrice,
                        MinPrice = MinPrice,
                        OpenedPrice = OpenedPrice,
                        PreClosePrice = PreClosePrice,
                    });
                }
                dataTypeDic.Add(2, new SecurityBarsDataType
                {
                    DataType = 2,
                    DataList = dataList
                });
                SecurityBarsDataTaskQueueInfo sendInfo = new SecurityBarsDataTaskQueueInfo
                {
                    PackageList = dataTypeDic,
                    TaskGuid = "",
                    HandlerType = 4,
                };
                SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                {
                    MsgId = 1,
                    MsgObj = sendInfo
                });
                startDateTm = startDateTm.AddDays(1);
            }
        }

        /// <summary>
        /// 发送指令获取券商板块K线数据
        /// </summary>
        /// <param name="list"></param>
        private void SendToGetPlateKLine(List<dynamic> sendList,int handlerType,string taskGuid)
        {
            Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new
            {
                TaskGuid = taskGuid,
                HandlerType = handlerType,
                PackageList = sendList
            })), "SecurityBars", "1min");
        }

        /// <summary>
        /// 解析时间值
        /// </summary>
        /// <returns></returns>
        private bool ParseTimeGroupKey(DateTime date,int type,ref long groupTimeKey) 
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
        /// 获取数据表名称
        /// </summary>
        /// <returns></returns>
        private bool ParseTableName(int type,ref string tableName) 
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
    }
}
