using FXCommon.Common;
using FXCommon.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeAPI;

namespace TransactionDataUpdate_NO4
{
    public class DataHelper
    {
        public class TRANSACTION_DATA_STRING
        {
            public StringBuilder sErrInfo { get; set; }
            public StringBuilder sResult { get; set; }
        }

        static TRANSACTION_DATA_STRING[] sm_td_string = new TRANSACTION_DATA_STRING[Singleton.Instance.hqClientCount];
        static DataHelper()
        {
            for (int idx = 0; idx < Singleton.Instance.hqClientCount; idx++)
            {
                sm_td_string[idx] = new TRANSACTION_DATA_STRING();
                sm_td_string[idx].sErrInfo = new StringBuilder(256);
                sm_td_string[idx].sResult = new StringBuilder(512 * Singleton.Instance.NewTransactionDataCount);
            }
        }

        /// <summary>
        /// 获取分笔成交数据
        /// </summary>
        /// <returns></returns>
        public static List<SharesTransactionDataInfo> TdxHq_GetTransactionDataBack()
        {
            ThreadMsgTemplate<SharesInfo> data = new ThreadMsgTemplate<SharesInfo>();
            data.Init();

            DateTime dateNow = DateTime.Now.Date;

            List<SharesInfo> sharesList = new List<SharesInfo>();
            Console.WriteLine("=======开始查询数据库股票数据=="+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+"===");
            #region================获取需要的股票==========================
            using (var db = new meal_ticketEntities())
            {
                string sql = string.Format(@"select Market,SharesCode
  from
  (
  select t.Market, t.SharesCode
  from v_shares_quotes_last t
  inner
  join t_shares_all t1 on t.Market = t1.Market and t.SharesCode = t1.SharesCode
  inner join t_shares_monitor t2 on t.Market = t2.Market and t.SharesCode = t2.SharesCode
  where t.LastModified > '{0}' and t2.[Status] = 1
  union all
  select t3.Market, t3.SharesCode
  from t_account_shares_conditiontrade_buy_details_other_trend t
  inner
  join t_account_shares_conditiontrade_buy_details_other t1 on t.OtherId = t1.Id

            inner
              join t_account_shares_conditiontrade_buy_details t2 on t1.DetailsId = t2.Id

            inner
              join t_account_shares_conditiontrade_buy t3 on t2.ConditionId = t3.Id
              where t.[Status] = 1 and t1.[Status] = 1 and t2.[Status] = 1 and t3.[Status] = 1 and(t.TrendId = 1 or t.TrendId = 2 or t.TrendId = 3 or t.TrendId = 6)
              group by t3.Market, t3.SharesCode
              union all
              select t3.Market, t3.SharesCode
              from t_account_shares_conditiontrade_buy_details_auto_trend t
              inner
              join t_account_shares_conditiontrade_buy_details_auto t1 on t.AutoId = t1.Id

            inner
              join t_account_shares_conditiontrade_buy_details t2 on t1.DetailsId = t2.Id

            inner
              join t_account_shares_conditiontrade_buy t3 on t2.ConditionId = t3.Id
              where t.[Status] = 1 and t1.[Status] = 1 and t2.[Status] = 1 and t3.[Status] = 1 and(t.TrendId = 1 or t.TrendId = 2 or t.TrendId = 3 or t.TrendId = 6)
              group by t3.Market, t3.SharesCode
              )t
              group by Market, SharesCode", dateNow.ToString("yyyy-MM-dd"));
                sharesList = db.Database.SqlQuery<SharesInfo>(sql).ToList();
            }
            #endregion
            Console.WriteLine("=======查询数据库股票数据结束==" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
            string SharesInfoNumArr = string.Empty;
            foreach (var item in sharesList)
            {
                item.SharesInfoNum = int.Parse(item.SharesCode) * 10 + item.Market;
                data.AddMessage(item);
                SharesInfoNumArr = SharesInfoNumArr + item.SharesInfoNum + "," ;
            }
            if (string.IsNullOrEmpty(SharesInfoNumArr))
            {
                return new List<SharesTransactionDataInfo>();
            }
            SharesInfoNumArr = SharesInfoNumArr.TrimEnd(',');

            List<LastData> lastList = new List<LastData>();
            string dateStart = dateNow.ToString("yyyy-MM-dd");
            string dateEnd = dateNow.AddDays(1).ToString("yyyy-MM-dd");
            Console.WriteLine("=======开始查询数据库最后一条数据==" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
            #region=================查询股票最后index数据=======================
            string sqlquery = string.Format(@"select SharesInfoNum,OrderIndex,[Time]
from
(
select SharesInfoNum,OrderIndex,[Time],row_number()over(partition by SharesInfoNum order by [Time] desc) num
from t_shares_transactiondata with(nolock)
where IsTimeFirst=1 and SharesInfoNum in ({0}) and [Time]>='{1}' and [Time]<'{2}'
)t
where t.num=1", SharesInfoNumArr, dateStart, dateEnd);
            using (SqlConnection conn = new SqlConnection(Singleton.Instance.connString_transactiondata))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlquery;
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            lastList.Add(new LastData
                            {
                                Time = (DateTime)reader["Time"],
                                OrderIndex = (int)reader["OrderIndex"],
                                SharesInfoNumArr = (int)reader["SharesInfoNum"]
                            });
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("查询股票最后index数据出错", ex);
                }
                finally
                {
                    conn.Close();
                }
            }
            #endregion
            Console.WriteLine("=======查询数据库最后一条数据结束==" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");

            List<SharesTransactionDataInfo> resultList = new List<SharesTransactionDataInfo>();
            object resultLock = new object();

            Task[] tArr = new Task[Singleton.Instance.hqClientCount];
            for (int i = 0; i < Singleton.Instance.hqClientCount; i++)
            {

                tArr[i] = new Task((ref_string) =>
                {
                    using (SqlConnection conn = new SqlConnection(Singleton.Instance.connString_transactiondata))
                    {
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }

                        int hqClient = Singleton.Instance.GetHqClient();
                        try
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandType = CommandType.Text;
                                do
                                {
                                    SharesInfo tempData = new SharesInfo();
                                    if (!data.GetMessage(ref tempData, true))
                                    {
                                        break;
                                    }

                                    bool isReconnectClient = false;

                                    DateTime? lastTime = null;
                                    int orderIndex = 0;
                                    var lastData=lastList.Where(e => e.SharesInfoNumArr == tempData.SharesInfoNum).FirstOrDefault();
                                    if (lastData != null)
                                    {
                                        orderIndex = lastData.OrderIndex-1;
                                        lastTime = lastData.Time;
                                    }
                                    var tempList = TdxHq_GetTransactionData_byShares(hqClient, tempData.Market, tempData.SharesCode, orderIndex, lastTime, conn, cmd, (TRANSACTION_DATA_STRING)ref_string, ref isReconnectClient);
                                    if (isReconnectClient) {
                                        Singleton.Instance.AddRetryClient(hqClient);
                                        hqClient = Singleton.Instance.GetHqClient();
                                    }

                                    lock (resultLock)
                                    {
                                        resultList.AddRange(tempList);
                                    }

                                } while (true);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("更新分笔数据数据库连接出错", ex);
                        }
                        finally
                        {
                            Singleton.Instance.AddHqClient(hqClient);
                            conn.Close();
                        }
                    }
                }, sm_td_string[i],TaskCreationOptions.LongRunning);
                tArr[i].Start();
            }
            //for (int i = 0; i < Singleton.Instance.hqClientCount; i++)
            //{ 
            //    tArr[i].Join();
            //}
            Task.WaitAll(tArr);
            data.Release();

            return resultList;
        }

        /// <summary>
        /// 获取分笔成交数据
        /// </summary>
        /// <returns></returns>
        public static List<SharesTransactionDataInfo> TdxHq_GetTransactionData(List<SharesInfo> sharesList)
        {
            ThreadMsgTemplate<SharesInfo> data = new ThreadMsgTemplate<SharesInfo>();
            data.Init();

            DateTime dateNow = DateTime.Now.Date;

            string SharesInfoNumArr = string.Empty;
            foreach (var item in sharesList)
            {
                item.SharesInfoNum = int.Parse(item.SharesCode) * 10 + item.Market;
                data.AddMessage(item);
                SharesInfoNumArr = SharesInfoNumArr + item.SharesInfoNum + ",";
            }
            if (string.IsNullOrEmpty(SharesInfoNumArr))
            {
                return new List<SharesTransactionDataInfo>();
            }
            SharesInfoNumArr = SharesInfoNumArr.TrimEnd(',');

            List<LastData> lastList = new List<LastData>();
            string dateStart = dateNow.ToString("yyyy-MM-dd");
            string dateEnd = dateNow.AddDays(1).ToString("yyyy-MM-dd");
            Console.WriteLine("=======开始查询数据库最后一条数据==" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");
            #region=================查询股票最后index数据=======================
            string sqlquery = string.Format(@"select SharesInfoNum,OrderIndex,[Time]
from
(
select SharesInfoNum,OrderIndex,[Time],row_number()over(partition by SharesInfoNum order by [Time] desc) num
from t_shares_transactiondata with(nolock)
where IsTimeFirst=1 and SharesInfoNum in ({0}) and [Time]>='{1}' and [Time]<'{2}'
)t
where t.num=1", SharesInfoNumArr, dateStart, dateEnd);
            using (SqlConnection conn = new SqlConnection(Singleton.Instance.connString_transactiondata))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlquery;
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            lastList.Add(new LastData
                            {
                                Time = (DateTime)reader["Time"],
                                OrderIndex = (int)reader["OrderIndex"],
                                SharesInfoNumArr = (int)reader["SharesInfoNum"]
                            });
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("查询股票最后index数据出错", ex);
                }
                finally
                {
                    conn.Close();
                }
            }
            #endregion
            Console.WriteLine("=======查询数据库最后一条数据结束==" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "===");

            List<SharesTransactionDataInfo> resultList = new List<SharesTransactionDataInfo>();
            object resultLock = new object();

            Task[] tArr = new Task[Singleton.Instance.hqClientCount];
            for (int i = 0; i < Singleton.Instance.hqClientCount; i++)
            {

                tArr[i] = new Task((ref_string) =>
                {
                    using (SqlConnection conn = new SqlConnection(Singleton.Instance.connString_transactiondata))
                    {
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }

                        int hqClient = Singleton.Instance.GetHqClient();
                        try
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandType = CommandType.Text;
                                do
                                {
                                    SharesInfo tempData = new SharesInfo();
                                    if (!data.GetMessage(ref tempData, true))
                                    {
                                        break;
                                    }

                                    bool isReconnectClient = false;

                                    DateTime? lastTime = null;
                                    int orderIndex = 0;
                                    var lastData = lastList.Where(e => e.SharesInfoNumArr == tempData.SharesInfoNum).FirstOrDefault();
                                    if (lastData != null)
                                    {
                                        orderIndex = lastData.OrderIndex - 1;
                                        lastTime = lastData.Time;
                                    }
                                    var tempList = TdxHq_GetTransactionData_byShares(hqClient, tempData.Market, tempData.SharesCode, orderIndex, lastTime, conn, cmd, (TRANSACTION_DATA_STRING)ref_string, ref isReconnectClient);
                                    if (isReconnectClient)
                                    {
                                        Singleton.Instance.AddRetryClient(hqClient);
                                        hqClient = Singleton.Instance.GetHqClient();
                                    }

                                    lock (resultLock)
                                    {
                                        resultList.AddRange(tempList);
                                    }

                                } while (true);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("更新分笔数据数据库连接出错", ex);
                        }
                        finally
                        {
                            Singleton.Instance.AddHqClient(hqClient);
                            conn.Close();
                        }
                    }
                }, sm_td_string[i], TaskCreationOptions.LongRunning);
                tArr[i].Start();
            }
            Task.WaitAll(tArr);
            data.Release();

            return resultList;
        }


        /// <summary>
        /// 获取某只股票分笔数据
        /// </summary>
        /// <returns></returns>
        public static List<SharesTransactionDataInfo> TdxHq_GetTransactionData_byShares(int hqClient, int market, string sharesCode,int orderIndex, DateTime? lastTime, SqlConnection conn,SqlCommand cmd, TRANSACTION_DATA_STRING result_info, ref bool isReconnectClient)
        {
            DateTime datePar = DateTime.Now.Date;
            List<SharesTransactionDataInfo> resultlist = new List<SharesTransactionDataInfo>();
            string date = datePar.ToString("yyyy-MM-dd");
            int totalCount = 0;
  
            short nStart = 0;
            short nCount = (short)Singleton.Instance.NewTransactionDataCount;
            bool iscontinue = true;
            do
            {
                StringBuilder sErrInfo = result_info.sErrInfo;
                StringBuilder sResult = result_info.sResult;
                sErrInfo.Clear();
                sResult.Clear();

                bool bRet = false;
                if (datePar.Date == DateTime.Now.Date)
                {
                    bRet = TradeX_M.TdxHq_GetTransactionData(hqClient, (byte)market, sharesCode, nStart, ref nCount, sResult, sErrInfo);
                }
                else
                {
                    bRet = TradeX_M.TdxHq_GetHistoryTransactionData(hqClient, (byte)market, sharesCode, nStart, ref nCount, int.Parse(datePar.ToString("yyyyMMdd")), sResult, sErrInfo);
                }

                if (!bRet)
                {
                    isReconnectClient = true;
                    string errDes = string.Format("获取分笔成交数据出错，原因：{0}", sErrInfo.ToString());
                    Console.WriteLine(errDes);
                    break;
                }
                else
                {
                    int tempOrder = 0;//临时排序值
                    string result = sResult.ToString();
                    string[] rows = result.Split('\n');
                    for (int i = 0; i < rows.Length; i++)
                    {
                        if (i == 0)
                        {
                            continue;
                        }
                        string[] column = rows[i].Split('\t');
                        if (column.Length < 5)
                        {
                            string errDes = "获取分笔成交数据结构有误";
                            Console.WriteLine(errDes);
                            break;
                        }

                        DateTime Time = DateTime.Parse(date + " " + column[0] + ":00");
                        string TimeStr = column[0];
                        long Price = (long)(Math.Round(float.Parse(column[1]) * 10000, 0));//现价
                        int Volume = 0;
                        int Stock = int.Parse(column[2]);
                        int Type;
                        if (datePar.Date == DateTime.Now.Date)
                        {
                            Type = int.Parse(column[4]);
                        }
                        else
                        {
                            Type = int.Parse(column[3]);
                        }
                        if (lastTime != null && lastTime.Value > Time)
                        {
                            iscontinue = false;
                        }
                        else
                        {
                            resultlist.Add(new SharesTransactionDataInfo
                            {
                                SharesCode = sharesCode,
                                Market = market,
                                Price = Price,
                                Volume = Volume,
                                Time = Time,
                                TimeStr = TimeStr,
                                Stock = Stock,
                                Type = Type,
                                OrderIndex = tempOrder - nCount - nStart
                            });
                            tempOrder++;
                        }
                    }
                    nStart = (short)(nStart + nCount);
                    totalCount += nCount;
                }
            } while (nCount >= Singleton.Instance.NewTransactionDataCount && iscontinue);

            DateTime? FirstTime=null;
            for (int i=0;i<resultlist.Count();i++)
            {
                resultlist[i].IsTimeLast = false;
                if (FirstTime == null || resultlist[i].Time > FirstTime.Value)
                {
                    resultlist[i].IsTimeFirst = true;
                    if (i > 0)
                    {
                        resultlist[i-1].IsTimeLast = true;
                    }
                }
                else
                {
                    resultlist[i].IsTimeFirst = false;
                }
                resultlist[i].OrderIndex = orderIndex + totalCount + 1 + resultlist[i].OrderIndex;
                FirstTime = resultlist[i].Time;
            }
            return resultlist;
        }

        /// <summary>
        /// 更新分笔数据
        /// </summary>
        /// <param name=""></param>
        public static bool UpdateTransactionData(List<SharesTransactionDataInfo> list)
        {
            bool IsSuccess = true;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString_data"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        string sql = string.Format("truncate table {0}", Singleton.Instance.TempTableName);
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                        using (SqlTransaction tran = conn.BeginTransaction())
                        {
                            cmd.Transaction = tran;
                            try
                            {
                                var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                                using (DataTable table = new DataTable())
                                {
                                    #region====定义表字段数据类型====
                                    table.Columns.Add("Id", typeof(long));
                                    table.Columns.Add("Market", typeof(int));
                                    table.Columns.Add("SharesCode", typeof(string));
                                    table.Columns.Add("Time", typeof(DateTime));
                                    table.Columns.Add("TimeStr", typeof(string));
                                    table.Columns.Add("Price", typeof(long));
                                    table.Columns.Add("Volume", typeof(int));
                                    table.Columns.Add("Stock", typeof(int));
                                    table.Columns.Add("Type", typeof(int));
                                    table.Columns.Add("OrderIndex", typeof(int));
                                    table.Columns.Add("LastModified", typeof(DateTime));
                                    table.Columns.Add("SharesInfo", typeof(string));
                                    table.Columns.Add("SharesInfoNum", typeof(int));
                                    table.Columns.Add("IsTimeFirst", typeof(bool));
                                    table.Columns.Add("IsTimeLast", typeof(bool)); 
                                    #endregion

                                    #region====绑定数据====
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
                                        row["OrderIndex"] = item.OrderIndex;
                                        row["LastModified"] = DateTime.Now;
                                        row["SharesInfo"] = item.SharesCode + ',' + item.Market;
                                        row["SharesInfoNum"] = int.Parse(item.SharesCode) * 10 + item.Market;
                                        row["IsTimeFirst"] = item.IsTimeFirst;
                                        row["IsTimeLast"] = item.IsTimeLast;
                                        table.Rows.Add(row);
                                    }
                                    #endregion

                                    #region====绑定结构字段====
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
                                    dic.Add("OrderIndex", "OrderIndex");
                                    dic.Add("LastModified", "LastModified");
                                    dic.Add("SharesInfo", "SharesInfo");
                                    dic.Add("SharesInfoNum", "SharesInfoNum");
                                    dic.Add("IsTimeFirst", "IsTimeFirst");
                                    dic.Add("IsTimeLast", "IsTimeLast");
                                    #endregion

                                    bulk.BulkCopyTimeout = 600;
                                    bulk.ColumnMappings = dic;
                                    bulk.BatchSize = 10000;

                                    bulk.BulkWriteToServer(conn, table, Singleton.Instance.TempTableName, tran);
                                }

                                Console.WriteLine("===开始导入数据到数据库===");

                                //更新数据到分笔表
                                sql = string.Format(@"merge into t_shares_transactiondata as t
using (select * from {0}) as t1
ON t.Market = t1.Market and t.SharesCode = t1.SharesCode and t.Time=t1.Time and t.OrderIndex=t1.OrderIndex
when not matched by target
then insert(Market,SharesCode,Time,TimeStr,Price,Volume,Stock,Type,OrderIndex,LastModified,SharesInfo,SharesInfoNum,IsTimeFirst,IsTimeLast) values(t1.Market,t1.SharesCode,t1.Time,t1.TimeStr,t1.Price,t1.Volume,t1.Stock,t1.Type,t1.OrderIndex,t1.LastModified,t1.SharesInfo,t1.SharesInfoNum,t1.IsTimeFirst,t1.IsTimeLast);", Singleton.Instance.TempTableName);
                                cmd.CommandText = sql;
                                cmd.CommandTimeout = 600;
                                cmd.ExecuteNonQuery();

                                tran.Commit();
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteFileLog("更新分笔数据意外出错", ex);
                                tran.Rollback();
                                IsSuccess = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("更新分笔数据数据库连接出错", ex);
                    IsSuccess = false;
                }
                finally
                {
                    conn.Close();
                }
                return IsSuccess;
            }
        }
    }
}
