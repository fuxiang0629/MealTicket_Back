using FXCommon.Common;
using MealTicket_DBCommon;
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
    public class SecurityBarsData_DateTask
    {
        /// <summary>
        /// 任务线程
        /// </summary>
        Thread TaskThread;

        /// <summary>
        /// K线数据回调队列
        /// </summary>
        public ThreadMsgTemplate<QueueMsgObj> SecurityBarsDataQueue;

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
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        public void DoTask()
        {
            TaskThread = new Thread(() =>
            {
                do
                {
                    QueueMsgObj msgObj = new QueueMsgObj();
                    SecurityBarsDataQueue.WaitMessage(ref msgObj);

                    if (msgObj.MsgId == -1)
                    {
                        break;
                    }
                    var resultData = msgObj.MsgObj as SecurityBarsDataTaskQueueInfo;
                    DoBusiness(resultData, msgObj.MsgId);
                } while (true);
            });
            TaskThread.Start();
        }

        /// <summary>
        /// 执行业务
        /// </summary>
        /// <returns></returns>
        private void DoBusiness(SecurityBarsDataTaskQueueInfo receivedObj, int msgId)
        {
            bool isFinish = false;

            if (UpdateToDataBase(receivedObj, ref isFinish))
            {
                if (!isFinish)
                {
                    SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                    {
                        MsgId = 2,
                        MsgObj = receivedObj
                    });
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
                            if (type == 0)
                            {
                                var resultData = CalculateMinutetimedata(receivedObj);
                                UpdateMinutetimeToDataBase(resultData);
                            }
                            else
                            {
                                var resultData = CalculateOtherData(type, receivedObj);
                                UpdateOtherToDataBase(type, resultData);
                            }
                        }, TaskCreationOptions.LongRunning);
                        taskArr[type].Start();
                    }
                    Task.WaitAll(taskArr);
                }
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
        private void UpdateOtherToDataBase(int type, List<SecurityBarsOtherData> dataList)
        {
            try
            {
                string sql = string.Empty;
                switch (type)
                {
                    case 0:
                        //sql = "exec P_Shares_MinuteTimeData_Update sharesSecurityBarsOtherData";
                        break;
                    case 1:
                        sql = "exec P_Shares_SecurityBarsData_5min_Update @sharesSecurityBarsOtherData";
                        break;
                    case 2:
                        sql = "exec P_Shares_SecurityBarsData_15min_Update @sharesSecurityBarsOtherData";
                        break;
                    case 3:
                        sql = "exec P_Shares_SecurityBarsData_30min_Update @sharesSecurityBarsOtherData";
                        break;
                    case 4:
                        sql = "exec P_Shares_SecurityBarsData_60min_Update @sharesSecurityBarsOtherData";
                        break;
                    case 5:
                        sql = "exec P_Shares_SecurityBarsData_1day_Update @sharesSecurityBarsOtherData";
                        break;
                    case 6:
                        sql = "exec P_Shares_SecurityBarsData_1week_Update @sharesSecurityBarsOtherData";
                        break;
                    case 7:
                        sql = "exec P_Shares_SecurityBarsData_1month_Update @sharesSecurityBarsOtherData";
                        break;
                    case 8:
                        sql = "exec P_Shares_SecurityBarsData_1quarter_Update @sharesSecurityBarsOtherData";
                        break;
                    case 9:
                        sql = "exec P_Shares_SecurityBarsData_1year_Update @sharesSecurityBarsOtherData";
                        break;
                    default:
                        break;
                }
                if (string.IsNullOrEmpty(sql))
                {
                    return;
                }

                DataTable table = new DataTable();
                table.Columns.Add("Market", typeof(int));
                table.Columns.Add("SharesCode", typeof(string));
                table.Columns.Add("Date", typeof(DateTime));
                table.Columns.Add("GroupTime", typeof(DateTime));
                table.Columns.Add("Time", typeof(DateTime));
                table.Columns.Add("TimeStr", typeof(string));
                table.Columns.Add("OpenedPrice", typeof(long));
                table.Columns.Add("ClosedPrice", typeof(long));
                table.Columns.Add("PreClosePrice", typeof(long));
                table.Columns.Add("MinPrice", typeof(long));
                table.Columns.Add("MaxPrice", typeof(long));
                table.Columns.Add("TradeStock", typeof(long));
                table.Columns.Add("TradeAmount", typeof(long));
                table.Columns.Add("LastTradeStock", typeof(int));
                table.Columns.Add("LastTradeAmount", typeof(long));
                table.Columns.Add("Tradable", typeof(long));
                table.Columns.Add("TotalCapital", typeof(long));
                table.Columns.Add("HandCount", typeof(int));
                table.Columns.Add("LastModified", typeof(DateTime));
                foreach (var item in dataList)
                {
                    DataRow row = table.NewRow();
                    row["Market"] = item.Market;
                    row["SharesCode"] = item.SharesCode;
                    row["Date"] = item.Date;
                    row["GroupTime"] = item.GroupTime;
                    row["Time"] = item.Time;
                    row["TimeStr"] = item.TimeStr;
                    row["OpenedPrice"] = item.OpenedPrice;
                    row["ClosedPrice"] = item.ClosedPrice;
                    row["PreClosePrice"] = item.PreClosePrice;
                    row["MinPrice"] = item.MinPrice;
                    row["MaxPrice"] = item.MaxPrice;
                    row["TradeStock"] = item.TradeStock;
                    row["TradeAmount"] = item.TradeAmount;
                    row["LastTradeStock"] = item.LastTradeStock;
                    row["LastTradeAmount"] = item.LastTradeAmount;
                    row["Tradable"] = item.Tradable;
                    row["TotalCapital"] = item.TotalCapital;
                    row["HandCount"] = item.HandCount;
                    row["LastModified"] = DateTime.Now;
                    table.Rows.Add(row);
                }

                using (var db = new meal_ticketEntities())
                {
                    db.Database.CommandTimeout = 600;
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesSecurityBarsOtherData", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesSecurityBarsOtherData";
                    //赋值
                    parameter.Value = table;
                    db.Database.ExecuteSqlCommand(sql, parameter);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新其他K线数据失败，type=" + type, ex);
            }
        }

        /// <summary>
        /// 更新分时数据
        /// </summary>
        /// <param name="dataList"></param>
        private void UpdateMinutetimeToDataBase(List<MinutetimeDataInfo> dataList)
        {
            try
            {
                DataTable table = new DataTable();
                table.Columns.Add("Market", typeof(int));
                table.Columns.Add("SharesCode", typeof(string));
                table.Columns.Add("Date", typeof(DateTime));
                table.Columns.Add("Time", typeof(DateTime));
                table.Columns.Add("TimeStr", typeof(string));
                table.Columns.Add("Price", typeof(long));
                table.Columns.Add("AvgPrice", typeof(long));
                table.Columns.Add("ClosedPrice", typeof(long));
                table.Columns.Add("TradeStock", typeof(long));
                table.Columns.Add("TradeAmount", typeof(long));
                table.Columns.Add("TotalTradeStock", typeof(int));
                table.Columns.Add("TotalTradeAmount", typeof(long));
                table.Columns.Add("Tradable", typeof(long));
                table.Columns.Add("TotalCapital", typeof(long));
                table.Columns.Add("HandCount", typeof(int));
                table.Columns.Add("LastModified", typeof(DateTime));
                foreach (var item in dataList)
                {
                    DataRow row = table.NewRow();
                    row["Market"] = item.Market;
                    row["SharesCode"] = item.SharesCode;
                    row["Date"] = item.Date;
                    row["Time"] = item.Time;
                    row["TimeStr"] = item.TimeStr;
                    row["Price"] = item.Price;
                    row["AvgPrice"] = item.AvgPrice;
                    row["ClosedPrice"] = item.PreClosePrice;
                    row["TradeStock"] = item.TradeStock;
                    row["TradeAmount"] = item.TradeAmount;
                    row["TotalTradeStock"] = item.TotalTradeStock;
                    row["TotalTradeAmount"] = item.TotalTradeAmount;
                    row["Tradable"] = item.Tradable;
                    row["TotalCapital"] = item.TotalCapital;
                    row["HandCount"] = item.HandCount;
                    row["LastModified"] = DateTime.Now;
                    table.Rows.Add(row);
                }

                using (var db = new meal_ticketEntities())
                {
                    db.Database.CommandTimeout = 600;
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesMinuteTimeData", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesMinuteTimeData";
                    //赋值
                    parameter.Value = table;
                    db.Database.ExecuteSqlCommand("exec P_Shares_MinuteTimeData_Update @sharesMinuteTimeData", parameter);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新分时数据失败", ex);
            }
        }

        /// <summary>
        /// 更新其他K线数据
        /// </summary>
        /// <param name="type">1.5分钟线 2.15分钟线 3.30分钟线 4.60分钟线 5.日线 6.周线 7.月线 8.季度线 9.年线</param>
        private List<SecurityBarsOtherData> CalculateOtherData(int type, SecurityBarsDataTaskQueueInfo dataObj)
        {
            List<SecurityBarsOtherData> resultList = new List<SecurityBarsOtherData>();
            try
            {
                //查询原始一分钟数据
                List<SecurityBarsData_1minInfo> SecurityBarsData_1minList = new List<SecurityBarsData_1minInfo>();
                List<SecurityBarsMaxTime> tempList = new List<SecurityBarsMaxTime>();

                using (var db = new meal_ticketEntities())
                {
                    string sql = @"select distinct t.Market,t.SharesCode,isnull(t.CirculatingCapital,0) Tradable,isnull(t.TotalCapital,0) TotalCapital,t.SharesHandCount HandCount
from v_shares_baseinfo t with(nolock)
inner join t_shares_limit_time t2 with(nolock) on (t2.LimitMarket=-1 or t2.LimitMarket=t.Market) and (t.SharesCode like t2.LimitKey+'%')";
                    tempList = db.Database.SqlQuery<SecurityBarsMaxTime>(sql).ToList();
                }
                foreach (var item in tempList)
                {
                    var tempDataList = (from x in dataObj.DataList
                                        where x.Market == item.Market && x.SharesCode == item.SharesCode
                                        orderby x.Time
                                        select new SecurityBarsData_1minInfo
                                        {
                                            SharesCode = x.SharesCode,
                                            TimeStr = x.TimeStr,
                                            ClosedPrice = x.ClosedPrice,
                                            Date = x.Date,
                                            HandCount = item.HandCount,
                                            TradeStock = x.TradeStock,
                                            Market = x.Market,
                                            MaxPrice = x.MaxPrice,
                                            MinPrice = x.MinPrice,
                                            OpenedPrice = x.OpenedPrice,
                                            PreClosePrice = x.PreClosePrice,
                                            Time = x.Time.Value,
                                            TotalCapital = item.TotalCapital,
                                            Tradable = item.Tradable,
                                            TradeAmount = x.TradeAmount
                                        }).ToList();
                    SecurityBarsData_1minList.AddRange(tempDataList);
                }
                int tempMarket = -1;
                string tempSharesCode = "";
                long tempTradeStock = 0;
                long tempTradeAmount = 0;
                int tempLastTradeStock = 0;
                long tempLastTradeAmount = 0;
                DateTime currGroupTime = DateTime.Parse("1991-01-01 00:00:00");
                DateTime tempDate = DateTime.Parse("1991-01-01 00:00:00");
                DateTime tempTime = DateTime.Parse("1991-01-01 00:00:00");
                string tempTimeStr = "";
                long defaultMaxPrice = 9999999999;
                long tempMinPrice = defaultMaxPrice;
                long tempMaxPrice = 0;
                long tempTradable = 0;
                long tempTotalCapital = 0;
                int tempHandCount = 0;
                long tempClosedPrice = 0;
                long tempOpenedPrice = 0;
                long tempPreClosePrice = 0;

                foreach (var item in SecurityBarsData_1minList)
                {
                    //判断当前分组时间
                    var tempGroupTime = _checkGroupTime(item.Market, item.SharesCode, item.Time, type);
                    if (tempGroupTime == null)
                    {
                        continue;
                    }
                    if (item.Market == tempMarket && item.SharesCode == tempSharesCode)
                    {
                        tempTradeStock = tempTradeStock + tempLastTradeStock;
                        tempTradeAmount = tempTradeAmount + tempLastTradeAmount;
                        tempLastTradeStock = 0;
                        tempLastTradeAmount = 0;
                    }
                    if (item.Market != tempMarket || item.SharesCode != tempSharesCode || tempGroupTime != currGroupTime)
                    {
                        if (tempMarket != -1)
                        {
                            resultList.Add(new SecurityBarsOtherData
                            {
                                Market = tempMarket,
                                SharesCode = tempSharesCode,
                                Date = tempDate,
                                GroupTime = currGroupTime,
                                Time = tempTime,
                                TimeStr = tempTimeStr,
                                OpenedPrice = tempOpenedPrice,
                                ClosedPrice = tempClosedPrice,
                                PreClosePrice = tempPreClosePrice,
                                MinPrice = tempMinPrice,
                                MaxPrice = tempMaxPrice,
                                TradeStock = tempTradeStock,
                                TradeAmount = tempTradeAmount,
                                LastTradeStock = tempLastTradeStock,
                                LastTradeAmount = tempLastTradeAmount,
                                Tradable = tempTradable,
                                TotalCapital = tempTotalCapital,
                                HandCount = tempHandCount,
                                LastModified = DateTime.Now
                            });
                        }
                        tempOpenedPrice = item.OpenedPrice;
                        tempPreClosePrice = item.PreClosePrice;
                        tempTradeStock = 0;
                        tempTradeAmount = 0;
                        tempMinPrice = defaultMaxPrice;
                        tempMaxPrice = 0;
                    }

                    tempMarket = item.Market;
                    tempSharesCode = item.SharesCode;
                    currGroupTime = tempGroupTime.Value;
                    tempDate = item.Date;
                    tempTime = item.Time;
                    tempTimeStr = item.TimeStr;
                    tempLastTradeStock = item.TradeStock;
                    tempLastTradeAmount = item.TradeAmount;
                    if (tempMinPrice > item.MinPrice)
                    {
                        tempMinPrice = item.MinPrice;
                    }
                    if (tempMaxPrice < item.MaxPrice)
                    {
                        tempMaxPrice = item.MaxPrice;
                    }
                    tempTradable = item.Tradable;
                    tempTotalCapital = item.TotalCapital;
                    tempHandCount = item.HandCount;
                    tempClosedPrice = item.ClosedPrice;
                }
                resultList.Add(new SecurityBarsOtherData
                {
                    Market = tempMarket,
                    SharesCode = tempSharesCode,
                    Date = tempDate,
                    GroupTime = currGroupTime,
                    Time = tempTime,
                    TimeStr = tempTimeStr,
                    OpenedPrice = tempOpenedPrice,
                    ClosedPrice = tempClosedPrice,
                    PreClosePrice = tempPreClosePrice,
                    MinPrice = tempMinPrice,
                    MaxPrice = tempMaxPrice,
                    TradeStock = tempTradeStock,
                    TradeAmount = tempTradeAmount,
                    LastTradeStock = tempLastTradeStock,
                    LastTradeAmount = tempLastTradeAmount,
                    Tradable = tempTradable,
                    TotalCapital = tempTotalCapital,
                    HandCount = tempHandCount,
                    LastModified = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新其他K线数据失败，type=" + type, ex);
            }
            return resultList;
        }

        private DateTime? _checkGroupTime(int market, string sharesCode, DateTime time, int type)
        {
            if (type == 1 || type == 2 || type == 3 || type == 4)
            {
                return _checkGroupTime1(market, sharesCode, time, type);
            }
            else if (type == 5)
            {
                return _checkGroupTime2(time);
            }
            else if (type == 6 || type == 7 || type == 8 || type == 9)
            {
                return _checkGroupTime3(time, type);
            }
            else
            {
                return null;
            }
        }

        private DateTime? _checkGroupTime1(int market, string sharesCode, DateTime time, int type)
        {
            try
            {
                TimeSpan timeSpan = TimeSpan.Parse(time.ToString("HH:mm:ss"));

                string time7 = (from item in Singleton.Instance._SharesLimitTimeSession.GetSessionData()
                                where (item.LimitMarket == -1 || item.LimitMarket == market) && sharesCode.StartsWith(item.LimitKey)
                                select item.Time7).FirstOrDefault();
                if (string.IsNullOrEmpty(time7))
                {
                    return null;
                }
                int riseMinute = type == 1 ? 5 : type == 2 ? 15 : type == 3 ? 30 : type == 4 ? 60 : 0;
                if (riseMinute == 0)
                {
                    return null;
                }
                string[] timeArr = time7.Split(',');
                for (int i = 0; i < timeArr.Length; i++)
                {
                    string[] timeInfo = timeArr[i].Split('-');
                    TimeSpan startTime = TimeSpan.Parse(timeInfo[0]);
                    TimeSpan endTime = TimeSpan.Parse(timeInfo[1]);
                    if (timeSpan < startTime || timeSpan > endTime)
                    {
                        continue;
                    }
                    do
                    {
                        startTime = startTime.Add(TimeSpan.FromMinutes(riseMinute));
                        if (startTime > endTime)
                        {
                            break;
                        }
                        if (startTime >= timeSpan)
                        {
                            string resultTime = time.Date.ToString("yyyy-MM-dd") + " " + startTime.ToString();
                            return DateTime.Parse(resultTime);
                        }
                    } while (true);
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("_checkGroupTime1出错", ex);
                return null;
            }
        }

        private DateTime _checkGroupTime2(DateTime time)
        {
            return time.Date;
        }

        private DateTime? _checkGroupTime3(DateTime time, int type)
        {
            try
            {
                var dimTimeSession = Singleton.Instance._DimTimeSession.GetSessionData();
                var the_day = (from item in dimTimeSession
                               where item.the_date == int.Parse(time.ToString("yyyyMMdd"))
                               select item).FirstOrDefault();
                if (the_day == null)
                {
                    return null;
                }
                int key = type == 6 ? the_day.the_week ?? 0 : type == 7 ? the_day.the_month ?? 0 : type == 8 ? the_day.the_quarter ?? 0 : type == 9 ? the_day.the_year ?? 0 : 0;
                var tradeDate = dimTimeSession.Where(e => e.week_day != 1 && e.week_day != 7).OrderByDescending(e => e.the_date).ToList();
                foreach (var item in tradeDate)
                {
                    var tempDate = (from x in Singleton.Instance._SharesLimitDateSession.GetSessionData()
                                    where item.the_date >= x.BeginDate && item.the_date <= x.EndDate
                                    select x).FirstOrDefault();
                    if (tempDate != null)
                    {
                        continue;
                    }

                    if ((type == 6 && item.the_week == key) || (type == 7 && item.the_month == key) || (type == 8 && item.the_quarter == key) || (type == 9 && item.the_year == key))
                    {
                        return DateTime.ParseExact(item.the_date.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("_checkGroupTime3出错", ex);
                return null;
            }
        }

        /// <summary>
        /// 计算分时数据
        /// </summary>
        /// <returns></returns>
        private List<MinutetimeDataInfo> CalculateMinutetimedata(SecurityBarsDataTaskQueueInfo dataObj)
        {
            List<MinutetimeDataInfo> resultList = new List<MinutetimeDataInfo>();
            try
            {
                List<MinutetimeDataInfo> tempList = new List<MinutetimeDataInfo>();
                var tempSharesDateList = (from item in dataObj.DataList
                                          group item by new { item.Market, item.SharesCode } into g
                                          select g).ToList();
                using (var db = new meal_ticketEntities())
                {
                    string date = dataObj.Date.ToString("yyyy-MM-dd");
                    var quoteDate = (from item in db.t_shares_quotes_date
                                     where item.Date == date
                                     select item).ToList();
                    tempList = (from item in tempSharesDateList
                                join item2 in Singleton.Instance._sharesBaseSession.GetSessionData() on new { item.Key.Market, item.Key.SharesCode } equals new { item2.Market, item2.SharesCode }
                                join item3 in quoteDate on new { item.Key.Market, item.Key.SharesCode } equals new { item3.Market, item3.SharesCode }
                                select new MinutetimeDataInfo
                                {
                                    SharesCode = item.Key.SharesCode,
                                    Market = item.Key.Market,
                                    Time = dataObj.Date,
                                    TotalTradeStock = 0,
                                    TotalTradeAmount = 0,
                                    Tradable = item2.CirculatingCapital,
                                    TotalCapital = item2.TotalCapital,
                                    HandCount = item2.SharesHandCount,
                                    PreClosePrice = item3.ClosedPrice
                                }).ToList();
                }
                foreach (var item in tempList)
                {
                    var tempDataList = (from x in dataObj.DataList
                                        where x.Market == item.Market && x.SharesCode == item.SharesCode
                                        orderby x.Time
                                        select x).ToList();
                    long totalTradeStock = item.TotalTradeStock;
                    long totalTradeAmount = item.TotalTradeAmount;
                    DateTime lastTime = DateTime.Parse("1991-01-01 00:00:00");
                    foreach (var mininfo in tempDataList)
                    {
                        if (mininfo.Time.Value == lastTime)
                        {
                            continue;
                        }
                        resultList.Add(new MinutetimeDataInfo
                        {
                            Market = mininfo.Market,
                            SharesCode = mininfo.SharesCode,
                            TimeStr = mininfo.TimeStr,
                            Time = mininfo.Time.Value,
                            Date = mininfo.Date,
                            Price = mininfo.ClosedPrice,
                            TotalCapital = item.TotalCapital,
                            Tradable = item.Tradable,
                            HandCount = item.HandCount,
                            LastModified = DateTime.Now,
                            TotalTradeAmount = totalTradeAmount,
                            TotalTradeStock = totalTradeStock,
                            TradeStock = mininfo.TradeStock,
                            TradeAmount = mininfo.TradeAmount,
                            PreClosePrice = item.PreClosePrice,
                            AvgPrice = totalTradeStock + mininfo.TradeStock == 0 ? mininfo.ClosedPrice : (totalTradeAmount + mininfo.TradeAmount) / (totalTradeStock + mininfo.TradeStock)
                        });
                        totalTradeStock = totalTradeStock + mininfo.TradeStock;
                        totalTradeAmount = totalTradeAmount + mininfo.TradeAmount;
                        lastTime = mininfo.Time.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("更新分时数据失败", ex);
            }
            return resultList;
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
