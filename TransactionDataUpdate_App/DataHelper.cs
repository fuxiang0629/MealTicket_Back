using FXCommon.Common;
using FXCommon.Database;
using MealTicket_DBCommon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeAPI;

namespace TransactionDataUpdate_App
{
    public class SharesInfo
    {
        public int Market { get; set; }

        public string SharesCode { get;set; }

        public int SharesInfoNum 
        {
            get 
            { 
                return int.Parse(SharesCode) * 10 + Market;
            } 
        }
    }
    public class LastData
    {
        public DateTime Time { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 股票数字
        /// </summary>
        public int SharesInfoNum { get; set; }
    }
    public class TRANSACTION_DATA_STRING
    {
        public StringBuilder sErrInfo { get; set; }
        public StringBuilder sResult { get; set; }
    }

    public class DataHelper
    {
        static TRANSACTION_DATA_STRING[] sm_td_string = new TRANSACTION_DATA_STRING[Singleton.Instance.hqClientCount];

        static DataHelper()
        {
            for (int idx = 0; idx < Singleton.Instance.hqClientCount; idx++)
            {
                sm_td_string[idx] = new TRANSACTION_DATA_STRING();
                sm_td_string[idx].sErrInfo = new StringBuilder(256);
                sm_td_string[idx].sResult = new StringBuilder(512 * Singleton.Instance.TransactionDataCount);
            }
        }

        /// <summary>
        /// 获取分笔成交数据
        /// </summary>
        public static List<SharesTransactionDataInfo> TdxHq_GetTransactionData()
        {
            using (var db = new meal_ticketEntities())
            {
                //查询数据库需要更新分笔的股票
                string sql = string.Format(@"select Market,SharesCode
  from t_shares_transactiondata_update with(nolock)
  where (LastModified>dateadd(SECOND,-{0},getdate()) and [Type]=0) or (LastModified>convert(varchar(10),getdate(),120) and [Type]=1)
  group by Market,SharesCode",Singleton.Instance.StopPeriodTime/1000);
                var list = db.Database.SqlQuery<SharesInfo>(sql).ToList();

                List<SharesTransactionDataInfo> resultList = new List<SharesTransactionDataInfo>();

                if (list.Count() > 0)
                {
                    //分批次更新
                    int totalCount = list.Count();
                    int onceCount = 32;
                    int batchCount = totalCount / onceCount;
                    if (totalCount % onceCount != 0)
                    {
                        batchCount = batchCount + 1;
                    }

                    for (int i = 0; i < batchCount; i++)
                    {
                        var tempList = list.Skip(i * onceCount).Take(onceCount).ToList();
                        resultList.AddRange(TdxHq_GetTransactionData(tempList));
                    }
                }
                return resultList;
            }
        }

        /// <summary>
        /// 获取分笔成交数据
        /// </summary>
        /// <returns></returns>
        public static List<SharesTransactionDataInfo> TdxHq_GetTransactionData(List<SharesInfo> sharesList)
        {
            List<SharesTransactionDataInfo> resultList = new List<SharesTransactionDataInfo>();

            ThreadMsgTemplate<SharesInfo> data = new ThreadMsgTemplate<SharesInfo>();
            data.Init();

            string SharesInfoNumArr = string.Empty;
            foreach (var item in sharesList)
            {
                data.AddMessage(item);
                SharesInfoNumArr = SharesInfoNumArr + item.SharesInfoNum + ",";
            }
            if (string.IsNullOrEmpty(SharesInfoNumArr))
            {
                return new List<SharesTransactionDataInfo>();
            }
            SharesInfoNumArr = SharesInfoNumArr.TrimEnd(',');

            List<LastData> lastList = new List<LastData>();
            #region=================查询股票最后index数据=======================
            using (var db = new meal_ticketEntities())
            {
                string sql = string.Format(@"select SharesInfoNum,[Time],OrderIndex
from
(
	select SharesInfoNum,[Time],OrderIndex,row_number() over(partition by SharesInfoNum order by [Time] desc,OrderIndex) num
	from t_shares_transactiondata with(nolock)
	where [Time]>convert(varchar(10),getdate(),120) and SharesInfoNum in ({0})
)t
where t.num=1", SharesInfoNumArr);
                lastList = db.Database.SqlQuery<LastData>(sql).ToList();
            }
            #endregion

            object resultLock = new object();
            Task[] tArr = new Task[Singleton.Instance.hqClientCount];
            for (int i = 0; i < Singleton.Instance.hqClientCount; i++)
            {
                tArr[i] = new Task((ref_string) =>
                {
                    int hqClient = Singleton.Instance.GetHqClient();
                    try
                    {
                        do
                        {
                            SharesInfo tempData = new SharesInfo();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }

                            bool isReconnectClient = false;

                            DateTime? lastTime = null;
                            int lastIndex = 1;
                            var lastData = lastList.Where(e => e.SharesInfoNum == tempData.SharesInfoNum).FirstOrDefault();
                            if (lastData != null)
                            {
                                lastTime = lastData.Time;
                                lastIndex = lastData.OrderIndex;
                            }
                            var tempList = TdxHq_GetTransactionData_byShares(hqClient, tempData.Market, tempData.SharesCode, lastTime, lastIndex,(TRANSACTION_DATA_STRING)ref_string, ref isReconnectClient);
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
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("更新分笔数据数据库连接出错", ex);
                    }
                    finally
                    {
                        Singleton.Instance.AddHqClient(hqClient);
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
        public static List<SharesTransactionDataInfo> TdxHq_GetTransactionData_byShares(int hqClient, int market, string sharesCode, DateTime? lastTime,int lastIndex, TRANSACTION_DATA_STRING result_info, ref bool isReconnectClient)
        {
            DateTime datePar = DateTime.Now.Date;
            List<SharesTransactionDataInfo> resultlist = new List<SharesTransactionDataInfo>();
            string date = datePar.ToString("yyyy-MM-dd");
            int totalCount = 0;

            short nStart = 0;
            short nCount = (short)Singleton.Instance.TransactionDataCount;
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

                    List<SharesTransactionDataInfo> tempResultlist = new List<SharesTransactionDataInfo>();
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
                            tempResultlist.Add(new SharesTransactionDataInfo
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
                    resultlist = tempResultlist.Concat(resultlist).ToList();
                    nStart = (short)(nStart + nCount);
                    totalCount += nCount;
                }
            } while (nCount >= Singleton.Instance.TransactionDataCount && iscontinue);

            foreach (var item in resultlist)
            {
                item.OrderIndex = lastIndex + totalCount+ item.OrderIndex;
            }
            return resultlist;
        }


        /// <summary>
        /// 数据更新到数据库
        /// </summary>
        /// <returns></returns>
        public static void UpdateToDataBase(List<SharesTransactionDataInfo> dataList)
        {
            //分批次更新
            int totalCount = dataList.Count();
            int onceCount = 5000;
            int batchCount = totalCount / onceCount;
            if (totalCount % onceCount != 0)
            {
                batchCount = batchCount + 1;
            }

            for (int i = 0; i < batchCount; i++)
            {
                var tempDataList=dataList.Skip(i * onceCount).Take(onceCount).ToList();

                DataTable table = new DataTable();
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
                foreach (var item in tempDataList)
                {
                    DataRow row = table.NewRow();
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
                    table.Rows.Add(row);
                }

                using (var db = new meal_ticketEntities())
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        //关键是类型
                        SqlParameter parameter = new SqlParameter("@sharesTransactiondata", SqlDbType.Structured);
                        //必须指定表类型名
                        parameter.TypeName = "dbo.SharesTransactiondata";
                        //赋值
                        parameter.Value = table;
                        db.Database.ExecuteSqlCommand("exec P_Shares_Transactiondata_Update @sharesTransactiondata", parameter);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                }
            }

            using (var db = new meal_ticketEntities())
            {
                string sql = "update t_shares_transactiondata_update set [Type]=0";
                db.Database.ExecuteSqlCommand(sql);
            }
        }


        /// <summary>
        /// 数据更新到数据库(通过temp表)
        /// </summary>
        /// <returns></returns>
        public static void UpdateToDataBaseByTemp(List<SharesTransactionDataInfo> dataList)
        {
            //分批次更新
            int totalCount = dataList.Count();
            int onceCount = 10000;
            int batchCount = totalCount / onceCount;
            if (totalCount % onceCount != 0)
            {
                batchCount = batchCount + 1;
            }

            for (int i = 0; i < batchCount; i++)
            {
                var tempDataList = dataList.Skip(i * onceCount).Take(onceCount).ToList();

                DataTable table = new DataTable();
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
                foreach (var item in tempDataList)
                {
                    DataRow row = table.NewRow();
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
                    table.Rows.Add(row);
                }

                var bulk = BulkFactory.CreateBulkCopy(DatabaseType.SqlServer);
                Dictionary<string, string> dic = new Dictionary<string, string>();
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
                bulk.ColumnMappings = dic;
                bulk.BatchSize = onceCount;

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    string sql = "truncate table t_shares_transactiondata_temp";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;   //sql语句
                        cmd.ExecuteNonQuery();
                    }
                    using (SqlTransaction tran = conn.BeginTransaction())//开启事务
                    {
                        try
                        {
                            bulk.BulkWriteToServer(conn, table, "t_shares_transactiondata_temp", tran);
                            sql = "exec P_Shares_Transactiondata_Update_Temp";
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = sql;   //sql语句
                                cmd.Transaction = tran;
                                cmd.ExecuteNonQuery();
                            }
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("更新K线数据出错", ex);
                            tran.Rollback();
                        }
                    }
                }
            }

            using (var db = new meal_ticketEntities())
            {
                string sql = "update t_shares_transactiondata_update set [Type]=0";
                db.Database.ExecuteSqlCommand(sql);
            }
        }
    }
}
