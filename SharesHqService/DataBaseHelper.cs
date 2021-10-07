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
        public static void UpdateAllSharesByTemp(List<SharesBaseInfo> list)
        {
            DateTime timeNow = DateTime.Now;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                string sqlDel = "truncate table t_shares_all_temp";
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sqlDel;   //sql语句
                    cmd.ExecuteNonQuery();
                }
                using (SqlTransaction tran = conn.BeginTransaction())//开启事务
                {
                    try
                    {
                        var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                        using (DataTable table = new DataTable())
                        {
                            table.Columns.Add("Id", typeof(long));
                            table.Columns.Add("SharesCode", typeof(string));
                            table.Columns.Add("SharesName", typeof(string));
                            table.Columns.Add("SharesPyjc", typeof(string));
                            table.Columns.Add("SharesHandCount", typeof(int));
                            table.Columns.Add("ShareClosedPrice", typeof(long));
                            table.Columns.Add("Market", typeof(int));
                            table.Columns.Add("LastModified", typeof(DateTime));

                            foreach (var item in list)
                            {
                                DataRow row = table.NewRow();
                                row["Id"] = 0;
                                row["SharesCode"] = item.ShareCode;
                                row["SharesName"] = item.ShareName;
                                row["SharesPyjc"] = item.Pyjc;
                                row["SharesHandCount"] = item.ShareHandCount;
                                row["ShareClosedPrice"] = item.ShareClosedPrice;
                                row["Market"] = item.Market;
                                row["LastModified"] = timeNow;
                                table.Rows.Add(row);
                            }
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("Id", "Id");
                            dic.Add("SharesCode", "SharesCode");
                            dic.Add("SharesName", "SharesName");
                            dic.Add("SharesPyjc", "SharesPyjc");
                            dic.Add("SharesHandCount", "SharesHandCount");
                            dic.Add("ShareClosedPrice", "ShareClosedPrice");
                            dic.Add("Market", "Market");
                            dic.Add("LastModified", "LastModified");
                            bulk.ColumnMappings = dic;
                            bulk.BatchSize = 10000;

                            bulk.BulkWriteToServer(conn, table, "t_shares_all_temp", tran);
                        }
                        string sqlUpdate = @"merge into t_shares_all as t
using (select * from t_shares_all_temp) as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode
when matched
then update set t.SharesName = t1.SharesName,t.SharesPyjc = t1.SharesPyjc,t.SharesHandCount = t1.SharesHandCount,t.ShareClosedPrice = t1.ShareClosedPrice,t.LastModified = t1.LastModified
when not matched by target
then insert(SharesCode,SharesName,SharesPyjc,SharesHandCount,ShareClosedPrice,Market,[Status],MarketStatus,ForbidStatus,LastModified) values(t1.SharesCode,t1.SharesName,t1.SharesPyjc,t1.SharesHandCount,t1.ShareClosedPrice,t1.Market,1,1,0,t1.LastModified)
when not matched by source
then delete;";
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            cmd.CommandText = sqlUpdate;   //sql语句
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            Singleton.Instance._setSharesBaseInfoList(list);
        }

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
            Singleton.Instance._setSharesBaseInfoList(list);
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

            list = (from x in list
                    join x2 in Singleton.Instance._getSharesBaseInfoList() on new { x.Market, x.SharesCode } equals new { x2.Market, SharesCode = x2.ShareCode }
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
                    var range = Singleton.Instance.RangeList.Where(e => (e.LimitMarket == item.Market || e.LimitMarket == -1) && item.SharesCode.StartsWith(e.LimitKey)).FirstOrDefault();
                    if (range != null && item.ClosedPrice > 0 && item.PresentPrice > 0)
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
            foreach (var item in list)
            {
                if (Singleton.Instance.LastSharesQuotesList.ContainsKey(item.Market + "" + item.SharesCode))
                {
                    Singleton.Instance.LastSharesQuotesList[item.Market + "" + item.SharesCode] = item;
                }
                else 
                {
                    Singleton.Instance.LastSharesQuotesList.Add(item.Market + "" + item.SharesCode,item);
                }
            }
        }

        /// <summary>
        /// 更新五档数据
        /// </summary>
        private static void UpdateQuotesByTemp(List<SharesQuotesInfo> list)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                try
                {
                    using (var cmd = conn.CreateCommand())
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            string sql = string.Empty;
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            //删除五档temp表数据
                            sql = string.Format("truncate table t_shares_quotes_temp");
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();

                            var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                            using (DataTable table = new DataTable())
                            {
                                #region====定义表字段数据类型====
                                table.Columns.Add("Id", typeof(long));
                                table.Columns.Add("Market", typeof(int));
                                table.Columns.Add("SharesCode", typeof(string));
                                table.Columns.Add("BackSharesCode", typeof(string));
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
                                #endregion

                                #region====绑定数据====
                                foreach (var item in list)
                                {
                                    if (string.IsNullOrEmpty(item.SharesName))
                                    {
                                        continue;
                                    }

                                    DataRow row = table.NewRow();
                                    row["Id"] = 0;
                                    row["Market"] = item.Market;
                                    row["SharesCode"] = item.SharesCode;
                                    row["BackSharesCode"] = item.BackSharesCode;
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
                                    table.Rows.Add(row);
                                }
                                #endregion

                                #region====绑定结构字段====
                                Dictionary<string, string> dic = new Dictionary<string, string>();
                                dic.Add("Id", "Id");
                                dic.Add("Market", "Market");
                                dic.Add("SharesCode", "SharesCode");
                                dic.Add("BackSharesCode", "BackSharesCode");
                                dic.Add("PresentPrice", "PresentPrice");
                                dic.Add("ClosedPrice", "ClosedPrice");
                                dic.Add("OpenedPrice", "OpenedPrice");
                                dic.Add("MaxPrice", "MaxPrice");
                                dic.Add("MinPrice", "MinPrice");
                                dic.Add("TotalCount", "TotalCount");
                                dic.Add("PresentCount", "PresentCount");
                                dic.Add("TotalAmount", "TotalAmount");
                                dic.Add("InvolCount", "InvolCount");
                                dic.Add("OuterCount", "OuterCount");
                                dic.Add("BuyPrice1", "BuyPrice1");
                                dic.Add("BuyCount1", "BuyCount1");
                                dic.Add("BuyPrice2", "BuyPrice2");
                                dic.Add("BuyCount2", "BuyCount2");
                                dic.Add("BuyPrice3", "BuyPrice3");
                                dic.Add("BuyCount3", "BuyCount3");
                                dic.Add("BuyPrice4", "BuyPrice4");
                                dic.Add("BuyCount4", "BuyCount4");
                                dic.Add("BuyPrice5", "BuyPrice5");
                                dic.Add("BuyCount5", "BuyCount5");
                                dic.Add("SellPrice1", "SellPrice1");
                                dic.Add("SellCount1", "SellCount1");
                                dic.Add("SellPrice2", "SellPrice2");
                                dic.Add("SellCount2", "SellCount2");
                                dic.Add("SellPrice3", "SellPrice3");
                                dic.Add("SellCount3", "SellCount3");
                                dic.Add("SellPrice4", "SellPrice4");
                                dic.Add("SellCount4", "SellCount4");
                                dic.Add("SellPrice5", "SellPrice5");
                                dic.Add("SellCount5", "SellCount5");
                                dic.Add("SpeedUp", "SpeedUp");
                                dic.Add("Activity", "Activity");
                                dic.Add("LastModified", "LastModified");
                                dic.Add("LimitUpPrice", "LimitUpPrice");
                                dic.Add("LimitDownPrice", "LimitDownPrice");
                                dic.Add("PriceType", "PriceType");
                                dic.Add("TriPriceType", "TriPriceType");
                                dic.Add("TriNearLimitType", "TriNearLimitType");
                                #endregion

                                bulk.ColumnMappings = dic;
                                bulk.BatchSize = 10000;

                                bulk.BulkWriteToServer(conn, table, "t_shares_quotes_temp", tran);

                                //更新数据到五档表
                                sql = @"merge into t_shares_quotes as t
using (select * from t_shares_quotes_temp) as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode and t.DataType=0
when matched
then update set t.PresentPrice = t1.PresentPrice,t.ClosedPrice = t1.ClosedPrice,t.OpenedPrice = t1.OpenedPrice,t.MaxPrice = t1.MaxPrice,t.MinPrice = t1.MinPrice,t.TotalCount = t1.TotalCount,t.PresentCount = t1.PresentCount,t.TotalAmount = t1.TotalAmount,t.InvolCount = t1.InvolCount,t.OuterCount = t1.OuterCount,t.BuyPrice1 = t1.BuyPrice1,t.BuyCount1 = t1.BuyCount1,t.BuyPrice2 = t1.BuyPrice2,t.BuyCount2 = t1.BuyCount2,t.BuyPrice3 = t1.BuyPrice3,t.BuyCount3 = t1.BuyCount3,t.BuyPrice4 = t1.BuyPrice4,t.BuyCount4 = t1.BuyCount4,t.BuyPrice5 = t1.BuyPrice5,t.BuyCount5 = t1.BuyCount5,t.SellPrice1 = t1.SellPrice1,t.SellCount1 = t1.SellCount1,t.SellPrice2 = t1.SellPrice2,t.SellCount2 = t1.SellCount2,t.SellPrice3 = t1.SellPrice3,t.SellCount3 = t1.SellCount3,t.SellPrice4 = t1.SellPrice4,t.SellCount4 = t1.SellCount4,t.SellPrice5 = t1.SellPrice5,t.SellCount5 = t1.SellCount5,t.SpeedUp = t1.SpeedUp,t.Activity = t1.Activity,t.LastModified = t1.LastModified,t.LimitUpPrice=t1.LimitUpPrice,t.LimitDownPrice=t1.LimitDownPrice,t.PriceType=t1.PriceType,t.TriPriceType=t1.TriPriceType,t.TriNearLimitType=t1.TriNearLimitType
when not matched by target
then insert(Market,SharesCode,PresentPrice,ClosedPrice,OpenedPrice,MaxPrice,MinPrice,TotalCount,PresentCount,TotalAmount,InvolCount,OuterCount,BuyPrice1,BuyCount1,BuyPrice2,BuyCount2,BuyPrice3,BuyCount3,BuyPrice4,BuyCount4,BuyPrice5,BuyCount5,SellPrice1,SellCount1,SellPrice2,SellCount2,SellPrice3,SellCount3,SellPrice4,SellCount4,SellPrice5,SellCount5,SpeedUp,Activity,LastModified,LimitUpPrice,LimitDownPrice,PriceType,TriPriceType,TriNearLimitType,DataType) values(t1.Market,t1.SharesCode,t1.PresentPrice,t1.ClosedPrice,t1.OpenedPrice,t1.MaxPrice,t1.MinPrice,t1.TotalCount,t1.PresentCount,t1.TotalAmount,t1.InvolCount,t1.OuterCount,t1.BuyPrice1,t1.BuyCount1,t1.BuyPrice2,t1.BuyCount2,t1.BuyPrice3,t1.BuyCount3,t1.BuyPrice4,t1.BuyCount4,t1.BuyPrice5,t1.BuyCount5,t1.SellPrice1,t1.SellCount1,t1.SellPrice2,t1.SellCount2,t1.SellPrice3,t1.SellCount3,t1.SellPrice4,t1.SellCount4,t1.SellPrice5,t1.SellCount5,t1.SpeedUp,t1.Activity,t1.LastModified,t1.LimitUpPrice,t1.LimitDownPrice,t1.PriceType,t1.TriPriceType,t1.TriNearLimitType,0);";
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();
                            }

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("更新五档数据意外出错", ex);
                            tran.Rollback();
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("更新五档数据数据库连接出错", ex);
                    return;
                }
                finally
                {
                    conn.Close();
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
        private static void UpdateQuotesDateByTemp(List<SharesQuotesInfo> list)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            string sql = string.Empty;

                            //删除temp表
                            sql = "truncate table t_shares_quotes_date_temp";
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();

                            var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                            using (DataTable table = new DataTable())
                            {
                                #region====定义表字段数据类型====
                                table.Columns.Add("Id", typeof(long));
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
                                #endregion

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
                                    Singleton.Instance.LastSharesQuotesList.TryGetValue(item.Market + "" + item.SharesCode, out lastQuote);
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
                                    row["Id"] = 0;
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
                                    table.Rows.Add(row);
                                }
                                #region====绑定结构字段====
                                Dictionary<string, string> dic = new Dictionary<string, string>();
                                dic.Add("Id", "Id");
                                dic.Add("Market", "Market");
                                dic.Add("SharesCode", "SharesCode");
                                dic.Add("PresentPrice", "PresentPrice");
                                dic.Add("ClosedPrice", "ClosedPrice");
                                dic.Add("OpenedPrice", "OpenedPrice");
                                dic.Add("MaxPrice", "MaxPrice");
                                dic.Add("MinPrice", "MinPrice");
                                dic.Add("LimitUpPrice", "LimitUpPrice");
                                dic.Add("LimitDownPrice", "LimitDownPrice");
                                dic.Add("LastModified", "LastModified");
                                dic.Add("Date", "Date");
                                dic.Add("LimitUpCount", "LimitUpCount");
                                dic.Add("LimitDownCount", "LimitDownCount");
                                dic.Add("LimitUpBombCount", "LimitUpBombCount");
                                dic.Add("LimitDownBombCount", "LimitDownBombCount");
                                dic.Add("TriLimitUpCount", "TriLimitUpCount");
                                dic.Add("TriLimitDownCount", "TriLimitDownCount");
                                dic.Add("TriLimitUpBombCount", "TriLimitUpBombCount");
                                dic.Add("TriLimitDownBombCount", "TriLimitDownBombCount");
                                dic.Add("PriceType", "PriceType");
                                dic.Add("TriPriceType", "TriPriceType");
                                dic.Add("TriNearLimitType", "TriNearLimitType");
                                dic.Add("TotalAmount", "TotalAmount");
                                dic.Add("TotalCount", "TotalCount");
                                #endregion
                                bulk.ColumnMappings = dic;
                                bulk.BatchSize = 10000;
                                bulk.BulkWriteToServer(conn, table, "t_shares_quotes_date_temp", tran);
                            }
                            //更新到数据表
                            sql = string.Format(@"merge into t_shares_quotes_date as t
using (select * from t_shares_quotes_date_temp) as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode and t.[Date]=t1.[Date]
when matched
then update set t.PresentPrice = t1.PresentPrice,t.ClosedPrice = t1.ClosedPrice,t.OpenedPrice = t1.OpenedPrice,t.MaxPrice = t1.MaxPrice,t.MinPrice = t1.MinPrice,t.LastModified = t1.LastModified,t.LimitUpPrice=t1.LimitUpPrice,t.LimitDownPrice=t1.LimitDownPrice,t.PriceType=t1.PriceType,t.TriPriceType=t1.TriPriceType,t.TriNearLimitType=t1.TriNearLimitType,t.TotalAmount=t1.TotalAmount,t.TotalCount=t1.TotalCount,t.LimitUpCount=t.LimitUpCount+t1.LimitUpCount,t.LimitDownCount=t.LimitDownCount+t1.LimitDownCount,t.LimitUpBombCount=t.LimitUpBombCount+t1.LimitUpBombCount,t.LimitDownBombCount=t.LimitDownBombCount+t1.LimitDownBombCount,t.TriLimitUpCount=t.TriLimitUpCount+t1.TriLimitUpCount,t.TriLimitDownCount=t.TriLimitDownCount+t1.TriLimitDownCount,t.TriLimitUpBombCount=t.TriLimitUpBombCount+t1.TriLimitUpBombCount,t.TriLimitDownBombCount=t.TriLimitDownBombCount+t1.TriLimitDownBombCount
when not matched by target
then insert(Market,SharesCode,PresentPrice,ClosedPrice,OpenedPrice,MaxPrice,MinPrice,LastModified,[Date],LimitUpPrice,LimitDownPrice,PriceType,TriPriceType,TriNearLimitType,TotalAmount,TotalCount,LimitUpCount,LimitDownCount,LimitUpBombCount,LimitDownBombCount,TriLimitUpCount,TriLimitDownCount,TriLimitUpBombCount,TriLimitDownBombCount) 
values(t1.Market,t1.SharesCode,t1.PresentPrice,t1.ClosedPrice,t1.OpenedPrice,t1.MaxPrice,t1.MinPrice,t1.LastModified,t1.[Date],t1.LimitUpPrice,t1.LimitDownPrice,t1.PriceType,t1.TriPriceType,t1.TriNearLimitType,t1.TotalAmount,t1.TotalCount,t1.LimitUpCount,t1.LimitDownCount,t1.LimitUpBombCount,t1.LimitDownBombCount,t1.TriLimitUpCount,t1.TriLimitDownCount,t1.TriLimitUpBombCount,t1.TriLimitDownBombCount);");
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
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
            #endregion

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
                Singleton.Instance.LastSharesQuotesList.TryGetValue(item.Market + "" + item.SharesCode, out lastQuote);
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

        /// <summary>
        /// 更新上一次五档数据
        /// </summary>
        private static void UpdatePreQuotes(List<SharesQuotesInfo> list)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            string sql = string.Empty;

                            //删除temp表
                            sql = "truncate table t_shares_quotes_last_temp";
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();

                            var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                            using (DataTable table = new DataTable())
                            {
                                #region====定义表字段数据类型====
                                table.Columns.Add("Id", typeof(long));
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
                                #endregion

                                #region====绑定数据====
                                foreach (var item in list)
                                {
                                    DataRow row = table.NewRow();
                                    row["Id"] = 0;
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
                                    table.Rows.Add(row);
                                }
                                #endregion

                                #region====绑定结构字段====
                                Dictionary<string, string> dic = new Dictionary<string, string>();
                                dic.Add("Id", "Id");
                                dic.Add("Market", "Market");
                                dic.Add("SharesCode", "SharesCode");
                                dic.Add("PresentPrice", "PresentPrice");
                                dic.Add("ClosedPrice", "ClosedPrice");
                                dic.Add("OpenedPrice", "OpenedPrice");
                                dic.Add("MaxPrice", "MaxPrice");
                                dic.Add("MinPrice", "MinPrice");
                                dic.Add("TotalCount", "TotalCount");
                                dic.Add("PresentCount", "PresentCount");
                                dic.Add("TotalAmount", "TotalAmount");
                                dic.Add("InvolCount", "InvolCount");
                                dic.Add("OuterCount", "OuterCount");
                                dic.Add("BuyPrice1", "BuyPrice1");
                                dic.Add("BuyCount1", "BuyCount1");
                                dic.Add("BuyPrice2", "BuyPrice2");
                                dic.Add("BuyCount2", "BuyCount2");
                                dic.Add("BuyPrice3", "BuyPrice3");
                                dic.Add("BuyCount3", "BuyCount3");
                                dic.Add("BuyPrice4", "BuyPrice4");
                                dic.Add("BuyCount4", "BuyCount4");
                                dic.Add("BuyPrice5", "BuyPrice5");
                                dic.Add("BuyCount5", "BuyCount5");
                                dic.Add("SellPrice1", "SellPrice1");
                                dic.Add("SellCount1", "SellCount1");
                                dic.Add("SellPrice2", "SellPrice2");
                                dic.Add("SellCount2", "SellCount2");
                                dic.Add("SellPrice3", "SellPrice3");
                                dic.Add("SellCount3", "SellCount3");
                                dic.Add("SellPrice4", "SellPrice4");
                                dic.Add("SellCount4", "SellCount4");
                                dic.Add("SellPrice5", "SellPrice5");
                                dic.Add("SellCount5", "SellCount5");
                                dic.Add("SpeedUp", "SpeedUp");
                                dic.Add("Activity", "Activity");
                                dic.Add("LastModified", "LastModified");
                                dic.Add("LimitUpPrice", "LimitUpPrice");
                                dic.Add("LimitDownPrice", "LimitDownPrice");
                                dic.Add("PriceType", "PriceType");
                                dic.Add("TriPriceType", "TriPriceType");
                                dic.Add("TriNearLimitType", "TriNearLimitType"); 
                                #endregion

                                bulk.ColumnMappings = dic;
                                bulk.BatchSize = 10000;
                                bulk.BulkWriteToServer(conn, table, "t_shares_quotes_last_temp", tran);
                            }
                            //更新到数据表
                            sql = string.Format(@"merge into t_shares_quotes as t
using (select * from t_shares_quotes_last_temp) as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode and t.DataType=1
when matched
then update set t.PresentPrice = t1.PresentPrice,t.ClosedPrice = t1.ClosedPrice,t.OpenedPrice = t1.OpenedPrice,t.MaxPrice = t1.MaxPrice,t.MinPrice = t1.MinPrice,t.TotalCount = t1.TotalCount,t.PresentCount = t1.PresentCount,t.TotalAmount = t1.TotalAmount,t.InvolCount = t1.InvolCount,t.OuterCount = t1.OuterCount,t.BuyPrice1 = t1.BuyPrice1,t.BuyCount1 = t1.BuyCount1,t.BuyPrice2 = t1.BuyPrice2,t.BuyCount2 = t1.BuyCount2,t.BuyPrice3 = t1.BuyPrice3,t.BuyCount3 = t1.BuyCount3,t.BuyPrice4 = t1.BuyPrice4,t.BuyCount4 = t1.BuyCount4,t.BuyPrice5 = t1.BuyPrice5,t.BuyCount5 = t1.BuyCount5,t.SellPrice1 = t1.SellPrice1,t.SellCount1 = t1.SellCount1,t.SellPrice2 = t1.SellPrice2,t.SellCount2 = t1.SellCount2,t.SellPrice3 = t1.SellPrice3,t.SellCount3 = t1.SellCount3,t.SellPrice4 = t1.SellPrice4,t.SellCount4 = t1.SellCount4,t.SellPrice5 = t1.SellPrice5,t.SellCount5 = t1.SellCount5,t.SpeedUp = t1.SpeedUp,t.Activity = t1.Activity,t.LastModified = t1.LastModified,t.LimitUpPrice=t1.LimitUpPrice,t.LimitDownPrice=t1.LimitDownPrice,t.PriceType=t1.PriceType,t.TriPriceType=t1.TriPriceType,t.TriNearLimitType=t1.TriNearLimitType
when not matched by target
then insert(Market,SharesCode,PresentPrice,ClosedPrice,OpenedPrice,MaxPrice,MinPrice,TotalCount,PresentCount,TotalAmount,InvolCount,OuterCount,BuyPrice1,BuyCount1,BuyPrice2,BuyCount2,BuyPrice3,BuyCount3,BuyPrice4,BuyCount4,BuyPrice5,BuyCount5,SellPrice1,SellCount1,SellPrice2,SellCount2,SellPrice3,SellCount3,SellPrice4,SellCount4,SellPrice5,SellCount5,SpeedUp,Activity,LastModified,LimitUpPrice,LimitDownPrice,PriceType,TriPriceType,TriNearLimitType,DataType) values(t1.Market,t1.SharesCode,t1.PresentPrice,t1.ClosedPrice,t1.OpenedPrice,t1.MaxPrice,t1.MinPrice,t1.TotalCount,t1.PresentCount,t1.TotalAmount,t1.InvolCount,t1.OuterCount,t1.BuyPrice1,t1.BuyCount1,t1.BuyPrice2,t1.BuyCount2,t1.BuyPrice3,t1.BuyCount3,t1.BuyPrice4,t1.BuyCount4,t1.BuyPrice5,t1.BuyCount5,t1.SellPrice1,t1.SellCount1,t1.SellPrice2,t1.SellCount2,t1.SellPrice3,t1.SellCount3,t1.SellPrice4,t1.SellCount4,t1.SellPrice5,t1.SellCount5,t1.SpeedUp,t1.Activity,t1.LastModified,t1.LimitUpPrice,t1.LimitDownPrice,t1.PriceType,t1.TriPriceType,t1.TriNearLimitType,1);");
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 更新上一次五档数据
        /// </summary>
        private static void UpdatePreQuotesBack(List<SharesQuotesInfo> list)
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
            #endregion

            #region====绑定数据====
            foreach (var item in list)
            {
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
                table.Rows.Add(row);
            }
            #endregion

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tran;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "P_SharesQuotesPre_Update";
                            //关键是类型
                            SqlParameter parameter = new SqlParameter("@sharesQuotesPre", SqlDbType.Structured);
                            //必须指定表类型名
                            parameter.TypeName = "dbo.SharesQuotesPre";
                            //赋值
                            parameter.Value = table;
                            cmd.Parameters.Add(parameter);
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
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
}
