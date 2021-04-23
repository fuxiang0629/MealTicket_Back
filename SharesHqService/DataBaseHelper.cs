using FXCommon.Common;
using FXCommon.Database;
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
        public static void UpdateAllShares(List<SharesBaseInfo> list, int market)
        {
            DateTime timeNow = DateTime.Now;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (SqlTransaction tran = conn.BeginTransaction())//开启事务
                {
                    try
                    {
                        string sqlDel = string.Format("delete t_shares_all_temp where Market={0}", market);

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sqlDel;   //sql语句
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                        }
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
                                row["Market"] = market;
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
                        string sqlUpdate = string.Format(@"merge into t_shares_all as t
using (select * from t_shares_all_temp where Market = {0}) as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode
when matched
then update set t.SharesCode = t1.SharesCode,t.SharesName = t1.SharesName,t.SharesPyjc = t1.SharesPyjc,t.SharesHandCount = t1.SharesHandCount,t.ShareClosedPrice = t1.ShareClosedPrice,t.LastModified = t1.LastModified
when not matched by target
then insert(SharesCode,SharesName,SharesPyjc,SharesHandCount,ShareClosedPrice,Market,[Status],MarketStatus,ForbidStatus,LastModified) values(t1.SharesCode,t1.SharesName,t1.SharesPyjc,t1.SharesHandCount,t1.ShareClosedPrice,t1.Market,1,1,0,t1.LastModified)
when not matched by source and t.Market = {0}
then delete;", market);
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

            if (Singleton.SharesBaseInfoList.ContainsKey(market))
            {
                Singleton.SharesBaseInfoList[market] = list;
            }
            else
            {
                Singleton.SharesBaseInfoList.Add(market, list);
            }
        }

        /// <summary>
        /// 更新股票实时行情数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="batchStr"></param>
        public static void UpdateSharesQuotes(List<SharesQuotesInfo> list)
        {
            DateTime timeNow = DateTime.Now;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (SqlTransaction tran = conn.BeginTransaction())//开启事务
                {
                    try
                    {
                        string sqlDel = string.Format("delete t_shares_quotes_temp");

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sqlDel;   //sql语句
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                        }
                        var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                        using (DataTable table = new DataTable())
                        {
                            try
                            {
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
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("出错1"+ex.Message);
                            }

                            try
                            {
                                foreach (var item in list)
                                {
                                    if (item == null)
                                    {
                                        continue;
                                    }

                                    long LimitUpPrice = 0;
                                    long LimitDownPrice = 0;
                                    int PriceType = 0;
                                    int range = Singleton.Instance.RangeList.Where(e=>(e.LimitMarket==item.Market || e.LimitMarket==-1) && item.SharesCode.StartsWith(e.LimitKey)).Select(e=>e.Range).FirstOrDefault();
                                    if (range > 0 && item.ClosedPrice > 0)
                                    {
                                        LimitUpPrice = (long)((item.ClosedPrice + item.ClosedPrice * (range * 1.0 / 10000)) / 100 + 0.5)*100;
                                        LimitDownPrice = (long)((item.ClosedPrice - item.ClosedPrice * (range * 1.0 / 10000)) / 100 + 0.5) * 100;
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
                                    row["LastModified"] = DateTime.Now;
                                    row["LimitUpPrice"] = LimitUpPrice;
                                    row["LimitDownPrice"] = LimitDownPrice;
                                    row["PriceType"] = PriceType;
                                    table.Rows.Add(row);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("出错2" + ex.Message);
                            }

                            try
                            {
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
                                bulk.ColumnMappings = dic;
                                bulk.BatchSize = 10000;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("出错3" + ex.Message);
                            }

                            try
                            {
                                bulk.BulkWriteToServer(conn, table, "t_shares_quotes_temp", tran);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("出错4" + ex.Message);
                            }
                        }
                        string sqlUpdate = string.Format(@"merge into t_shares_quotes as t
using (select * from (select *,ROW_NUMBER() OVER(partition by Market,SharesCode order by LastModified desc) num from t_shares_quotes_temp)t where t.num=1) as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode
when matched
then update set t.PresentPrice = t1.PresentPrice,t.ClosedPrice = t1.ClosedPrice,t.OpenedPrice = t1.OpenedPrice,t.MaxPrice = t1.MaxPrice,t.MinPrice = t1.MinPrice,t.TotalCount = t1.TotalCount,t.PresentCount = t1.PresentCount,t.TotalAmount = t1.TotalAmount,t.InvolCount = t1.InvolCount,t.OuterCount = t1.OuterCount,t.BuyPrice1 = t1.BuyPrice1,t.BuyCount1 = t1.BuyCount1,t.BuyPrice2 = t1.BuyPrice2,t.BuyCount2 = t1.BuyCount2,t.BuyPrice3 = t1.BuyPrice3,t.BuyCount3 = t1.BuyCount3,t.BuyPrice4 = t1.BuyPrice4,t.BuyCount4 = t1.BuyCount4,t.BuyPrice5 = t1.BuyPrice5,t.BuyCount5 = t1.BuyCount5,t.SellPrice1 = t1.SellPrice1,t.SellCount1 = t1.SellCount1,t.SellPrice2 = t1.SellPrice2,t.SellCount2 = t1.SellCount2,t.SellPrice3 = t1.SellPrice3,t.SellCount3 = t1.SellCount3,t.SellPrice4 = t1.SellPrice4,t.SellCount4 = t1.SellCount4,t.SellPrice5 = t1.SellPrice5,t.SellCount5 = t1.SellCount5,t.SpeedUp = t1.SpeedUp,t.Activity = t1.Activity,t.LastModified = t1.LastModified,t.LimitUpPrice=t1.LimitUpPrice,t.LimitDownPrice=t1.LimitDownPrice,t.PriceType=t1.PriceType
when not matched by target
then insert(Market,SharesCode,PresentPrice,ClosedPrice,OpenedPrice,MaxPrice,MinPrice,TotalCount,PresentCount,TotalAmount,InvolCount,OuterCount,BuyPrice1,BuyCount1,BuyPrice2,BuyCount2,BuyPrice3,BuyCount3,BuyPrice4,BuyCount4,BuyPrice5,BuyCount5,SellPrice1,SellCount1,SellPrice2,SellCount2,SellPrice3,SellCount3,SellPrice4,SellCount4,SellPrice5,SellCount5,SpeedUp,Activity,LastModified,LimitUpPrice,LimitDownPrice,PriceType) values(t1.Market,t1.SharesCode,t1.PresentPrice,t1.ClosedPrice,t1.OpenedPrice,t1.MaxPrice,t1.MinPrice,t1.TotalCount,t1.PresentCount,t1.TotalAmount,t1.InvolCount,t1.OuterCount,t1.BuyPrice1,t1.BuyCount1,t1.BuyPrice2,t1.BuyCount2,t1.BuyPrice3,t1.BuyCount3,t1.BuyPrice4,t1.BuyCount4,t1.BuyPrice5,t1.BuyCount5,t1.SellPrice1,t1.SellCount1,t1.SellPrice2,t1.SellCount2,t1.SellPrice3,t1.SellCount3,t1.SellPrice4,t1.SellCount4,t1.SellPrice5,t1.SellCount5,t1.SpeedUp,t1.Activity,t1.LastModified,t1.LimitUpPrice,t1.LimitDownPrice,t1.PriceType);");
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
                        Console.WriteLine("这里出错了"+ex.Message);
                        Logger.WriteFileLog("更新股票五档行情数据出错", ex);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 更新股票k线数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="batchStr"></param>
        public static void UpdateSharesBars(List<SharesBarsInfo> list, string batchStr)
        {
            DateTime timeNow = DateTime.Now;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (SqlTransaction tran = conn.BeginTransaction())//开启事务
                {
                    try
                    {
                        Console.WriteLine("=============开始批量导入数据==================");
                        var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                        using (DataTable table = new DataTable())
                        {
                            table.Columns.Add("Id", typeof(long));
                            table.Columns.Add("Market", typeof(int));
                            table.Columns.Add("SharesCode", typeof(string));
                            table.Columns.Add("Time", typeof(DateTime));
                            table.Columns.Add("TimeStr", typeof(string));
                            table.Columns.Add("Type", typeof(int));
                            table.Columns.Add("OpenedPrice", typeof(long));
                            table.Columns.Add("ClosedPrice", typeof(long));
                            table.Columns.Add("MaxPrice", typeof(long));
                            table.Columns.Add("MinPrice", typeof(long));
                            table.Columns.Add("Volume", typeof(int));
                            table.Columns.Add("Turnover", typeof(long));
                            table.Columns.Add("LastModified", typeof(DateTime));
                            table.Columns.Add("BatchTime", typeof(string));

                            foreach (var item in list)
                            {
                                DataRow row = table.NewRow();
                                row["Id"] = 0;
                                row["Market"] = item.Market;
                                row["SharesCode"] = item.SharesCode;
                                row["Time"] = item.Time;
                                row["TimeStr"] = item.TimeStr;
                                row["Type"] = item.Type;
                                row["OpenedPrice"] = item.OpenedPrice;
                                row["ClosedPrice"] = item.ClosedPrice;
                                row["MaxPrice"] = item.MaxPrice;
                                row["MinPrice"] = item.MinPrice;
                                row["Volume"] = item.Volume;
                                row["Turnover"] = item.Turnover;
                                row["LastModified"] = DateTime.Now;
                                row["BatchTime"] = batchStr;
                                table.Rows.Add(row);
                            }
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("Id", "Id");
                            dic.Add("Market", "Market");
                            dic.Add("SharesCode", "SharesCode");
                            dic.Add("Time", "Time");
                            dic.Add("TimeStr", "TimeStr");
                            dic.Add("Type", "Type");
                            dic.Add("OpenedPrice", "OpenedPrice");
                            dic.Add("ClosedPrice", "ClosedPrice");
                            dic.Add("MaxPrice", "MaxPrice");
                            dic.Add("MinPrice", "MinPrice");
                            dic.Add("Volume", "Volume");
                            dic.Add("Turnover", "Turnover");
                            dic.Add("LastModified", "LastModified");
                            dic.Add("BatchTime", "BatchTime");
                            bulk.ColumnMappings = dic;
                            bulk.BatchSize = 10000;

                            bulk.BulkWriteToServer(conn, table, "t_shares_bars_temp", tran);
                        }
                        Console.WriteLine("=============批量导入数据完成==================");

                        Console.WriteLine("=============开始更新数据==================");
                        string sqlUpdate = string.Format(@"merge into t_shares_bars as t
using (select * from t_shares_bars_temp where BatchTime = '{0}') as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode and t.TimeStr=t1.TimeStr and t.[Type]=t1.[Type]
when not matched by target
then insert values(t1.Market,t1.SharesCode,t1.[Time],t1.TimeStr,t1.[Type],t1.OpenedPrice,t1.ClosedPrice,t1.MaxPrice,t1.MinPrice,t1.Volume,t1.Turnover,t1.LastModified);", batchStr);
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            cmd.CommandText = sqlUpdate;   //sql语句
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("=============更新数据完成==================");

                        Console.WriteLine("=============开始删除临时数据==================");
                        string sqlDel = string.Format("delete t_shares_bars_temp where BatchTime={0}", batchStr);
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            cmd.CommandText = sqlDel;   //sql语句
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("=============临时数据删除完成==================");

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Console.WriteLine(ex.Message);
                        Logger.WriteFileLog("更新股票k线数据出错", ex);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 更新指数k线数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="batchStr"></param>
        public static void UpdateSharesIndexBars(List<SharesIndexBarsInfo> list, string batchStr)
        {
            DateTime timeNow = DateTime.Now;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (SqlTransaction tran = conn.BeginTransaction())//开启事务
                {
                    try
                    {
                        Console.WriteLine("=============开始批量导入数据==================");
                        var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                        using (DataTable table = new DataTable())
                        {
                            table.Columns.Add("Id", typeof(long));
                            table.Columns.Add("Market", typeof(int));
                            table.Columns.Add("SharesCode", typeof(string));
                            table.Columns.Add("Time", typeof(DateTime));
                            table.Columns.Add("TimeStr", typeof(string));
                            table.Columns.Add("Type", typeof(int));
                            table.Columns.Add("OpenedPrice", typeof(long));
                            table.Columns.Add("ClosedPrice", typeof(long));
                            table.Columns.Add("MaxPrice", typeof(long));
                            table.Columns.Add("MinPrice", typeof(long));
                            table.Columns.Add("Volume", typeof(int));
                            table.Columns.Add("Turnover", typeof(long));
                            table.Columns.Add("RiseCount", typeof(int));
                            table.Columns.Add("FallCount", typeof(int));
                            table.Columns.Add("LastModified", typeof(DateTime));
                            table.Columns.Add("BatchTime", typeof(string));

                            foreach (var item in list)
                            {
                                DataRow row = table.NewRow();
                                row["Id"] = 0;
                                row["Market"] = item.Market;
                                row["SharesCode"] = item.SharesCode;
                                row["Time"] = item.Time;
                                row["TimeStr"] = item.TimeStr;
                                row["Type"] = item.Type;
                                row["OpenedPrice"] = item.OpenedPrice;
                                row["ClosedPrice"] = item.ClosedPrice;
                                row["MaxPrice"] = item.MaxPrice;
                                row["MinPrice"] = item.MinPrice;
                                row["Volume"] = item.Volume;
                                row["Turnover"] = item.Turnover;
                                row["RiseCount"] = item.RiseCount;
                                row["FallCount"] = item.FallCount;
                                row["LastModified"] = DateTime.Now;
                                row["BatchTime"] = batchStr;
                                table.Rows.Add(row);
                            }
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("Id", "Id");
                            dic.Add("Market", "Market");
                            dic.Add("SharesCode", "SharesCode");
                            dic.Add("Time", "Time");
                            dic.Add("TimeStr", "TimeStr");
                            dic.Add("Type", "Type");
                            dic.Add("OpenedPrice", "OpenedPrice");
                            dic.Add("ClosedPrice", "ClosedPrice");
                            dic.Add("MaxPrice", "MaxPrice");
                            dic.Add("MinPrice", "MinPrice");
                            dic.Add("Volume", "Volume");
                            dic.Add("Turnover", "Turnover");
                            dic.Add("RiseCount", "RiseCount");
                            dic.Add("FallCount", "FallCount");
                            dic.Add("LastModified", "LastModified");
                            dic.Add("BatchTime", "BatchTime");
                            bulk.ColumnMappings = dic;
                            bulk.BatchSize = 10000;

                            bulk.BulkWriteToServer(conn, table, "t_shares_bars_index_temp", tran);
                        }
                        Console.WriteLine("=============批量导入数据完成==================");

                        Console.WriteLine("=============开始更新数据==================");
                        string sqlUpdate = string.Format(@"merge into t_shares_bars_index as t
using (select * from t_shares_bars_index_temp where BatchTime = '{0}') as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode and t.TimeStr=t1.TimeStr and t.[Type]=t1.[Type]
when not matched by target
then insert values(t1.Market,t1.SharesCode,t1.[Time],t1.TimeStr,t1.[Type],t1.OpenedPrice,t1.ClosedPrice,t1.MaxPrice,t1.MinPrice,t1.Volume,t1.Turnover,t1.RiseCount,t1.FallCount,t1.LastModified);", batchStr);
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            cmd.CommandText = sqlUpdate;   //sql语句
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("=============更新数据完成==================");

                        Console.WriteLine("=============开始删除临时数据==================");
                        string sqlDel = string.Format("delete t_shares_bars_index_temp where BatchTime={0}", batchStr);
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            cmd.CommandText = sqlDel;   //sql语句
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("=============临时数据删除完成==================");
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Console.WriteLine(ex.Message);
                        Logger.WriteFileLog("更新指数k线数据出错", ex);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 更新分时数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="batchStr"></param>
        public static void UpdateMinuteTimeData(List<SharesMinuteTimeData> list, string batchStr)
        {
            DateTime timeNow = DateTime.Now;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (SqlTransaction tran = conn.BeginTransaction())//开启事务
                {
                    try
                    {
                        Console.WriteLine("=============开始批量导入数据==================");
                        var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                        using (DataTable table = new DataTable())
                        {
                            table.Columns.Add("Id", typeof(long));
                            table.Columns.Add("Market", typeof(int));
                            table.Columns.Add("SharesCode", typeof(string));
                            table.Columns.Add("Time", typeof(DateTime));
                            table.Columns.Add("Price", typeof(long));
                            table.Columns.Add("Volume", typeof(int));
                            table.Columns.Add("LastModified", typeof(DateTime));
                            table.Columns.Add("BatchTime", typeof(string));

                            foreach (var item in list)
                            {
                                DataRow row = table.NewRow();
                                row["Id"] = 0;
                                row["Market"] = item.Market;
                                row["SharesCode"] = item.SharesCode;
                                row["Time"] = item.Time;
                                row["Price"] = item.Price;
                                row["Volume"] = item.Volume;
                                row["LastModified"] = DateTime.Now;
                                row["BatchTime"] = batchStr;
                                table.Rows.Add(row);
                            }
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("Id", "Id");
                            dic.Add("Market", "Market");
                            dic.Add("SharesCode", "SharesCode");
                            dic.Add("Time", "Time");
                            dic.Add("Price", "Price");
                            dic.Add("Volume", "Volume");
                            dic.Add("LastModified", "LastModified");
                            dic.Add("BatchTime", "BatchTime");
                            bulk.ColumnMappings = dic;
                            bulk.BatchSize = 10000;

                            bulk.BulkWriteToServer(conn, table, "t_shares_minutetimedata_temp", tran);
                        }
                        Console.WriteLine("=============批量导入数据完成==================");

                        Console.WriteLine("=============开始更新数据==================");
                        string sqlUpdate = string.Format(@"merge into t_shares_minutetimedata as t
using (select * from t_shares_minutetimedata_temp where BatchTime = '{0}') as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode and t.[Time]=t1.[Time]
when not matched by target
then insert values(t1.Market,t1.SharesCode,t1.[Time],t1.Price,t1.Volume,t1.LastModified);", batchStr);
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            cmd.CommandText = sqlUpdate;   //sql语句
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("=============更新数据完成==================");

                        Console.WriteLine("=============开始删除临时数据==================");
                        string sqlDel = string.Format("delete t_shares_minutetimedata_temp where BatchTime={0}", batchStr);
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            cmd.CommandText = sqlDel;   //sql语句
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("=============临时数据删除完成==================");
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Console.WriteLine(ex.Message);
                        Logger.WriteFileLog("更新指数k线数据出错", ex);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 更新分笔成交数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="batchStr"></param>
        public static void UpdateTransactionData(List<SharesTransactionDataInfo> list)
        {
            DateTime timeNow = DateTime.Now;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (SqlTransaction tran = conn.BeginTransaction())//开启事务
                {
                    try
                    {
                        string sqlDel = string.Format("truncate table t_shares_transactiondata_temp");
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sqlDel;   //sql语句
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                        }
                        var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                        using (DataTable table = new DataTable())
                        {
                            table.Columns.Add("Id", typeof(long));
                            table.Columns.Add("Market", typeof(int));
                            table.Columns.Add("SharesCode", typeof(string));
                            table.Columns.Add("Time", typeof(DateTime));
                            table.Columns.Add("TimeStr", typeof(string));
                            table.Columns.Add("Price", typeof(long));
                            table.Columns.Add("Volume", typeof(int));
                            table.Columns.Add("Stock", typeof(int));
                            table.Columns.Add("Type", typeof(int));
                            table.Columns.Add("LastModified", typeof(DateTime));

                            foreach (var item in list)
                            {
                                DataRow row = table.NewRow();
                                row["Id"] = 0;
                                row["Market"] = item.Market;
                                row["SharesCode"] = item.SharesCode;
                                row["Time"] = item.Time;
                                row["TimeStr"] = item.TimeStr;
                                row["Price"] = item.Price;
                                row["Volume"] = item.Volume;
                                row["Stock"] = item.Stock;
                                row["Type"] = item.Type;
                                row["LastModified"] = DateTime.Now;
                                table.Rows.Add(row);
                            }
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("Id", "Id");
                            dic.Add("Market", "Market");
                            dic.Add("SharesCode", "SharesCode");
                            dic.Add("Time", "Time");
                            dic.Add("TimeStr", "TimeStr");
                            dic.Add("Price", "Price");
                            dic.Add("Volume", "Volume");
                            dic.Add("Stock", "Stock");
                            dic.Add("Type", "Type");
                            dic.Add("LastModified", "LastModified");
                            bulk.ColumnMappings = dic;
                            bulk.BatchSize = 10000;

                            bulk.BulkWriteToServer(conn, table, "t_shares_transactiondata_temp", tran);
                        }

                        string sqlUpdate = string.Format(@"merge into t_shares_transactiondata as t
using (select * from t_shares_transactiondata_temp) as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode and t.[Time]=t1.[Time]
when not matched by target
then insert values(t1.Market,t1.SharesCode,t1.[Time],t1.TimeStr,t1.Price,t1.Volume,t1.Stock,t1.[Type],t1.LastModified);");
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
                        Console.WriteLine(ex.Message);
                        Logger.WriteFileLog("更新更新分笔成交数据出错", ex);
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
