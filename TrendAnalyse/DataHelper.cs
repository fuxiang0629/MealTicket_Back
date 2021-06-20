using FXCommon.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockTrendMonitor.Define.StockMonitorDefine;

namespace TrendAnalyse
{
    public class DataHelper
    {
        /// <summary>
        /// 判断是否交易日
        /// </summary>
        /// <param name="time">日期，null表示当天</param>
        /// <returns></returns>
        public static bool CheckTradeDate(DateTime? date=null)
        {
            DateTime timeDate = DateTime.Now.Date;
            if (date != null)
            {
                timeDate = date.Value.Date;
            }
            int timeDateInt = int.Parse(timeDate.ToString("yyyyMMdd"));

            using (SqlConnection conn = new SqlConnection(Singleton.Instance.connString_meal_ticket))
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
        /// 检查当前是否处理时间
        /// </summary>
        /// <returns></returns>
        public static bool CheckHandlerTime(DateTime? time = null)
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

            List<string> TimeArr = new List<string>();
            using (SqlConnection conn = new SqlConnection(Singleton.Instance.connString_meal_ticket))
            {
                conn.Open();
                try
                {
                    string sqlQuery = "select Time3 from t_shares_limit_time with(nolock)";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sqlQuery;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            string timeStr = Convert.ToString(reader["Time3"]);
                            TimeArr.Add(timeStr);
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
            foreach (var item in TimeArr)
            {
                if (item != null)
                {
                    string[] timeArr = item.Split(',');
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

        /// <summary>
        /// 走势分析
        /// </summary>
        /// <param name="sharesList"></param>
        public static void ToAnalyse(List<SharesInfo> sharesList)
        {
            if (sharesList == null || sharesList.Count() <= 0)
            {
                return;
            }
            string sqlQuery=CreateSql(sharesList);
            var dataList=GetTrendPar(sqlQuery);

            var accountDataList = (from item in dataList
                                   group item by item.AccountId into g
                                   select g).ToList();
            foreach (var data in accountDataList)
            {
                var List_Trend1 = data.Where(e => e.TrendId == 1).ToList();
                var List_Trend2 = data.Where(e => e.TrendId == 2).ToList();
                var List_Trend3 = data.Where(e => e.TrendId == 3).ToList();

                List<TREND_RESULT_RAPID_UP> resultInfo_Trend1 = new List<TREND_RESULT_RAPID_UP>();
                int errorCode_Trend1 = Analysis_Trend1(List_Trend1, ref resultInfo_Trend1);
                if (errorCode_Trend1 == 0)
                {
                    foreach (var item in List_Trend1)
                    {
                        var temp = resultInfo_Trend1.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(temp.strStockCode) && temp.dicUpOrDownInfo.Count() > 0)
                        {
                            var tempModel = temp.dicUpOrDownInfo.FirstOrDefault();
                            var pushTime = tempModel.Value.lastestInfo.dtTradeTime;
                            var pushDesc = tempModel.Key + "分钟上涨" + (tempModel.Value.iLastestPercent * 1.0 / 100).ToString("N2") + "%";
                            TrendTri(item.RelId, item.Market, item.SharesCode, data.Key, item.TrendId, item.SharesName, item.TrendName, item.TrendDescription, item.OptionalId, pushTime, pushDesc);
                        }
                    }
                }

                List<TREND_RESULT_LINE_UP> resultInfo_Trend2 = new List<TREND_RESULT_LINE_UP>();
                int errorCode_Trend2 = Analysis_Trend2(List_Trend2, ref resultInfo_Trend2);
                if (errorCode_Trend2 == 0)
                {
                    foreach (var item in List_Trend2)
                    {
                        var temp = resultInfo_Trend2.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(temp.strStockCode))
                        {
                            var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                            var pushDesc = (temp.upOrDownInfo.iLastestPercent * 1.0 / 100).ToString("N2") + "%";
                            TrendTri(item.RelId, item.Market, item.SharesCode, data.Key, item.TrendId, item.SharesName, item.TrendName, item.TrendDescription, item.OptionalId, pushTime, pushDesc);
                        }
                    }
                }

                List<TREND_RESULT_BOX_BREACH> resultInfo_Trend3 = new List<TREND_RESULT_BOX_BREACH>();
                int errorCode_Trend3 = Analysis_Trend3(List_Trend3, ref resultInfo_Trend3);
                if (errorCode_Trend3 == 0)
                {
                    foreach (var item in List_Trend3)
                    {
                        var temp = resultInfo_Trend3.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(temp.strStockCode))
                        {
                            var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                            var pushDesc = (temp.upOrDownInfo.iLastestPercent * 1.0 / 100).ToString("N2") + "%";
                            TrendTri(item.RelId, item.Market, item.SharesCode, data.Key, item.TrendId, item.SharesName, item.TrendName, item.TrendDescription, item.OptionalId, pushTime, pushDesc);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 拼接sql语句
        /// </summary>
        /// <returns></returns>
        private static string CreateSql(List<SharesInfo> sharesList) 
        {
            DateTime timeNow = DateTime.Now;

            StringBuilder sharesQuery = new StringBuilder();
            sharesQuery.Append("(");
            for (int i = 0; i < sharesList.Count; i++)
            {
                sharesQuery.Append("'");
                sharesQuery.Append(sharesList[i].SharesCode);
                sharesQuery.Append(",");
                sharesQuery.Append(sharesList[i].Market.ToString());
                sharesQuery.Append("'");
                if (i < sharesList.Count - 1)
                {
                    sharesQuery.Append(",");
                }
            }
            sharesQuery.Append(")");

            string sql = string.Format(@"select t.AccountId,t.Market,t.SharesCode,t5.SharesName,t3.TrendId,t4.Name TrendName,t4.[Description] TrendDescription,t3.Id RelId,t.Id OptionalId,t6.ParamsInfo
from t_account_shares_optional t with(nolock) 
inner join t_account_shares_optional_seat_rel t1 with(nolock) on t.Id=t1.OptionalId
inner join t_account_shares_seat t2 with(nolock) on t1.SeatId=t2.Id and t.AccountId=t2.AccountId
inner join t_account_shares_optional_trend_rel t3 with(xlock) on t.Id=t3.OptionalId
inner join t_shares_monitor_trend t4 with(xlock) on t3.TrendId=t4.Id
inner join t_shares_all t5 with(xlock) on t.SharesCode=t5.SharesCode and t.Market=t5.Market
inner join t_account_shares_optional_trend_rel_par t6 with(xlock) on t3.Id=t6.RelId
inner join t_account_login_token_web t7 on t.AccountId=t7.AccountId
where t2.[Status]=1 and t3.[Status]=1 and t4.[Status]=1 and t7.[Status]=1 and datediff(SS,t7.HeartTime,'{0}')<{2} and t2.ValidEndTime>'{0}' and 
(t.SharesCode+','+convert(varchar(10),t.Market)) in {1}", timeNow.ToString("yyyy-MM-dd HH:mm:ss"), sharesQuery.ToString(),Singleton.Instance.HeartSecond);
            return sql;
        }

        /// <summary>
        /// 查询分析参数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static List<OptionalTrend> GetTrendPar(string sql) 
        {
            List<OptionalTrend> list = new List<OptionalTrend>();
            using (SqlConnection conn = new SqlConnection(Singleton.Instance.connString_meal_ticket))
            {
                conn.Open();
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;   //sql语句
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            long AccountId = Convert.ToInt64(reader["AccountId"]);
                            int Market = Convert.ToInt32(reader["Market"]);
                            string SharesCode = Convert.ToString(reader["SharesCode"]);
                            string SharesName = Convert.ToString(reader["SharesName"]);
                            long TrendId = Convert.ToInt64(reader["TrendId"]);
                            string TrendName = Convert.ToString(reader["TrendName"]);
                            string TrendDescription = Convert.ToString(reader["TrendDescription"]);
                            long RelId = Convert.ToInt64(reader["RelId"]);
                            long OptionalId = Convert.ToInt64(reader["OptionalId"]);
                            string ParamsInfo = Convert.ToString(reader["ParamsInfo"]);

                            var temp = list.Where(e => e.AccountId == AccountId && e.Market == Market && e.SharesCode == SharesCode && e.TrendId == TrendId && e.RelId == RelId && e.OptionalId == OptionalId).FirstOrDefault();
                            if (temp == null)
                            {
                                list.Add(new OptionalTrend
                                {
                                    AccountId = AccountId,
                                    Market = Market,
                                    SharesCode = SharesCode,
                                    SharesName = SharesName,
                                    TrendId = TrendId,
                                    TrendName = TrendName,
                                    TrendDescription = TrendDescription,
                                    RelId = RelId,
                                    OptionalId = OptionalId,
                                    ParList = new List<string> 
                                    {
                                        ParamsInfo
                                    }
                                });
                            }
                            else
                            {
                                temp.ParList.Add(ParamsInfo);
                            }
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

            return list;
        }

        /// <summary>
        /// 分析快速上涨
        /// </summary>
        private static int Analysis_Trend1(List<OptionalTrend> list, ref List<TREND_RESULT_RAPID_UP> resultInfo)
        {
            List<ANALYSIS_TREND_RAPID_UP> lstElement = new List<ANALYSIS_TREND_RAPID_UP>();
            foreach (var item in list)
            {
                ANALYSIS_TREND_RAPID_UP _element = new ANALYSIS_TREND_RAPID_UP();
                _element.dicParam = new Dictionary<int, List<TREND_PARAM_RAPID_UP>>();

                foreach (var par in item.ParList)
                {
                    var temp = JsonConvert.DeserializeObject<Trend1>(par);

                    List<TREND_PARAM_RAPID_UP> lstParam = new List<TREND_PARAM_RAPID_UP>();
                    foreach (var par2 in temp.ParList)
                    {
                        TREND_PARAM_RAPID_UP _param = new TREND_PARAM_RAPID_UP();
                        _param.iAllowRange = par2.YXFWFD;
                        _param.iMinUpPercent = par2.SZFD;
                        lstParam.Add(_param);
                    }
                    _element.dicParam.Add(temp.ZHFZS, lstParam);
                }

                _element.strStockCode = item.SharesCode + "," + item.Market;

                lstElement.Add(_element);
            }
            int iErrorCode = Singleton.Instance.m_stockMonitor.AnalysisTrend_RapidUp(lstElement, ref resultInfo);
            return iErrorCode;
        }

        /// <summary>
        /// 分析多头上涨
        /// </summary>
        private static int Analysis_Trend2(List<OptionalTrend> list, ref List<TREND_RESULT_LINE_UP> resultInfo)
        {
            List<ANALYSIS_TREND_LINE_UP> lstElement = new List<ANALYSIS_TREND_LINE_UP>();
            foreach (var item in list)
            {
                var temp = JsonConvert.DeserializeObject<Trend2>(item.ParList.FirstOrDefault());
                ANALYSIS_TREND_LINE_UP _element = new ANALYSIS_TREND_LINE_UP();
                _element.param.bTopPriceExceed = temp.CCJRZGD;
                _element.param.iMaxMintues = temp.ZDFSSL;
                _element.param.iMinMintues = temp.ZXFSSL;
                _element.param.iMinUpPercent = temp.TPFD;
                _element.param.iAllowRange = temp.YXFWFD;
                _element.param.iAllowWidth = temp.YXJJBL;

                _element.strStockCode = item.SharesCode + "," + item.Market;
                lstElement.Add(_element);
            }
            int iErrorCode = Singleton.Instance.m_stockMonitor.AnalysisTrend_LineUp(lstElement, ref resultInfo);
            return iErrorCode;
        }

        /// <summary>
        /// 分析箱体上涨
        /// </summary>
        private static int Analysis_Trend3(List<OptionalTrend> list, ref List<TREND_RESULT_BOX_BREACH> resultInfo)
        {
            List<ANALYSIS_TREND_BOX_BREACH> lstElement = new List<ANALYSIS_TREND_BOX_BREACH>();
            foreach (var item in list)
            {
                var temp = JsonConvert.DeserializeObject<Trend3>(item.ParList.FirstOrDefault());
                TREND_PARAM_BOX_BREACH _param = new TREND_PARAM_BOX_BREACH();
                _param.bTopOfDayExceed = temp.CCJRZGD;
                _param.iMinMintues = temp.ZXFSSL;
                _param.iAllowRange = temp.YXJJBL;
                _param.iBoxPercentExceed = temp.YXJXWC;
                _param.iBreachMinutes = temp.TPHSJ;

                ANALYSIS_TREND_BOX_BREACH _element = new ANALYSIS_TREND_BOX_BREACH();
                _element.param = _param;
                _element.strStockCode = item.SharesCode + "," + item.Market;
                lstElement.Add(_element);
            }
            int iErrorCode = Singleton.Instance.m_stockMonitor.AnalysisTrend_BoxBreach(lstElement, ref resultInfo);
            return iErrorCode;
        }

        /// <summary>
        /// 触发
        /// </summary>
        private static void TrendTri(long relId, int market, string sharesCode, long accountId, long trendId, string sharesName, string trendName, string trendDescription, long optionalId, DateTime pushTime,string pushDesc)
        {
            bool IsPush = false;
            bool IsFirst = false;

            DateTime timeNow = DateTime.Now;
            long currPrice = 0;
            long closePrice = 0;
            int riseRate = 0;
            using (SqlConnection conn = new SqlConnection(Singleton.Instance.connString_meal_ticket))
            {
                conn.Open();
                try
                {
                    using (SqlCommand comm = conn.CreateCommand())
                    {
                        string sql = string.Format("select top 1 PresentPrice,ClosedPrice from v_shares_quotes_last with(nolock) where Market={0} and SharesCode='{1}'", market, sharesCode);
                        comm.CommandType = CommandType.Text;
                        comm.CommandText = sql;
                        SqlDataReader reader = comm.ExecuteReader();
                        if (reader.Read())
                        {
                            currPrice = Convert.ToInt64(reader["PresentPrice"]);
                            closePrice = Convert.ToInt64(reader["ClosedPrice"]);
                        }
                        reader.Close();
                    }

                    if (currPrice <= 0)
                    {
                        return;
                    }
                    if (closePrice <= 0)
                    {
                        return;
                    }
                    riseRate = (int)((currPrice - closePrice) * 1.0 / closePrice * 10000);

                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            string sql = string.Format("select top 1 LastPushTime,LastPushRiseRate,LastPushPrice,MinPushTimeInterval,MinPushRateInterval,MinTodayPrice,LastModified from t_account_shares_optional_trend_rel_tri with(xlock) where RelId={0}", relId);

                            using (SqlCommand comm = conn.CreateCommand())
                            {
                                comm.Transaction = tran;
                                comm.CommandType = System.Data.CommandType.Text;

                                comm.CommandText = sql;
                                SqlDataReader reader = comm.ExecuteReader();
                                if (reader.Read())
                                {
                                    DateTime LastModified = Convert.ToDateTime(reader["LastModified"]);

                                    if (reader["LastPushTime"] == DBNull.Value || reader["LastPushRiseRate"] == DBNull.Value || reader["LastPushPrice"] == DBNull.Value)
                                    {
                                        IsPush = true;
                                        IsFirst = true;
                                    }
                                    else if (Convert.ToDateTime(reader["LastPushTime"]).Date < pushTime.Date)
                                    {
                                        IsPush = true;
                                        IsFirst = true;
                                    }
                                    else
                                    {
                                        DateTime LastPushTime = Convert.ToDateTime(reader["LastPushTime"]);
                                        int MinPushTimeInterval = Convert.ToInt32(reader["MinPushTimeInterval"]);
                                        int LastPushRiseRate = Convert.ToInt32(reader["LastPushRiseRate"]);
                                        int MinPushRateInterval = Convert.ToInt32(reader["MinPushRateInterval"]);
                                        long LastPushPrice = Convert.ToInt64(reader["LastPushPrice"]);
                                        long MinTodayPrice = Convert.ToInt64(reader["MinTodayPrice"]);

                                        if ((MinTodayPrice != -1 && currPrice > MinTodayPrice) || (MinPushTimeInterval != -1 && (pushTime - LastPushTime).TotalSeconds >= MinPushTimeInterval) || (MinPushRateInterval != -1 && (riseRate - LastPushRiseRate) >= MinPushRateInterval))
                                        {
                                            IsPush = true;
                                        }
                                    }

                                    reader.Close();
                                    if (IsPush)
                                    {
                                        sql = string.Format("update t_account_shares_optional_trend_rel_tri set LastPushTime='{0}',LastPushRiseRate={1},LastPushPrice={2},LastPushDesc='{4}',TriCountToday=TriCountToday+1 where RelId={3}", pushTime.ToString("yyyy-MM-dd HH:mm:ss"), riseRate, currPrice, relId, pushDesc);
                                        comm.CommandText = sql;
                                        comm.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    IsPush = true;
                                    IsFirst = true;
                                    reader.Close();
                                    sql = string.Format("insert into t_account_shares_optional_trend_rel_tri(RelId,LastPushTime,LastPushRiseRate,LastPushPrice,LastPushDesc,TriCountToday,MinPushTimeInterval,MinPushRateInterval,MinTodayPrice,CreateTime,LastModified) values({0},'{1}',{2},{3},'{8}',{4},{5},{6},{7},'{1}','{1}')", relId, pushTime.ToString("yyyy-MM-dd HH:mm:ss"), riseRate, currPrice, 1, 0, 0, -1, pushDesc);
                                    comm.CommandText = sql;
                                    comm.ExecuteNonQuery();
                                }


                                if (IsPush)
                                {
                                    if (IsFirst)
                                    {
                                        sql = string.Format("update t_account_shares_optional set TriCountToday=1 where Id={0}", optionalId);
                                        comm.CommandText = sql;
                                        comm.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        sql = string.Format("update t_account_shares_optional set TriCountToday=TriCountToday+1 where Id={0}", optionalId);
                                        comm.CommandText = sql;
                                        comm.ExecuteNonQuery();
                                    }

                                    sql = string.Format("update t_account_shares_optional set IsTrendClose=0 where Id={0} and IsTrendClose=1", optionalId);
                                    comm.CommandText = sql;
                                    comm.ExecuteNonQuery();
                                }


                                sql = string.Format("insert into t_account_shares_optional_trend_rel_tri_record(RelId,AccountId,Market,SharesCode,SharesName,TrendId,TrendName,TrendDescription,TriPrice,IsPush,CreateTime,OptionalId,TriDesc) values({0},{1},{2},'{3}','{4}',{5},'{6}','{7}',{8},{9},'{10}',{11},'{12}')", relId, accountId, market, sharesCode, sharesName, trendId, trendName, trendDescription, currPrice, IsPush ? 1 : 0, timeNow.ToString("yyyy-MM-dd HH:mm:ss"), optionalId, pushDesc);
                                comm.CommandText = sql;
                                comm.ExecuteNonQuery();
                            }

                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw ex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("触发分析出错", ex);
                }
                finally 
                {
                    conn.Close();
                }
            }
        }
    }
}
