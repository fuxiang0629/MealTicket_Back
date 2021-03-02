using FXCommon.Common;
using FXCommon.Database;
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
using System.Transactions;
using TradeAPI;

namespace TransactionDataUpdate_NO3
{
    public class DataHelper
    {
        /// <summary>
        /// 获取分笔成交数据
        /// </summary>
        public static List<SharesTransactionDataInfo> TdxHq_GetTransactionData(int market, string sharesCode, DateTime datePar, int type, out DateTime? lastTime)
        {
            lastTime = null;
            int orderIndex = 0;
            if (type != 2)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString_data"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string dateStart = datePar.Date.ToString("yyyy-MM-dd");
                    string dateEnd = datePar.Date.AddDays(1).ToString("yyyy-MM-dd");
                    string sqlquery = string.Format("select top 1 [Time],OrderIndex from (select [Time],OrderIndex,ROW_NUMBER()OVER(order by OrderIndex desc) num from (select top 2 [Time],Max(OrderIndex) OrderIndex FROM t_shares_transactiondata with(nolock) where Market={0} and SharesCode='{1}' and [Time]>='{2}' and [Time]<'{3}' group by [Time] order by OrderIndex desc)m)t where t.num=2", market, sharesCode, dateStart, dateEnd);
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlquery;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            lastTime = (DateTime)reader["Time"];
                            orderIndex = (int)reader["OrderIndex"];
                        }
                        reader.Close();
                    }
                }
            }
            List<SharesTransactionDataInfo> resultlist = new List<SharesTransactionDataInfo>();
            string date = datePar.ToString("yyyy-MM-dd");
            int hqClient = Singleton.Instance.GetHqClient();
            int totalCount = 0;
            try
            {
                short nStart = 0;
                short nCount = (short)Singleton.Instance.TransactionDataCount;
                bool iscontinue = true;
                do
                {
                    StringBuilder sErrInfo = new StringBuilder(256);
                    StringBuilder sResult = new StringBuilder(1024 * 2048);
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
                        Singleton.Instance.AddRetryClient(hqClient);
                        hqClient = -1;
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
                            if (column.Length <5)
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

                            if (lastTime != null && lastTime.Value >= Time)
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
                } while (nCount >= Singleton.Instance.TransactionDataCount && iscontinue);

            }
            finally
            {
                if (hqClient != -1)
                {
                    //归还行情链接
                    Singleton.Instance.AddHqClient(hqClient);
                }
            }
            foreach (var item in resultlist)
            {
                item.OrderIndex = orderIndex + totalCount + 1 + item.OrderIndex;
            }
            return resultlist;
        }

        /// <summary>
        /// 更新分笔数据
        /// </summary>
        /// <param name=""></param>
        public static void UpdateTransactionData(List<SharesTransactionDataInfo> list,int market, string sharesCode,DateTime timePar,DateTime? lastTime)
        {
            DateTime timeNow = DateTime.Now;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString_data"].ConnectionString;
            //设置配置信息
            TransactionOptions options = new TransactionOptions();
            //options.Timeout = new TimeSpan(0, 0, 60); //设置超时时间；
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    try
                    {
                        if (lastTime == null)
                        {
                            lastTime = timePar.Date;
                        }
                        DateTime timeStart = timePar.Date;
                        DateTime timeEnd = timePar.Date.AddDays(1);

                        string sqlDel = string.Format("delete t_shares_transactiondata where Market={0} and SharesCode='{1}' and Time>='{2}' and Time<'{3}' and Time>'{4}'", market, sharesCode, timeStart, timeEnd, lastTime.Value);
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sqlDel;   //sql语句
                            cmd.ExecuteNonQuery();

                            int i = 0;
                            string sqlInsert = "";
                            foreach (var item in list)
                            {
                                i++;
                                sqlInsert = sqlInsert + string.Format("insert into t_shares_transactiondata(Market,SharesCode,[Time],TimeStr,Price,Volume,Stock,[Type],OrderIndex,LastModified,SharesInfo,SharesInfoNum) values({0},'{1}','{2}','{3}',{4},{5},{6},{7},{8},'{9}','{10}',{11}); ", item.Market, item.SharesCode, item.Time.ToString("yyyy-MM-dd HH:mm:ss"), item.TimeStr, item.Price, item.Volume, item.Stock, item.Type, item.OrderIndex, DateTime.Now, item.SharesCode+','+ item.Market,int.Parse(item.SharesCode)*10+item.Market);
                                if ((i % 100) == 0 || i == list.Count())
                                {
                                    cmd.CommandText = sqlInsert;   //sql语句
                                    cmd.ExecuteNonQuery();
                                    sqlInsert = "";
                                }
                            }
                        }
                        scope.Complete();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Logger.WriteFileLog("更新更新分笔成交数据出错", ex);
                    }
                    finally
                    {
                        conn.Close(); 
                        scope.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// 检查当天是否交易日
        /// </summary>
        /// <returns></returns>
        public static bool CheckTradeDate(DateTime? time = null)
        {
            DateTime timeDate = DateTime.Now.Date;
            if (time != null)
            {
                timeDate = time.Value.Date;
            }
            int timeDateInt = int.Parse(timeDate.ToString("yyyyMMdd"));
            using (var db = new meal_ticketEntities())
            {
                //排除周末
                var timeWeek = (from item in db.t_dim_time
                                where item.the_date == timeDateInt
                                select item).FirstOrDefault();
                if (timeWeek == null || timeWeek.week_day == 7 || timeWeek.week_day == 1)
                {
                    return false;
                }
                //排除节假日
                var tradeDate = (from item in db.t_shares_limit_date_group
                                 join item2 in db.t_shares_limit_date on item.Id equals item2.GroupId
                                 where item.Status == 1 && item2.Status == 1 && item2.BeginDate <= timeDate && item2.EndDate >= timeDate
                                 select item2).FirstOrDefault();
                if (tradeDate != null)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// 检查当前是否交易时间
        /// </summary>
        /// <returns></returns>
        public static bool CheckTradeTime(DateTime? time = null)
        {
            if (!CheckTradeDate())
            {
                return false;
            }
            DateTime timeDis = DateTime.Now;
            if (time != null)
            {
                timeDis = time.Value;
            }
            TimeSpan timeSpanNow = TimeSpan.Parse(timeDis.ToString("HH:mm:ss"));
            using (var db = new meal_ticketEntities())
            {
                var tradeTime = (from item in db.t_shares_limit_time
                                 select item).ToList();
                foreach (var item in tradeTime)
                {
                    //解析time1
                    if (item.Time1 != null)
                    {
                        string[] timeArr = item.Time1.Split(',');
                        foreach (var times in timeArr)
                        {
                            var timeSpanArr = times.Split('-');
                            if (timeSpanArr.Length != 2)
                            {
                                continue;
                            }
                            TimeSpan timeStart = TimeSpan.Parse(timeSpanArr[0]);
                            TimeSpan timeEnd = TimeSpan.Parse(timeSpanArr[1]);
                            if (timeSpanNow >= timeStart && timeSpanNow < timeEnd)
                            {
                                return true;
                            }
                        }
                    }
                    //解析time3
                    if (item.Time3 != null)
                    {
                        string[] timeArr = item.Time3.Split(',');
                        foreach (var times in timeArr)
                        {
                            var timeSpanArr = times.Split('-');
                            if (timeSpanArr.Length != 2)
                            {
                                continue;
                            }
                            TimeSpan timeStart = TimeSpan.Parse(timeSpanArr[0]);
                            TimeSpan timeEnd = TimeSpan.Parse(timeSpanArr[1]);
                            if (timeSpanNow >= timeStart && timeSpanNow < timeEnd)
                            {
                                return true;
                            }
                        }
                    }
                    //解析time4
                    if (item.Time4 != null)
                    {
                        string[] timeArr = item.Time4.Split(',');
                        foreach (var times in timeArr)
                        {
                            var timeSpanArr = times.Split('-');
                            if (timeSpanArr.Length != 2)
                            {
                                continue;
                            }
                            TimeSpan timeStart = TimeSpan.Parse(timeSpanArr[0]);
                            TimeSpan timeEnd = TimeSpan.Parse(timeSpanArr[1]);
                            if (timeSpanNow >= timeStart && timeSpanNow < timeEnd)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }
    }
}
