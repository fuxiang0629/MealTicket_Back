using FXCommon.Common;
using FXCommon.Database;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    public class DataBaseHelper
    {
        /// <summary>
        /// 更新数据库所有股票数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="market"></param>
        public static void UpdateAllShares(List<SharesBaseInfo> list)
        {
            DateTime timeNow = DateTime.Now;
            DataTable table = new DataTable();
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("SharesName", typeof(string));
            table.Columns.Add("SharesPyjc", typeof(string));
            table.Columns.Add("SharesHandCount", typeof(int));
            table.Columns.Add("ShareClosedPrice", typeof(long));
            table.Columns.Add("LastModified", typeof(DateTime));

            foreach (var item in list)
            {
                DataRow row = table.NewRow();
                row["Market"] = item.Market;
                row["SharesCode"] = item.ShareCode;
                row["SharesName"] = item.ShareName;
                row["SharesPyjc"] = item.Pyjc;
                row["SharesHandCount"] = item.ShareHandCount;
                row["ShareClosedPrice"] = item.ShareClosedPrice;
                row["LastModified"] = timeNow;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesAllInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesAllInfo";
                    //赋值
                    parameter.Value = table;
                    db.Database.ExecuteSqlCommand("exec P_SharesAll_Update @sharesAllInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("更新所有股票数据出错", ex);
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 更新股票实时行情数据
        /// </summary>
        /// <param name="list"></param>
        public static void UpdateSharesQuotes(List<SharesQuotesInfo> list)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string sql = string.Empty;
            DateTime timeNow = DateTime.Now;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            Console.WriteLine("\t===========开始更新行情=================");
            //查询新股关键字
            string Param = string.Empty;
            using (var db = new meal_ticketEntities())
            {
                sql = "select top 1 ParamValue from t_system_param with(nolock) where ParamName='NewSharesFeatures'";
                Param = db.Database.SqlQuery<string>(sql).FirstOrDefault();
            }

            var baseList = Singleton.Instance.session.GetSharesBaseInfoList();
            list = (from x in list
                    join x2 in baseList on new { x.Market, x.SharesCode } equals new { x2.Market, SharesCode = x2.ShareCode }
                    select new SharesQuotesInfo
                    {
                        SharesCode = x.SharesCode,
                        SellCount1 = x.SellCount1,
                        SellCount2 = x.SellCount2,
                        SellCount3 = x.SellCount3,
                        SellCount5 = x.SellCount5,
                        SellCount4 = x.SellCount4,
                        SellPrice1 = x.SellPrice1,
                        Activity = x.Activity,
                        BackSharesCode = x.BackSharesCode,
                        SellPrice2 = x.SellPrice2,
                        SellPrice3 = x.SellPrice3,
                        SellPrice5 = x.SellPrice5,
                        BuyCount1 = x.BuyCount1,
                        SellPrice4 = x.SellPrice4,
                        SpeedUp = x.SpeedUp,
                        BuyCount2 = x.BuyCount2,
                        SharesName = x2.ShareName,
                        BuyCount3 = x.BuyCount3,
                        BuyCount4 = x.BuyCount4,
                        BuyCount5 = x.BuyCount5,
                        BuyPrice1 = x.BuyPrice1,
                        BuyPrice2 = x.BuyPrice2,
                        BuyPrice3 = x.BuyPrice3,
                        BuyPrice4 = x.BuyPrice4,
                        BuyPrice5 = x.BuyPrice5,
                        ClosedPrice = x.ClosedPrice,
                        InvolCount = x.InvolCount,
                        LastModified = timeNow,
                        LimitDownPrice = x.LimitDownPrice,
                        LimitUpPrice = x.LimitUpPrice,
                        Market = x.Market,
                        MaxPrice = x.MaxPrice,
                        MinPrice = x.MinPrice,
                        OpenedPrice = x.OpenedPrice,
                        OuterCount = x.OuterCount,
                        PresentCount = x.PresentCount,
                        PresentPrice = x.PresentPrice,
                        PriceType = x.PriceType,
                        TotalAmount = x.TotalAmount,
                        TotalCount = x.TotalCount,
                        TriNearLimitType = x.TriNearLimitType,
                        TriPriceType = x.TriPriceType
                    }).ToList();

            foreach (var item in list)
            {
                item.LimitUpPrice = 0;
                item.LimitDownPrice = 0;
                item.PriceType = 0;
                item.TriPriceType = 0;
                item.TriNearLimitType = 0;

                if (item.SharesName.Contains("ST"))
                {
                    item.IsST = true;
                }

                bool IsNewSharesFeatures = false;//是否新股
                string[] ParamValue = Param.Split(',');
                for (int i = 0; i < ParamValue.Length; i++)
                {
                    if (item.SharesName.StartsWith(ParamValue[i]))
                    {
                        IsNewSharesFeatures = true;
                        break;
                    }
                }
                if (!IsNewSharesFeatures)
                {
                    var range = Singleton.Instance.session.GetRangeList().Where(e => (e.LimitMarket == item.Market || e.LimitMarket == -1) && item.SharesCode.StartsWith(e.LimitKey)).FirstOrDefault();
                    if (range != null && item.ClosedPrice > 0 && item.PresentPrice > 0 && item.OpenedPrice>0 && item.MaxPrice>0 && item.MinPrice>0)
                    {
                        int maxRange = range.Range;
                        int maxNearLimitRange = range.NearLimitRange;
                        if (maxRange > 0)
                        {
                            if (item.SharesName.Contains("ST"))
                            {
                                maxRange = maxRange / 2;
                            }
                            item.LimitUpPrice = (long)((item.ClosedPrice + item.ClosedPrice * (maxRange * 1.0 / 10000)) / 100 + 0.5) * 100;
                            item.LimitDownPrice = (long)((item.ClosedPrice - item.ClosedPrice * (maxRange * 1.0 / 10000)) / 100 + 0.5) * 100;
                            item.PriceType = item.LimitUpPrice == item.BuyPrice1 ? 1 : item.LimitDownPrice == item.SellPrice1 ? 2 : 0;
                            item.TriPriceType = item.LimitUpPrice == item.PresentPrice ? 1 : item.LimitDownPrice == item.PresentPrice ? 2 : 0;
                        }
                        if (maxNearLimitRange > 0)
                        {
                            if (item.SharesName.Contains("ST"))
                            {
                                maxNearLimitRange = maxNearLimitRange / 2;
                            }
                            int tempRange = (int)((item.PresentPrice - item.ClosedPrice) * 1.0 / item.ClosedPrice * 10000);
                            if (tempRange >= maxNearLimitRange)
                            {
                                item.TriNearLimitType = 1;
                            }
                            else if (tempRange <= -maxNearLimitRange)
                            {
                                item.TriNearLimitType = 2;
                            }
                        }
                    }
                }
            }


            bool taskExec = Helper.CheckTradeTimeForQuotes();

            //任务类型1.更新t_shares_quotes_date 2.更新t_shares_quotes_last 3.添加t_shares_quotes_record 4.记录所有板块涨跌幅
            ThreadMsgTemplate<int> taskData1 = new ThreadMsgTemplate<int>();
            Task[] tArr1 = null;
            if (taskExec)
            {
                taskData1.Init();
                taskData1.AddMessage(1);
                int taskCount = taskData1.GetCount();
                tArr1 = new Task[taskCount];
                for (int i = 0; i < taskCount; i++)
                {
                    tArr1[i] = new Task(() =>
                    {
                        do
                        {
                            int taskType = 0;
                            if (!taskData1.GetMessage(ref taskType, true))
                            {
                                break;
                            }

                            if (taskType == 1)
                            {
                                try
                                {
                                    UpdateQuotesDate(list);
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteFileLog("UpdateQuotesDate出错", ex);
                                }
                            }
                        } while (true);
                    });
                    tArr1[i].Start();
                }
            }

            //更新五档数据
            UpdateQuotes(list);

            stopwatch.Stop();
            Console.WriteLine("\t=========行情更新结束:" + stopwatch.ElapsedMilliseconds + "========");
            //执行业务任务更新其他数据
            Console.WriteLine("\t==========开始执行任务=================");
            stopwatch.Restart();

            if (tArr1 != null)
            {
                Task.WaitAll(tArr1);
                taskData1.Release();
            }
            stopwatch.Stop();
            Console.WriteLine("\t=====任务执行结束:" + stopwatch.ElapsedMilliseconds + "============");

            var lastQuotes = Singleton.Instance.session.GetLastSharesQuotesList();
            foreach (var x in list)
            {
                SharesQuotesInfo temp = new SharesQuotesInfo 
                {
                    SharesName=x.SharesName,
                    BackSharesCode=x.BackSharesCode,
                    IsST=x.IsST,
                    SellCount1 = x.SellCount1,
                    SellCount2 = x.SellCount2,
                    SellCount3 = x.SellCount3,
                    SellCount4 = x.SellCount4,
                    Activity = x.Activity,
                    SellCount5 = x.SellCount5,
                    SellPrice1 = x.SellPrice1,
                    SellPrice2 = x.SellPrice2,
                    SellPrice3 = x.SellPrice3,
                    SellPrice4 = x.SellPrice4,
                    SellPrice5 = x.SellPrice5,
                    SharesCode = x.SharesCode,
                    SpeedUp = x.SpeedUp,
                    BuyCount1 = x.BuyCount1,
                    BuyCount2 = x.BuyCount2,
                    BuyCount3 = x.BuyCount3,
                    BuyCount4 = x.BuyCount4,
                    BuyCount5 = x.BuyCount5,
                    BuyPrice1 = x.BuyPrice1,
                    BuyPrice2 = x.BuyPrice2,
                    BuyPrice3 = x.BuyPrice3,
                    BuyPrice4 = x.BuyPrice4,
                    BuyPrice5 = x.BuyPrice5,
                    ClosedPrice = x.ClosedPrice,
                    InvolCount = x.InvolCount,
                    LastModified = x.LastModified,
                    LimitDownPrice = x.LimitDownPrice,
                    LimitUpPrice = x.LimitUpPrice,
                    MaxPrice = x.MaxPrice,
                    MinPrice = x.MinPrice,
                    OpenedPrice = x.OpenedPrice,
                    OuterCount = x.OuterCount,
                    PresentCount = x.PresentCount,
                    PresentPrice = x.PresentPrice,
                    PriceType = x.PriceType,
                    TriNearLimitType = x.TriNearLimitType,
                    TotalAmount = x.TotalAmount,
                    TotalCount = x.TotalCount,
                    TriPriceType = x.TriPriceType,
                    Market = x.Market
                };
                int key = int.Parse(x.SharesCode) * 10 + x.Market;
                if (lastQuotes.ContainsKey(key))
                {
                    lastQuotes[key] = temp;
                }
                else
                {
                    lastQuotes.Add(key, temp);
                }
            }
        }

        /// <summary>
        /// 更新五档数据
        /// </summary>
        private static void UpdateQuotes(List<SharesQuotesInfo> list)
        {
            DataTable table = new DataTable();
            #region====定义表字段数据类型====
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("PresentPrice", typeof(long));
            table.Columns.Add("ClosedPrice", typeof(long));
            table.Columns.Add("OpenedPrice", typeof(long));
            table.Columns.Add("MaxPrice", typeof(long));
            table.Columns.Add("MinPrice", typeof(long));
            table.Columns.Add("TotalCount", typeof(int));
            table.Columns.Add("PresentCount", typeof(int));
            table.Columns.Add("TotalAmount", typeof(long));
            table.Columns.Add("InvolCount", typeof(int));
            table.Columns.Add("OuterCount", typeof(int));
            table.Columns.Add("BuyPrice1", typeof(long));
            table.Columns.Add("BuyCount1", typeof(int));
            table.Columns.Add("BuyPrice2", typeof(long));
            table.Columns.Add("BuyCount2", typeof(int));
            table.Columns.Add("BuyPrice3", typeof(long));
            table.Columns.Add("BuyCount3", typeof(int));
            table.Columns.Add("BuyPrice4", typeof(long));
            table.Columns.Add("BuyCount4", typeof(int));
            table.Columns.Add("BuyPrice5", typeof(long));
            table.Columns.Add("BuyCount5", typeof(int));
            table.Columns.Add("SellPrice1", typeof(long));
            table.Columns.Add("SellCount1", typeof(int));
            table.Columns.Add("SellPrice2", typeof(long));
            table.Columns.Add("SellCount2", typeof(int));
            table.Columns.Add("SellPrice3", typeof(long));
            table.Columns.Add("SellCount3", typeof(int));
            table.Columns.Add("SellPrice4", typeof(long));
            table.Columns.Add("SellCount4", typeof(int));
            table.Columns.Add("SellPrice5", typeof(long));
            table.Columns.Add("SellCount5", typeof(int));
            table.Columns.Add("SpeedUp", typeof(string));
            table.Columns.Add("Activity", typeof(string));
            table.Columns.Add("LastModified", typeof(DateTime));
            table.Columns.Add("LimitUpPrice", typeof(long));
            table.Columns.Add("LimitDownPrice", typeof(long));
            table.Columns.Add("PriceType", typeof(int));
            table.Columns.Add("TriPriceType", typeof(int));
            table.Columns.Add("TriNearLimitType", typeof(int));
            table.Columns.Add("DataType", typeof(int));
            #endregion

            #region====绑定数据====
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.SharesName))
                {
                    continue;
                }

                DataRow row = table.NewRow();
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["PresentPrice"] = item.PresentPrice;
                row["ClosedPrice"] = item.ClosedPrice;
                row["OpenedPrice"] = item.OpenedPrice;
                row["MaxPrice"] = item.MaxPrice;
                row["MinPrice"] = item.MinPrice;
                row["TotalCount"] = item.TotalCount;
                row["PresentCount"] = item.PresentCount;
                row["TotalAmount"] = item.TotalAmount;
                row["InvolCount"] = item.InvolCount;
                row["OuterCount"] = item.OuterCount;
                row["BuyPrice1"] = item.BuyPrice1;
                row["BuyCount1"] = item.BuyCount1;
                row["BuyPrice2"] = item.BuyPrice2;
                row["BuyCount2"] = item.BuyCount2;
                row["BuyPrice3"] = item.BuyPrice3;
                row["BuyCount3"] = item.BuyCount3;
                row["BuyPrice4"] = item.BuyPrice4;
                row["BuyCount4"] = item.BuyCount4;
                row["BuyPrice5"] = item.BuyPrice5;
                row["BuyCount5"] = item.BuyCount5;
                row["SellPrice1"] = item.SellPrice1;
                row["SellCount1"] = item.SellCount1;
                row["SellPrice2"] = item.SellPrice2;
                row["SellCount2"] = item.SellCount2;
                row["SellPrice3"] = item.SellPrice3;
                row["SellCount3"] = item.SellCount3;
                row["SellPrice4"] = item.SellPrice4;
                row["SellCount4"] = item.SellCount4;
                row["SellPrice5"] = item.SellPrice5;
                row["SellCount5"] = item.SellCount5;
                row["SpeedUp"] = item.SpeedUp;
                row["Activity"] = item.Activity;
                row["LastModified"] = item.LastModified;
                row["LimitUpPrice"] = item.LimitUpPrice;
                row["LimitDownPrice"] = item.LimitDownPrice;
                row["PriceType"] = item.PriceType;
                row["TriPriceType"] = item.TriPriceType;
                row["TriNearLimitType"] = item.TriNearLimitType;
                row["DataType"] = 0;
                table.Rows.Add(row);
            }
            #endregion

            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try 
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesQuotes", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesQuotes";
                    //赋值
                    parameter.Value = table;
                    db.Database.ExecuteSqlCommand("exec P_SharesQuotes_Update @sharesQuotes", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("更新五档数据出错",ex);
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 更新每天五档数据
        /// </summary>
        private static void UpdateQuotesDate(List<SharesQuotesInfo> list)
        {
            DataTable table = new DataTable();
            #region====定义表字段数据类型====
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("PresentPrice", typeof(long));
            table.Columns.Add("ClosedPrice", typeof(long));
            table.Columns.Add("OpenedPrice", typeof(long));
            table.Columns.Add("MaxPrice", typeof(long));
            table.Columns.Add("MinPrice", typeof(long));
            table.Columns.Add("LimitUpPrice", typeof(long));
            table.Columns.Add("LimitDownPrice", typeof(long));
            table.Columns.Add("LastModified", typeof(DateTime));
            table.Columns.Add("Date", typeof(string));
            table.Columns.Add("LimitUpCount", typeof(int));
            table.Columns.Add("LimitDownCount", typeof(int));
            table.Columns.Add("LimitUpBombCount", typeof(int));
            table.Columns.Add("LimitDownBombCount", typeof(int));
            table.Columns.Add("TriLimitUpCount", typeof(int));
            table.Columns.Add("TriLimitDownCount", typeof(int));
            table.Columns.Add("TriLimitUpBombCount", typeof(int));
            table.Columns.Add("TriLimitDownBombCount", typeof(int));
            table.Columns.Add("PriceType", typeof(int));
            table.Columns.Add("TriPriceType", typeof(int));
            table.Columns.Add("TriNearLimitType", typeof(int));
            table.Columns.Add("TotalAmount", typeof(long));
            table.Columns.Add("TotalCount", typeof(int));
            table.Columns.Add("IsST", typeof(bool));
            #endregion

            var lastQuotes = Singleton.Instance.session.GetLastSharesQuotesList();
            foreach (var item in list)
            {
                int LimitUpCount = 0;
                int LimitDownCount = 0;
                int LimitUpBombCount = 0;
                int LimitDownBombCount = 0;
                int TriLimitUpCount = 0;
                int TriLimitDownCount = 0;
                int TriLimitUpBombCount = 0;
                int TriLimitDownBombCount = 0;

                SharesQuotesInfo lastQuote = null;
                lastQuotes.TryGetValue(int.Parse(item.SharesCode)*10+item.Market, out lastQuote);
                if (item.PriceType == 1 && (lastQuote == null || lastQuote.PriceType != 1))
                {
                    LimitUpCount = 1;
                }
                if (item.PriceType == 2 && (lastQuote == null || lastQuote.PriceType != 2))
                {
                    LimitDownCount = 1;
                }
                if (item.PriceType == 0 && lastQuote != null && lastQuote.PriceType == 1)
                {
                    LimitUpBombCount = 1;
                }
                if (item.PriceType == 0 && lastQuote != null && lastQuote.PriceType == 2)
                {
                    LimitDownBombCount = 1;
                }
                if (item.TriPriceType == 1 && (lastQuote == null || lastQuote.TriPriceType != 1))
                {
                    TriLimitUpCount = 1;
                }
                if (item.TriPriceType == 2 && (lastQuote == null || lastQuote.TriPriceType != 2))
                {
                    TriLimitDownCount = 1;
                }
                if (item.TriPriceType == 0 && lastQuote != null && lastQuote.TriPriceType == 1)
                {
                    TriLimitUpBombCount = 1;
                }
                if (item.TriPriceType == 0 && lastQuote != null && lastQuote.TriPriceType == 2)
                {
                    TriLimitDownBombCount = 1;
                }

                DataRow row = table.NewRow();
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["PresentPrice"] = item.PresentPrice;
                row["ClosedPrice"] = item.ClosedPrice;
                row["OpenedPrice"] = item.OpenedPrice;
                row["MaxPrice"] = item.MaxPrice;
                row["MinPrice"] = item.MinPrice;
                row["LimitUpPrice"] = item.LimitUpPrice;
                row["LimitDownPrice"] = item.LimitDownPrice;
                row["LastModified"] = item.LastModified;
                row["Date"] = item.LastModified.ToString("yyyy-MM-dd");
                row["LimitUpCount"] = LimitUpCount;
                row["LimitDownCount"] = LimitDownCount;
                row["LimitUpBombCount"] = LimitUpBombCount;
                row["LimitDownBombCount"] = LimitDownBombCount;
                row["TriLimitUpCount"] = TriLimitUpCount;
                row["TriLimitDownCount"] = TriLimitDownCount;
                row["TriLimitUpBombCount"] = TriLimitUpBombCount;
                row["TriLimitDownBombCount"] = TriLimitDownBombCount;
                row["PriceType"] = item.PriceType;
                row["TriPriceType"] = item.TriPriceType;
                row["TriNearLimitType"] = item.TriNearLimitType;
                row["TotalAmount"] = item.TotalAmount;
                row["TotalCount"] = item.TotalCount;
                row["IsST"] = item.IsST;
                table.Rows.Add(row);
            }


            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesQuotesDate", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesQuotesDate";
                    //赋值
                    parameter.Value = table;
                    db.Database.ExecuteSqlCommand("exec P_SharesQuotesDate_Update @sharesQuotesDate", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("更新每天五档数据出错", ex);
                    tran.Rollback();
                    throw ex;
                }
            }
        }
    }
}
