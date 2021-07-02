using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace stock_db_core
{
    public class DB_STOCK_INFO
    {
        /// <summary>
        /// 股票代码(【代码,市场】，其中市场:0深圳 1上海)
        /// </summary>
        public string strStockCode;

        /// <summary>
        /// 股票名称
        /// </summary>
        public string strStockName;

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public int iYesterdayClosingPrice;

        /// <summary>
        /// 平均振幅
        /// </summary>
        public int iAmplitudes;
    };

    public class DB_TIME_OF_USE_PRICE
    {
        /// <summary>
        /// 一分钟开盘价格
        /// </summary>
        public int iOpeningPrice;

        /// <summary>
        /// 一分钟收盘价格
        /// </summary>
        public int iLastestPrice;

        /// <summary>
        /// 一分钟总交易笔数
        /// </summary>
        public int iTradeAmount;

        /// <summary>
        /// 一分钟内交易金额
        /// </summary>
        public long lTradePrice;

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime dtTradeTime;

        /// <summary>
        /// 该分钟前全天均价
        /// </summary>
        public int iBalancePrice;

        /// <summary>
        /// 一分钟内最低价
        /// </summary>
        public int iLowestPrice;

        /// <summary>
        /// 一分钟内最高价
        /// </summary>
        public int iTopPrice;

        public int iBalancePriceOfDay;
    };

    public class DB_TRADE_PRICE_INFO
    {
        /// <summary>
        /// 上一个交易日收盘价格
        /// </summary>
        public int iPreDayTradePrice;

        /// <summary>
        /// 当前交易日开盘价格
        /// </summary>
        public int iBiddingTradePrice;

        /// <summary>
        /// 高点价格（最高价那分钟分时）
        /// </summary>
        public DB_TIME_OF_USE_PRICE topInfo;

        /// <summary>
        /// 低点价格（最低价那分钟分时） 
        /// </summary>
        public DB_TIME_OF_USE_PRICE lowestInfo;

        /// <summary>
        /// 开盘价格（第30分钟分时）
        /// </summary>
        public DB_TIME_OF_USE_PRICE openingInfo;

        /// <summary>
        /// 高点价格（每分钟最后一比）
        /// </summary>
        public DB_TIME_OF_USE_PRICE topInfo2;

        /// <summary>
        /// 低点价格（每分钟最后一比）
        /// </summary>
        public DB_TIME_OF_USE_PRICE lowestInfo2;

        /// <summary>
        /// 最新均价（指定分钟分时）
        /// </summary>
        public DB_TIME_OF_USE_PRICE lastestInfo;
    };

    /// <summary>
    /// 错误信息
    /// </summary>
    public enum DB_Error 
    {
        /// <summary>
        /// 成功
        /// </summary>
        SUCCESS=0,

        /// <summary>
        /// 数据库连接出错
        /// </summary>
        STRCONNERROR =1,

        /// <summary>
        /// 数据库操作出错
        /// </summary>
        SQLOPERERROR = 2,

        /// <summary>
        /// 股票代码参数错误
        /// </summary>
        STOCKPARERROR =3,

        /// <summary>
        /// 其他错误
        /// </summary>
        OTHERERROR=999,
    }

    public class DB_Model
    {
        /// <summary>
        /// 最大时间参数
        /// </summary>
        public const string MAX_DATE_TIME = "9999-12-31 23:59:59";

        public const string MIN_DATE_TIME = "1900-01-01 00:00:00";

        public const uint MAX_TIME_OF_USER_PRICE_CNT = (15 + 60 * 4 + 30);
    }

    class StockBase 
    {
        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }
    }

    public class HqAnalyse
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private string _strConnString1;

        /// <summary>
        /// 数据库连接字符串(分笔数据)
        /// </summary>
        private string _strConnString2;

        public HqAnalyse()
        {

        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="strConnString">连接字符串(两个链接字符串|隔开,后面一个是分笔数据库)</param>
        /// <returns></returns>
        public int Connect(string strConnString)
        {
            try
            {
                var tempStr = strConnString.Split('|');
                _strConnString1 = tempStr[0];
                _strConnString2 = tempStr[1];

                using (SqlConnection conn = new SqlConnection(_strConnString1))
                {
                    conn.Open();
                    conn.Close();
                }
                using (SqlConnection conn = new SqlConnection(_strConnString2))
                {
                    conn.Open();
                    conn.Close();
                }
            }
            catch (SqlException)
            {
                return (int)DB_Error.SQLOPERERROR;
            }
            catch (InvalidOperationException)
            {
                return (int)DB_Error.STRCONNERROR;
            }
            catch (Exception)
            {
                Disconnect();
                return (int)DB_Error.OTHERERROR;
            }
            return (int)DB_Error.SUCCESS;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            _strConnString1 = "";
            _strConnString2 = "";
        }

        /// <summary>
        /// 获取所有股票基础信息
        /// </summary>
        /// <param name="iCalcDays">连续交易日天数（0表示最近交易日）</param>
        /// <param name="ref_lstStockInfo">结果集</param>
        /// <returns>0：成功 1：失败</returns>
        public int GetStockInfo(int iCalcDays, ref List<DB_STOCK_INFO> ref_lstStockInfo)
        {
            ref_lstStockInfo = new List<DB_STOCK_INFO>();
            try
            {
                DateTime dateNow = DateTime.Now.Date;
                DateTime dateFirst = dateNow.AddDays(1);
                using (SqlConnection conn = new SqlConnection(_strConnString1))
                {
                    conn.Open();
                    //查询最前一个交易日日期
                    for (int i = 0; i <= iCalcDays;)
                    {
                        dateFirst = dateFirst.AddDays(-1);
                        if (CheckTradeDate(dateFirst))
                        {
                            i++;
                        }
                    }

                    try
                    {
                        string sqlQuery = string.Format("select SharesCode,Market,Max(ClosedPrice)ClosedPrice,AVG(case when ClosedPrice<=0 then 0 else convert(int,(MaxPrice-MinPrice)*1.0/ClosedPrice*10000) end)Amplitudes from t_shares_quotes_date with(nolock) where Date>='{0}' and Date<='{1}' group by SharesCode,Market", dateFirst.ToString("yyyy-MM-dd"), dateNow.ToString("yyyy-MM-dd"));
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sqlQuery;   //sql语句
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                DB_STOCK_INFO info = new DB_STOCK_INFO();
                                info.iAmplitudes = (int)reader["Amplitudes"];
                                info.iYesterdayClosingPrice = int.Parse(reader["ClosedPrice"].ToString());
                                info.strStockCode = reader["SharesCode"].ToString() + "," + reader["Market"].ToString();
                                info.strStockName = "";
                                ref_lstStockInfo.Add(info);
                            }
                            reader.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
                return (int)DB_Error.SUCCESS;
            }
            catch (SqlException)
            {
                return (int)DB_Error.SQLOPERERROR;
            }
            catch (InvalidOperationException)
            {
                return (int)DB_Error.STRCONNERROR;
            }
            catch (Exception)
            {
                ref_lstStockInfo = new List<DB_STOCK_INFO>();
                return (int)DB_Error.OTHERERROR;
            }
        }

        /// <summary>
        /// 获取指定股票分笔价格信息
        /// </summary>
        /// <param name="strStockCode">股票代码(【代码,市场】，其中市场0：深圳 1：上海)</param>
        /// <param name="dtTime">时间</param>
        /// <param name="bPriceOrAmount">排序(1价格排序 2成交量排序)</param>
        /// <param name="ref_priceInfo">结果集1</param>
        /// <param name="ref_lstTimeOfUsePrice">结果集2</param>
        /// <returns></returns>
        public int GetTradePriceInfo(string strStockCode, DateTime dtTime, bool bPriceOrAmount, ref DB_TRADE_PRICE_INFO ref_priceInfo, ref List<DB_TIME_OF_USE_PRICE> ref_lstTimeOfUsePrice)
        {
            int iPriceOrAmount = bPriceOrAmount ? 1 : 2;
            ref_priceInfo = new DB_TRADE_PRICE_INFO();
            ref_lstTimeOfUsePrice = new List<DB_TIME_OF_USE_PRICE>();

            var tempCode = strStockCode.Split(',');
            if (tempCode.Count() != 2)
            {
                return (int)DB_Error.STOCKPARERROR;
            }
            int market;
            if (!int.TryParse(tempCode[1], out market))
            {
                return (int)DB_Error.STOCKPARERROR;
            }
            string sharesCode = tempCode[0];

            if (dtTime.ToString("yyyy-MM-dd HH:mm:ss") == DB_Model.MAX_DATE_TIME)
            {
                dtTime = DateTime.Now;
            }

            try
            {
                //判断传入时间是否交易日期
                while (true)
                {
                    if (CheckTradeDate(dtTime))
                    {
                        break;
                    }
                    dtTime = dtTime.AddDays(-1);
                }

                if (dtTime > DateTime.Parse(dtTime.ToString("yyyy-MM-dd 15:00:00")))
                {
                    dtTime = DateTime.Parse(dtTime.ToString("yyyy-MM-dd 15:00:00"));
                }

                var list = GetTradeDateMin(market, sharesCode, DateTime.Parse(dtTime.ToString("yyyy-MM-dd HH:mm:00")));

                //查询股票当天开盘价和收盘价
                GetStockPrice(market, sharesCode, dtTime, ref ref_priceInfo.iPreDayTradePrice, ref ref_priceInfo.iBiddingTradePrice);

                //查询第30分钟分笔数据
                ref_priceInfo.openingInfo = list.Where(e => e.dtTradeTime == DateTime.Parse(dtTime.ToString("yyyy-MM-dd 09:30:00"))).FirstOrDefault();
                //查询指定分钟分笔数据
                ref_priceInfo.lastestInfo = list.Where(e => e.dtTradeTime == DateTime.Parse(dtTime.ToString("yyyy-MM-dd HH:mm:00"))).FirstOrDefault();
                //查询最高价格分钟分笔数据
                //1.查询最高价格分钟数
                //2.查询分笔数据
                DateTime? MaxMinTime = GetStockMinTime(1, iPriceOrAmount, market, sharesCode, dtTime);
                if (MaxMinTime == null)
                {
                    ref_priceInfo.topInfo2 = new DB_TIME_OF_USE_PRICE();
                }
                else
                {
                    ref_priceInfo.topInfo2 = list.Where(e => e.dtTradeTime == MaxMinTime.Value).FirstOrDefault();
                }
                //查询最低价格分钟分笔数据
                //1.查询最低价格分钟数
                //2.查询分笔数据
                DateTime? MinMinTime = GetStockMinTime(2, iPriceOrAmount, market, sharesCode, dtTime);
                if (MinMinTime == null)
                {
                    ref_priceInfo.lowestInfo2 = new DB_TIME_OF_USE_PRICE();
                }
                else
                {
                    ref_priceInfo.lowestInfo2 = list.Where(e => e.dtTradeTime == MinMinTime.Value).FirstOrDefault();
                }

                //查询所有分笔数据
                ref_lstTimeOfUsePrice = list.Where(e => e.dtTradeTime <= dtTime).ToList();
                return (int)DB_Error.SUCCESS;
            }
            catch (SqlException)
            {
                return (int)DB_Error.SQLOPERERROR;
            }
            catch (InvalidOperationException)
            {
                return (int)DB_Error.STRCONNERROR;
            }
            catch (Exception ex)
            {
                ref_priceInfo = new DB_TRADE_PRICE_INFO();
                ref_lstTimeOfUsePrice = new List<DB_TIME_OF_USE_PRICE>();
                return (int)DB_Error.OTHERERROR;
            }
        }

        /// <summary>
        /// 判断是否交易日
        /// </summary>
        /// <param name="time">日期，null表示当天</param>
        /// <returns></returns>
        private bool CheckTradeDate(DateTime? date)
        {
            DateTime timeDate = DateTime.Now.Date;
            if (date != null)
            {
                timeDate = date.Value.Date;
            }
            int timeDateInt = int.Parse(timeDate.ToString("yyyyMMdd"));

            using (SqlConnection conn = new SqlConnection(_strConnString1))
            {
                conn.Open();

                try
                {
                    string sqlQuery1 = string.Format("select top 1 week_day from t_dim_time with(nolock) where the_date={0}", timeDateInt);
                    string sqlQuery2 = string.Format("select top 1 t.Id from t_shares_limit_date_group t with(nolock) inner join t_shares_limit_date t1 with(nolock) on t.Id=t1.GroupId where t.Status=1 and t1.Status=1 and t1.BeginDate<='{0}' and t1.EndDate>='{0}'", timeDate.ToString("yyyy-MM-dd"));
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery1;   //sql语句
                        var week_day = cmd.ExecuteScalar();
                        if (week_day == null || (int)week_day == 7 || (int)week_day == 1)
                        {
                            return false;
                        }
                        cmd.CommandText = sqlQuery2;   //sql语句
                        var tradeDate = cmd.ExecuteScalar();
                        if (tradeDate != null)
                        {
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
            return true;
        }

        /// <summary>
        /// 获取股票指定分钟数分笔数据
        /// </summary>
        /// <param name="market">市场0深圳 1上海</param>
        /// <param name="sharesCode">股票代码</param>
        /// <param name="dataTime">时间</param>
        /// <returns></returns>
        private List<DB_TIME_OF_USE_PRICE> GetTradeDateMin(int market, string sharesCode, DateTime dataTime)
        {
            DateTime dateNow = DateTime.Now.Date;
            string tableName = "t_shares_transactiondata";
            if (dataTime < dateNow)
            {
                tableName = "t_shares_transactiondata_history";
            }
            List<DB_TIME_OF_USE_PRICE> list = new List<DB_TIME_OF_USE_PRICE>();
            using (SqlConnection conn = new SqlConnection(_strConnString2))
            {
                conn.Open();

                try
                {
                    string sqlQuery1 = string.Format("select SharesCode,Market,[Time],sum(Stock)iTradeStock,sum(case when num1=1 then Price else null end) iOpeningPrice,sum(case when num2=1 then Price else null end) iLastestPrice, sum(Stock * Price)lTradeAmount,Min(Price) minPrice,Max(Price) maxPrice from(select SharesCode, Market,[Time], Stock, Price,ROW_NUMBER()OVER(partition by SharesCode, Market,[Time] order by OrderIndex) num1,ROW_NUMBER()OVER(partition by SharesCode, Market,[Time] order by OrderIndex desc) num2 from {4} t with(nolock) where SharesCode = '{0}' and Market = {1} and [Time] > '{2}' and [Time] <= '{3}')t group by SharesCode, Market,[Time] order by [Time]", sharesCode, market, dataTime.ToString("yyyy-MM-dd"), dataTime.ToString("yyyy-MM-dd HH:mm:00"), tableName);
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery1;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            DB_TIME_OF_USE_PRICE result=new DB_TIME_OF_USE_PRICE();
                            result.dtTradeTime = DateTime.Parse(reader["Time"].ToString());
                            result.iTradeAmount = int.Parse(reader["iTradeStock"].ToString());
                            result.iOpeningPrice = int.Parse(reader["iOpeningPrice"].ToString());
                            result.iLastestPrice = int.Parse(reader["iLastestPrice"].ToString());
                            result.lTradePrice = long.Parse(reader["lTradeAmount"].ToString());
                            result.iLowestPrice = int.Parse(reader["minPrice"].ToString());
                            result.iTopPrice = int.Parse(reader["maxPrice"].ToString());
                            list.Add(result);
                        }
                        reader.Close();
                    }

                    int totalStock = 0;
                    long totalAmount = 0;
                    foreach (var item in list)
                    {
                        totalAmount += item.lTradePrice;
                        totalStock += item.iTradeAmount;
                        item.iBalancePrice = (totalStock == 0 ? 0 : (int)(totalAmount * 1.0 / totalStock));
                    }

                    var tempTime = DateTime.Parse(dataTime.ToString("yyyy-MM-dd HH:mm:00"));
                    var startTime = DateTime.Parse(dataTime.ToString("yyyy-MM-dd 09:25:00"));

                    List<DB_TIME_OF_USE_PRICE> resultList = new List<DB_TIME_OF_USE_PRICE>();
                    DB_TIME_OF_USE_PRICE lastest = null;
                    while (startTime <= tempTime)
                    {
                        if (startTime == DateTime.Parse(dataTime.ToString("yyyy-MM-dd 09:26:00")))
                        {
                            startTime = DateTime.Parse(dataTime.ToString("yyyy-MM-dd 09:30:00"));
                        }
                        if (startTime == DateTime.Parse(dataTime.ToString("yyyy-MM-dd 11:31:00")))
                        {
                            startTime = DateTime.Parse(dataTime.ToString("yyyy-MM-dd 13:00:00"));
                        }
                        var sourceItem = list.Where(e => e.dtTradeTime == startTime).FirstOrDefault();
                        if (sourceItem != null)
                        {
                            resultList.Add(sourceItem);
                            lastest = sourceItem;
                        }
                        else
                        {
                            resultList.Add(new DB_TIME_OF_USE_PRICE 
                            {
                                iTradeAmount=0,
                                dtTradeTime= startTime,
                                iBalancePrice= lastest ==null?0: lastest.iBalancePrice,
                                iLastestPrice = lastest == null ? 0 : lastest.iLastestPrice,
                                iOpeningPrice = lastest == null ? 0 : lastest.iOpeningPrice,
                                lTradePrice =0,
                                iLowestPrice=0,
                                iTopPrice=0
                            });
                        }

                        startTime = startTime.AddMinutes(1);
                    }

                    return resultList;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally 
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 查询股票当天价格信息
        /// </summary>
        /// <param name="market">市场0深圳 1上海</param>
        /// <param name="sharesCode">股票代码</param>
        /// <param name="dataTime">时间</param>
        private void GetStockPrice(int market, string sharesCode, DateTime dataTime,ref int iPreDayTradePrice,ref int iOpeningTradePrice)
        {
            iPreDayTradePrice = 0;
            iOpeningTradePrice = 0;
            using (SqlConnection conn = new SqlConnection(_strConnString1))
            {
                conn.Open();
                try
                {
                    //查询股票当天开盘价和收盘价
                    string sqlQuery1 = string.Format("select top 1 ClosedPrice,OpenedPrice from t_shares_quotes_date with(nolock) where Market={0} and SharesCode='{1}' and Date='{2}'", market, sharesCode, dataTime.ToString("yyyy-MM-dd"));
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery1;   //sql语句
                        SqlDataReader reader1 = cmd.ExecuteReader();
                        if (reader1.Read())
                        {
                            iPreDayTradePrice = int.Parse(reader1["ClosedPrice"].ToString());
                            iOpeningTradePrice = int.Parse(reader1["OpenedPrice"].ToString());
                        }
                        reader1.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 获取股票最高价/最低价分钟时间
        /// </summary>
        /// <param name="type">1.最高价 2.最低价</param>
        /// <param name="iPriceOrAmount">排序(1价格排序 2成交量排序)</param>
        /// <param name="market">市场0深圳 1上海</param>
        /// <param name="sharesCode">股票代码</param>
        /// <param name="dataTime">时间</param>
        /// <returns></returns>
        private DateTime? GetStockMinTime(int type,int iPriceOrAmount, int market, string sharesCode, DateTime dataTime)
        {
            DateTime dateNow = DateTime.Now.Date;
            string tableName = "t_shares_transactiondata";
            if (dataTime < dateNow)
            {
                tableName = "t_shares_transactiondata_history";
            }
            using (SqlConnection conn = new SqlConnection(_strConnString2))
            {
                conn.Open();
                try
                {
                    string sqlQuery = "";
                    if (type == 1 && iPriceOrAmount == 1)
                    {
                        sqlQuery = string.Format("select top 1 [Time] from(select ROW_NUMBER()OVER(order by Price desc) num,[Time] from {4} with(nolock) where SharesCode = '{0}' and Market = {1} and [Time] <= '{2}' and [Time]>'{3}')t where t.num = 1", sharesCode, market, dataTime.ToString("yyyy-MM-dd HH:mm:00"), dataTime.ToString("yyyy-MM-dd"), tableName);
                    }
                    else if (type == 1 && iPriceOrAmount == 2)
                    {
                        sqlQuery = string.Format("select top 1 [Time] from(select ROW_NUMBER()OVER(order by Stock desc) num,[Time] from {4} with(nolock) where SharesCode = '{0}' and Market = {1} and [Time] <= '{2}' and [Time]>'{3}')t where t.num = 1", sharesCode, market, dataTime.ToString("yyyy-MM-dd HH:mm:00"), dataTime.ToString("yyyy-MM-dd"), tableName);
                    }
                    else if (type == 2 && iPriceOrAmount == 1)
                    {
                        sqlQuery = string.Format("select top 1 [Time] from(select ROW_NUMBER()OVER(order by Price) num,[Time] from {4} with(nolock) where SharesCode = '{0}' and Market = {1} and [Time] <= '{2}' and [Time]>'{3}')t where t.num = 1", sharesCode, market, dataTime.ToString("yyyy-MM-dd HH:mm:00"), dataTime.ToString("yyyy-MM-dd"), tableName);
                    }
                    else if (type == 2 && iPriceOrAmount == 2)
                    {
                        sqlQuery = string.Format("select top 1 [Time] from(select ROW_NUMBER()OVER(order by Stock) num,[Time] from {4} with(nolock) where SharesCode = '{0}' and Market = {1} and [Time] <= '{2}' and [Time]>'{3}')t where t.num = 1", sharesCode, market, dataTime.ToString("yyyy-MM-dd HH:mm:00"), dataTime.ToString("yyyy-MM-dd"), tableName);
                    }
                    else
                    {
                        return null;
                    }
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery;   //sql语句
                        var time = cmd.ExecuteScalar();
                        if (time == null)
                        {
                            return null;
                        }
                        return (DateTime)time;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 批量获取指定股票分笔价格信息
        /// </summary>
        /// <param name="strStockCode"></param>
        /// <param name="dtTime"></param>
        /// <param name="bPriceOrAmount"></param>
        /// <param name="ref_priceInfoDic"></param>
        /// <param name="ref_lstTimeOfUsePriceDic"></param>
        /// <returns></returns>
        public int GetTradePriceInfoBatch(List<string> strStockCodeList,DateTime bgTime, DateTime dtTime, ref Dictionary<string, DB_TRADE_PRICE_INFO> ref_priceInfoDic, ref Dictionary<string, List<DB_TIME_OF_USE_PRICE>> ref_lstTimeOfUsePriceDic, ref Dictionary<string, int> error)
        {
            ref_lstTimeOfUsePriceDic = new Dictionary<string, List<DB_TIME_OF_USE_PRICE>>();
               strStockCodeList = strStockCodeList.Distinct().ToList();

            error = new Dictionary<string, int>();
            List<StockBase> stockList = new List<StockBase>();
            foreach (var strStockCode in strStockCodeList)
            {
                var tempCode = strStockCode.Split(',');
                if (tempCode.Count() != 2)
                {
                    error.Add(strStockCode, (int)DB_Error.STOCKPARERROR);
                    continue;
                }
                int market;
                if (!int.TryParse(tempCode[1], out market))
                {
                    error.Add(strStockCode, (int)DB_Error.STOCKPARERROR);
                    continue;
                }
                string sharesCode = tempCode[0];

                //判断股票是否存在
                stockList.Add(new StockBase
                {
                    SharesCode = sharesCode,
                    Market = market
                });
            }

            if (bgTime.ToString("yyyy-MM-dd HH:mm:ss") == DB_Model.MIN_DATE_TIME)
            {
                bgTime = DateTime.Parse(dtTime.ToString("yyyy-MM-dd 09:25:00"));
            }

            if (dtTime.ToString("yyyy-MM-dd HH:mm:ss") == DB_Model.MAX_DATE_TIME)
            {
                dtTime = DateTime.Now;
            }

            try
            {
                //判断传入时间是否交易日期
                while (true)
                {
                    if (CheckTradeDate(dtTime))
                    {
                        break;
                    }
                    dtTime = dtTime.AddDays(-1);
                }

                if (dtTime > DateTime.Parse(dtTime.ToString("yyyy-MM-dd 15:00:00")))
                {
                    dtTime = DateTime.Parse(dtTime.ToString("yyyy-MM-dd 15:00:00"));
                }

                //查询股票当天开盘价和收盘价
                var priceDic = GetStockPriceBatch(stockList, dtTime);

                var TradeDateMinDic = GetTradeDateMinBatch(stockList, DateTime.Parse(bgTime.ToString("yyyy-MM-dd HH:mm:00")), DateTime.Parse(dtTime.ToString("yyyy-MM-dd HH:mm:00")));

                foreach (var item in priceDic)
                {
                    item.Value.openingInfo = TradeDateMinDic[item.Key].Where(e => e.dtTradeTime == DateTime.Parse(dtTime.ToString("yyyy-MM-dd 09:25:00"))).FirstOrDefault();
                    item.Value.lastestInfo = TradeDateMinDic[item.Key].OrderByDescending(e=>e.dtTradeTime).FirstOrDefault();
                    item.Value.topInfo = TradeDateMinDic[item.Key].OrderByDescending(e=>e.iTopPrice).ThenBy(e=>e.dtTradeTime).FirstOrDefault();
                    item.Value.lowestInfo = TradeDateMinDic[item.Key].OrderBy(e => e.iLowestPrice).ThenBy(e => e.dtTradeTime).FirstOrDefault();
                    item.Value.topInfo2 = TradeDateMinDic[item.Key].OrderByDescending(e=>e.iLastestPrice).ThenBy(e => e.dtTradeTime).FirstOrDefault();
                    item.Value.lowestInfo2 = TradeDateMinDic[item.Key].OrderBy(e => e.iLastestPrice).ThenBy(e => e.dtTradeTime).FirstOrDefault();
                }

                //查询指定分钟分笔数据
                ref_lstTimeOfUsePriceDic = TradeDateMinDic;
                //查询所有分笔数据
                ref_priceInfoDic = priceDic;
                return (int)DB_Error.SUCCESS;
            }
            catch (SqlException ex)
            {
                ref_lstTimeOfUsePriceDic = new Dictionary<string, List<DB_TIME_OF_USE_PRICE>>();
                ref_priceInfoDic = new Dictionary<string, DB_TRADE_PRICE_INFO>();
                return (int)DB_Error.SQLOPERERROR;
            }
            catch (InvalidOperationException)
            {
                ref_lstTimeOfUsePriceDic = new Dictionary<string, List<DB_TIME_OF_USE_PRICE>>();
                ref_priceInfoDic = new Dictionary<string, DB_TRADE_PRICE_INFO>();
                return (int)DB_Error.STRCONNERROR;
            }
            catch (Exception ex)
            {
                ref_lstTimeOfUsePriceDic = new Dictionary<string, List<DB_TIME_OF_USE_PRICE>>();
                ref_priceInfoDic = new Dictionary<string, DB_TRADE_PRICE_INFO>();
                return (int)DB_Error.OTHERERROR;
            }
        }

        /// <summary>
        /// 批量获取股票指定分钟数分笔数据（增量）
        /// </summary>
        /// <param name="market">市场0深圳 1上海</param>
        /// <param name="sharesCode">股票代码</param>
        /// <param name="dataTime">时间</param>
        /// <returns></returns>
        private Dictionary<string, List<DB_TIME_OF_USE_PRICE>> GetTradeDateMinBatchBack(List<StockBase> stockList, DateTime bgTime, DateTime dataTime)
        {
            DateTime dateNow = DateTime.Now.Date;
            Dictionary<string, List<DB_TIME_OF_USE_PRICE>> resultDic = new Dictionary<string, List<DB_TIME_OF_USE_PRICE>>();
            if (stockList.Count() <= 0)
            {
                return resultDic;
            }
            StringBuilder strStockCode = new StringBuilder();
            strStockCode.Append("(");
            for (int i = 0; i < stockList.Count(); i++)
            {
                resultDic.Add(stockList[i].SharesCode + "," + stockList[i].Market, new List<DB_TIME_OF_USE_PRICE>());
                strStockCode.Append(int.Parse(stockList[i].SharesCode)*10 + stockList[i].Market);
                if (i < stockList.Count() - 1)
                {
                    strStockCode.Append(",");
                }
            }
            strStockCode.Append(")");

            string sqlQuery = string.Format(@"select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,iMinPrice minPrice,iMaxPrice maxPrice
from t_shares_transactiondata_time_analyse with(nolock)
where SharesInfoNum in {0}
            and [Time] >= '{1}' and [Time] <= '{2}' and [Date] = '{3}'", strStockCode, bgTime.ToString("yyyy-MM-dd HH:mm:00"), dataTime.ToString("yyyy-MM-dd HH:mm:00"), dataTime.ToString("yyyy-MM-dd"));

            using (SqlConnection conn = new SqlConnection(_strConnString2))
            {
                conn.Open();

                Dictionary<string, List<DB_TIME_OF_USE_PRICE>> dic = new Dictionary<string, List<DB_TIME_OF_USE_PRICE>>();
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            int market = int.Parse(reader["Market"].ToString());
                            string sharesCode = reader["SharesCode"].ToString();
                            DB_TIME_OF_USE_PRICE result = new DB_TIME_OF_USE_PRICE();
                            result.dtTradeTime = DateTime.Parse(reader["Time"].ToString());
                            result.iTradeAmount = int.Parse(reader["iTradeStock"].ToString());
                            result.iOpeningPrice = int.Parse(reader["iOpeningPrice"].ToString());
                            result.iLastestPrice = int.Parse(reader["iLastestPrice"].ToString());
                            result.lTradePrice = long.Parse(reader["lTradeAmount"].ToString());
                            result.iLowestPrice = int.Parse(reader["minPrice"].ToString());
                            result.iTopPrice = int.Parse(reader["maxPrice"].ToString());
                            if (dic.ContainsKey(sharesCode + "," + market))
                            {
                                dic[sharesCode + "," + market].Add(result);
                            }
                            else
                            {
                                dic.Add(sharesCode + "," + market, new List<DB_TIME_OF_USE_PRICE> { result });
                            }
                        }
                        reader.Close();
                    }

                    int totalStock = 0;
                    long totalAmount = 0;
                    foreach (var item in dic)
                    {
                        foreach (var item2 in item.Value)
                        {
                            totalAmount += item2.lTradePrice;
                            totalStock += item2.iTradeAmount;
                            item2.iBalancePrice = (totalStock == 0 ? 0 : (int)(totalAmount * 1.0 / totalStock));
                        }
                    }
                    foreach (var result in resultDic)
                    {
                        var tempTime = DateTime.Parse(dataTime.ToString("yyyy-MM-dd HH:mm:00"));
                        var startTime = DateTime.Parse(bgTime.ToString("yyyy-MM-dd HH:mm:00"));
                        DB_TIME_OF_USE_PRICE lastest = null;
                        while (startTime <= tempTime)
                        {
                            if (startTime == DateTime.Parse(dataTime.ToString("yyyy-MM-dd 09:26:00")))
                            {
                                startTime = DateTime.Parse(dataTime.ToString("yyyy-MM-dd 09:30:00"));
                                continue;
                            }
                            if (startTime == DateTime.Parse(dataTime.ToString("yyyy-MM-dd 11:31:00")))
                            {
                                startTime = DateTime.Parse(dataTime.ToString("yyyy-MM-dd 13:00:00"));
                                continue;
                            }
                            if (dic.ContainsKey(result.Key))
                            {
                                var sourceItem = dic[result.Key].Where(e => e.dtTradeTime == startTime).FirstOrDefault();
                                if (sourceItem != null)
                                {
                                    result.Value.Add(sourceItem);
                                    lastest = sourceItem;
                                }
                                else
                                {
                                    result.Value.Add(new DB_TIME_OF_USE_PRICE
                                    {
                                        iTradeAmount = 0,
                                        dtTradeTime = startTime,
                                        iBalancePrice = lastest == null ? 0 : lastest.iBalancePrice,
                                        iLastestPrice = lastest == null ? 0 : lastest.iLastestPrice,
                                        iOpeningPrice = lastest == null ? 0 : lastest.iOpeningPrice,
                                        lTradePrice = 0,
                                        iLowestPrice=0,
                                        iTopPrice=0
                                    });
                                }
                            }
                            else
                            {
                                result.Value.Add(new DB_TIME_OF_USE_PRICE
                                {
                                    iTradeAmount = 0,
                                    dtTradeTime = startTime,
                                    iBalancePrice = 0,
                                    iLastestPrice = 0,
                                    iOpeningPrice = 0,
                                    lTradePrice = 0,
                                    iLowestPrice = 0,
                                    iTopPrice = 0
                                });
                            }
                            startTime = startTime.AddMinutes(1);
                        }
                    }
                    return resultDic;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 批量获取股票指定分钟数分笔数据（增量）
        /// </summary>
        /// <param name="market">市场0深圳 1上海</param>
        /// <param name="sharesCode">股票代码</param>
        /// <param name="dataTime">时间</param>
        /// <returns></returns>
        private Dictionary<string, List<DB_TIME_OF_USE_PRICE>> GetTradeDateMinBatch(List<StockBase> stockList, DateTime bgTime, DateTime dataTime)
        {
            DateTime dateNow = DateTime.Now.Date;
            Dictionary<string, List<DB_TIME_OF_USE_PRICE>> resultDic = new Dictionary<string, List<DB_TIME_OF_USE_PRICE>>();
            if (stockList.Count() <= 0)
            {
                return resultDic;
            }
            StringBuilder strStockCode = new StringBuilder();
            strStockCode.Append("(");
            for (int i = 0; i < stockList.Count(); i++)
            {
                resultDic.Add(stockList[i].SharesCode + "," + stockList[i].Market, new List<DB_TIME_OF_USE_PRICE>());
                strStockCode.Append(int.Parse(stockList[i].SharesCode) * 10 + stockList[i].Market);
                if (i < stockList.Count() - 1)
                {
                    strStockCode.Append(",");
                }
            }
            strStockCode.Append(")");

            string sqlQuery = string.Format(@"select SharesCode,Market,[Time],iTradeStock,iOpeningPrice,iLastestPrice,lTradeAmount,iMinPrice,iMaxPrice
from t_shares_transactiondata_time_analyse with(nolock)
where SharesInfoNum in {0} and Time <= '{1}' and Time>='{2}'
order by SharesInfoNum,[Time]", strStockCode, dataTime.ToString("yyyy-MM-dd HH:mm:00"), bgTime.ToString("yyyy-MM-dd HH:mm:00"));

            using (SqlConnection conn = new SqlConnection(_strConnString2))
            {
                conn.Open();

                Dictionary<string, List<DB_TIME_OF_USE_PRICE>> dic = new Dictionary<string, List<DB_TIME_OF_USE_PRICE>>();
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            int market = int.Parse(reader["Market"].ToString());
                            string sharesCode = reader["SharesCode"].ToString();
                            DB_TIME_OF_USE_PRICE result = new DB_TIME_OF_USE_PRICE();
                            result.dtTradeTime = DateTime.Parse(reader["Time"].ToString());
                            result.iTradeAmount = int.Parse(reader["iTradeStock"].ToString());
                            result.iOpeningPrice = int.Parse(reader["iOpeningPrice"].ToString());
                            result.iLastestPrice = int.Parse(reader["iLastestPrice"].ToString());
                            result.lTradePrice = long.Parse(reader["lTradeAmount"].ToString());
                            result.iLowestPrice = int.Parse(reader["iMinPrice"].ToString());
                            result.iTopPrice = int.Parse(reader["iMaxPrice"].ToString());
                            result.iBalancePrice = result.iTradeAmount == 0 ? 0 : (int)(result.lTradePrice / result.iTradeAmount);
                            if (dic.ContainsKey(sharesCode + "," + market))
                            {
                                dic[sharesCode + "," + market].Add(result);
                            }
                            else
                            {
                                dic.Add(sharesCode + "," + market, new List<DB_TIME_OF_USE_PRICE> { result });
                            }
                        }
                        reader.Close();
                    }
                    foreach (var result in resultDic)
                    {
                        var tempTime = DateTime.Parse(dataTime.ToString("yyyy-MM-dd HH:mm:00"));
                        var startTime = DateTime.Parse(bgTime.ToString("yyyy-MM-dd HH:mm:00"));
                        DB_TIME_OF_USE_PRICE lastest = null;
                        while (startTime <= tempTime)
                        {
                            if (startTime == DateTime.Parse(dataTime.ToString("yyyy-MM-dd 09:26:00")))
                            {
                                startTime = DateTime.Parse(dataTime.ToString("yyyy-MM-dd 09:30:00"));
                                continue;
                            }
                            if (startTime == DateTime.Parse(dataTime.ToString("yyyy-MM-dd 11:31:00")))
                            {
                                startTime = DateTime.Parse(dataTime.ToString("yyyy-MM-dd 13:00:00"));
                                continue;
                            }
                            if (dic.ContainsKey(result.Key))
                            {
                                var sourceItem = dic[result.Key].Where(e => e.dtTradeTime == startTime).FirstOrDefault();
                                if (sourceItem != null)
                                {
                                    result.Value.Add(sourceItem);
                                    lastest = sourceItem;
                                }
                                else
                                {
                                    result.Value.Add(new DB_TIME_OF_USE_PRICE
                                    {
                                        iTradeAmount = 0,
                                        dtTradeTime = startTime,
                                        iBalancePrice = lastest == null ? 0 : lastest.iBalancePrice,
                                        iLastestPrice = lastest == null ? 0 : lastest.iLastestPrice,
                                        iOpeningPrice = lastest == null ? 0 : lastest.iLastestPrice,
                                        lTradePrice = 0,
                                        iLowestPrice = lastest == null ? 0 : lastest.iLastestPrice,
                                        iTopPrice = lastest == null ? 0 : lastest.iLastestPrice,
                                    });
                                }
                            }
                            else
                            {
                                result.Value.Add(new DB_TIME_OF_USE_PRICE
                                {
                                    iTradeAmount = 0,
                                    dtTradeTime = startTime,
                                    iBalancePrice = 0,
                                    iLastestPrice = 0,
                                    iOpeningPrice = 0,
                                    lTradePrice = 0,
                                    iLowestPrice = 0,
                                    iTopPrice = 0
                                });
                            }
                            startTime = startTime.AddMinutes(1);
                        }
                    }
                    return resultDic;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 批量获取股票指定分钟数分笔数据（全局）
        /// </summary>
        /// <param name="market">市场0深圳 1上海</param>
        /// <param name="sharesCode">股票代码</param>
        /// <param name="dataTime">时间</param>
        /// <returns></returns>
        private Dictionary<string, List<DB_TIME_OF_USE_PRICE>> GetTradeDateMinBatch_Day(List<StockBase> stockList, DateTime dataTime,int type)
        {
            DateTime dateNow = DateTime.Now.Date;
            Dictionary<string, List<DB_TIME_OF_USE_PRICE>> resultDic = new Dictionary<string, List<DB_TIME_OF_USE_PRICE>>();
            if (stockList.Count() <= 0)
            {
                return resultDic;
            }
            StringBuilder strStockCode = new StringBuilder();
            strStockCode.Append("(");
            for (int i = 0; i < stockList.Count(); i++)
            {
                resultDic.Add(stockList[i].SharesCode + "," + stockList[i].Market, new List<DB_TIME_OF_USE_PRICE>());
                strStockCode.Append(int.Parse(stockList[i].SharesCode) * 10 + stockList[i].Market);
                if (i < stockList.Count() - 1)
                {
                    strStockCode.Append(",");
                }
            }
            strStockCode.Append(")");

            string sqlQuery = string.Empty;
            if (type == 1)//30分钟的
            {
                sqlQuery = string.Format(@"select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,iMinPrice minPrice,iMaxPrice maxPrice
from t_shares_transactiondata_time_analyse with(nolock)
where SharesInfoNum in {0}
            and [Time] >= '{1}' and [Time] <= '{1}' and [Date] = '{2}'", strStockCode, dataTime.ToString("yyyy-MM-dd 09:30:00"), dataTime.ToString("yyyy-MM-dd"));
            }
            if (type == 2)//当前分钟的
            {
                sqlQuery = string.Format(@"select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,minPrice,maxPrice
from
(
	select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,iMinPrice minPrice,iMaxPrice maxPrice,
	row_number()over(partition by SharesCode,Market order by [Time] desc) num
	from t_shares_transactiondata_time_analyse with(nolock)
	where SharesInfoNum in {0} and [Date] = '{1}' and [Time]<='{2}'
)t
where t.num=1", strStockCode, dataTime.ToString("yyyy-MM-dd"), dataTime.ToString("yyyy-MM-dd HH:mm:00"));
            }
            if (type == 3)//最高价分钟的
            {
                sqlQuery = string.Format(@"select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,minPrice,maxPrice
from(
select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,iMinPrice minPrice,iMaxPrice maxPrice,
row_number() over(partition by SharesCode,Market order by iMaxPrice desc)num
from t_shares_transactiondata_time_analyse with(nolock)
where SharesInfoNum in {0} and [Date] = '{1}' and [Time]<='{2}'
)t
where t.num=1", strStockCode, dataTime.ToString("yyyy-MM-dd"), dataTime.ToString("yyyy-MM-dd HH:mm:00"));
            }
            if (type == 4)//最低价
            {
                sqlQuery = string.Format(@"select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,minPrice,maxPrice
from(
select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,iMinPrice minPrice,iMaxPrice maxPrice,
row_number() over(partition by SharesCode,Market order by iMinPrice)num
from t_shares_transactiondata_time_analyse with(nolock)
where SharesInfoNum in {0} and [Date] = '{1}' and [Time]<='{2}'
)t
where t.num=1", strStockCode, dataTime.ToString("yyyy-MM-dd"), dataTime.ToString("yyyy-MM-dd HH:mm:00"));
            }
            if (type == 5)//最后一笔最高价
            {
                sqlQuery = string.Format(@"select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,minPrice,maxPrice
from(
select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,iMinPrice minPrice,iMaxPrice maxPrice,
row_number() over(partition by SharesCode,Market order by iLastestPrice desc)num
from t_shares_transactiondata_time_analyse with(nolock)
where SharesInfoNum in {0} and [Date] = '{1}' and [Time]<='{2}'
)t
where t.num=1", strStockCode, dataTime.ToString("yyyy-MM-dd"), dataTime.ToString("yyyy-MM-dd HH:mm:00"));
            }
            if (type == 6)//最后一笔最低价
            {
                sqlQuery = string.Format(@"select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,minPrice,maxPrice
from(
select SharesCode,Market,[Time],iTradeStock,lTradeAmount,iOpeningPrice,iLastestPrice,iMinPrice minPrice,iMaxPrice maxPrice,
row_number() over(partition by SharesCode,Market order by iLastestPrice)num
from t_shares_transactiondata_time_analyse with(nolock)
where SharesInfoNum in {0} and [Date] = '{1}' and [Time]<='{2}'
)t
where t.num=1", strStockCode, dataTime.ToString("yyyy-MM-dd"), dataTime.ToString("yyyy-MM-dd HH:mm:00"));
            }
            using (SqlConnection conn = new SqlConnection(_strConnString2))
            {
                conn.Open();

                Dictionary<string, List<DB_TIME_OF_USE_PRICE>> dic = new Dictionary<string, List<DB_TIME_OF_USE_PRICE>>();
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            int market = int.Parse(reader["Market"].ToString());
                            string sharesCode = reader["SharesCode"].ToString();
                            DB_TIME_OF_USE_PRICE result = new DB_TIME_OF_USE_PRICE();
                            result.dtTradeTime = DateTime.Parse(reader["Time"].ToString());
                            result.iTradeAmount = int.Parse(reader["iTradeStock"].ToString());
                            result.iOpeningPrice = int.Parse(reader["iOpeningPrice"].ToString());
                            result.iLastestPrice = int.Parse(reader["iLastestPrice"].ToString());
                            result.lTradePrice = long.Parse(reader["lTradeAmount"].ToString());
                            result.iLowestPrice = int.Parse(reader["minPrice"].ToString());
                            result.iTopPrice = int.Parse(reader["maxPrice"].ToString());
                            if (dic.ContainsKey(sharesCode + "," + market))
                            {
                                dic[sharesCode + "," + market].Add(result);
                            }
                            else
                            {
                                dic.Add(sharesCode + "," + market, new List<DB_TIME_OF_USE_PRICE> { result });
                            }
                        }
                        reader.Close();
                    }

                    int totalStock = 0;
                    long totalAmount = 0;
                    foreach (var item in dic)
                    {
                        foreach (var item2 in item.Value)
                        {
                            totalAmount += item2.lTradePrice;
                            totalStock += item2.iTradeAmount;
                            item2.iBalancePrice = (totalStock == 0 ? 0 : (int)(totalAmount * 1.0 / totalStock));
                        }
                    }
                    foreach (var result in resultDic)
                    {
                        if (dic.ContainsKey(result.Key))
                        {
                            var sourceItem = dic[result.Key].FirstOrDefault();
                            if (sourceItem != null)
                            {
                                result.Value.Add(sourceItem);
                            }
                            else
                            {
                                result.Value.Add(new DB_TIME_OF_USE_PRICE());
                            }
                        }
                        else
                        {
                            result.Value.Add(new DB_TIME_OF_USE_PRICE());
                        }
                    }
                    return resultDic;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 批量查询股票当天价格信息
        /// </summary>
        /// <param name="market">市场0深圳 1上海</param>
        /// <param name="sharesCode">股票代码</param>
        /// <param name="dataTime">时间</param>
        private Dictionary<string, DB_TRADE_PRICE_INFO> GetStockPriceBatch(List<StockBase> stockList, DateTime dataTime)
        {
            StringBuilder strStockCode = new StringBuilder();
            Dictionary<string, DB_TRADE_PRICE_INFO> resultDic = new Dictionary<string, DB_TRADE_PRICE_INFO>();
            if (stockList.Count() <=0)
            {
                return resultDic;
            }

            strStockCode.Append("(");
            for (int i = 0; i < stockList.Count(); i++)
            {
                resultDic.Add(stockList[i].SharesCode + "," + stockList[i].Market, new DB_TRADE_PRICE_INFO
                {
                    iBiddingTradePrice = 0,
                    iPreDayTradePrice = 0
                });
                strStockCode.Append("'");
                strStockCode.Append(stockList[i].SharesCode + "," + stockList[i].Market);
                strStockCode.Append("'");
                if (i < stockList.Count() - 1)
                {
                    strStockCode.Append(",");
                }
            }
            strStockCode.Append(")");

            using (SqlConnection conn = new SqlConnection(_strConnString1))
            {
                conn.Open();
                try
                {
                    //查询股票当天开盘价和收盘价
                    string sqlQuery = string.Format("select SharesCode,Market,ClosedPrice,OpenedPrice from t_shares_quotes_date with(nolock) where SharesCode+','+convert(varchar(10),Market) in {0} and Date='{1}'", strStockCode, dataTime.ToString("yyyy-MM-dd"));
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            int market = int.Parse(reader["Market"].ToString());
                            string sharesCode = reader["SharesCode"].ToString();
                            int closedPrice = int.Parse(reader["ClosedPrice"].ToString());
                            int openedPrice = int.Parse(reader["OpenedPrice"].ToString());

                            resultDic[sharesCode + "," + market].iBiddingTradePrice = openedPrice;
                            resultDic[sharesCode + "," + market].iPreDayTradePrice = closedPrice;
                        }
                        reader.Close();
                    }
                    return resultDic;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 批量获取股票最高价/最低价分钟时间
        /// </summary>
        /// <param name="type">1.最高价 2.最低价</param>
        /// <param name="iPriceOrAmount">排序(1价格排序 2成交量排序)</param>
        /// <param name="dataTime">时间</param>
        /// <returns></returns>
        private Dictionary<string, DateTime?> GetStockMinTimeBatchBack(List<StockBase> stockList, DateTime bgTime,DateTime dataTime)
        {
            DateTime dateNow = DateTime.Now.Date;

            Dictionary<string, DateTime?> resultDic = new Dictionary<string, DateTime?>();
            if (stockList.Count() <= 0)
            {
                return resultDic;
            }
            StringBuilder strStockCode = new StringBuilder();
            strStockCode.Append("(");
            for (int i = 0; i < stockList.Count(); i++)
            {
                resultDic.Add(1 + "|" + stockList[i].SharesCode + "," + stockList[i].Market, null);
                resultDic.Add(2 + "|" + stockList[i].SharesCode + "," + stockList[i].Market, null);
                resultDic.Add(3 + "|" + stockList[i].SharesCode + "," + stockList[i].Market, null);
                resultDic.Add(4 + "|" + stockList[i].SharesCode + "," + stockList[i].Market, null);
                strStockCode.Append(int.Parse(stockList[i].SharesCode)*10+ stockList[i].Market);
                if (i < stockList.Count() - 1)
                {
                    strStockCode.Append(",");
                }
            }
            strStockCode.Append(")");

            string sqlQuery = string.Format(@"select SharesCode,Market,[Time],Price,[Type]
  from t_shares_transactiondata_date_analyse with(nolock)
  where SharesInfoNum in {0} and [Time] <= '{1}' and [Time] >= '{2}' and [Date]='{3}' and [Type]<=4", strStockCode.ToString(), dataTime.ToString("yyyy-MM-dd HH:mm:00"), bgTime.ToString("yyyy-MM-dd HH:mm:00"), dataTime.ToString("yyyy -MM-dd"));
         
            using (SqlConnection conn = new SqlConnection(_strConnString2))
            {
                conn.Open();
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            int market = int.Parse(reader["Market"].ToString());
                            string sharesCode = reader["SharesCode"].ToString();
                            int type = int.Parse(reader["Type"].ToString());
                            DateTime time = DateTime.Parse(reader["Time"].ToString());
                            if (type == 1 || type==5)
                            {
                                if (resultDic.ContainsKey("1|" + sharesCode + "," + market))
                                {
                                    resultDic["1|" + sharesCode + "," + market] = time;
                                }
                            }
                            if (type == 2 || type==6)
                            {
                                if (resultDic.ContainsKey("2|" + sharesCode + "," + market))
                                {
                                    resultDic["2|" + sharesCode + "," + market] = time;
                                }
                            }
                            if (type == 3 || type == 7)
                            {
                                if (resultDic.ContainsKey("3|" + sharesCode + "," + market))
                                {
                                    resultDic["3|" + sharesCode + "," + market] = time;
                                }
                            }
                            if (type == 4 || type == 8)
                            {
                                if (resultDic.ContainsKey("4|" + sharesCode + "," + market))
                                {
                                    resultDic["4|" + sharesCode + "," + market] = time;
                                }
                            }
                        }
                        reader.Close();
                    }
                    return resultDic;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// 批量获取股票最高价/最低价分钟时间
        /// </summary>
        /// <param name="type">1.最高价 2.最低价</param>
        /// <param name="iPriceOrAmount">排序(1价格排序 2成交量排序)</param>
        /// <param name="dataTime">时间</param>
        /// <returns></returns>
        private Dictionary<string, DateTime?> GetStockMinTimeBatch(List<StockBase> stockList, DateTime bgTime, DateTime dataTime)
        {
            DateTime dateNow = DateTime.Now.Date;

            Dictionary<string, DateTime?> resultDic = new Dictionary<string, DateTime?>();
            if (stockList.Count() <= 0)
            {
                return resultDic;
            }
            StringBuilder strStockCode = new StringBuilder();
            strStockCode.Append("(");
            for (int i = 0; i < stockList.Count(); i++)
            {
                resultDic.Add(1 + "|" + stockList[i].SharesCode + "," + stockList[i].Market, null);
                resultDic.Add(2 + "|" + stockList[i].SharesCode + "," + stockList[i].Market, null);
                resultDic.Add(3 + "|" + stockList[i].SharesCode + "," + stockList[i].Market, null);
                resultDic.Add(4 + "|" + stockList[i].SharesCode + "," + stockList[i].Market, null);
                strStockCode.Append(int.Parse(stockList[i].SharesCode) * 10 + stockList[i].Market);
                if (i < stockList.Count() - 1)
                {
                    strStockCode.Append(",");
                }
            }
            strStockCode.Append(")");

            string sqlQuery = string.Format(@"select SharesCode,Market,[Time],Price,num,num2,num3,num4
from
(
  select SharesCode,Market,Time,Price,
  row_number()over(partition by SharesCode,Market order by Price,Time) num,
  row_number()over(partition by SharesCode,Market order by Price desc,Time) num2,
  row_number()over(partition by SharesCode,Market order by IsTimeLast desc,Price,Time) num3,
  row_number()over(partition by SharesCode,Market order by IsTimeLast desc,Price desc,Time) num4
  from t_shares_transactiondata with(nolock)
  where SharesInfoNum in {0} and Time <= '{1}' and Time>='{2}'
)t
where t.num=1 or t.num2=1 or t.num3=1 or t.num4=1", strStockCode.ToString(), dataTime.ToString("yyyy-MM-dd HH:mm:00"), bgTime.ToString("yyyy-MM-dd HH:mm:00"));

            using (SqlConnection conn = new SqlConnection(_strConnString2))
            {
                conn.Open();
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            int market = int.Parse(reader["Market"].ToString());
                            string sharesCode = reader["SharesCode"].ToString();
                            int num = int.Parse(reader["num"].ToString());
                            int num2 = int.Parse(reader["num2"].ToString());
                            int num3 = int.Parse(reader["num3"].ToString());
                            int num4 = int.Parse(reader["num4"].ToString());
                            DateTime time = DateTime.Parse(reader["Time"].ToString());
                            if (num==1)
                            {
                                if (resultDic.ContainsKey("1|" + sharesCode + "," + market))
                                {
                                    resultDic["1|" + sharesCode + "," + market] = time;
                                }
                            }
                            if (num2 == 1)
                            {
                                if (resultDic.ContainsKey("2|" + sharesCode + "," + market))
                                {
                                    resultDic["2|" + sharesCode + "," + market] = time;
                                }
                            }
                            if (num3 == 1)
                            {
                                if (resultDic.ContainsKey("3|" + sharesCode + "," + market))
                                {
                                    resultDic["3|" + sharesCode + "," + market] = time;
                                }
                            }
                            if (num4 == 1)
                            {
                                if (resultDic.ContainsKey("4|" + sharesCode + "," + market))
                                {
                                    resultDic["4|" + sharesCode + "," + market] = time;
                                }
                            }
                        }
                        reader.Close();
                    }
                    return resultDic;
                }
                catch (Exception ex)
                {
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
