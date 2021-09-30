using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using stock_db_core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static StockTrendMonitor.Define.StockMonitorDefine;

namespace MealTicket_Web_Handler
{
    public class SharesTrendPar 
    {
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public long TrendId { get; set; }
        public string ParamsInfo { get; set; }
        public List<string> ParamsList { get; set; }
    }

    public class TrendAnalyseResult
    {
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public DateTime PushTime { get; set; }
        public string PushDesc { get; set; }
        public long TrendId { get; set; }
    }

    public class DataHelper
    {
        /// <summary>
        /// 判断是否交易日
        /// </summary>
        /// <param name="time">日期，null表示当天</param>
        /// <returns></returns>
        public static bool CheckTradeDate(DateTime? date = null)
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
            var dataList = (from item in Singleton.Instance.SharesTrendParSession
                            join item2 in sharesList on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                            select item).ToList();

            var List_Trend1 = (from item in dataList
                               where item.TrendId == 1
                               select new OptionalTrend
                               {
                                   SharesCode = item.SharesCode,
                                   Market = item.Market,
                                   TrendId = item.TrendId,
                                   ParList = item.ParamsList
                               }).ToList();
            var List_Trend2 = (from item in dataList
                               where item.TrendId == 2
                               select new OptionalTrend
                               {
                                   SharesCode = item.SharesCode,
                                   Market = item.Market,
                                   TrendId = item.TrendId,
                                   ParList = item.ParamsList
                               }).ToList();
            var List_Trend3 = (from item in dataList
                               where item.TrendId == 3
                               select new OptionalTrend
                               {
                                   SharesCode = item.SharesCode,
                                   Market = item.Market,
                                   TrendId = item.TrendId,
                                   ParList = item.ParamsList
                               }).ToList();

            List<TrendAnalyseResult> trendResult = new List<TrendAnalyseResult>();
            List<TREND_RESULT_RAPID_UP> resultInfo_Trend1 = new List<TREND_RESULT_RAPID_UP>();
            int errorCode_Trend1 = Analysis_Trend1(List_Trend1, ref resultInfo_Trend1);
            foreach (var item in List_Trend1)
            {
                var temp = resultInfo_Trend1.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                if (!string.IsNullOrEmpty(temp.strStockCode) && temp.dicUpOrDownInfo.Count() > 0)
                {
                    var tempModel = temp.dicUpOrDownInfo.FirstOrDefault();
                    var pushTime = tempModel.Value.lastestInfo.dtTradeTime;
                    var pushDesc = tempModel.Key + "分钟上涨" + (tempModel.Value.iLastestPercent * 1.0 / 100).ToString("N2") + "%";
                    trendResult.Add(new TrendAnalyseResult
                    {
                        SharesCode = item.SharesCode,
                        Market = item.Market,
                        PushDesc = pushDesc,
                        PushTime = pushTime,
                        TrendId = item.TrendId
                    });
                }
            }
            List<TREND_RESULT_LINE_UP> resultInfo_Trend2 = new List<TREND_RESULT_LINE_UP>();
            int errorCode_Trend2 = Analysis_Trend2(List_Trend2, ref resultInfo_Trend2);
            foreach (var item in List_Trend2)
            {
                var temp = resultInfo_Trend2.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                if (!string.IsNullOrEmpty(temp.strStockCode))
                {
                    var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                    var pushDesc = (temp.upOrDownInfo.iLastestPercent * 1.0 / 100).ToString("N2") + "%";
                    trendResult.Add(new TrendAnalyseResult
                    {
                        SharesCode = item.SharesCode,
                        Market = item.Market,
                        PushDesc = pushDesc,
                        PushTime = pushTime,
                        TrendId = item.TrendId
                    });
                }
            }
            List<TREND_RESULT_BOX_BREACH> resultInfo_Trend3 = new List<TREND_RESULT_BOX_BREACH>();
            int errorCode_Trend3 = Analysis_Trend3(List_Trend3, ref resultInfo_Trend3);
            foreach (var item in List_Trend3)
            {
                var temp = resultInfo_Trend3.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                if (!string.IsNullOrEmpty(temp.strStockCode))
                {
                    var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                    var pushDesc = (temp.upOrDownInfo.iLastestPercent * 1.0 / 100).ToString("N2") + "%";
                    trendResult.Add(new TrendAnalyseResult
                    {
                        SharesCode = item.SharesCode,
                        Market = item.Market,
                        PushDesc = pushDesc,
                        PushTime = pushTime,
                        TrendId = item.TrendId
                    });
                }
            }
            TrendTri(trendResult);

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
inner join t_account_shares_optional_trend_rel t3 with(nolock) on t.Id=t3.OptionalId
inner join t_shares_monitor_trend t4 with(nolock) on t3.TrendId=t4.Id
inner join t_shares_all t5 with(nolock) on t.SharesCode=t5.SharesCode and t.Market=t5.Market
inner join t_account_shares_optional_trend_rel_par t6 with(nolock) on t3.Id=t6.RelId
inner join t_account_login_token_web t7 with(nolock) on t.AccountId=t7.AccountId
where t2.[Status]=1 and t3.[Status]=1 and t4.[Status]=1 and t7.[Status]=1 and datediff(SS,t7.HeartTime,'{0}')<{2} and t2.ValidEndTime>'{0}' and 
t.SharesInfo in {1}", timeNow.ToString("yyyy-MM-dd HH:mm:ss"), sharesQuery.ToString(), Singleton.Instance.HeartSecond);
            return sql;
        }

        /// <summary>
        /// 查询分析参数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static List<SharesTrendPar> GetTrendPar()
        {
            string sql = @"select Market,SharesCode,TrendId,ParamsInfo
  from t_shares_monitor t with(nolock)
  inner join t_shares_monitor_trend_rel t1 with(nolock) on t.Id=t1.MonitorId
  inner join t_shares_monitor_trend_rel_par t2 with(nolock) on t1.Id=t2.RelId
  where t.[Status]=1 and t1.[Status]=1";
            using (var db = new meal_ticketEntities())
            {
                var sqlResult = db.Database.SqlQuery<SharesTrendPar>(sql).ToList();
                var result = (from item in sqlResult
                              group item by new { item.Market, item.SharesCode, item.TrendId } into g
                              select new SharesTrendPar
                              {
                                  SharesCode = g.Key.SharesCode,
                                  Market = g.Key.Market,
                                  TrendId = g.Key.TrendId,
                                  ParamsList = g.Select(e => e.ParamsInfo).ToList()
                              }).ToList();
                return result;
            }
        }

        /// <summary>
        /// 分析快速上涨
        /// </summary>
        public static int Analysis_Trend1(List<OptionalTrend> list, ref List<TREND_RESULT_RAPID_UP> resultInfo)
        {
            try
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
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析快速上涨出错了", ex);
                return -1;
            }
        }

        /// <summary>
        /// 分析多头上涨
        /// </summary>
        public static int Analysis_Trend2(List<OptionalTrend> list, ref List<TREND_RESULT_LINE_UP> resultInfo)
        {
            try
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
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析多头上涨出错了", ex);
                return -1;
            }
        }

        /// <summary>
        /// 分析箱体上涨
        /// </summary>
        public static int Analysis_Trend3(List<OptionalTrend> list, ref List<TREND_RESULT_BOX_BREACH> resultInfo)
        {
            try
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
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析箱体上涨出错了", ex);
                return -1;
            }
        }

        //分析历史涨跌幅数据

        public static int Analysis_HisRiseRate(string sharesCode, int market, List<string> par,StringBuilder logRecord)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int day = 0;
                int type = 0;
                int compare = 0;
                int count = 0;
                int rateCompare = 0;
                int rateCount = 0;
                int limitDay = 0;
                bool flatRise = false;
                bool triPrice = false;
                try
                {
                    day = Convert.ToInt32(temp.Day);
                }
                catch (Exception) { }
                try
                {
                    type = Convert.ToInt32(temp.Type);
                }
                catch (Exception) { }
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                }
                catch (Exception) { }
                try
                {
                    count = (type == 11 || type == 12 || type == 14) ? (int)(Convert.ToDouble(temp.Rise) * 100) : Convert.ToInt32(temp.Count);
                }
                catch (Exception) { }
                try
                {
                    rateCompare = Convert.ToInt32(temp.RateCompare);
                }
                catch (Exception) { }
                try
                {
                    rateCount = (int)(Convert.ToDouble(temp.RateCount) * 100);
                }
                catch (Exception) { }
                try
                {
                    limitDay = Convert.ToInt32(temp.LimitDay);
                }
                catch (Exception) { }
                try
                {
                    triPrice = Convert.ToBoolean(temp.TriPrice);
                }
                catch (Exception) { }
                try
                {
                    flatRise = Convert.ToBoolean(temp.FlatRise);
                }
                catch (Exception) { }

                if (day <= 0)
                {
                    logRecord.Append("\t\t条件不成立(参数day错误)");
                    return -1;
                }
                DateTime dateNow = DateTime.Now.Date;
                using (var db = new meal_ticketEntities())
                {
                    var quotes_date = (from item in db.t_shares_quotes_date
                                       where item.SharesCode == sharesCode && item.Market == market && item.LastModified < dateNow
                                       orderby item.Date descending
                                       select item).Take(day).ToList();
                    //没涨停次数
                    if (type == 1)
                    {
                        int tempCount = quotes_date.Where(e => e.PriceType != 1).Count();
                        string conditionDes = compare == 1 ? ("没涨停次数大于等于" + count) : ("没涨停次数小于等于" + count);
                        if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                        {
                            logRecord.Append("\t\t条件成立(当前没涨停次数:" + tempCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.Append("\t\t条件不成立(当前没涨停次数:" + tempCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //涨停次数
                    else if (type == 2)
                    {
                        int tempCount = 0;
                        if (triPrice)
                        {
                            tempCount = quotes_date.Where(e => e.TriPriceType == 1).Count();
                        }
                        else
                        {
                            tempCount = quotes_date.Where(e => e.PriceType == 1).Count();
                        }

                        string conditionDes = compare == 1 ? ("涨停次数大于等于" + count) : ("涨停次数小于等于" + count);
                        if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                        {
                            logRecord.Append("\t\t条件成立(当前涨停次数:" + tempCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.Append("\t\t条件不成立(当前涨停次数:" + tempCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //跌停次数
                    else if (type == 3)
                    {
                        int tempCount = 0;
                        if (triPrice)
                        {
                            tempCount = quotes_date.Where(e => e.TriPriceType == 2).Count();
                        }
                        else
                        {
                            tempCount = quotes_date.Where(e => e.PriceType == 2).Count();
                        }
                        string conditionDes = compare == 1 ? ("跌停次数大于等于" + count) : ("跌停次数小于等于" + count);
                        if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                        {
                            logRecord.Append("\t\t条件成立(当前跌停次数:" + tempCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.Append("\t\t条件不成立(当前跌停次数:" + tempCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //炸板次数
                    else if (type == 4)
                    {
                        int tempCount = quotes_date.Where(e => e.PriceType != 1 && e.LimitUpCount > 0).Count();
                        string conditionDes = compare == 1 ? ("炸板次数大于等于" + count) : ("炸板次数小于等于" + count);
                        if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                        {
                            logRecord.Append("\t\t条件成立(当前炸板次数:" + tempCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.Append("\t\t条件不成立(当前炸板次数:" + tempCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //每日涨跌幅
                    else if (type == 13)
                    {
                        int i = 0;
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PresentPrice <= 0 || quote.ClosedPrice <= 0)
                            {
                                continue;
                            }
                            int rate = (int)((quote.PresentPrice - quote.ClosedPrice) * 1.0 / quote.ClosedPrice * 10000);
                            if (!flatRise && rate == 0)
                            {
                                continue;
                            }
                            if (rateCompare == 1 && rate >= rateCount)
                            {
                                i++;
                                continue;
                            }
                            if (rateCompare == 2 && rate <= rateCount)
                            {
                                i++;
                                continue;
                            }
                        }
                        string rateDes = rateCompare == 1 ? ("大于等于" + rateCount) : ("小于等于" + rateCount);
                        string conditionDes = compare == 1 ? ("每日涨跌幅" + rateDes + "的次数大于等于" + count) : ("每日涨跌幅" + rateDes + "的次数小于等于" + count);
                        if ((compare == 1 && i >= count) || (compare == 2 && i <= count))
                        {
                            logRecord.Append("\t\t条件成立(当前符合条件次数" + i + ", 条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.Append("\t\t条件不成立(当前符合条件次数" + i + ", 条件:" + conditionDes + ")");
                        return -1;
                    }
                    //包含连续涨停
                    else if (type == 7)
                    {
                        int tempi = 0;//连续涨停天数
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PriceType == 1)
                            {
                                tempi++;
                            }
                            else
                            {
                                tempi = 0;
                            }
                            if (tempi >= limitDay)
                            {
                                logRecord.Append("\t\t条件成立(符合连续涨停" + limitDay + "天)");
                                return 0;
                            }
                        }
                        logRecord.Append("\t\t条件不成立(不符合连续涨停" + limitDay + "天)");
                        return -1;
                    }
                    //排除连续涨停
                    else if (type == 8)
                    {
                        int tempi = 0;//连续涨停天数
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PriceType == 1)
                            {
                                tempi++;
                            }
                            else
                            {
                                tempi = 0;
                            }
                            if (tempi >= limitDay)
                            {
                                logRecord.Append("\t\t条件不成立(不符合排除连续涨停" + limitDay + "天)");
                                return -1;
                            }
                        }
                        logRecord.Append("\t\t条件成立(符合排除连续涨停" + limitDay + "天)");
                        return 0;
                    }
                    //包含连续跌停
                    else if (type == 9)
                    {
                        int tempi = 0;//连续跌停天数
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PriceType == 2)
                            {
                                tempi++;
                            }
                            else
                            {
                                tempi = 0;
                            }
                            if (tempi >= limitDay)
                            {
                                logRecord.Append("\t\t条件成立(符合连续跌停" + limitDay + "天)");
                                return 0;
                            }
                        }
                        logRecord.Append("\t\t条件不成立(不符合连续跌停" + limitDay + "天)");
                        return -1;
                    }
                    //排除连续跌停
                    else if (type == 10)
                    {
                        int tempi = 0;//连续跌停天数
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PriceType == 2)
                            {
                                tempi++;
                            }
                            else
                            {
                                tempi = 0;
                            }
                            if (tempi >= limitDay)
                            {
                                logRecord.Append("\t\t条件不成立(不符合排除连续跌停" + limitDay + "天)");
                                return -1;
                            }
                        }
                        logRecord.Append("\t\t条件成立(符合排除连续跌停" + limitDay + "天)");
                        return 0;
                    }
                    //总涨跌幅
                    else if (type == 14)
                    {
                        long closePrice = quotes_date.OrderBy(e => e.Date).Select(e => e.ClosedPrice).FirstOrDefault();
                        long presentPrice = quotes_date.OrderByDescending(e => e.Date).Select(e => e.PresentPrice).FirstOrDefault();
                        if (closePrice <= 0 || presentPrice <= 0)
                        {
                            logRecord.Append("\t\t条件不成立(五档数据有误)");
                            return -1;
                        }
                        int rate = (int)((presentPrice - closePrice) * 1.0 / closePrice * 10000);
                        if (!flatRise && rate == 0)
                        {
                            logRecord.Append("\t\t条件不成立(当前为平盘，条件不包含平盘)");
                            return -1;
                        }
                        string conditionDes = compare == 1 ? ("总涨跌幅大于等于" + count) : ("总涨跌幅小于等于" + count);
                        if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                        {
                            logRecord.Append("\t\t条件成立(当前总涨跌幅:" + rate + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.Append("\t\t条件不成立(当前总涨跌幅:" + rate + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    else
                    {
                        logRecord.Append("\t\t条件不成立(参数type错误)");
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析历史涨跌幅数据出错了", ex);
                logRecord.Append("\t\t条件不成立(异常报错)");
                return -1;
            }
        }

        public static int Analysis_HisRiseRate_back(string sharesCode, int market, List<string> par)
        {
            try
            {
                using (var db = new meal_ticketEntities())
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("declare @trendpar_table TrendPar;");
                    sql.AppendLine("declare @tempTri bit=0;");
                    foreach (var parStr in par)
                    {
                        sql.AppendLine("insert into @trendpar_table(Par)");
                        sql.AppendLine(string.Format("values('{0}');", parStr));
                    }
                    sql.AppendLine(string.Format("exec P_TradePar_Parse_HisRiseRate @trendpar_table,'{0}',{1},@tempTri output;", sharesCode, market));
                    sql.AppendLine("select @tempTri");

                    bool result = db.Database.SqlQuery<bool>(sql.ToString()).FirstOrDefault();
                    if (result)
                    {
                        return 0;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_ReferAverage异常", ex);
                return -1;
            }
        }

        //分析今日涨跌幅
        public static int Analysis_TodayRiseRate(string sharesCode, int market, List<string> par,StringBuilder logRecord)
        {
            string comeTime = string.Empty;
            string outTime = string.Empty;
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int type = 0;
                int compare = 0;
                long count = 0;
                bool triPrice = false;
                int firstDay = 0;
                int secondDay = 0;
                double multiple = 0;
                int compare2 = 0;
                int dayShortageType = 0;
                try
                {
                    type = Convert.ToInt32(temp.Type);
                }
                catch (Exception) { }
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                }
                catch (Exception ex) { }
                try
                {
                    count = (type == 5 || type == 6 || type==9) ? (int)(Convert.ToDouble(temp.Count) * 100) : Convert.ToInt64(temp.Count);
                }
                catch (Exception ex) { }
                try
                {
                    triPrice = Convert.ToBoolean(temp.TriPrice);
                }
                catch (Exception) { }
                try
                {
                    firstDay = Convert.ToInt32(temp.FirstDay);
                }
                catch (Exception) { }
                try
                {
                    secondDay = Convert.ToInt32(temp.SecondDay);
                }
                catch (Exception) { }
                try
                {
                    multiple = Convert.ToDouble(temp.Multiple);
                }
                catch (Exception) { }
                try
                {
                    compare2 = Convert.ToInt32(temp.Compare2);
                }
                catch (Exception) { }
                try
                {
                    dayShortageType = Convert.ToInt32(temp.DayShortageType);
                }
                catch (Exception) { }

                DateTime dateNow = DateTime.Now.Date;
                using (var db = new meal_ticketEntities())
                {
                    comeTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string sqlQuery = string.Format("select top 1 LimitUpCount,TriLimitUpCount,LimitDownCount,TriLimitDownCount,LimitUpBombCount,OpenedPrice,ClosedPrice,PresentPrice,MaxPrice,MinPrice,TotalAmount,TotalCount from t_shares_quotes_date with(nolock) where SharesCode='{0}' and Market={1} and LastModified>'{2}'", sharesCode, market, dateNow.ToString("yyyy-MM-dd"));
                    var quotes_date=db.Database.SqlQuery<QuotesDate>(sqlQuery).FirstOrDefault();
                   
                    outTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    if (quotes_date == null)
                    {
                        logRecord.AppendLine("\t\t条件不成立(五档数据不实时)");
                        return -1;
                    }
                    //涨停次数
                    if (type == 1)
                    {
                        int currCount = triPrice ? quotes_date.TriLimitUpCount : quotes_date.LimitUpCount;
                        string conditionDes = compare == 1 ? ("涨停次数大于等于" + count) : ("涨停次数小于等于" + count);
                        if ((compare == 1 && currCount >= count) || (compare == 2 && currCount <= count))
                        {
                            logRecord.AppendLine("\t\t条件成立(当前涨停次数："+ currCount + ",条件:"+ conditionDes + ")");
                            return 0;
                        }
                        logRecord.AppendLine("\t\t条件不成立(当前涨停次数：" + currCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //跌停次数
                    if (type == 2)
                    {
                        int currCount = triPrice ? quotes_date.TriLimitDownCount : quotes_date.LimitDownCount;
                        string conditionDes = compare == 1 ? ("跌停次数大于等于" + count) : ("跌停次数小于等于" + count);
                        if ((compare == 1 && currCount >= count) || (compare == 2 && currCount <= count))
                        {
                            logRecord.AppendLine("\t\t条件成立(当前跌停次数：" + currCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.AppendLine("\t\t条件不成立(当前跌停次数：" + currCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //炸板次数
                    else if (type == 3)
                    {
                        string conditionDes = compare == 1 ? ("炸板次数大于等于" + count) : ("炸板次数小于等于" + count);
                        if ((compare == 1 && quotes_date.LimitUpBombCount >= count) || (compare == 2 && quotes_date.LimitUpBombCount <= count))
                        {
                            logRecord.AppendLine("\t\t条件成立(当前炸板次数：" + quotes_date.LimitUpBombCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.AppendLine("\t\t条件不成立(当前炸板次数：" + quotes_date.LimitUpBombCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //盘中涨跌幅
                    else if (type == 4)
                    {
                        int i = -1;
                        foreach (var item in temp.riseRateList)
                        {
                            UP_OR_DOWN_INFO upOrDownInfo = new UP_OR_DOWN_INFO();
                            DB_TRADE_PRICE_INFO DB_TRADE_PRICE_INFO = new DB_TRADE_PRICE_INFO();
                            int Minute = item.Minute;
                            int Compare = item.Compare;
                            double Rate = item.Rate;
                            int iErrorCode = Singleton.Instance.m_stockMonitor.GetUpOrDownInfoInTime(sharesCode + "," + market, Minute, ref DB_TRADE_PRICE_INFO, ref upOrDownInfo);
                            if (iErrorCode != 0)
                            {
                                Logger.WriteFileLog(iErrorCode + "" + JsonConvert.SerializeObject(upOrDownInfo), null);
                                continue;
                            }
                            if (upOrDownInfo.iLastestPercent == -0x0FFFFFFF)
                            {
                                continue;
                            }
                            if (Compare == 1 && upOrDownInfo.iLastestPercent >= Rate * 100)
                            {
                                i++;
                                break;
                            }
                            if (Compare == 2 && upOrDownInfo.iLastestPercent <= Rate * 100)
                            {
                                i++;
                                break;
                            }
                        }
                        if (i == 0)
                        {
                            logRecord.AppendLine("\t\t条件成立(符合盘中涨跌幅条件)");
                        }
                        else
                        {
                            logRecord.AppendLine("\t\t条件不成立(不符合盘中涨跌幅条件)");
                        }
                        return i;
                    }
                    //开盘涨跌幅
                    else if (type == 5)
                    {
                        long openPrice = quotes_date.OpenedPrice;
                        long closePrice = quotes_date.ClosedPrice;
                        if (closePrice <= 0 || openPrice <= 0)
                        {
                            return -1;
                        }
                        int rate = (int)((openPrice - closePrice) * 1.0 / closePrice * 10000);
                        string conditionDes = compare == 1 ? ("开盘涨跌幅大于等于" + count) : ("开盘涨跌幅小于等于" + count);
                        if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                        {
                            logRecord.AppendLine("\t\t条件成立(当前开盘涨跌幅：" + rate + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.AppendLine("\t\t条件不成立(当前开盘涨跌幅：" + rate + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //振幅
                    else if (type == 6)
                    {
                        long maxPrice = quotes_date.MaxPrice;
                        long minPrice = quotes_date.MinPrice;
                        long closePrice = quotes_date.ClosedPrice;
                        if (closePrice <= 0 || maxPrice <= 0 || minPrice <= 0)
                        {
                            return -1;
                        }
                        int rate = (int)((maxPrice - minPrice) * 1.0 / closePrice * 10000);
                        string conditionDes = compare == 1 ? ("振幅大于等于" + count) : ("振幅小于等于" + count);
                        if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                        {
                            logRecord.AppendLine("\t\t条件成立(当前振幅：" + rate + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.AppendLine("\t\t条件不成立(当前振幅：" + rate + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //成交额
                    else if (type == 7)
                    {
                        var quotesCount = (from item in db.t_shares_quotes_date
                                           where item.SharesCode == sharesCode && item.Market == market && item.LastModified < dateNow
                                           select item).Count();
                        if (dayShortageType == 2 && quotesCount < firstDay)
                        {
                            logRecord.AppendLine("\t\t条件成立(符合成交额条件)");
                            return 0;
                        }
                        if (dayShortageType == 3 && quotesCount < firstDay)
                        {
                            logRecord.AppendLine("\t\t条件不成立(不符合成交额条件)");
                            return -1;
                        }
                        //查询前几个交易日平均交易额
                        string sql = string.Format(@"select convert(bigint,avg(TotalAmount)) AvgTotalAmount
  from
  (
	select [Date],TotalAmount,row_number()over(order by [Date] desc) num
	from t_shares_quotes_date with(nolock)
	where LastModified<'{0}' and SharesCode='{1}' and Market={2}
  )t
  where t.num>={3} and t.num<={4}", dateNow.ToString("yyyy-MM-dd"), sharesCode, market, secondDay, firstDay);
                        long avgResult = db.Database.SqlQuery<long>(sql).FirstOrDefault();
                        if ((compare == 1 && quotes_date.TotalAmount >= avgResult * multiple) || (compare == 2 && quotes_date.TotalAmount <= avgResult * multiple))
                        {
                            if ((compare2 == 1 && avgResult >= count * 10000) || (compare2 == 2 && avgResult <= count * 10000))
                            {
                                logRecord.AppendLine("\t\t条件成立(符合成交额条件)");
                                return 0;
                            }
                        }
                        logRecord.AppendLine("\t\t条件不成立(不符合成交额条件)");
                        return -1;
                    }
                    //成交量
                    else if (type == 8)
                    {
                        var quotesCount = (from item in db.t_shares_quotes_date
                                           where item.SharesCode == sharesCode && item.Market == market && item.LastModified < dateNow
                                           select item).Count();
                        if (dayShortageType == 2 && quotesCount < firstDay)
                        {
                            logRecord.AppendLine("\t\t条件成立(符合成交量条件)");
                            return 0;
                        }
                        if (dayShortageType == 3 && quotesCount < firstDay)
                        {
                            logRecord.AppendLine("\t\t条件不成立(不符合成交量条件)");
                            return -1;
                        }
                        //查询前几个交易日平均成交量
                        string sql = string.Format(@"select convert(bigint,avg(TotalCount)) AvgTotalCount
  from
  (
	select [Date],TotalCount,row_number()over(order by [Date] desc) num
	from t_shares_quotes_date with(nolock)
	where LastModified<'{0}' and SharesCode='{1}' and Market={2}
  )t
  where t.num>={3} and t.num<={4}", dateNow.ToString("yyyy-MM-dd"), sharesCode, market, secondDay, firstDay);
                        long avgResult = db.Database.SqlQuery<long>(sql).FirstOrDefault();
                        if ((compare == 1 && quotes_date.TotalCount >= avgResult * multiple) || (compare == 2 && quotes_date.TotalCount <= avgResult * multiple))
                        {
                            if ((compare2 == 1 && avgResult * 100 >= count) || (compare2 == 2 && avgResult * 100 <= count))
                            {
                                logRecord.AppendLine("\t\t条件成立(符合成交量条件)");
                                return 0;
                            }
                        }
                        logRecord.AppendLine("\t\t条件不成立(不符合成交量条件)");
                        return -1;
                    }
                    //最新涨跌幅
                    else if (type == 9)
                    {
                        long currPrice = quotes_date.PresentPrice;
                        long closePrice = quotes_date.ClosedPrice;
                        if (closePrice <= 0 || currPrice <= 0)
                        {
                            return -1;
                        }
                        int rate = (int)((currPrice - closePrice) * 1.0 / closePrice * 10000);
                        string conditionDes = compare == 1 ? ("最新涨跌幅大于等于" + count) : ("最新涨跌幅小于等于" + count);
                        if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                        {
                            logRecord.AppendLine("\t\t条件成立(当前最新涨跌幅：" + rate + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.AppendLine("\t\t条件不成立(当前最新涨跌幅：" + rate + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    else
                    {
                        logRecord.AppendLine("\t\t条件不成立(参数type错误)");
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog(string.Format("分析今日涨跌幅出错了-{0}-{1}-{2}", sharesCode, comeTime, outTime), ex);
                logRecord.AppendLine("\t\t条件不成立(异常错误)");
                return -1;
            }
        }

        //分析板块涨跌幅
        public static int Analysis_PlateRiseRate(string sharesCode, int market, List<string> par,StringBuilder logRecord)
        {
            try
            {
                using (var db = new meal_ticketEntities())
                {
                    //查询股票所属板块及板块涨跌幅
                    var plateRate = (from item in db.t_shares_plate_rel
                                     join item2 in db.t_shares_plate_riserate_last on item.PlateId equals item2.PlateId
                                     join item3 in db.t_shares_plate on item.PlateId equals item3.Id
                                     where item.Market == market && item.SharesCode == sharesCode
                                     select new
                                     {
                                         SharesCount=item2.SharesCount,
                                         PlateId=item.PlateId,
                                         RiseRate=item2.RiseRate,
                                         PlateType=item3.Type,
                                     }).ToList();
                    List<long> AllPlateId = plateRate.Select(e => e.PlateId).ToList();

                    var plateBase = (from item in db.t_shares_plate
                                     where item.BaseStatus == 1 && AllPlateId.Contains(item.Id)
                                     select item).ToList();


                    List<long> PlateList1 = new List<long>();
                    List<long> PlateList2 = new List<long>();
                    List<string> AllSetPar = new List<string>();
                    foreach (var p in par)
                    {
                        var temp = JsonConvert.DeserializeObject<dynamic>(p);
                        long groupId = -1;
                        int dataType = 1;
                        int compare = 0;
                        double rate = 0;
                        try
                        {
                            groupId = temp.GroupId;
                        }
                        catch (Exception ex){}
                        try
                        {
                            compare = temp.Compare;
                        }
                        catch (Exception ex) { }
                        try
                        {
                            rate = temp.Rate;
                        }
                        catch (Exception ex) { }
                        try
                        {
                            dataType = temp.DataType;
                        }
                        catch (Exception ex) { }

                        if (groupId == 0)
                        {
                            AllSetPar.Add(p);
                        }
                        var plate = plateRate.Where(e => e.PlateId == groupId).FirstOrDefault();
                        if (dataType == 2)
                        {
                            if (plate == null)
                            {
                                continue;
                            }
                            if (compare == 1 && plate.RiseRate < rate * 100)
                            {
                                continue;
                            }
                            if (compare == 2 && plate.RiseRate > rate * 100)
                            {
                                continue;
                            }
                            PlateList2.Add(plate.PlateId);
                        }
                        else
                        {
                            if (groupId == -1)//行业基础版块
                            {
                                PlateList1.AddRange(plateBase.Where(e=>e.Type==1).Select(e=>e.Id).ToList());
                            }
                            else if (groupId == -2)//地区基础版块
                            {
                                PlateList1.AddRange(plateBase.Where(e => e.Type == 2).Select(e => e.Id).ToList());
                            }
                            else if (groupId == -3)//概念基础版块
                            {
                                PlateList1.AddRange(plateBase.Where(e => e.Type == 3).Select(e => e.Id).ToList());
                            }
                            else
                            {
                                if (plate == null)
                                {
                                    continue;
                                }
                                PlateList1.Add(plate.PlateId);
                            }
                        }
                    }
                    PlateList1 = PlateList1.Distinct().ToList();

                    foreach (var p in AllSetPar)
                    {
                        var temp = JsonConvert.DeserializeObject<dynamic>(p);
                        int groupType = temp.GroupType;
                        int tempCompare = temp.Compare;
                        double tempRate = temp.Rate;
                        var plate = plateRate.Where(e => e.PlateType == groupType && ((tempCompare == 1 && e.RiseRate >= tempRate * 100) || (tempCompare == 2 && e.RiseRate <= tempRate * 100))).Select(e => e.PlateId).ToList();
                        if (plate.Count() <= 0)
                        {
                            continue;
                        }
                        PlateList2.AddRange(plate);
                    }
                    PlateList2 = PlateList2.Distinct().ToList();

                    var newList = PlateList1.Intersect(PlateList2);
                    if (newList.Count() > 0)
                    {
                        logRecord.AppendLine("\t\t条件成立(符合板块涨跌幅条件)");
                        return 0;
                    }
                    logRecord.AppendLine("\t\t条件不成立(不符合板块涨跌幅条件)");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析板块涨跌幅出错了", ex);
                logRecord.AppendLine("\t\t条件成立(异常错误)");
                return -1;
            }
        }

        public static int Analysis_PlateRiseRate_back(string sharesCode, int market, List<string> par)
        {
            try
            {
                using (var db = new meal_ticketEntities())
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("declare @trendpar_table TrendPar;");
                    sql.AppendLine("declare @tempTri bit=0;");
                    foreach (var parStr in par)
                    {
                        sql.AppendLine("insert into @trendpar_table(Par)");
                        sql.AppendLine(string.Format("values('{0}');", parStr));
                    }
                    sql.AppendLine(string.Format("exec P_TradePar_Parse_PlateRiseRate @trendpar_table,'{0}',{1},@tempTri output;", sharesCode, market));
                    sql.AppendLine("select @tempTri");

                    bool result = db.Database.SqlQuery<bool>(sql.ToString()).FirstOrDefault();
                    if (result)
                    {
                        return 0;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_PlateRiseRate异常", ex);
                return -1;
            }
        }

        //分析买卖单占比
        public static int Analysis_BuyOrSellCount(string sharesCode, int market, List<string> par,StringBuilder logRecord)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int type = 0;
                int compare = 0;
                double count = 0;
                int countType = 0;
                try
                {
                    type = Convert.ToInt32(temp.Type);
                    compare = Convert.ToInt32(temp.Compare);
                    count = Convert.ToDouble(temp.Count);
                    countType = Convert.ToInt32(temp.CountType);
                }
                catch (Exception)
                {
                    logRecord.AppendLine("\t\t条件不成立(参数错误)");
                    return -1;
                }
                DateTime timeNow = DateTime.Now;
                using (var db = new meal_ticketEntities())
                {
                    var quotes = (from item in db.v_shares_quotes_last
                                  where item.Market == market && item.SharesCode == sharesCode && SqlFunctions.DateAdd("MI", 1, item.LastModified) > timeNow
                                  select item).FirstOrDefault();
                    if (quotes == null)
                    {
                        return -1;
                    }
                    int realCount;
                    string realCountDes;
                    switch (type)
                    {
                        case 1:
                            realCount = quotes.BuyCount1;
                            realCountDes = "买一挂单";
                            break;
                        case 2:
                            realCount = quotes.BuyCount2;
                            realCountDes = "买二挂单";
                            break;
                        case 3:
                            realCount = quotes.BuyCount3;
                            realCountDes = "买三挂单";
                            break;
                        case 4:
                            realCount = quotes.BuyCount4;
                            realCountDes = "买四挂单";
                            break;
                        case 5:
                            realCount = quotes.BuyCount5;
                            realCountDes = "买五挂单";
                            break;
                        case 6:
                            realCount = quotes.SellCount1;
                            realCountDes = "卖一挂单";
                            break;
                        case 7:
                            realCount = quotes.SellCount2;
                            realCountDes = "卖二挂单";
                            break;
                        case 8:
                            realCount = quotes.SellCount3;
                            realCountDes = "卖三挂单";
                            break;
                        case 9:
                            realCount = quotes.SellCount4;
                            realCountDes = "卖四挂单";
                            break;
                        case 10:
                            realCount = quotes.SellCount5;
                            realCountDes = "卖五挂单";
                            break;
                        default:
                            logRecord.AppendLine("\t\t条件不成立(参数type错误)");
                            return -1;
                    }
                    realCount = realCount * 100;

                    if (countType == 1)//流通股百分比
                    {
                        //查询流通股
                        var sharesInfo = (from item in db.t_shares_markettime
                                          where item.Market == market && item.SharesCode == sharesCode
                                          select item).FirstOrDefault();
                        if (sharesInfo == null)
                        {
                            logRecord.AppendLine("\t\t条件不成立(流通股本数据不存在)");
                            return -1;
                        }
                        long circulatingCapital = sharesInfo.CirculatingCapital;
                        long setCount = (long)(circulatingCapital * (count / 100));

                        string conditionDes = realCountDes + (compare == 1 ? ("大于等于" + setCount) : ("小于等于" + setCount));
                        if ((compare == 1 && realCount >= setCount) || (compare == 2 && realCount <= setCount))
                        {
                            logRecord.AppendLine("\t\t条件成立(当前挂单:"+ realCount + ",条件:"+ conditionDes + ")");
                            return 0;
                        }
                        logRecord.AppendLine("\t\t条件不成立(当前挂单:" + realCount + ",条件:" + conditionDes + ")");
                    }
                    else if (countType == 2)//股数
                    {
                        string conditionDes = realCountDes + (compare == 1 ? ("大于等于" + count) : ("小于等于" + count));
                        if ((compare == 1 && realCount >= count) || (compare == 2 && realCount <= count))
                        {
                            logRecord.AppendLine("\t\t条件成立(当前挂单:" + realCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logRecord.AppendLine("\t\t条件不成立(当前挂单:" + realCount + ",条件:" + conditionDes + ")");
                    }
                    else if (countType == 3)//流通市值百分比
                    {

                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析买卖单占比出错了", ex);
                logRecord.AppendLine("\t\t条件不成立(异常错误)");
                return -1;
            }
        }

        public static int Analysis_BuyOrSellCount_back(string sharesCode, int market, List<string> par)
        {
            try
            {
                using (var db = new meal_ticketEntities())
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("declare @trendpar_table TrendPar;");
                    sql.AppendLine("declare @tempTri bit=0;");
                    foreach (var parStr in par)
                    {
                        sql.AppendLine("insert into @trendpar_table(Par)");
                        sql.AppendLine(string.Format("values('{0}');", parStr));
                    }
                    sql.AppendLine(string.Format("exec P_TradePar_Parse_BuyOrSellCount @trendpar_table,'{0}',{1},@tempTri output;", sharesCode, market));
                    sql.AppendLine("select @tempTri");

                    bool result = db.Database.SqlQuery<bool>(sql.ToString()).FirstOrDefault();
                    if (result)
                    {
                        return 0;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_ReferAverage异常", ex);
                return -1;
            }
        }

        //分析按参照价格
        public static int Analysis_ReferPrice(string sharesCode, int market, List<string> par,StringBuilder logRecord)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int compare = 0;
                int day = 0;
                double count = 0;
                int priceType = 0;
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                    day = Convert.ToInt32(temp.Day);
                    count = Convert.ToDouble(temp.Count);
                    priceType = Convert.ToInt32(temp.PriceType);
                }
                catch (Exception)
                {
                    logRecord.AppendLine("\t\t条件不成立(参数错误)");
                    return -1;
                }
                DateTime timeNow = DateTime.Now;
                using (var db = new meal_ticketEntities())
                {
                    var quotes = (from item in db.t_shares_quotes_date
                                  where item.Market == market && item.SharesCode == sharesCode
                                  orderby item.Date descending
                                  select item).Take(day + 1).ToList();
                    if (quotes.Count() <= 0)
                    {
                        logRecord.AppendLine("\t\t条件不成立(五档行情有误)");
                        return -1;
                    }
                    var presentInfo = quotes.OrderByDescending(e => e.Date).FirstOrDefault();
                    if (presentInfo.LastModified < timeNow.AddMinutes(-1))
                    {
                        logRecord.AppendLine("\t\t条件不成立(五档行情不实时)");
                        return -1;
                    }
                    long presentPrice = presentInfo.PresentPrice;
                    if (presentPrice <= 0)
                    {
                        logRecord.AppendLine("\t\t条件不成立(五档行情价格有误)");
                        return -1;
                    }

                    var lastInfo = quotes.Where(e => e.LastModified < timeNow.Date).ToList();
                    long realPrice;
                    switch (priceType)
                    {
                        case 1:
                            realPrice = compare == 1 ? lastInfo.Max(e => e.OpenedPrice) : lastInfo.Min(e => e.OpenedPrice);
                            break;
                        case 2:
                            realPrice = compare == 1 ? lastInfo.Max(e => e.PresentPrice) : lastInfo.Min(e => e.PresentPrice);
                            break;
                        case 3:
                            realPrice = compare == 1 ? lastInfo.Max(e => e.MaxPrice) : lastInfo.Min(e => e.MaxPrice);
                            break;
                        case 4:
                            realPrice = compare == 1 ? lastInfo.Max(e => e.MinPrice) : lastInfo.Min(e => e.MinPrice);
                            break;
                        default:
                            logRecord.AppendLine("\t\t条件不成立(参数priceType错误)");
                            return -1;
                    }

                    if (count != 0)//计算偏差
                    {
                        realPrice = (long)(realPrice * (count / 100) + realPrice);
                    }

                    string conditionDes =compare == 1 ? ("当前价格大于等于" + realPrice) : ("当前价格小于等于" + realPrice);
                    if ((compare == 1 && presentPrice >= realPrice) || (compare == 2 && presentPrice <= realPrice))
                    {
                        logRecord.AppendLine("\t\t条件成立(当前价格:" + presentPrice + ",条件:" + conditionDes + ")");
                        return 0;
                    }
                    logRecord.AppendLine("\t\t条件不成立(当前价格:" + presentPrice + ",条件:" + conditionDes + ")");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按参照价格出错了", ex);
                logRecord.AppendLine("\t\t条件不成立(异常错误)");
                return -1;
            }
        }

        public static int Analysis_ReferPrice_back(string sharesCode, int market, List<string> par)
        {
            try
            {
                using (var db = new meal_ticketEntities())
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("declare @trendpar_table TrendPar;");
                    sql.AppendLine("declare @tempTri bit=0;");
                    foreach (var parStr in par)
                    {
                        sql.AppendLine("insert into @trendpar_table(Par)");
                        sql.AppendLine(string.Format("values('{0}');", parStr));
                    }
                    sql.AppendLine(string.Format("exec P_TradePar_Parse_ReferPrice @trendpar_table,'{0}',{1},@tempTri output;", sharesCode, market));
                    sql.AppendLine("select @tempTri");

                    bool result = db.Database.SqlQuery<bool>(sql.ToString()).FirstOrDefault();
                    if (result)
                    {
                        return 0;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_ReferAverage异常", ex);
                return -1;
            }
        }

        //分析按均线价格
        public static int Analysis_ReferAverage(string sharesCode, int market, List<string> par,StringBuilder logRecord)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int compare = 0;
                int day1 = 0;
                int day2 = 0;
                double count = 0;
                int upOrDown = 0;
                int dayShortageType = 1;
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                    day1 = Convert.ToInt32(temp.Day1);
                    count = Convert.ToDouble(temp.Count);
                }
                catch (Exception)
                {
                    logRecord.AppendLine("\t\t条件不成立(参数错误)");
                    return -1;
                }
                try
                {
                    day2 = Convert.ToInt32(temp.Day2);
                    day2 = day2 + 1;//多取一天
                    upOrDown = Convert.ToInt32(temp.UpOrDown);
                }
                catch (Exception)
                {

                }
                try
                {
                    dayShortageType = Convert.ToInt32(temp.DayShortageType);
                }
                catch (Exception )
                {
                }

                DateTime timeNow = DateTime.Now;
                using (var db = new meal_ticketEntities())
                {
                    var quotes = (from item in db.t_shares_quotes_date
                                  where item.Market == market && item.SharesCode == sharesCode
                                  orderby item.Date descending
                                  select item).Take(day1 + day2).ToList();
                    var presentInfo = quotes.OrderByDescending(e => e.Date).FirstOrDefault();
                    if (presentInfo.LastModified < timeNow.AddMinutes(-1))
                    {
                        logRecord.AppendLine("\t\t条件不成立(五档行情不实时)");
                        return -1;
                    }
                    long presentPrice = presentInfo.PresentPrice;
                    if (presentPrice <= 0)
                    {
                        logRecord.AppendLine("\t\t条件不成立(五档行情价格有误)");
                        return -1;
                    }
                    //计算均线价格
                    var list1 = quotes.Take(day1).ToList();
                    if (list1.Count() <= 0)
                    {
                        logRecord.AppendLine("\t\t条件不成立(暂无均线)");
                        return -1;
                    }
                    if (list1.Count() < day1)
                    {
                        if (dayShortageType == 2)
                        {
                            logRecord.AppendLine("\t\t条件成立(均线不够返回true)");
                            return 0;
                        }
                        if (dayShortageType == 3)
                        {
                            logRecord.AppendLine("\t\t条件不成立(均线不够返回false)");
                            return -1;
                        }
                    }
                    long averagePrice = (long)list1.Average(e => e.PresentPrice);
                    if (count != 0)//计算偏差
                    {
                        averagePrice = (long)(averagePrice * (count / 100) + averagePrice);
                    }

                    string conditionDes = compare == 1 ? ("当前价格大于等于" + averagePrice) : ("当前价格小于等于" + averagePrice);
                    if ((compare == 1 && presentPrice >= averagePrice) || (compare == 2 && presentPrice <= averagePrice))
                    {
                        if (day2 >= 2)//计算每日均线
                        {
                            long lastAveragePrice = 0;
                            for (int i = 0; i < day2; i++)
                            {
                                long tempAveragePrice;
                                var list2 = quotes.Skip(i).Take(day1).ToList();
                                if (list2.Count() <= 0)
                                {
                                    tempAveragePrice = 0;
                                }
                                else
                                {
                                    tempAveragePrice = (long)list2.Average(e => e.PresentPrice);
                                }
                                if (upOrDown == 1)
                                {
                                    //向上，则当前必须<=前一个数据
                                    if (tempAveragePrice > lastAveragePrice && lastAveragePrice != 0)
                                    {
                                        logRecord.AppendLine("\t\t条件不成立(均线非连续向上)");
                                        return -1;
                                    }
                                    lastAveragePrice = tempAveragePrice;
                                    continue;
                                }
                                else if (upOrDown == 2)
                                {
                                    //向下，则当前必须>=前一个数据
                                    if (tempAveragePrice < lastAveragePrice && lastAveragePrice != 0)
                                    {
                                        logRecord.AppendLine("\t\t条件不成立(均线非连续向下)");
                                        return -1;
                                    }
                                    lastAveragePrice = tempAveragePrice;
                                    continue;
                                }
                                else
                                {
                                    logRecord.AppendLine("\t\t条件不成立(参数upOrDown错误)");
                                }
                            }
                        }
                        logRecord.AppendLine("\t\t条件成立(当前价格:" + presentPrice + ",条件:" + conditionDes + ")");
                        return 0;
                    }
                    logRecord.AppendLine("\t\t条件不成立(当前价格:" + presentPrice + ",条件:" + conditionDes + ")");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按均线价格出错了", ex);
                logRecord.AppendLine("\t\t条件不成立(异常错误)");
                return -1;
            }
        }

        public static int Analysis_ReferAverage_back(string sharesCode, int market, List<string> par)
        {
            try
            {
                using (var db = new meal_ticketEntities())
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("declare @trendpar_table TrendPar;");
                    sql.AppendLine("declare @tempTri bit=0;");
                    foreach (var parStr in par)
                    {
                        sql.AppendLine("insert into @trendpar_table(Par)");
                        sql.AppendLine(string.Format("values('{0}');", parStr));
                    }
                    sql.AppendLine(string.Format("exec P_TradePar_Parse_ReferAverage @trendpar_table,'{0}',{1},@tempTri output;", sharesCode, market));
                    sql.AppendLine("select @tempTri");

                    bool result = db.Database.SqlQuery<bool>(sql.ToString()).FirstOrDefault();
                    if (result)
                    {
                        return 0;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_ReferAverage异常", ex);
                return -1;
            }
        }

        //分析买卖变化速度
        public static int Analysis_QuotesChangeRate(string sharesCode, int market,long currPrice ,List<string> par,StringBuilder logRecord)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
            int compare = 0;
            int type = 0;
            double count = 0;
            int othercompare = 0;
            try
            {
                compare = Convert.ToInt32(temp.Compare);
                type = Convert.ToInt32(temp.Type);
                count = Convert.ToDouble(temp.Count);
            }
            catch (Exception)
            {
                logRecord.AppendLine("\t\t条件不成立(参数错误)");
                return -1;
            }
            try
            {
                othercompare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception)
            {
            }

            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var quotes = (from item in db.t_shares_quotes
                              where item.Market == market && item.SharesCode == sharesCode && SqlFunctions.DateAdd("MI", 1, item.LastModified) > timeNow
                              select item).ToList();
                var quotesLast = quotes.Where(e => e.DataType == 0).FirstOrDefault();
                var quotesPre = quotes.Where(e => e.DataType == 1).FirstOrDefault();
                if (quotesLast == null || quotesPre == null)
                {
                    logRecord.AppendLine("\t\t条件不成立(t五档数据有误)");
                    return -1;
                }
               
                int disCount = 0;
                long disPrice = 0;
                int lastCount = 0;
                long lastPrice = 0;
                int tradetype = 0;

                switch (type)
                {
                    case 1:
                        disCount = quotesLast.BuyCount1;
                        disPrice = quotesLast.BuyPrice1;
                        lastCount = quotesPre.BuyCount1;
                        lastPrice = quotesPre.BuyPrice1;
                        tradetype = 1;
                        break;
                    case 2:
                        disCount = quotesLast.BuyCount2;
                        disPrice = quotesLast.BuyPrice2;
                        lastCount = quotesPre.BuyCount2;
                        lastPrice = quotesPre.BuyPrice2;
                        tradetype = 1;
                        break;
                    case 3:
                        disCount = quotesLast.BuyCount3;
                        disPrice = quotesLast.BuyPrice3;
                        lastCount = quotesPre.BuyCount3;
                        lastPrice = quotesPre.BuyPrice3;
                        tradetype = 1;
                        break;
                    case 4:
                        disCount = quotesLast.BuyCount4;
                        disPrice = quotesLast.BuyPrice4;
                        lastCount = quotesPre.BuyCount4;
                        lastPrice = quotesPre.BuyPrice4;
                        tradetype = 1;
                        break;
                    case 5:
                        disCount = quotesLast.BuyCount5;
                        disPrice = quotesLast.BuyPrice5;
                        lastCount = quotesPre.BuyCount5;
                        lastPrice = quotesPre.BuyPrice5;
                        tradetype = 1;
                        break;
                    case 6:
                        disCount = quotesLast.SellCount1;
                        disPrice = quotesLast.SellPrice1;
                        lastCount = quotesPre.SellCount1;
                        lastPrice = quotesPre.SellPrice1;
                        tradetype = 2;
                        break;
                    case 7:
                        disCount = quotesLast.SellCount2;
                        disPrice = quotesLast.SellPrice2;
                        lastCount = quotesPre.SellCount2;
                        lastPrice = quotesPre.SellPrice2;
                        tradetype = 2;
                        break;
                    case 8:
                        disCount = quotesLast.SellCount3;
                        disPrice = quotesLast.SellPrice3;
                        lastCount = quotesPre.SellCount3;
                        lastPrice = quotesPre.SellPrice3;
                        tradetype = 2;
                        break;
                    case 9:
                        disCount = quotesLast.SellCount4;
                        disPrice = quotesLast.SellPrice4;
                        lastCount = quotesPre.SellCount4;
                        lastPrice = quotesPre.SellPrice4;
                        tradetype = 2;
                        break;
                    case 10:
                        disCount = quotesLast.SellCount5;
                        disPrice = quotesLast.SellPrice5;
                        lastCount = quotesPre.SellCount5;
                        lastPrice = quotesPre.SellPrice5;
                        tradetype = 2;
                        break;
                    default:
                        return -1;
                }

                if ((othercompare == 1 && disPrice < currPrice) || (othercompare == 2 && disPrice > currPrice))
                {
                    return -1;
                }
                if (lastCount <= 0 || lastPrice <= 0)
                {
                    return -1;
                }

                double rate = 0;
                //当前价格等于0，速率为-100%
                if (disPrice <= 0)
                {
                    rate = -100;
                }
                //价格相等，计算速率
                else if (lastPrice == disPrice)
                {
                    rate = (disCount - lastCount) * 1.0 / lastCount * 100;
                }
                else if (lastPrice > disPrice)
                {
                    //卖单则为无限大（999999999）
                    if (tradetype == 2)
                    {
                        rate = 999999999;
                    }
                    else
                    {
                        rate = -100;
                    }
                }
                else
                {
                    //买单则为无限大（999999999）
                    if (tradetype == 1)
                    {
                        rate = 999999999;
                    }
                    else
                    {
                        rate = -100;
                    }
                }

                if (compare == 1 && rate >= count)
                {
                    return 0;
                }
                if (compare == 2 && rate <= count)
                {
                    return 0;
                }
                return -1;
            }
        }

        public static int Analysis_QuotesChangeRate_back(string sharesCode, int market, List<string> par)
        {
            try
            {
                using (var db = new meal_ticketEntities())
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("declare @trendpar_table TrendPar;");
                    sql.AppendLine("declare @tempTri bit=0;");
                    foreach (var parStr in par)
                    {
                        sql.AppendLine("insert into @trendpar_table(Par)");
                        sql.AppendLine(string.Format("values('{0}');", parStr));
                    }
                    sql.AppendLine(string.Format("exec P_TradePar_Parse_QuotesChangeRate @trendpar_table,'{0}',{1},@tempTri output;", sharesCode, market));
                    sql.AppendLine("select @tempTri");

                    bool result = db.Database.SqlQuery<bool>(sql.ToString()).FirstOrDefault();
                    if (result)
                    {
                        return 0;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_QuotesChangeRate异常", ex);
                return -1;
            }
        }

        //分析五档变化速度
        public static int Analysis_QuotesTypeChangeRate(string sharesCode, int market, long currPrice, List<string> par,StringBuilder logRecord)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int compare = 0;
                int type = 0;
                double count = 0;
                int priceType = 1;
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                    type = Convert.ToInt32(temp.Type);
                    count = Convert.ToDouble(temp.Count);
                    priceType = Convert.ToInt32(temp.PriceType);
                }
                catch (Exception)
                {
                }
                if (type == 0)
                {
                    logRecord.AppendLine("\t\t条件不成立(参数错误)");
                    return -1;
                }

                DateTime timeNow = DateTime.Now;
                using (var db = new meal_ticketEntities())
                {
                    var quotes = (from item in db.t_shares_quotes
                                  where item.Market == market && item.SharesCode == sharesCode && SqlFunctions.DateAdd("MI", 1, item.LastModified) > timeNow
                                  select item).ToList();
                    var quotesLast = quotes.Where(e => e.DataType == 0).FirstOrDefault();
                    var quotesPre = quotes.Where(e => e.DataType == 1).FirstOrDefault();
                    if (quotesLast == null || quotesPre == null)
                    {
                        logRecord.AppendLine("\t\t条件不成立(五档行情数据有误)");
                        return -1;
                    }

                    if (priceType == 2)
                    {
                        currPrice = quotesLast.LimitUpPrice;
                    }
                    else if (priceType == 3)
                    {
                        currPrice = quotesLast.LimitDownPrice;
                    }

                    int disCount = 0;
                    int lastCount = 0;

                    double rate = 0;
                    switch (type)
                    {
                        case 1:
                            if (quotesPre.BuyPrice1 == currPrice)
                            {
                                lastCount = quotesPre.BuyCount1;
                            }
                            else if (quotesPre.BuyPrice2 == currPrice)
                            {
                                lastCount = quotesPre.BuyCount2;
                            }
                            else if (quotesPre.BuyPrice3 == currPrice)
                            {
                                lastCount = quotesPre.BuyCount3;
                            }
                            else if (quotesPre.BuyPrice4 == currPrice)
                            {
                                lastCount = quotesPre.BuyCount4;
                            }
                            else if (quotesPre.BuyPrice5 == currPrice)
                            {
                                lastCount = quotesPre.BuyCount5;
                            }
                            else
                            {
                                return -1;
                            }
                            if (quotesLast.BuyPrice1 == currPrice)
                            {
                                disCount = quotesLast.BuyCount1;
                            }
                            else if (quotesLast.BuyPrice2 == currPrice)
                            {
                                disCount = quotesLast.BuyCount2;
                            }
                            else if (quotesLast.BuyPrice3 == currPrice)
                            {
                                disCount = quotesLast.BuyCount3;
                            }
                            else if (quotesLast.BuyPrice4 == currPrice)
                            {
                                disCount = quotesLast.BuyCount4;
                            }
                            else if (quotesLast.BuyPrice5 == currPrice)
                            {
                                disCount = quotesLast.BuyCount5;
                            }
                            else
                            {
                                if (quotesLast.BuyPrice1 > currPrice)
                                {
                                    rate = 0x0FFFFFFF;
                                }
                                else
                                {
                                    rate = -100;
                                }
                            }
                            break;
                        case 2:
                            if (quotesPre.SellPrice1 == currPrice)
                            {
                                lastCount = quotesPre.SellCount1;
                            }
                            else if (quotesPre.SellPrice2 == currPrice)
                            {
                                lastCount = quotesPre.SellCount2;
                            }
                            else if (quotesPre.SellPrice3 == currPrice)
                            {
                                lastCount = quotesPre.SellCount3;
                            }
                            else if (quotesPre.SellPrice4 == currPrice)
                            {
                                lastCount = quotesPre.SellCount4;
                            }
                            else if (quotesPre.SellPrice5 == currPrice)
                            {
                                lastCount = quotesPre.SellCount5;
                            }
                            else
                            {
                                return -1;
                            }
                            if (quotesLast.SellPrice1 == currPrice)
                            {
                                disCount = quotesLast.SellCount1;
                            }
                            else if (quotesLast.SellPrice2 == currPrice)
                            {
                                disCount = quotesLast.SellCount2;
                            }
                            else if (quotesLast.SellPrice3 == currPrice)
                            {
                                disCount = quotesLast.SellCount3;
                            }
                            else if (quotesLast.SellPrice4 == currPrice)
                            {
                                disCount = quotesLast.SellCount4;
                            }
                            else if (quotesLast.SellPrice5 == currPrice)
                            {
                                disCount = quotesLast.SellCount5;
                            }
                            else
                            {
                                if (quotesLast.SellPrice1 > currPrice)
                                {
                                    rate = -100;
                                }
                                else
                                {
                                    rate = 0x0FFFFFFF;
                                }
                            }
                            break;
                        default:
                            return -1;
                    }

                    if (rate == 0)
                    {
                        if (lastCount == 0)
                        {
                            logRecord.AppendLine("\t\t条件不成立(上一次五档找不到目标价格)");
                            return -1;
                        }
                        rate = (disCount - lastCount) * 1.0 / lastCount * 100;
                    }


                    if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                    {
                        logRecord.AppendLine("\t\t条件成立(目标价在上一次五档挂单数量:" + lastCount + ",在本次五档挂单数量:" + disCount + ")");
                        return 0;
                    }
                    logRecord.AppendLine("\t\t条件不成立(目标价在上一次五档挂单数量:" + lastCount + ",在本次五档挂单数量:" + disCount + ")");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析五档变化速度出错了", ex);
                logRecord.AppendLine("\t\t条件不成立(异常错误)");
                return -1;
            }
        }

        //分析按当前价格
        public static int Analysis_CurrentPrice(string sharesCode, int market, List<string> par, StringBuilder logRecord)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
            int riseType = 0;
            int compare = 0;
            long count = 0;
            try
            {
                riseType = Convert.ToInt32(temp.RiseType);
            }
            catch (Exception)
            {
                logRecord.Append("\t\t条件不成立(参数配置错误)");
                return -1;
            }
            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception)
            {
            }
            try
            {
                count = (long)(Convert.ToDouble(temp.Count)*10000);
            }
            catch (Exception)
            {
            }

            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                //当前价格
                var quotes = (from item in db.v_shares_quotes_last
                              where item.Market == market && item.SharesCode == sharesCode && SqlFunctions.DateAdd("MI", 1, item.LastModified) > timeNow
                              select item).FirstOrDefault();
                if (quotes == null)
                {
                    logRecord.Append("\t\t条件不成立(五档行情不实时)");
                    return -1;
                }

                if (riseType == 1 && quotes.TriPriceType == 1)
                {
                    logRecord.Append("\t\t条件成立(当前触发涨停,条件触发涨停)");
                    return 0;
                }

                if (riseType == 2 && quotes.TriPriceType == 2)
                {
                    logRecord.Append("\t\t条件成立(当前触发跌停,条件触发跌停)");
                    return 0;
                }

                if (riseType == 3 && quotes.PriceType == 1)
                {
                    logRecord.Append("\t\t条件成立(当前涨停封板,条件涨停封板)");
                    return 0;
                }

                if (riseType == 4 && quotes.PriceType == 2)
                {
                    logRecord.Append("\t\t条件成立(当前跌停封板,条件跌停封板)");
                    return 0;
                }
                if (riseType == 5)
                {
                    string conditionDes = compare == 1 ? ("价格大于等于"+ count) : ("价格小于等于" + count);
                    if ((compare == 1 && quotes.PresentPrice >= count) || (compare == 2 && quotes.PresentPrice <= count))
                    {
                        logRecord.Append("\t\t条件成立(当前价格:"+ quotes.PresentPrice + ",条件:"+ conditionDes + ")");
                        return 0;
                    }
                    logRecord.Append("\t\t条件不成立(当前价格:" + quotes.PresentPrice + ",条件:" + conditionDes + ")");
                }
                if (riseType == 6)
                {
                    //查询流通股
                    var sharesInfo = (from item in db.t_shares_markettime
                                      where item.Market == market && item.SharesCode == sharesCode
                                      select item).FirstOrDefault();
                    if (sharesInfo == null)
                    {
                        logRecord.Append("\t\t条件不成立(流通股本数据不存在)");
                        return -1;
                    }
                    long circulatingCapital = sharesInfo.CirculatingCapital;
                    string conditionDes = compare==1? ("流通市值大于等于"+ count): ("流通市值小于等于" + count);
                    if ((compare == 1 && quotes.PresentPrice* circulatingCapital >= count) || (compare == 2 && quotes.PresentPrice * circulatingCapital <= count))
                    {
                        logRecord.Append("\t\t条件成立(当前流通市值:"+ quotes.PresentPrice * circulatingCapital + ",条件:"+ conditionDes + ")");
                        return 0;
                    }
                    logRecord.Append("\t\t条件不成立(当前流通市值:" + quotes.PresentPrice * circulatingCapital + ",条件:" + conditionDes + ")");
                }
                if (riseType == 7)
                {
                    //查询总股本
                    var sharesInfo = (from item in db.t_shares_markettime
                                      where item.Market == market && item.SharesCode == sharesCode
                                      select item).FirstOrDefault();
                    if (sharesInfo == null)
                    {
                        logRecord.Append("\t\t条件不成立(总股本数据不存在)");
                        return -1;
                    }
                    long totalCapital = sharesInfo.TotalCapital;
                    string conditionDes = compare == 1 ? ("总市值大于等于" + count) : ("总市值小于等于" + count);
                    if ((compare == 1 && quotes.PresentPrice * totalCapital >= count) || (compare == 2 && quotes.PresentPrice * totalCapital <= count))
                    {
                        logRecord.Append("\t\t条件成立(当前总市值:" + quotes.PresentPrice * totalCapital + ",条件:" + conditionDes + ")");
                        return 0;
                    }
                    logRecord.Append("\t\t条件不成立(当前总市值:" + quotes.PresentPrice * totalCapital + ",条件:" + conditionDes + ")");
                }
                return -1;
            }
        }

        public static int Analysis_CurrentPrice_back(string sharesCode, int market, List<string> par)
        {
            try
            {
                using (var db = new meal_ticketEntities())
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("declare @trendpar_table TrendPar;");
                    sql.AppendLine("declare @tempTri bit=0;");
                    foreach (var parStr in par)
                    {
                        sql.AppendLine("insert into @trendpar_table(Par)");
                        sql.AppendLine(string.Format("values('{0}');", parStr));
                    }
                    sql.AppendLine(string.Format("exec P_TradePar_Parse_CurrentPrice @trendpar_table,'{0}',{1},@tempTri output;", sharesCode, market));
                    sql.AppendLine("select @tempTri");

                    bool result = db.Database.SqlQuery<bool>(sql.ToString()).FirstOrDefault();
                    if (result)
                    {
                        return 0;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_CurrentPrice异常", ex);
                return -1;
            }
        }

        //分析价格条件
        public static int Analysis_Price_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            string logDes = "【{0}】";
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int priceType = 0;
                int compare = 0;
                long priceError = 0;
                try
                {
                    priceType = Convert.ToInt32(temp.PriceType);
                    compare = Convert.ToInt32(temp.Compare);
                }
                catch (Exception)
                {
                    logDes = string.Format(logDes, "条件不成立(参数配置错误)");
                    return -1;
                }
                try
                {
                    priceError = (long)(Convert.ToDouble(temp.PriceError) * 10000);
                }
                catch (Exception)
                {
                }

                DateTime timeNow = DateTime.Now;
                using (var db = new meal_ticketEntities())
                {
                    //当前价格
                    var quotes = (from item in db.v_shares_quotes_last
                                  where item.Market == market && item.SharesCode == sharesCode && item.LastModified.AddMinutes(1) > timeNow
                                  select item).FirstOrDefault();
                    if (quotes == null)
                    {
                        logDes = string.Format(logDes, "条件不成立(五档行情不实时)");
                        return -1;
                    }
                    long disPrice = priceType == 1 ? (quotes.LimitUpPrice + priceError)
                        : priceType == 2 ? (quotes.LimitDownPrice + priceError)
                        : priceType == 3 ? (long)(quotes.ClosedPrice * 1.0 / 100 * priceError + quotes.ClosedPrice)
                        : 0;
                    if (disPrice <= 0 || quotes.PresentPrice <= 0)
                    {
                        logDes = string.Format(logDes, "条件不成立(股票价格有误)");
                        return -1;
                    }
                    if ((compare == 1 && quotes.PresentPrice >= disPrice) || (compare == 2 && quotes.PresentPrice <= disPrice))
                    {
                        logDes = string.Format(logDes, "条件成立");
                        return 0;
                    }
                    logDes = string.Format(logDes, "条件不成立");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析价格条件出错了", ex);
                logDes = string.Format(logDes, "条件不成立(异常错误)");
                return -1;
            }
            finally
            {
                if (logRecord != null)
                {
                    logRecord.Append(logDes);
                }
            }
        }

        //分析时间段
        public static int Analysis_TimeSlot_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            DateTime timeNow = DateTime.Now;
            string logDes = "【条件{0}成立(当前时间:{1},条件时段:{2}-{3})】";
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                JArray timeList = temp.Times;
                if (timeNow >= DateTime.Parse(timeList[0].ToString()) && timeNow < DateTime.Parse(timeList[1].ToString()))
                {
                    logDes = string.Format(logDes,"", timeNow.ToString("yyyy-MM-dd HH:mm:ss"), timeList[0].ToString(), timeList[1].ToString());
                    return 0;
                }
                logDes = string.Format(logDes, "不", timeNow.ToString("yyyy-MM-dd HH:mm:ss"), timeList[0].ToString(), timeList[1].ToString());
                return -1;
            }
            catch (Exception ex)
            {
                logDes = ex.Message;
                Logger.WriteFileLog("分析时间段出错了", ex);
                return -1;
            }
            finally 
            {
                if (logRecord != null)
                {
                    logRecord.Append(logDes);
                }
            }
        }

        //分析历史涨跌幅数据
        public static int Analysis_HisRiseRate_New(string sharesCode, int market, List<string> par, StringBuilder logRecord=null)
        {
            string logDes = "【{0}】";
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int day = 0;
                int type = 0;
                int compare = 0;
                int count = 0;
                int rateCompare = 0;
                int rateCount = 0;
                int limitDay = 0;
                bool flatRise = false;
                bool triPrice = false;
                try
                {
                    day = Convert.ToInt32(temp.Day);
                }
                catch (Exception) { }
                try
                {
                    type = Convert.ToInt32(temp.Type);
                }
                catch (Exception) { }
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                }
                catch (Exception) { }
                try
                {
                    count = (type == 11 || type == 12 || type == 14) ? (int)(Convert.ToDouble(temp.Rise) * 100) : Convert.ToInt32(temp.Count);
                }
                catch (Exception) { }
                try
                {
                    rateCompare = Convert.ToInt32(temp.RateCompare);
                }
                catch (Exception) { }
                try
                {
                    rateCount = (int)(Convert.ToDouble(temp.RateCount) * 100);
                }
                catch (Exception) { }
                try
                {
                    limitDay = Convert.ToInt32(temp.LimitDay);
                }
                catch (Exception) { }
                try
                {
                    triPrice = Convert.ToBoolean(temp.TriPrice);
                }
                catch (Exception) { }
                try
                {
                    flatRise = Convert.ToBoolean(temp.FlatRise);
                }
                catch (Exception) { }

                if (day <= 0)
                {
                    logDes = string.Format(logDes, "条件不成立(参数day错误)");
                    return -1;
                }
                DateTime dateNow = DateTime.Now.Date;
                using (var db = new meal_ticketEntities())
                {
                    var quotes_date = (from item in db.t_shares_quotes_date
                                       where item.SharesCode == sharesCode && item.Market == market && item.LastModified < dateNow
                                       orderby item.Date descending
                                       select item).Take(day).ToList();
                    //没涨停次数
                    if (type == 1)
                    {
                        int tempCount = quotes_date.Where(e => e.PriceType != 1).Count();
                        string conditionDes = compare == 1 ? ("没涨停次数大于等于" + count) : ("没涨停次数小于等于" + count);
                        if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前没涨停次数:" + tempCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前没涨停次数:" + tempCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //涨停次数
                    else if (type == 2)
                    {
                        int tempCount = 0;
                        if (triPrice)
                        {
                            tempCount = quotes_date.Where(e => e.TriPriceType == 1).Count();
                        }
                        else
                        {
                            tempCount = quotes_date.Where(e => e.PriceType == 1).Count();
                        }

                        string conditionDes = compare == 1 ? ("涨停次数大于等于" + count) : ("涨停次数小于等于" + count);
                        if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前涨停次数:" + tempCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前涨停次数:" + tempCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //跌停次数
                    else if (type == 3)
                    {
                        int tempCount = 0;
                        if (triPrice)
                        {
                            tempCount = quotes_date.Where(e => e.TriPriceType == 2).Count();
                        }
                        else
                        {
                            tempCount = quotes_date.Where(e => e.PriceType == 2).Count();
                        }
                        string conditionDes = compare == 1 ? ("跌停次数大于等于" + count) : ("跌停次数小于等于" + count);
                        if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前跌停次数:" + tempCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前跌停次数:" + tempCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //炸板次数
                    else if (type == 4)
                    {
                        int tempCount = quotes_date.Where(e => e.PriceType != 1 && e.LimitUpCount > 0).Count();
                        string conditionDes = compare == 1 ? ("炸板次数大于等于" + count) : ("炸板次数小于等于" + count);
                        if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前炸板次数:" + tempCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前炸板次数:" + tempCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //每日涨跌幅
                    else if (type == 13)
                    {
                        int i = 0;
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PresentPrice <= 0 || quote.ClosedPrice <= 0)
                            {
                                continue;
                            }
                            int rate = (int)((quote.PresentPrice - quote.ClosedPrice) * 1.0 / quote.ClosedPrice * 10000);
                            if (!flatRise && rate == 0)
                            {
                                continue;
                            }
                            if (rateCompare == 1 && rate >= rateCount)
                            {
                                i++;
                                continue;
                            }
                            if (rateCompare == 2 && rate <= rateCount)
                            {
                                i++;
                                continue;
                            }
                        }
                        string rateDes = rateCompare == 1 ? ("大于等于" + rateCount) : ("小于等于" + rateCount);
                        string conditionDes = compare == 1 ? ("每日涨跌幅" + rateDes + "的次数大于等于" + count) : ("每日涨跌幅" + rateDes + "的次数小于等于" + count);
                        if ((compare == 1 && i >= count) || (compare == 2 && i <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前符合条件次数" + i + ", 条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前符合条件次数" + i + ", 条件:" + conditionDes + ")");
                        return -1;
                    }
                    //包含连续涨停
                    else if (type == 7)
                    {
                        int tempi = 0;//连续涨停天数
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PriceType == 1)
                            {
                                tempi++;
                            }
                            else
                            {
                                tempi = 0;
                            }
                            if (tempi >= limitDay)
                            {
                                logDes = string.Format(logDes, "条件成立(符合连续涨停" + limitDay + "天)");
                                return 0;
                            }
                        }
                        logDes = string.Format(logDes, "条件不成立(不符合连续涨停" + limitDay + "天)");
                        return -1;
                    }
                    //排除连续涨停
                    else if (type == 8)
                    {
                        int tempi = 0;//连续涨停天数
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PriceType == 1)
                            {
                                tempi++;
                            }
                            else
                            {
                                tempi = 0;
                            }
                            if (tempi >= limitDay)
                            {
                                logDes = string.Format(logDes, "条件不成立(不符合排除连续涨停" + limitDay + "天)");
                                return -1;
                            }
                        }
                        logDes = string.Format(logDes, "条件成立(符合排除连续涨停" + limitDay + "天)");
                        return 0;
                    }
                    //包含连续跌停
                    else if (type == 9)
                    {
                        int tempi = 0;//连续跌停天数
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PriceType == 2)
                            {
                                tempi++;
                            }
                            else
                            {
                                tempi = 0;
                            }
                            if (tempi >= limitDay)
                            {
                                logDes = string.Format(logDes, "条件成立(符合连续跌停" + limitDay + "天)");
                                return 0;
                            }
                        }
                        logDes = string.Format(logDes, "条件不成立(不符合连续跌停" + limitDay + "天)");
                        return -1;
                    }
                    //排除连续跌停
                    else if (type == 10)
                    {
                        int tempi = 0;//连续跌停天数
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PriceType == 2)
                            {
                                tempi++;
                            }
                            else
                            {
                                tempi = 0;
                            }
                            if (tempi >= limitDay)
                            {
                                logDes = string.Format(logDes, "条件不成立(不符合排除连续跌停" + limitDay + "天)");
                                return -1;
                            }
                        }
                        logDes = string.Format(logDes, "条件成立(符合排除连续跌停" + limitDay + "天)");
                        return 0;
                    }
                    //总涨跌幅
                    else if (type == 14)
                    {
                        long closePrice = quotes_date.OrderBy(e => e.Date).Select(e => e.ClosedPrice).FirstOrDefault();
                        long presentPrice = quotes_date.OrderByDescending(e => e.Date).Select(e => e.PresentPrice).FirstOrDefault();
                        if (closePrice <= 0 || presentPrice <= 0)
                        {
                            logDes = string.Format(logDes, "条件不成立(五档数据有误)");
                            return -1;
                        }
                        int rate = (int)((presentPrice - closePrice) * 1.0 / closePrice * 10000);
                        if (!flatRise && rate == 0)
                        {
                            logDes = string.Format(logDes, "条件不成立(当前为平盘，条件不包含平盘)");
                            return -1;
                        }
                        string conditionDes = compare == 1 ? ("总涨跌幅大于等于" + count) : ("总涨跌幅小于等于" + count);
                        if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前总涨跌幅:" + rate + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前总涨跌幅:" + rate + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    else
                    {
                        logDes = string.Format(logDes, "条件不成立(参数type错误)");
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析历史涨跌幅数据出错了", ex);
                logDes = string.Format(logDes, "条件不成立(异常报错)");
                return -1;
            }
            finally 
            {
                if (logRecord != null)
                {
                    logRecord.Append(logDes);
                }
            }
        }

        //分析今日涨跌幅
        public static int Analysis_TodayRiseRate_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            string logDes = "【{0}】";
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int type = 0;
                int compare = 0;
                long count = 0;
                bool triPrice = false;
                int firstDay = 0;
                int secondDay = 0;
                double multiple = 0;
                int compare2 = 0;
                int dayShortageType = 0;
                try
                {
                    type = Convert.ToInt32(temp.Type);
                }
                catch (Exception) { }
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                }
                catch (Exception ex) { }
                try
                {
                    count = (type == 5 || type == 6 || type == 9) ? (int)(Convert.ToDouble(temp.Count) * 100) : Convert.ToInt64(temp.Count);
                }
                catch (Exception ex) { }
                try
                {
                    triPrice = Convert.ToBoolean(temp.TriPrice);
                }
                catch (Exception) { }
                try
                {
                    firstDay = Convert.ToInt32(temp.FirstDay);
                }
                catch (Exception) { }
                try
                {
                    secondDay = Convert.ToInt32(temp.SecondDay);
                }
                catch (Exception) { }
                try
                {
                    multiple = Convert.ToDouble(temp.Multiple);
                }
                catch (Exception) { }
                try
                {
                    compare2 = Convert.ToInt32(temp.Compare2);
                }
                catch (Exception) { }
                try
                {
                    dayShortageType = Convert.ToInt32(temp.DayShortageType);
                }
                catch (Exception) { }

                DateTime dateNow = DateTime.Now.Date;
                using (var db = new meal_ticketEntities())
                {
                    string sqlQuery = string.Format("select top 1 LimitUpCount,TriLimitUpCount,LimitDownCount,TriLimitDownCount,LimitUpBombCount,OpenedPrice,ClosedPrice,MaxPrice,MinPrice,TotalAmount,TotalCount from t_shares_quotes_date with(nolock) where SharesCode='{0}' and Market={1} and LastModified>'{2}'", sharesCode, market, dateNow.ToString("yyyy-MM-dd"));
                    var quotes_date = db.Database.SqlQuery<QuotesDate>(sqlQuery).FirstOrDefault();

                    if (quotes_date == null)
                    {
                        logDes = string.Format(logDes, "条件不成立(五档数据不实时)");
                        return -1;
                    }
                    //涨停次数
                    if (type == 1)
                    {
                        int currCount = triPrice ? quotes_date.TriLimitUpCount : quotes_date.LimitUpCount;
                        string conditionDes = compare == 1 ? ("涨停次数大于等于" + count) : ("涨停次数小于等于" + count);
                        if ((compare == 1 && currCount >= count) || (compare == 2 && currCount <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前涨停次数：" + currCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前涨停次数：" + currCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //跌停次数
                    if (type == 2)
                    {
                        int currCount = triPrice ? quotes_date.TriLimitDownCount : quotes_date.LimitDownCount;
                        string conditionDes = compare == 1 ? ("跌停次数大于等于" + count) : ("跌停次数小于等于" + count);
                        if ((compare == 1 && currCount >= count) || (compare == 2 && currCount <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前跌停次数：" + currCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前跌停次数：" + currCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //炸板次数
                    else if (type == 3)
                    {
                        string conditionDes = compare == 1 ? ("炸板次数大于等于" + count) : ("炸板次数小于等于" + count);
                        if ((compare == 1 && quotes_date.LimitUpBombCount >= count) || (compare == 2 && quotes_date.LimitUpBombCount <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前炸板次数：" + quotes_date.LimitUpBombCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前炸板次数：" + quotes_date.LimitUpBombCount + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //盘中涨跌幅
                    else if (type == 4)
                    {
                        int i = -1;
                        foreach (var item in temp.riseRateList)
                        {
                            UP_OR_DOWN_INFO upOrDownInfo = new UP_OR_DOWN_INFO();
                            DB_TRADE_PRICE_INFO DB_TRADE_PRICE_INFO = new DB_TRADE_PRICE_INFO();
                            int Minute = item.Minute;
                            int Compare = item.Compare;
                            double Rate = item.Rate;
                            int iErrorCode = Singleton.Instance.m_stockMonitor.GetUpOrDownInfoInTime(sharesCode + "," + market, Minute, ref DB_TRADE_PRICE_INFO, ref upOrDownInfo);
                            if (iErrorCode != 0)
                            {
                                Logger.WriteFileLog(iErrorCode + "" + JsonConvert.SerializeObject(upOrDownInfo), null);
                                continue;
                            }
                            if (upOrDownInfo.iLastestPercent == -0x0FFFFFFF)
                            {
                                continue;
                            }
                            if (Compare == 1 && upOrDownInfo.iLastestPercent >= Rate * 100)
                            {
                                i++;
                                break;
                            }
                            if (Compare == 2 && upOrDownInfo.iLastestPercent <= Rate * 100)
                            {
                                i++;
                                break;
                            }
                        }
                        if (i == 0)
                        {
                            logDes = string.Format(logDes, "条件成立(符合盘中涨跌幅条件)");
                        }
                        else
                        {
                            logDes = string.Format(logDes, "条件不成立(不符合盘中涨跌幅条件)");
                        }
                        return i;
                    }
                    //开盘涨跌幅
                    else if (type == 5)
                    {
                        long openPrice = quotes_date.OpenedPrice;
                        long closePrice = quotes_date.ClosedPrice;
                        if (closePrice <= 0 || openPrice <= 0)
                        {
                            return -1;
                        }
                        int rate = (int)((openPrice - closePrice) * 1.0 / closePrice * 10000);
                        string conditionDes = compare == 1 ? ("开盘涨跌幅大于等于" + count) : ("开盘涨跌幅小于等于" + count);
                        if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前开盘涨跌幅：" + rate + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前开盘涨跌幅：" + rate + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //振幅
                    else if (type == 6)
                    {
                        long maxPrice = quotes_date.MaxPrice;
                        long minPrice = quotes_date.MinPrice;
                        long closePrice = quotes_date.ClosedPrice;
                        if (closePrice <= 0 || maxPrice <= 0 || minPrice <= 0)
                        {
                            return -1;
                        }
                        int rate = (int)((maxPrice - minPrice) * 1.0 / closePrice * 10000);
                        string conditionDes = compare == 1 ? ("振幅大于等于" + count) : ("振幅小于等于" + count);
                        if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前振幅：" + rate + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前振幅：" + rate + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    //成交额
                    else if (type == 7)
                    {
                        var quotesCount = (from item in db.t_shares_quotes_date
                                           where item.SharesCode == sharesCode && item.Market == market && item.LastModified < dateNow
                                           select item).Count();
                        if (dayShortageType == 2 && quotesCount < firstDay)
                        {
                            logDes = string.Format(logDes, "条件成立(符合成交额条件)");
                            return 0;
                        }
                        if (dayShortageType == 3 && quotesCount < firstDay)
                        {
                            logDes = string.Format(logDes, "条件不成立(不符合成交额条件)");
                            return -1;
                        }
                        //查询前几个交易日平均交易额
                        string sql = string.Format(@"select convert(bigint,avg(TotalAmount)) AvgTotalAmount
  from
  (
	select [Date],TotalAmount,row_number()over(order by [Date] desc) num
	from t_shares_quotes_date with(nolock)
	where LastModified<'{0}' and SharesCode='{1}' and Market={2}
  )t
  where t.num>={3} and t.num<={4}", dateNow.ToString("yyyy-MM-dd"), sharesCode, market, secondDay, firstDay);
                        long avgResult = db.Database.SqlQuery<long>(sql).FirstOrDefault();
                        if ((compare == 1 && quotes_date.TotalAmount >= avgResult * multiple) || (compare == 2 && quotes_date.TotalAmount <= avgResult * multiple))
                        {
                            if ((compare2 == 1 && avgResult >= count * 10000) || (compare2 == 2 && avgResult <= count * 10000))
                            {
                                logDes = string.Format(logDes, "条件成立(符合成交额条件)");
                                return 0;
                            }
                        }
                        logDes = string.Format(logDes, "条件不成立(不符合成交额条件)");
                        return -1;
                    }
                    //成交量
                    else if (type == 8)
                    {
                        var quotesCount = (from item in db.t_shares_quotes_date
                                           where item.SharesCode == sharesCode && item.Market == market && item.LastModified < dateNow
                                           select item).Count();
                        if (dayShortageType == 2 && quotesCount < firstDay)
                        {
                            logDes = string.Format(logDes, "条件成立(符合成交量条件)");
                            return 0;
                        }
                        if (dayShortageType == 3 && quotesCount < firstDay)
                        {
                            logDes = string.Format(logDes, "条件不成立(不符合成交量条件)");
                            return -1;
                        }
                        //查询前几个交易日平均成交量
                        string sql = string.Format(@"select convert(bigint,avg(TotalCount)) AvgTotalCount
  from
  (
	select [Date],TotalCount,row_number()over(order by [Date] desc) num
	from t_shares_quotes_date with(nolock)
	where LastModified<'{0}' and SharesCode='{1}' and Market={2}
  )t
  where t.num>={3} and t.num<={4}", dateNow.ToString("yyyy-MM-dd"), sharesCode, market, secondDay, firstDay);
                        long avgResult = db.Database.SqlQuery<long>(sql).FirstOrDefault();
                        if ((compare == 1 && quotes_date.TotalCount >= avgResult * multiple) || (compare == 2 && quotes_date.TotalCount <= avgResult * multiple))
                        {
                            if ((compare2 == 1 && avgResult * 100 >= count) || (compare2 == 2 && avgResult * 100 <= count))
                            {
                                logDes = string.Format(logDes, "条件成立(符合成交量条件)");
                                return 0;
                            }
                        }
                        logDes = string.Format(logDes, "条件不成立(不符合成交量条件)");
                        return -1;
                    }
                    //最新涨跌幅
                    else if (type == 9)
                    {
                        long currPrice = quotes_date.PresentPrice;
                        long closePrice = quotes_date.ClosedPrice;
                        if (closePrice <= 0 || currPrice <= 0)
                        {
                            return -1;
                        }
                        int rate = (int)((currPrice - closePrice) * 1.0 / closePrice * 10000);
                        string conditionDes = compare == 1 ? ("最新涨跌幅大于等于" + count) : ("最新涨跌幅小于等于" + count);
                        if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                        {
                            logDes = "\t\t条件成立(当前最新涨跌幅：" + rate + ",条件:" + conditionDes + ")";
                            return 0;
                        }
                        logDes = "\t\t条件不成立(当前最新涨跌幅：" + rate + ",条件:" + conditionDes + ")";
                        return -1;
                    }
                    else
                    {
                        logDes = string.Format(logDes, "条件不成立(参数type错误)");
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog(string.Format("分析今日涨跌幅出错了-{0}", sharesCode), ex);
                logDes = string.Format(logDes, "条件不成立(异常错误)");
                return -1;
            }
            finally
            {
                if (logRecord != null)
                {
                    logRecord.Append(logDes);
                }
            }
        }

        //分析板块涨跌幅
        public static int Analysis_PlateRiseRate_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            string logDes = "【{0}】";
            try
            {
                using (var db = new meal_ticketEntities())
                {
                    //查询股票所属板块及板块涨跌幅
                    var plateRate = (from item in db.t_shares_plate_rel
                                     join item2 in db.t_shares_plate_riserate_last on item.PlateId equals item2.PlateId
                                     join item3 in db.t_shares_plate on item.PlateId equals item3.Id
                                     where item.Market == market && item.SharesCode == sharesCode
                                     select new
                                     {
                                         SharesCount = item2.SharesCount,
                                         PlateId = item.PlateId,
                                         RiseRate = item2.RiseRate,
                                         PlateType = item3.Type,
                                     }).ToList();
                    List<long> AllPlateId = plateRate.Select(e => e.PlateId).ToList();

                    var plateBase = (from item in db.t_shares_plate
                                     where item.BaseStatus == 1 && AllPlateId.Contains(item.Id)
                                     select item).ToList();


                    List<long> PlateList1 = new List<long>();
                    List<long> PlateList2 = new List<long>();
                    List<string> AllSetPar = new List<string>();
                    foreach (var p in par)
                    {
                        var temp = JsonConvert.DeserializeObject<dynamic>(p);
                        long groupId = -1;
                        int dataType = 1;
                        int compare = 0;
                        double rate = 0;
                        try
                        {
                            groupId = temp.GroupId;
                        }
                        catch (Exception ex) { }
                        try
                        {
                            compare = temp.Compare;
                        }
                        catch (Exception ex) { }
                        try
                        {
                            rate = temp.Rate;
                        }
                        catch (Exception ex) { }
                        try
                        {
                            dataType = temp.DataType;
                        }
                        catch (Exception ex) { }

                        if (groupId == 0)
                        {
                            AllSetPar.Add(p);
                        }
                        var plate = plateRate.Where(e => e.PlateId == groupId).FirstOrDefault();
                        if (dataType == 2)
                        {
                            if (plate == null)
                            {
                                continue;
                            }
                            if (compare == 1 && plate.RiseRate < rate * 100)
                            {
                                continue;
                            }
                            if (compare == 2 && plate.RiseRate > rate * 100)
                            {
                                continue;
                            }
                            PlateList2.Add(plate.PlateId);
                        }
                        else
                        {
                            if (groupId == -1)//行业基础版块
                            {
                                PlateList1.AddRange(plateBase.Where(e => e.Type == 1).Select(e => e.Id).ToList());
                            }
                            else if (groupId == -2)//地区基础版块
                            {
                                PlateList1.AddRange(plateBase.Where(e => e.Type == 2).Select(e => e.Id).ToList());
                            }
                            else if (groupId == -3)//概念基础版块
                            {
                                PlateList1.AddRange(plateBase.Where(e => e.Type == 3).Select(e => e.Id).ToList());
                            }
                            else
                            {
                                if (plate == null)
                                {
                                    continue;
                                }
                                PlateList1.Add(plate.PlateId);
                            }
                        }
                    }
                    PlateList1 = PlateList1.Distinct().ToList();

                    foreach (var p in AllSetPar)
                    {
                        var temp = JsonConvert.DeserializeObject<dynamic>(p);
                        int groupType = temp.GroupType;
                        int tempCompare = temp.Compare;
                        double tempRate = temp.Rate;
                        var plate = plateRate.Where(e => e.PlateType == groupType && ((tempCompare == 1 && e.RiseRate >= tempRate * 100) || (tempCompare == 2 && e.RiseRate <= tempRate * 100))).Select(e => e.PlateId).ToList();
                        if (plate.Count() <= 0)
                        {
                            continue;
                        }
                        PlateList2.AddRange(plate);
                    }
                    PlateList2 = PlateList2.Distinct().ToList();

                    var newList = PlateList1.Intersect(PlateList2);
                    if (newList.Count() > 0)
                    {
                        logDes = string.Format(logDes, "条件成立(符合板块涨跌幅条件)");
                        return 0;
                    }
                    logDes = string.Format(logDes, "条件不成立(不符合板块涨跌幅条件)");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析板块涨跌幅出错了", ex);
                logDes = string.Format(logDes, "条件成立(异常错误)");
                return -1;
            }
            finally
            {
                if (logRecord != null)
                {
                    logRecord.Append(logDes);
                }
            }
        }

        //分析买卖单占比
        public static int Analysis_BuyOrSellCount_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            string logDes = "【{0}】";
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int type = 0;
                int compare = 0;
                double count = 0;
                int countType = 0;
                try
                {
                    type = Convert.ToInt32(temp.Type);
                    compare = Convert.ToInt32(temp.Compare);
                    count = Convert.ToDouble(temp.Count);
                    countType = Convert.ToInt32(temp.CountType);
                }
                catch (Exception)
                {
                    logDes = string.Format(logDes, "条件不成立(参数错误)");
                    return -1;
                }
                DateTime timeNow = DateTime.Now;
                using (var db = new meal_ticketEntities())
                {
                    var quotes = (from item in db.v_shares_quotes_last
                                  where item.Market == market && item.SharesCode == sharesCode && SqlFunctions.DateAdd("MI", 1, item.LastModified) > timeNow
                                  select item).FirstOrDefault();
                    if (quotes == null)
                    {
                        return -1;
                    }
                    int realCount;
                    string realCountDes;
                    switch (type)
                    {
                        case 1:
                            realCount = quotes.BuyCount1;
                            realCountDes = "买一挂单";
                            break;
                        case 2:
                            realCount = quotes.BuyCount2;
                            realCountDes = "买二挂单";
                            break;
                        case 3:
                            realCount = quotes.BuyCount3;
                            realCountDes = "买三挂单";
                            break;
                        case 4:
                            realCount = quotes.BuyCount4;
                            realCountDes = "买四挂单";
                            break;
                        case 5:
                            realCount = quotes.BuyCount5;
                            realCountDes = "买五挂单";
                            break;
                        case 6:
                            realCount = quotes.SellCount1;
                            realCountDes = "卖一挂单";
                            break;
                        case 7:
                            realCount = quotes.SellCount2;
                            realCountDes = "卖二挂单";
                            break;
                        case 8:
                            realCount = quotes.SellCount3;
                            realCountDes = "卖三挂单";
                            break;
                        case 9:
                            realCount = quotes.SellCount4;
                            realCountDes = "卖四挂单";
                            break;
                        case 10:
                            realCount = quotes.SellCount5;
                            realCountDes = "卖五挂单";
                            break;
                        default:
                            logDes = string.Format(logDes, "条件不成立(参数type错误)");
                            return -1;
                    }
                    realCount = realCount * 100;

                    if (countType == 1)//流通股百分比
                    {
                        //查询流通股
                        var sharesInfo = (from item in db.t_shares_markettime
                                          where item.Market == market && item.SharesCode == sharesCode
                                          select item).FirstOrDefault();
                        if (sharesInfo == null)
                        {
                            logDes = string.Format(logDes, "条件不成立(流通股本数据不存在)");
                            return -1;
                        }
                        long circulatingCapital = sharesInfo.CirculatingCapital;
                        long setCount = (long)(circulatingCapital * (count / 100));

                        string conditionDes = realCountDes + (compare == 1 ? ("大于等于" + setCount) : ("小于等于" + setCount));
                        if ((compare == 1 && realCount >= setCount) || (compare == 2 && realCount <= setCount))
                        {
                            logDes = string.Format(logDes, "条件成立(当前挂单:" + realCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前挂单:" + realCount + ",条件:" + conditionDes + ")");
                    }
                    else if (countType == 2)//股数
                    {
                        string conditionDes = realCountDes + (compare == 1 ? ("大于等于" + count) : ("小于等于" + count));
                        if ((compare == 1 && realCount >= count) || (compare == 2 && realCount <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前挂单:" + realCount + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前挂单:" + realCount + ",条件:" + conditionDes + ")");
                    }
                    else if (countType == 3)//流通市值百分比
                    {

                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析买卖单占比出错了", ex);
                logDes = string.Format(logDes, "条件不成立(异常错误)");
                return -1;
            }
            finally
            {
                if (logRecord != null)
                {
                    logRecord.Append(logDes);
                }
            }
        }

        //分析按参照价格
        public static int Analysis_ReferPrice_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            string logDes = "【{0}】";
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int compare = 0;
                int day = 0;
                double count = 0;
                int priceType = 0;
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                    day = Convert.ToInt32(temp.Day);
                    count = Convert.ToDouble(temp.Count);
                    priceType = Convert.ToInt32(temp.PriceType);
                }
                catch (Exception)
                {
                    logDes = string.Format(logDes, "条件不成立(参数错误)");
                    return -1;
                }
                DateTime timeNow = DateTime.Now;
                using (var db = new meal_ticketEntities())
                {
                    var quotes = (from item in db.t_shares_quotes_date
                                  where item.Market == market && item.SharesCode == sharesCode
                                  orderby item.Date descending
                                  select item).Take(day + 1).ToList();
                    if (quotes.Count() <= 0)
                    {
                        logDes = string.Format(logDes, "条件不成立(五档行情有误)");
                        return -1;
                    }
                    var presentInfo = quotes.OrderByDescending(e => e.Date).FirstOrDefault();
                    if (presentInfo.LastModified < timeNow.AddMinutes(-1))
                    {
                        logDes = string.Format(logDes, "条件不成立(五档行情不实时)");
                        return -1;
                    }
                    long presentPrice = presentInfo.PresentPrice;
                    if (presentPrice <= 0)
                    {
                        logDes = string.Format(logDes, "条件不成立(五档行情价格有误)");
                        return -1;
                    }

                    var lastInfo = quotes.Where(e => e.LastModified < timeNow.Date).ToList();
                    long realPrice;
                    switch (priceType)
                    {
                        case 1:
                            realPrice = compare == 1 ? lastInfo.Max(e => e.OpenedPrice) : lastInfo.Min(e => e.OpenedPrice);
                            break;
                        case 2:
                            realPrice = compare == 1 ? lastInfo.Max(e => e.PresentPrice) : lastInfo.Min(e => e.PresentPrice);
                            break;
                        case 3:
                            realPrice = compare == 1 ? lastInfo.Max(e => e.MaxPrice) : lastInfo.Min(e => e.MaxPrice);
                            break;
                        case 4:
                            realPrice = compare == 1 ? lastInfo.Max(e => e.MinPrice) : lastInfo.Min(e => e.MinPrice);
                            break;
                        default:
                            logDes = string.Format(logDes, "条件不成立(参数priceType错误)");
                            return -1;
                    }

                    if (count != 0)//计算偏差
                    {
                        realPrice = (long)(realPrice * (count / 100) + realPrice);
                    }

                    string conditionDes = compare == 1 ? ("当前价格大于等于" + realPrice) : ("当前价格小于等于" + realPrice);
                    if ((compare == 1 && presentPrice >= realPrice) || (compare == 2 && presentPrice <= realPrice))
                    {
                        logDes = string.Format(logDes, "条件成立(当前价格:" + presentPrice + ",条件:" + conditionDes + ")");
                        return 0;
                    }
                    logDes = string.Format(logDes, "条件不成立(当前价格:" + presentPrice + ",条件:" + conditionDes + ")");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按参照价格出错了", ex);
                logDes = string.Format(logDes, "条件不成立(异常错误)");
                return -1;
            }
            finally
            {
                if (logRecord != null)
                {
                    logRecord.Append(logDes);
                }
            }
        }

        //分析按均线价格
        public static int Analysis_ReferAverage_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            string logDes = "【{0}】";
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int compare = 0;
                int day1 = 0;
                int day2 = 0;
                double count = 0;
                int upOrDown = 0;
                int dayShortageType = 1;
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                    day1 = Convert.ToInt32(temp.Day1);
                    count = Convert.ToDouble(temp.Count);
                }
                catch (Exception)
                {
                    logDes = string.Format(logDes, "条件不成立(参数错误)");
                    return -1;
                }
                try
                {
                    day2 = Convert.ToInt32(temp.Day2);
                    day2 = day2 + 1;//多取一天
                    upOrDown = Convert.ToInt32(temp.UpOrDown);
                }
                catch (Exception)
                {

                }
                try
                {
                    dayShortageType = Convert.ToInt32(temp.DayShortageType);
                }
                catch (Exception)
                {
                }

                DateTime timeNow = DateTime.Now;
                using (var db = new meal_ticketEntities())
                {
                    var quotes = (from item in db.t_shares_quotes_date
                                  where item.Market == market && item.SharesCode == sharesCode
                                  orderby item.Date descending
                                  select item).Take(day1 + day2).ToList();
                    var presentInfo = quotes.OrderByDescending(e => e.Date).FirstOrDefault();
                    if (presentInfo.LastModified < timeNow.AddMinutes(-1))
                    {
                        logDes = string.Format(logDes, "条件不成立(五档行情不实时)");
                        return -1;
                    }
                    long presentPrice = presentInfo.PresentPrice;
                    if (presentPrice <= 0)
                    {
                        logDes = string.Format(logDes, "条件不成立(五档行情价格有误)");
                        return -1;
                    }
                    //计算均线价格
                    var list1 = quotes.Take(day1).ToList();
                    if (list1.Count() <= 0)
                    {
                        logDes = string.Format(logDes, "条件不成立(暂无均线)");
                        return -1;
                    }
                    if (list1.Count() < day1)
                    {
                        if (dayShortageType == 2)
                        {
                            logDes = string.Format(logDes, "条件成立(均线不够返回true)");
                            return 0;
                        }
                        if (dayShortageType == 3)
                        {
                            logDes = string.Format(logDes, "条件不成立(均线不够返回false)");
                            return -1;
                        }
                    }
                    long averagePrice = (long)list1.Average(e => e.PresentPrice);
                    if (count != 0)//计算偏差
                    {
                        averagePrice = (long)(averagePrice * (count / 100) + averagePrice);
                    }

                    string conditionDes = compare == 1 ? ("当前价格大于等于" + averagePrice) : ("当前价格小于等于" + averagePrice);
                    if ((compare == 1 && presentPrice >= averagePrice) || (compare == 2 && presentPrice <= averagePrice))
                    {
                        if (day2 >= 2)//计算每日均线
                        {
                            long lastAveragePrice = 0;
                            for (int i = 0; i < day2; i++)
                            {
                                long tempAveragePrice;
                                var list2 = quotes.Skip(i).Take(day1).ToList();
                                if (list2.Count() <= 0)
                                {
                                    tempAveragePrice = 0;
                                }
                                else
                                {
                                    tempAveragePrice = (long)list2.Average(e => e.PresentPrice);
                                }
                                if (upOrDown == 1)
                                {
                                    //向上，则当前必须<=前一个数据
                                    if (tempAveragePrice > lastAveragePrice && lastAveragePrice != 0)
                                    {
                                        logDes = string.Format(logDes, "条件不成立(均线非连续向上)");
                                        return -1;
                                    }
                                    lastAveragePrice = tempAveragePrice;
                                    continue;
                                }
                                else if (upOrDown == 2)
                                {
                                    //向下，则当前必须>=前一个数据
                                    if (tempAveragePrice < lastAveragePrice && lastAveragePrice != 0)
                                    {
                                        logDes = string.Format(logDes, "条件不成立(均线非连续向下)");
                                        return -1;
                                    }
                                    lastAveragePrice = tempAveragePrice;
                                    continue;
                                }
                                else
                                {
                                    logDes = string.Format(logDes, "条件不成立(参数upOrDown错误)");
                                    return -1;
                                }
                            }
                        }
                        logDes = string.Format(logDes, "条件成立(当前价格:" + presentPrice + ",条件:" + conditionDes + ")");
                        return 0;
                    }
                    logDes = string.Format(logDes, "条件不成立(当前价格:" + presentPrice + ",条件:" + conditionDes + ")");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按均线价格出错了", ex);
                logDes = string.Format(logDes, "条件不成立(异常错误)");
                return -1;
            }
            finally
            {
                if (logRecord != null)
                {
                    logRecord.Append(logDes);
                }
            }
        }

        //分析买卖变化速度
        public static int Analysis_QuotesChangeRate_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            string logDes = "【{0}】";
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int compare = 0;
                int type = 0;
                double count = 0;
                int othercompare = 0;
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                    type = Convert.ToInt32(temp.Type);
                    count = Convert.ToDouble(temp.Count);
                }
                catch (Exception)
                {
                    logDes = string.Format(logDes, "条件不成立(参数错误)");
                    return -1;
                }
                try
                {
                    othercompare = Convert.ToInt32(temp.Compare);
                }
                catch (Exception)
                {
                }

                DateTime timeNow = DateTime.Now;
                using (var db = new meal_ticketEntities())
                {
                    var quotes = (from item in db.t_shares_quotes
                                  where item.Market == market && item.SharesCode == sharesCode && SqlFunctions.DateAdd("MI", 1, item.LastModified) > timeNow
                                  select item).ToList();
                    var quotesLast = quotes.Where(e => e.DataType == 0).FirstOrDefault();
                    var quotesPre = quotes.Where(e => e.DataType == 1).FirstOrDefault();
                    if (quotesLast == null || quotesPre == null)
                    {
                        logDes = string.Format(logDes, "条件不成立(t五档数据有误)");
                        return -1;
                    }

                    int disCount = 0;
                    long disPrice = 0;
                    int lastCount = 0;
                    long lastPrice = 0;
                    int tradetype = 0;

                    switch (type)
                    {
                        case 1:
                            disCount = quotesLast.BuyCount1;
                            disPrice = quotesLast.BuyPrice1;
                            lastCount = quotesPre.BuyCount1;
                            lastPrice = quotesPre.BuyPrice1;
                            tradetype = 1;
                            break;
                        case 2:
                            disCount = quotesLast.BuyCount2;
                            disPrice = quotesLast.BuyPrice2;
                            lastCount = quotesPre.BuyCount2;
                            lastPrice = quotesPre.BuyPrice2;
                            tradetype = 1;
                            break;
                        case 3:
                            disCount = quotesLast.BuyCount3;
                            disPrice = quotesLast.BuyPrice3;
                            lastCount = quotesPre.BuyCount3;
                            lastPrice = quotesPre.BuyPrice3;
                            tradetype = 1;
                            break;
                        case 4:
                            disCount = quotesLast.BuyCount4;
                            disPrice = quotesLast.BuyPrice4;
                            lastCount = quotesPre.BuyCount4;
                            lastPrice = quotesPre.BuyPrice4;
                            tradetype = 1;
                            break;
                        case 5:
                            disCount = quotesLast.BuyCount5;
                            disPrice = quotesLast.BuyPrice5;
                            lastCount = quotesPre.BuyCount5;
                            lastPrice = quotesPre.BuyPrice5;
                            tradetype = 1;
                            break;
                        case 6:
                            disCount = quotesLast.SellCount1;
                            disPrice = quotesLast.SellPrice1;
                            lastCount = quotesPre.SellCount1;
                            lastPrice = quotesPre.SellPrice1;
                            tradetype = 2;
                            break;
                        case 7:
                            disCount = quotesLast.SellCount2;
                            disPrice = quotesLast.SellPrice2;
                            lastCount = quotesPre.SellCount2;
                            lastPrice = quotesPre.SellPrice2;
                            tradetype = 2;
                            break;
                        case 8:
                            disCount = quotesLast.SellCount3;
                            disPrice = quotesLast.SellPrice3;
                            lastCount = quotesPre.SellCount3;
                            lastPrice = quotesPre.SellPrice3;
                            tradetype = 2;
                            break;
                        case 9:
                            disCount = quotesLast.SellCount4;
                            disPrice = quotesLast.SellPrice4;
                            lastCount = quotesPre.SellCount4;
                            lastPrice = quotesPre.SellPrice4;
                            tradetype = 2;
                            break;
                        case 10:
                            disCount = quotesLast.SellCount5;
                            disPrice = quotesLast.SellPrice5;
                            lastCount = quotesPre.SellCount5;
                            lastPrice = quotesPre.SellPrice5;
                            tradetype = 2;
                            break;
                        default:
                            return -1;
                    }

                    long currPrice = quotesLast.PresentPrice;
                    if ((othercompare == 1 && disPrice < currPrice) || (othercompare == 2 && disPrice > currPrice))
                    {
                        return -1;
                    }
                    if (lastCount <= 0 || lastPrice <= 0)
                    {
                        return -1;
                    }

                    double rate = 0;
                    //当前价格等于0，速率为-100%
                    if (disPrice <= 0)
                    {
                        rate = -100;
                    }
                    //价格相等，计算速率
                    else if (lastPrice == disPrice)
                    {
                        rate = (disCount - lastCount) * 1.0 / lastCount * 100;
                    }
                    else if (lastPrice > disPrice)
                    {
                        //卖单则为无限大（999999999）
                        if (tradetype == 2)
                        {
                            rate = 999999999;
                        }
                        else
                        {
                            rate = -100;
                        }
                    }
                    else
                    {
                        //买单则为无限大（999999999）
                        if (tradetype == 1)
                        {
                            rate = 999999999;
                        }
                        else
                        {
                            rate = -100;
                        }
                    }

                    if (compare == 1 && rate >= count)
                    {
                        return 0;
                    }
                    if (compare == 2 && rate <= count)
                    {
                        return 0;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析买卖变化速度出错了", ex);
                logDes = string.Format(logDes, "条件不成立(异常错误)");
                return -1;
            }
            finally
            {
                if (logRecord != null)
                {
                    logRecord.Append(logDes);
                }
            }
        }

        //分析按当前价格
        public static int Analysis_CurrentPrice_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            string logDes = "【{0}】";
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int riseType = 0;
                int compare = 0;
                long count = 0;
                try
                {
                    riseType = Convert.ToInt32(temp.RiseType);
                }
                catch (Exception)
                {
                    logDes = string.Format(logDes, "条件不成立(参数配置错误)");
                    return -1;
                }
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                }
                catch (Exception)
                {
                }
                try
                {
                    count = (long)(Convert.ToDouble(temp.Count) * 10000);
                }
                catch (Exception)
                {
                }

                DateTime timeNow = DateTime.Now;
                using (var db = new meal_ticketEntities())
                {
                    //当前价格
                    var quotes = (from item in db.v_shares_quotes_last
                                  where item.Market == market && item.SharesCode == sharesCode && SqlFunctions.DateAdd("MI", 1, item.LastModified) > timeNow
                                  select item).FirstOrDefault();
                    if (quotes == null)
                    {
                        logDes = string.Format(logDes, "条件不成立(五档行情不实时)");
                        return -1;
                    }

                    if (riseType == 1 && quotes.TriPriceType == 1)
                    {
                        logDes = string.Format(logDes, "条件成立(当前触发涨停,条件触发涨停)");
                        return 0;
                    }

                    if (riseType == 2 && quotes.TriPriceType == 2)
                    {
                        logDes = string.Format(logDes, "条件成立(当前触发跌停,条件触发跌停)");
                        return 0;
                    }

                    if (riseType == 3 && quotes.PriceType == 1)
                    {
                        logDes = string.Format(logDes, "条件成立(当前涨停封板,条件涨停封板)");
                        return 0;
                    }

                    if (riseType == 4 && quotes.PriceType == 2)
                    {
                        logDes = string.Format(logDes, "条件成立(当前跌停封板,条件跌停封板)");
                        return 0;
                    }
                    if (riseType == 5)
                    {
                        string conditionDes = compare == 1 ? ("价格大于等于" + count) : ("价格小于等于" + count);
                        if ((compare == 1 && quotes.PresentPrice >= count) || (compare == 2 && quotes.PresentPrice <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前价格:" + quotes.PresentPrice + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前价格:" + quotes.PresentPrice + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    if (riseType == 6)
                    {
                        //查询流通股
                        var sharesInfo = (from item in db.t_shares_markettime
                                          where item.Market == market && item.SharesCode == sharesCode
                                          select item).FirstOrDefault();
                        if (sharesInfo == null)
                        {
                            logDes = string.Format(logDes, "条件不成立(流通股本数据不存在)");
                            return -1;
                        }
                        long circulatingCapital = sharesInfo.CirculatingCapital;
                        string conditionDes = compare == 1 ? ("流通市值大于等于" + count) : ("流通市值小于等于" + count);
                        if ((compare == 1 && quotes.PresentPrice * circulatingCapital >= count) || (compare == 2 && quotes.PresentPrice * circulatingCapital <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前流通市值:" + quotes.PresentPrice * circulatingCapital + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前流通市值:" + quotes.PresentPrice * circulatingCapital + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    if (riseType == 7)
                    {
                        //查询总股本
                        var sharesInfo = (from item in db.t_shares_markettime
                                          where item.Market == market && item.SharesCode == sharesCode
                                          select item).FirstOrDefault();
                        if (sharesInfo == null)
                        {
                            logDes = string.Format(logDes, "条件不成立(总股本数据不存在)");
                            return -1;
                        }
                        long totalCapital = sharesInfo.TotalCapital;
                        string conditionDes = compare == 1 ? ("总市值大于等于" + count) : ("总市值小于等于" + count);
                        if ((compare == 1 && quotes.PresentPrice * totalCapital >= count) || (compare == 2 && quotes.PresentPrice * totalCapital <= count))
                        {
                            logDes = string.Format(logDes, "条件成立(当前总市值:" + quotes.PresentPrice * totalCapital + ",条件:" + conditionDes + ")");
                            return 0;
                        }
                        logDes = string.Format(logDes, "条件不成立(当前总市值:" + quotes.PresentPrice * totalCapital + ",条件:" + conditionDes + ")");
                        return -1;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按当前价格出错了", ex);
                logDes = string.Format(logDes, "条件不成立(异常错误)");
                return -1;
            }
            finally
            {
                if (logRecord != null)
                {
                    logRecord.Append(logDes);
                }
            }
        }

        //分析五档变化速度
        public static int Analysis_QuotesTypeChangeRate_New(string sharesCode, int market, List<string> par, StringBuilder logRecord=null)
        {
            string logDes = "【{0}】";
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par[0]);
                int compare = 0;
                int type = 0;
                double count = 0;
                int priceType = 1;
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                    type = Convert.ToInt32(temp.Type);
                    count = Convert.ToDouble(temp.Count);
                    priceType = Convert.ToInt32(temp.PriceType);
                }
                catch (Exception)
                {
                }
                if (type == 0)
                {
                    logDes = string.Format(logDes, "条件不成立(参数错误)");
                    return -1;
                }

                DateTime timeNow = DateTime.Now;
                using (var db = new meal_ticketEntities())
                {
                    var quotes = (from item in db.t_shares_quotes
                                  where item.Market == market && item.SharesCode == sharesCode && SqlFunctions.DateAdd("MI", 1, item.LastModified) > timeNow
                                  select item).ToList();
                    var quotesLast = quotes.Where(e => e.DataType == 0).FirstOrDefault();
                    var quotesPre = quotes.Where(e => e.DataType == 1).FirstOrDefault();
                    if (quotesLast == null || quotesPre == null)
                    {
                        logDes = string.Format(logDes, "条件不成立(五档行情数据有误)");
                        return -1;
                    }
                    long currPrice = quotesLast.PresentPrice;
                    if (priceType == 2)
                    {
                        currPrice = quotesLast.LimitUpPrice;
                    }
                    else if (priceType == 3)
                    {
                        currPrice = quotesLast.LimitDownPrice;
                    }

                    int disCount = 0;
                    int lastCount = 0;

                    double rate = 0;
                    switch (type)
                    {
                        case 1:
                            if (quotesPre.BuyPrice1 == currPrice)
                            {
                                lastCount = quotesPre.BuyCount1;
                            }
                            else if (quotesPre.BuyPrice2 == currPrice)
                            {
                                lastCount = quotesPre.BuyCount2;
                            }
                            else if (quotesPre.BuyPrice3 == currPrice)
                            {
                                lastCount = quotesPre.BuyCount3;
                            }
                            else if (quotesPre.BuyPrice4 == currPrice)
                            {
                                lastCount = quotesPre.BuyCount4;
                            }
                            else if (quotesPre.BuyPrice5 == currPrice)
                            {
                                lastCount = quotesPre.BuyCount5;
                            }
                            else
                            {
                                return -1;
                            }
                            if (quotesLast.BuyPrice1 == currPrice)
                            {
                                disCount = quotesLast.BuyCount1;
                            }
                            else if (quotesLast.BuyPrice2 == currPrice)
                            {
                                disCount = quotesLast.BuyCount2;
                            }
                            else if (quotesLast.BuyPrice3 == currPrice)
                            {
                                disCount = quotesLast.BuyCount3;
                            }
                            else if (quotesLast.BuyPrice4 == currPrice)
                            {
                                disCount = quotesLast.BuyCount4;
                            }
                            else if (quotesLast.BuyPrice5 == currPrice)
                            {
                                disCount = quotesLast.BuyCount5;
                            }
                            else
                            {
                                if (quotesLast.BuyPrice1 > currPrice)
                                {
                                    rate = 0x0FFFFFFF;
                                }
                                else
                                {
                                    rate = -100;
                                }
                            }
                            break;
                        case 2:
                            if (quotesPre.SellPrice1 == currPrice)
                            {
                                lastCount = quotesPre.SellCount1;
                            }
                            else if (quotesPre.SellPrice2 == currPrice)
                            {
                                lastCount = quotesPre.SellCount2;
                            }
                            else if (quotesPre.SellPrice3 == currPrice)
                            {
                                lastCount = quotesPre.SellCount3;
                            }
                            else if (quotesPre.SellPrice4 == currPrice)
                            {
                                lastCount = quotesPre.SellCount4;
                            }
                            else if (quotesPre.SellPrice5 == currPrice)
                            {
                                lastCount = quotesPre.SellCount5;
                            }
                            else
                            {
                                return -1;
                            }
                            if (quotesLast.SellPrice1 == currPrice)
                            {
                                disCount = quotesLast.SellCount1;
                            }
                            else if (quotesLast.SellPrice2 == currPrice)
                            {
                                disCount = quotesLast.SellCount2;
                            }
                            else if (quotesLast.SellPrice3 == currPrice)
                            {
                                disCount = quotesLast.SellCount3;
                            }
                            else if (quotesLast.SellPrice4 == currPrice)
                            {
                                disCount = quotesLast.SellCount4;
                            }
                            else if (quotesLast.SellPrice5 == currPrice)
                            {
                                disCount = quotesLast.SellCount5;
                            }
                            else
                            {
                                if (quotesLast.SellPrice1 > currPrice)
                                {
                                    rate = -100;
                                }
                                else
                                {
                                    rate = 0x0FFFFFFF;
                                }
                            }
                            break;
                        default:
                            return -1;
                    }

                    if (rate == 0)
                    {
                        if (lastCount == 0)
                        {
                            logDes = string.Format(logDes, "条件不成立(上一次五档找不到目标价格)");
                            return -1;
                        }
                        rate = (disCount - lastCount) * 1.0 / lastCount * 100;
                    }


                    if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                    {
                        logDes = string.Format(logDes, "条件成立(目标价在上一次五档挂单数量:" + lastCount + ",在本次五档挂单数量:" + disCount + ")");
                        return 0;
                    }
                    logDes = string.Format(logDes, "条件不成立(目标价在上一次五档挂单数量:" + lastCount + ",在本次五档挂单数量:" + disCount + ")");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析五档变化速度出错了", ex);
                logDes = string.Format(logDes, "条件不成立(异常错误)");
                return -1;
            }
            finally
            {
                if (logRecord != null)
                {
                    logRecord.Append(logDes);
                }
            }
        }

        //分析箱体上涨
        public static int Analysis_Trend3_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            DateTime timeNow = DateTime.Now;
            List<TREND_RESULT_BOX_BREACH> resultInfo_Trend3 = new List<TREND_RESULT_BOX_BREACH>();
            int errorCode_Trend3 = DataHelper.Analysis_Trend3(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=sharesCode,
                                        Market=market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend3);
            DateTime? resultPushTime = null;
            if (errorCode_Trend3 == 0)
            {
                var temp = resultInfo_Trend3.Where(e => e.strStockCode == (sharesCode + "," + market)).FirstOrDefault();
                if (!string.IsNullOrEmpty(temp.strStockCode))
                {
                    var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                    resultPushTime = pushTime;
                    if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                    {
                        return 0;
                    }
                }
            }
            return -1;
        }
        private static int _Analysis_Trend1(List<OptionalTrend> list, ref List<TREND_RESULT_BOX_BREACH> resultInfo)
        {
            try
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
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析箱体上涨出错了", ex);
                return -1;
            }
        }

        //分析多头上涨
        public static int Analysis_Trend2_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            DateTime timeNow = DateTime.Now;
            List<TREND_RESULT_LINE_UP> resultInfo_Trend2 = new List<TREND_RESULT_LINE_UP>();
            int errorCode_Trend2 = DataHelper.Analysis_Trend2(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=sharesCode,
                                        Market=market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend2);
            DateTime? resultPushTime = null;
            if (errorCode_Trend2 == 0)
            {
                var temp = resultInfo_Trend2.Where(e => e.strStockCode == (sharesCode + "," + market)).FirstOrDefault();
                if (!string.IsNullOrEmpty(temp.strStockCode))
                {
                    var pushTime = temp.upOrDownInfo.lastestInfo.dtTradeTime;
                    resultPushTime = pushTime;
                    if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                    {
                        return 0;
                    }
                }
            }
            return -1;
        }
        private static int _Analysis_Trend2(List<OptionalTrend> list, ref List<TREND_RESULT_LINE_UP> resultInfo)
        {
            try
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
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析多头上涨出错了", ex);
                return -1;
            }
        }

        //分析快速上涨
        public static int Analysis_Trend1_New(string sharesCode, int market, List<string> par, StringBuilder logRecord = null)
        {
            DateTime timeNow = DateTime.Now;
            List<TREND_RESULT_RAPID_UP> resultInfo_Trend1 = new List<TREND_RESULT_RAPID_UP>();
            int errorCode_Trend1 = DataHelper.Analysis_Trend1(new List<OptionalTrend>
                                {
                                    new OptionalTrend
                                    {
                                        SharesCode=sharesCode,
                                        Market=market,
                                        ParList=par
                                    }
                                }, ref resultInfo_Trend1);
            DateTime? resultPushTime = null;
            if (errorCode_Trend1 == 0)
            {
                var temp = resultInfo_Trend1.Where(e => e.strStockCode == (sharesCode + "," + market)).FirstOrDefault();
                if (!string.IsNullOrEmpty(temp.strStockCode) && temp.dicUpOrDownInfo.Count() > 0)
                {
                    var tempModel = temp.dicUpOrDownInfo.FirstOrDefault();
                    var pushTime = tempModel.Value.lastestInfo.dtTradeTime;
                    resultPushTime = pushTime;
                    if (pushTime >= DateTime.Parse(timeNow.ToString("yyyy-MM-dd HH:mm:00")))
                    {
                        return 0;
                    }
                }
            }
            return -1;
        }
        private static int _Analysis_Trend1(List<OptionalTrend> list, ref List<TREND_RESULT_RAPID_UP> resultInfo)
        {
            try
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
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析快速上涨出错了", ex);
                return -1;
            }
        }

        /// <summary>
        /// 触发
        /// </summary>
        private static void TrendTri(List<TrendAnalyseResult> trendResult)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Market", typeof(int));
            dataTable.Columns.Add("SharesCode", typeof(string));
            dataTable.Columns.Add("PushTime", typeof(DateTime));
            dataTable.Columns.Add("PushDesc", typeof(string));
            dataTable.Columns.Add("TrendId", typeof(long));

            trendResult = (from item in trendResult
                           group item by new { item.Market, item.SharesCode,item.TrendId } into g
                           select g.FirstOrDefault()).ToList();

            foreach (var item in trendResult)
            {
                DataRow row = dataTable.NewRow();
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["PushTime"] = item.PushTime;
                row["PushDesc"] = item.PushDesc;
                row["TrendId"] = item.TrendId;
                dataTable.Rows.Add(row);
            }
            using (SqlConnection conn = new SqlConnection(Singleton.Instance.connString_meal_ticket))
            {
                conn.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("P_TradeAnalyseResult_Update", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //关键是类型
                        SqlParameter parameter = new SqlParameter("@analyseResult", SqlDbType.Structured);
                        //必须指定表类型名
                        parameter.TypeName = "dbo.TrendAnalyseResult";
                        //赋值
                        parameter.Value = dataTable;
                        cmd.Parameters.Add(parameter);
                        cmd.ExecuteNonQuery();
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

        //分析价格条件-批量
        public static List<SharesBase_Session> Analysis_Price_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                int priceType = 0;
                int compare = 0;
                long priceError = 0;

                priceType = Convert.ToInt32(temp.PriceType);
                compare = Convert.ToInt32(temp.Compare);

                try
                {
                    priceError = (long)(Convert.ToDouble(temp.PriceError) * 10000);
                }
                catch (Exception)
                {
                }

                string sql = @"select Market,SharesCode,ClosedPrice,PresentPrice,LimitUpPrice,LimitDownPrice
from v_shares_quotes_last with(nolock)";
                List<QuotesDateInfo> quotes_list = new List<QuotesDateInfo>();
                using (var db = new meal_ticketEntities())
                {
                    quotes_list = db.Database.SqlQuery<QuotesDateInfo>(sql).ToList();
                }

                ThreadMsgTemplate<SharesBase_Session> data = new ThreadMsgTemplate<SharesBase_Session>();
                data.Init();
                foreach (var item in sharesList)
                {
                    data.AddMessage(item);
                }

                List<SharesBase_Session> result = new List<SharesBase_Session>();
                object dataLock = new object();

                int defaultCount = Singleton.Instance.MaxTrendCheckTaskCount;
                int taskCount = data.GetCount();
                if (taskCount > defaultCount)
                {
                    taskCount = defaultCount;
                }

                Task[] taskArr = new Task[taskCount];
                for (int index = 0; index < taskCount; index++)
                {
                    taskArr[index] = new Task(() =>
                    {
                        do
                        {
                            SharesBase_Session tempData = new SharesBase_Session();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }
                            var quotes = quotes_list.Where(e => e.Market == tempData.Market && e.SharesCode == tempData.SharesCode).FirstOrDefault();
                            if (quotes == null)
                            {
                                continue;
                            }
                            long disPrice = priceType == 1 ? (quotes.LimitUpPrice + priceError)
                                : priceType == 2 ? (quotes.LimitDownPrice + priceError)
                                : priceType == 3 ? (long)(quotes.ClosedPrice * 1.0 / 100 * priceError/10000 + quotes.ClosedPrice)
                                : 0;
                            if (disPrice <= 0 || quotes.PresentPrice <= 0)
                            {
                                continue;
                            }
                            if ((compare == 1 && quotes.PresentPrice >= disPrice) || (compare == 2 && quotes.PresentPrice <= disPrice))
                            {
                                lock (dataLock)
                                {
                                    result.Add(new SharesBase_Session
                                    {
                                        Market = tempData.Market,
                                        SharesCode = tempData.SharesCode
                                    });
                                }
                            }
                        } while (true);
                    }, TaskCreationOptions.LongRunning);
                    taskArr[index].Start();
                }
                Task.WaitAll(taskArr);
                data.Release();
                return result;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_Price_New_Batch出错了", ex);
                return new List<SharesBase_Session>();
            }
        }

        //分析时间段-批量
        public static List<SharesBase_Session> Analysis_TimeSlot_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            DateTime timeNow = DateTime.Now;
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                JArray timeList = temp.Times;
                if (timeNow >= DateTime.Parse(timeList[0].ToString()) && timeNow < DateTime.Parse(timeList[1].ToString()))
                {
                    return sharesList;
                }
                return new List<SharesBase_Session>();
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析时间段出错了", ex);
                return new List<SharesBase_Session>();
            }
        }

        //分析历史涨跌幅数据-批量
        public static List<SharesBase_Session> Analysis_HisRiseRate_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                int day = 0;
                int type = 0;
                int compare = 0;
                int count = 0;
                int rateCompare = 0;
                int rateCount = 0;
                int limitDay = 0;
                bool flatRise = false;
                bool triPrice = false;
                try
                {
                    day = Convert.ToInt32(temp.Day);
                }
                catch (Exception) { }
                try
                {
                    type = Convert.ToInt32(temp.Type);
                }
                catch (Exception) { }
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                }
                catch (Exception) { }
                try
                {
                    count = (type == 11 || type == 12 || type == 14) ? (int)(Convert.ToDouble(temp.Rise) * 100) : Convert.ToInt32(temp.Count);
                }
                catch (Exception) { }
                try
                {
                    rateCompare = Convert.ToInt32(temp.RateCompare);
                }
                catch (Exception) { }
                try
                {
                    rateCount = (int)(Convert.ToDouble(temp.RateCount) * 100);
                }
                catch (Exception) { }
                try
                {
                    limitDay = Convert.ToInt32(temp.LimitDay);
                }
                catch (Exception) { }
                try
                {
                    triPrice = Convert.ToBoolean(temp.TriPrice);
                }
                catch (Exception) { }
                try
                {
                    flatRise = Convert.ToBoolean(temp.FlatRise);
                }
                catch (Exception) { }

                if (day <= 0)
                {
                    throw new Exception("参数day错误");
                }

                string sql = string.Format(@"declare @currTime datetime,@currDate datetime;
  set @currTime=getdate();
  set @currDate=convert(varchar(10), @currTime, 23)

  declare @minDate datetime;
  set @minDate=dbo.f_getTradeDate(@currDate,-{0});
 
  select t.Market,t.SharesCode,t.PriceType,t.TriPriceType,t.LimitUpCount,t.PresentPrice,t.ClosedPrice,
  case when t.ClosedPrice<=0 then 0 else convert(int,(t.PresentPrice-t.ClosedPrice)*1.0/t.ClosedPrice*10000) end RiseRate,t.[Date]
  from t_shares_quotes_date t with(nolock)
  where t.LastModified<@currDate and t.LastModified>@minDate", day);
                List<QuotesDateInfo> quotes_date_list = new List<QuotesDateInfo>();
                using (var db = new meal_ticketEntities())
                {
                    quotes_date_list = db.Database.SqlQuery<QuotesDateInfo>(sql).ToList();
                }

                ThreadMsgTemplate<SharesBase_Session> data = new ThreadMsgTemplate<SharesBase_Session>();
                data.Init();
                foreach (var item in sharesList)
                {
                    data.AddMessage(item);
                }

                List<SharesBase_Session> result = new List<SharesBase_Session>();
                object dataLock = new object();


                int defaultCount = Singleton.Instance.MaxTrendCheckTaskCount;
                int taskCount = data.GetCount();
                if (taskCount > defaultCount)
                {
                    taskCount = defaultCount;
                }

                Task[] taskArr = new Task[taskCount];
                for (int index = 0; index < taskCount; index++)
                {
                    taskArr[index] = new Task(() =>
                    {
                        do
                        {
                            SharesBase_Session tempData = new SharesBase_Session();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }
                            var quotes_date = quotes_date_list.Where(e => e.Market == tempData.Market && e.SharesCode == tempData.SharesCode).ToList();
                            //没涨停次数
                            if (type == 1)
                            {
                                int tempCount = quotes_date.Where(e => e.PriceType != 1).Count();
                                if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode= tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            //涨停次数
                            else if (type == 2)
                            {
                                int tempCount = 0;
                                if (triPrice)
                                {
                                    tempCount = quotes_date.Where(e => e.TriPriceType == 1).Count();
                                }
                                else
                                {
                                    tempCount = quotes_date.Where(e => e.PriceType == 1).Count();
                                }

                                if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            //跌停次数
                            else if (type == 3)
                            {
                                int tempCount = 0;
                                if (triPrice)
                                {
                                    tempCount = quotes_date.Where(e => e.TriPriceType == 2).Count();
                                }
                                else
                                {
                                    tempCount = quotes_date.Where(e => e.PriceType == 2).Count();
                                }
                                if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            //炸板次数
                            else if (type == 4)
                            {
                                int tempCount = quotes_date.Where(e => e.PriceType != 1 && e.LimitUpCount > 0).Count();
                                if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            //每日涨跌幅
                            else if (type == 13)
                            {
                                int i = 0;
                                foreach (var quote in quotes_date)
                                {
                                    if (quote.PresentPrice <= 0 || quote.ClosedPrice <= 0)
                                    {
                                        continue;
                                    }
                                    int rate = (int)((quote.PresentPrice - quote.ClosedPrice) * 1.0 / quote.ClosedPrice * 10000);
                                    if (!flatRise && rate == 0)
                                    {
                                        continue;
                                    }
                                    if (rateCompare == 1 && rate >= rateCount)
                                    {
                                        i++;
                                        continue;
                                    }
                                    if (rateCompare == 2 && rate <= rateCount)
                                    {
                                        i++;
                                        continue;
                                    }
                                }
                                if ((compare == 1 && i >= count) || (compare == 2 && i <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            //包含连续涨停
                            else if (type == 7)
                            {
                                int tempi = 0;//连续涨停天数
                                foreach (var quote in quotes_date)
                                {
                                    if (quote.PriceType == 1)
                                    {
                                        tempi++;
                                    }
                                    else
                                    {
                                        tempi = 0;
                                    }
                                    if (tempi >= limitDay)
                                    {
                                        lock (dataLock)
                                        {
                                            result.Add(new SharesBase_Session
                                            {
                                                Market = tempData.Market,
                                                SharesCode = tempData.SharesCode
                                            });
                                        }
                                        break ;
                                    }
                                }
                            }
                            //排除连续涨停
                            else if (type == 8)
                            {
                                int tempi = 0;//连续涨停天数
                                foreach (var quote in quotes_date)
                                {
                                    if (quote.PriceType == 1)
                                    {
                                        tempi++;
                                    }
                                    else
                                    {
                                        tempi = 0;
                                    }
                                    if (tempi >= limitDay)
                                    {
                                        break;
                                    }
                                }
                                lock (dataLock)
                                {
                                    result.Add(new SharesBase_Session
                                    {
                                        Market = tempData.Market,
                                        SharesCode = tempData.SharesCode
                                    });
                                }
                            }
                            //包含连续跌停
                            else if (type == 9)
                            {
                                int tempi = 0;//连续跌停天数
                                foreach (var quote in quotes_date)
                                {
                                    if (quote.PriceType == 2)
                                    {
                                        tempi++;
                                    }
                                    else
                                    {
                                        tempi = 0;
                                    }
                                    if (tempi >= limitDay)
                                    {
                                        lock (dataLock)
                                        {
                                            result.Add(new SharesBase_Session
                                            {
                                                Market = tempData.Market,
                                                SharesCode = tempData.SharesCode
                                            });
                                        }
                                        break;
                                    }
                                }
                            }
                            //排除连续跌停
                            else if (type == 10)
                            {
                                int tempi = 0;//连续跌停天数
                                foreach (var quote in quotes_date)
                                {
                                    if (quote.PriceType == 2)
                                    {
                                        tempi++;
                                    }
                                    else
                                    {
                                        tempi = 0;
                                    }
                                    if (tempi >= limitDay)
                                    {
                                        break;
                                    }
                                }
                                lock (dataLock)
                                {
                                    result.Add(new SharesBase_Session
                                    {
                                        Market = tempData.Market,
                                        SharesCode = tempData.SharesCode
                                    });
                                }
                            }
                            //总涨跌幅
                            else if (type == 14)
                            {
                                long closePrice = quotes_date.OrderBy(e => e.Date).Select(e => e.ClosedPrice).FirstOrDefault();
                                long presentPrice = quotes_date.OrderByDescending(e => e.Date).Select(e => e.PresentPrice).FirstOrDefault();
                                if (closePrice <= 0 || presentPrice <= 0)
                                {
                                    continue;
                                }
                                int rate = (int)((presentPrice - closePrice) * 1.0 / closePrice * 10000);
                                if (!flatRise && rate == 0)
                                {
                                    continue;
                                }
                                if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                        } while (true);
                    },TaskCreationOptions.LongRunning);
                    taskArr[index].Start();
                }
                Task.WaitAll(taskArr);
                data.Release();
                return result;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_HisRiseRate_New_Batch出错了", ex);
                return new List<SharesBase_Session>();
            }
        }

        //分析今日涨跌幅-批量
        public static List<SharesBase_Session> Analysis_TodayRiseRate_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                int type = 0;
                int compare = 0;
                long count = 0;
                bool triPrice = false;
                int firstDay = 0;
                int secondDay = 0;
                double multiple = 0;
                int compare2 = 0;
                int dayShortageType = 0;
                try
                {
                    type = Convert.ToInt32(temp.Type);
                }
                catch (Exception) { }
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                }
                catch (Exception ex) { }
                try
                {
                    count = (type == 5 || type == 6) ? (int)(Convert.ToDouble(temp.Count) * 100) : Convert.ToInt64(temp.Count);
                }
                catch (Exception ex) { }
                try
                {
                    triPrice = Convert.ToBoolean(temp.TriPrice);
                }
                catch (Exception) { }
                try
                {
                    firstDay = Convert.ToInt32(temp.FirstDay);
                }
                catch (Exception) { }
                try
                {
                    secondDay = Convert.ToInt32(temp.SecondDay);
                }
                catch (Exception) { }
                try
                {
                    multiple = Convert.ToDouble(temp.Multiple);
                }
                catch (Exception) { }
                try
                {
                    compare2 = Convert.ToInt32(temp.Compare2);
                }
                catch (Exception) { }
                try
                {
                    dayShortageType = Convert.ToInt32(temp.DayShortageType);
                }
                catch (Exception) { }

                string sql = @"select Market,SharesCode,LimitUpCount,TriLimitUpCount,LimitDownCount,TriLimitDownCount,LimitUpBombCount,OpenedPrice,ClosedPrice,MaxPrice,MinPrice,TotalAmount,TotalCount 
from t_shares_quotes_date with(nolock)
where [Date] = convert(varchar(10), getdate(), 120)";
                List<QuotesDateInfo> quotes_date_list = new List<QuotesDateInfo>();
                List<dynamic> dealList = new List<dynamic>();
                using (var db = new meal_ticketEntities())
                {
                    quotes_date_list = db.Database.SqlQuery<QuotesDateInfo>(sql).ToList();
                    if (type == 7)
                    {
                        sql = string.Format(@"declare @currTime datetime,@currDate datetime;
set @currTime=getdate();
set @currDate=convert(varchar(10), @currTime, 23)

declare @firstDayTime datetime,@secondDayTime datetime;
set @firstDayTime=dbo.f_getTradeDate(@currDate,-{0});
set @secondDayTime=dbo.f_getTradeDate(@currDate,-{1});

select Market,SharesCode,convert(bigint,avg(TotalAmount)) AvgTotalAmount,count(*) DateCount
from t_shares_quotes_date with(nolock)
where LastModified>@secondDayTime and LastModified<@firstDayTime
group by Market,SharesCode",firstDay<=0?0: firstDay-1, secondDay);
                        dealList = db.Database.SqlQuery<dynamic>(sql).ToList();
                    }
                    else if (type == 8)
                    {
                        sql = string.Format(@"declare @currTime datetime,@currDate datetime;
set @currTime=getdate();
set @currDate=convert(varchar(10), @currTime, 23)

declare @firstDayTime datetime,@secondDayTime datetime;
set @firstDayTime=dbo.f_getTradeDate(@currDate,-{0});
set @secondDayTime=dbo.f_getTradeDate(@currDate,-{1});

select Market,SharesCode,convert(bigint,avg(TotalCount)) AvgTotalCount,count(*) DateCount
from t_shares_quotes_date with(nolock)
where LastModified>@secondDayTime and LastModified<@firstDayTime
group by Market,SharesCode", firstDay <= 0 ? 0 : firstDay - 1, secondDay);
                        dealList = db.Database.SqlQuery<dynamic>(sql).ToList();
                    }
                }

                ThreadMsgTemplate<SharesBase_Session> data = new ThreadMsgTemplate<SharesBase_Session>();
                data.Init();
                foreach (var item in sharesList)
                {
                    data.AddMessage(item);
                }

                List<SharesBase_Session> result = new List<SharesBase_Session>();
                object dataLock = new object();

                int defaultCount = Singleton.Instance.MaxTrendCheckTaskCount;
                int taskCount = data.GetCount();
                if (taskCount > defaultCount)
                {
                    taskCount = defaultCount;
                }

                Task[] taskArr = new Task[taskCount];
                for (int index = 0; index < taskCount; index++)
                {
                    taskArr[index] = new Task(() =>
                    {
                        do
                        {
                            SharesBase_Session tempData = new SharesBase_Session();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }
                            var quotes_date = quotes_date_list.Where(e => e.Market == tempData.Market && e.SharesCode == tempData.SharesCode).FirstOrDefault();
                            if (quotes_date == null)
                            {
                                continue;
                            }
                            //涨停次数
                            if (type == 1)
                            {
                                int currCount = triPrice ? quotes_date.TriLimitUpCount : quotes_date.LimitUpCount;
                                if ((compare == 1 && currCount >= count) || (compare == 2 && currCount <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            //跌停次数
                            else if (type == 2)
                            {
                                int currCount = triPrice ? quotes_date.TriLimitDownCount : quotes_date.LimitDownCount;
                                if ((compare == 1 && currCount >= count) || (compare == 2 && currCount <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            //炸板次数
                            else if (type == 3)
                            {
                                if ((compare == 1 && quotes_date.LimitUpBombCount >= count) || (compare == 2 && quotes_date.LimitUpBombCount <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            //盘中涨跌幅
                            else if (type == 4)
                            {
                                int i = -1;
                                foreach (var item in temp.riseRateList)
                                {
                                    UP_OR_DOWN_INFO upOrDownInfo = new UP_OR_DOWN_INFO();
                                    DB_TRADE_PRICE_INFO DB_TRADE_PRICE_INFO = new DB_TRADE_PRICE_INFO();
                                    int Minute = item.Minute;
                                    int Compare = item.Compare;
                                    double Rate = item.Rate;
                                    int iErrorCode = Singleton.Instance.m_stockMonitor.GetUpOrDownInfoInTime(tempData.SharesCode + "," + tempData.Market, Minute, ref DB_TRADE_PRICE_INFO, ref upOrDownInfo);
                                    if (iErrorCode != 0)
                                    {
                                        Logger.WriteFileLog(iErrorCode + "" + JsonConvert.SerializeObject(upOrDownInfo), null);
                                        continue;
                                    }
                                    if (upOrDownInfo.iLastestPercent == -0x0FFFFFFF)
                                    {
                                        continue;
                                    }
                                    if (Compare == 1 && upOrDownInfo.iLastestPercent >= Rate * 100)
                                    {
                                        i++;
                                        break;
                                    }
                                    if (Compare == 2 && upOrDownInfo.iLastestPercent <= Rate * 100)
                                    {
                                        i++;
                                        break;
                                    }
                                }
                                if (i == 0)
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            //开盘涨跌幅
                            else if (type == 5)
                            {
                                long openPrice = quotes_date.OpenedPrice;
                                long closePrice = quotes_date.ClosedPrice;
                                if (closePrice <= 0 || openPrice <= 0)
                                {
                                    continue;
                                }
                                int rate = (int)((openPrice - closePrice) * 1.0 / closePrice * 10000);
                                if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            //振幅
                            else if (type == 6)
                            {
                                long maxPrice = quotes_date.MaxPrice;
                                long minPrice = quotes_date.MinPrice;
                                long closePrice = quotes_date.ClosedPrice;
                                if (closePrice <= 0 || maxPrice <= 0 || minPrice <= 0)
                                {
                                    continue;
                                }
                                int rate = (int)((maxPrice - minPrice) * 1.0 / closePrice * 10000);
                                if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            //成交额
                            else if (type == 7)
                            {
                                var deal = dealList.Where(e => e.Market == quotes_date.Market && e.SharesCode == quotes_date.SharesCode).FirstOrDefault();
                                if (deal == null)
                                {
                                    continue;
                                }
                                int quotesCount = deal.DateCount;
                                if (dayShortageType == 2 && quotesCount < firstDay)
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                                if (dayShortageType == 3 && quotesCount < firstDay)
                                {
                                    continue;
                                }
                               
                                long avgResult = deal.AvgTotalAmount;
                                if ((compare == 1 && quotes_date.TotalAmount >= avgResult * multiple) || (compare == 2 && quotes_date.TotalAmount <= avgResult * multiple))
                                {
                                    if ((compare2 == 1 && avgResult >= count * 10000) || (compare2 == 2 && avgResult <= count * 10000))
                                    {
                                        lock (dataLock)
                                        {
                                            result.Add(new SharesBase_Session
                                            {
                                                Market = tempData.Market,
                                                SharesCode = tempData.SharesCode
                                            });
                                        }
                                    }
                                }
                            }
                            //成交量
                            else if (type == 8)
                            {
                                var deal = dealList.Where(e => e.Market == quotes_date.Market && e.SharesCode == quotes_date.SharesCode).FirstOrDefault();
                                if (deal == null)
                                {
                                    continue;
                                }
                                int quotesCount = deal.DateCount;
                                if (dayShortageType == 2 && quotesCount < firstDay)
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                                if (dayShortageType == 3 && quotesCount < firstDay)
                                {
                                    continue;
                                }

                                long avgResult = deal.AvgTotalCount;
                                if ((compare == 1 && quotes_date.TotalCount >= avgResult * multiple) || (compare == 2 && quotes_date.TotalCount <= avgResult * multiple))
                                {
                                    if ((compare2 == 1 && avgResult * 100 >= count) || (compare2 == 2 && avgResult * 100 <= count))
                                    {
                                        lock (dataLock)
                                        {
                                            result.Add(new SharesBase_Session
                                            {
                                                Market = tempData.Market,
                                                SharesCode = tempData.SharesCode
                                            });
                                        }
                                    }
                                }
                            }
                        } while (true);
                    }, TaskCreationOptions.LongRunning);
                    taskArr[index].Start();
                }
                Task.WaitAll(taskArr);
                data.Release();
                return result;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_TodayRiseRate_New_Batch出错了", ex);
                return new List<SharesBase_Session>();
            }
        }

        //分析板块涨跌幅
        public static List<SharesBase_Session> Analysis_PlateRiseRate_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            try
            {
                List<SharesBase_Session> resultList = new List<SharesBase_Session>();
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                int connect = 0;
                List<long> plateList = new List<long>();
                connect = temp.PlateInfoJson.Connect;
                plateList= temp.PlateInfoJson.PlateList.ToObject<List<long>>();

                int compare = 0;
                int riseRate = 0;
                try
                {
                    compare=temp.Compare;
                } 
                catch (Exception ex) { }
                try
                {
                    riseRate = Convert.ToInt32(Convert.ToDouble(temp.RiseRate) * 100);
                }
                catch (Exception ex) { }

                using (var db = new meal_ticketEntities())
                {
                    var totalPlateList = (from item in Singleton.Instance._SharesPlateSession.GetSessionData()
                                          join item2 in Singleton.Instance._SharesPlateQuotesSession.GetSessionData() on item.PlateId equals item2.PlateId
                                          where plateList.Contains(item.PlateId)
                                          select new { item, item2 }).ToList();
                    var sharesPlate = (from item in Singleton.Instance._SharesPlateRelSession.GetSessionData()
                                       where plateList.Contains(item.PlateId)
                                       select item).ToList();
                    var result = (from item in sharesList
                                  join item2 in sharesPlate on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                  join item3 in totalPlateList on item2.PlateId equals item3.item.PlateId
                                  where compare == 0 || (compare == 1 && item3.item2.RiseRate >= riseRate) || (compare == 2 && item3.item2.RiseRate <= riseRate)
                                  group item by item into g
                                  select new
                                  {
                                      Market = g.Key.Market,
                                      SharesCode = g.Key.SharesCode,
                                      Count = g.Count()
                                  }).ToList();
                    if (connect == 1)//或
                    {
                        resultList = (from item in result
                                      select new SharesBase_Session
                                      {
                                          Market = item.Market,
                                          SharesCode = item.SharesCode
                                      }).ToList();
                    }
                    else if (connect == 2)//且
                    {
                        int plateCount = totalPlateList.Count();
                        resultList = (from item in result
                                      where item.Count>= plateCount
                                      select new SharesBase_Session
                                      {
                                          Market = item.Market,
                                          SharesCode = item.SharesCode
                                      }).ToList();
                    }
                    else
                    {
                        throw new Exception("参数有误");
                    }
                    return resultList;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_PlateRiseRate_New出错了", ex);
                return new List<SharesBase_Session>();
            }
        }

        //分析买卖单占比-批量
        public static List<SharesBase_Session> Analysis_BuyOrSellCount_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                int type = 0;
                int compare = 0;
                double count = 0;
                int countType = 0;

                type = Convert.ToInt32(temp.Type);
                compare = Convert.ToInt32(temp.Compare);
                count = Convert.ToDouble(temp.Count);
                countType = Convert.ToInt32(temp.CountType);

                List<QuotesDateInfo> quotes_list = (from item in Singleton.Instance._SharesQuotesSession.GetSessionData()
                                                    join item2 in sharesList on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                                    select new QuotesDateInfo
                                                    {
                                                        Market = item.Market,
                                                        SharesCode = item.SharesCode,
                                                        BuyCount1 = item.BuyCount1,
                                                        BuyCount2 = item.BuyCount2,
                                                        BuyCount3 = item.BuyCount3,
                                                        BuyCount4 = item.BuyCount4,
                                                        BuyCount5 = item.BuyCount5,
                                                        SellCount1 = item.SellCount1,
                                                        SellCount2 = item.SellCount2,
                                                        SellCount3 = item.SellCount3,
                                                        SellCount4 = item.SellCount4,
                                                        SellCount5 = item.SellCount5,
                                                    }).ToList();
                ThreadMsgTemplate<SharesBase_Session> data = new ThreadMsgTemplate<SharesBase_Session>();
                data.Init();
                foreach (var item in sharesList)
                {
                    data.AddMessage(item);
                }

                List<SharesBase_Session> result = new List<SharesBase_Session>();
                object dataLock = new object();

                int defaultCount = Singleton.Instance.MaxTrendCheckTaskCount;
                int taskCount = data.GetCount();
                if (taskCount > defaultCount)
                {
                    taskCount = defaultCount;
                }

                Task[] taskArr = new Task[taskCount];
                for (int index = 0; index < taskCount; index++)
                {
                    taskArr[index] = new Task(() =>
                    {
                        do
                        {
                            SharesBase_Session tempData = new SharesBase_Session();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }
                            var quotes = quotes_list.Where(e => e.Market == tempData.Market && e.SharesCode == tempData.SharesCode).FirstOrDefault();
                            if (quotes == null)
                            {
                                continue;
                            }
                            int realCount;
                            string realCountDes;
                            switch (type)
                            {
                                case 1:
                                    realCount = quotes.BuyCount1;
                                    realCountDes = "买一挂单";
                                    break;
                                case 2:
                                    realCount = quotes.BuyCount2;
                                    realCountDes = "买二挂单";
                                    break;
                                case 3:
                                    realCount = quotes.BuyCount3;
                                    realCountDes = "买三挂单";
                                    break;
                                case 4:
                                    realCount = quotes.BuyCount4;
                                    realCountDes = "买四挂单";
                                    break;
                                case 5:
                                    realCount = quotes.BuyCount5;
                                    realCountDes = "买五挂单";
                                    break;
                                case 6:
                                    realCount = quotes.SellCount1;
                                    realCountDes = "卖一挂单";
                                    break;
                                case 7:
                                    realCount = quotes.SellCount2;
                                    realCountDes = "卖二挂单";
                                    break;
                                case 8:
                                    realCount = quotes.SellCount3;
                                    realCountDes = "卖三挂单";
                                    break;
                                case 9:
                                    realCount = quotes.SellCount4;
                                    realCountDes = "卖四挂单";
                                    break;
                                case 10:
                                    realCount = quotes.SellCount5;
                                    realCountDes = "卖五挂单";
                                    break;
                                default:
                                    continue;
                            }
                            realCount = realCount * 100;

                            if (countType == 1)//流通股百分比
                            {
                                //查询流通股
                                var sharesInfo = (from item in Singleton.Instance._SharesBaseSession.GetSessionData()
                                                  where item.Market == tempData.Market && item.SharesCode == tempData.SharesCode
                                                  select item).FirstOrDefault();
                                if (sharesInfo == null)
                                {
                                    continue;
                                }
                                long circulatingCapital = sharesInfo.CirculatingCapital;
                                long setCount = (long)(circulatingCapital * (count / 100));

                                if ((compare == 1 && realCount >= setCount) || (compare == 2 && realCount <= setCount))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            else if (countType == 2)//股数
                            {
                                if ((compare == 1 && realCount >= count) || (compare == 2 && realCount <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            else if (countType == 3)//流通市值百分比
                            {

                            }
                        } while (true);
                    }, TaskCreationOptions.LongRunning);
                    taskArr[index].Start();
                }
                Task.WaitAll(taskArr);
                data.Release();
                return result;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_BuyOrSellCount_New_Batch出错了", ex);
                return new List<SharesBase_Session>();
            }
        }

        //分析按参照价格-批量
        public static List<SharesBase_Session> Analysis_ReferPrice_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            DateTime dateNow = DateTime.Now.Date;
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                int compare = 0;
                int day = 0;
                double count = 0;
                int priceType = 0;

                compare = Convert.ToInt32(temp.Compare);
                day = Convert.ToInt32(temp.Day);
                priceType = Convert.ToInt32(temp.PriceType);

                try
                {
                    count = Convert.ToDouble(temp.Count);
                }
                catch (Exception) { }


                List<QuotesDateInfo> quotes_date_list = new List<QuotesDateInfo>();
                string sql = string.Format(@"declare @currTime datetime,@currDate datetime;
  set @currTime=getdate();
  set @currDate=convert(varchar(10), @currTime, 23)

  declare @minDate datetime;
  set @minDate=dbo.f_getTradeDate(@currDate,-{0});
 
  select t.Market,t.SharesCode,t.PresentPrice,t.OpenedPrice,t.MaxPrice,t.MinPrice,t.LastModified
  from t_shares_quotes_date t with(nolock)
  where t.LastModified>@minDate", day);
                using (var db = new meal_ticketEntities())
                {
                    quotes_date_list = db.Database.SqlQuery<QuotesDateInfo>(sql).ToList();
                }

                ThreadMsgTemplate<SharesBase_Session> data = new ThreadMsgTemplate<SharesBase_Session>();
                data.Init();
                foreach (var item in sharesList)
                {
                    data.AddMessage(item);
                }

                List<SharesBase_Session> result = new List<SharesBase_Session>();
                object dataLock = new object();

                int defaultCount = Singleton.Instance.MaxTrendCheckTaskCount;
                int taskCount = data.GetCount();
                if (taskCount > defaultCount)
                {
                    taskCount = defaultCount;
                }

                Task[] taskArr = new Task[taskCount];
                for (int index = 0; index < taskCount; index++)
                {
                    taskArr[index] = new Task(() =>
                    {
                        do
                        {
                            SharesBase_Session tempData = new SharesBase_Session();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }
                            var quotes = quotes_date_list.Where(e=>e.Market== tempData.Market && e.SharesCode== tempData.SharesCode).ToList();
                            if (quotes.Count() <= 0)
                            {
                                continue;
                            }
                            var presentInfo = quotes.Where(e=>e.LastModified> dateNow).FirstOrDefault();
                            long presentPrice = presentInfo.PresentPrice;
                            if (presentPrice <= 0)
                            {
                                continue;
                            }

                            var lastInfo = quotes.Where(e => e.LastModified < dateNow).ToList();
                            long realPrice;
                            switch (priceType)
                            {
                                case 1:
                                    realPrice = compare == 1 ? lastInfo.Max(e => e.OpenedPrice) : lastInfo.Min(e => e.OpenedPrice);
                                    break;
                                case 2:
                                    realPrice = compare == 1 ? lastInfo.Max(e => e.PresentPrice) : lastInfo.Min(e => e.PresentPrice);
                                    break;
                                case 3:
                                    realPrice = compare == 1 ? lastInfo.Max(e => e.MaxPrice) : lastInfo.Min(e => e.MaxPrice);
                                    break;
                                case 4:
                                    realPrice = compare == 1 ? lastInfo.Max(e => e.MinPrice) : lastInfo.Min(e => e.MinPrice);
                                    break;
                                default:
                                    continue;
                            }

                            if (count != 0)//计算偏差
                            {
                                realPrice = (long)(realPrice * (count / 100) + realPrice);
                            }

                            if ((compare == 1 && presentPrice >= realPrice) || (compare == 2 && presentPrice <= realPrice))
                            {
                                lock (dataLock)
                                {
                                    result.Add(new SharesBase_Session
                                    {
                                        Market = tempData.Market,
                                        SharesCode = tempData.SharesCode
                                    });
                                }
                            }
                        } while (true);
                    }, TaskCreationOptions.LongRunning);
                    taskArr[index].Start();
                }
                Task.WaitAll(taskArr);
                data.Release();
                return result;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_ReferPrice_New_Batch出错了", ex);
                return new List<SharesBase_Session>();
            }
        }

        //分析按均线价格-批量
        public static List<SharesBase_Session> Analysis_ReferAverage_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            DateTime dateNow = DateTime.Now.Date;
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                int compare = 0;
                int day1 = 0;
                int day2 = 0;
                double count = 0;
                int upOrDown = 0;
                int dayShortageType = 1;

                compare = Convert.ToInt32(temp.Compare);
                day1 = Convert.ToInt32(temp.Day1);
              
                try
                {
                    day2 = Convert.ToInt32(temp.Day2);
                    day2 = day2 + 1;//多取一天
                    upOrDown = Convert.ToInt32(temp.UpOrDown);
                }
                catch (Exception)
                {

                }
                try
                {
                    dayShortageType = Convert.ToInt32(temp.DayShortageType);
                }
                catch (Exception)
                {
                }

                List<QuotesDateInfo> quotes_date_list = new List<QuotesDateInfo>();
                string sql = string.Format(@"declare @currTime datetime,@currDate datetime;
  set @currTime=getdate();
  set @currDate=convert(varchar(10), @currTime, 23)

  declare @minDate datetime;
  set @minDate=dbo.f_getTradeDate(@currDate,-{0});
 
  select t.Market,t.SharesCode,t.PresentPrice,t.OpenedPrice,t.MaxPrice,t.MinPrice,t.LastModified
  from t_shares_quotes_date t with(nolock)
  where t.LastModified>@minDate", day1 + day2);
                using (var db = new meal_ticketEntities())
                {
                    quotes_date_list = db.Database.SqlQuery<QuotesDateInfo>(sql).ToList();
                }

                ThreadMsgTemplate<SharesBase_Session> data = new ThreadMsgTemplate<SharesBase_Session>();
                data.Init();
                foreach (var item in sharesList)
                {
                    data.AddMessage(item);
                }

                List<SharesBase_Session> result = new List<SharesBase_Session>();
                object dataLock = new object();

                int defaultCount = Singleton.Instance.MaxTrendCheckTaskCount;
                int taskCount = data.GetCount();
                if (taskCount > defaultCount)
                {
                    taskCount = defaultCount;
                }

                Task[] taskArr = new Task[taskCount];
                for (int index = 0; index < taskCount; index++)
                {
                    taskArr[index] = new Task(() =>
                    {
                        do
                        {
                            SharesBase_Session tempData = new SharesBase_Session();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }
                            var quotes = quotes_date_list.Where(e => e.Market == tempData.Market && e.SharesCode == tempData.SharesCode).ToList();
                            if (quotes.Count() <= 0)
                            {
                                continue;
                            }
                            var presentInfo = quotes.Where(e => e.LastModified > dateNow).FirstOrDefault();
                            long presentPrice = presentInfo.PresentPrice;
                            if (presentPrice <= 0)
                            {
                                continue;
                            }
                            //计算均线价格
                            var list1 = quotes.Take(day1).ToList();
                            if (list1.Count() <= 0)
                            {
                                continue;
                            }
                            if (list1.Count() < day1)
                            {
                                if (dayShortageType == 2)
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                                if (dayShortageType == 3)
                                {
                                    continue;
                                }
                            }
                            long averagePrice = (long)list1.Average(e => e.PresentPrice);
                            if (count != 0)//计算偏差
                            {
                                averagePrice = (long)(averagePrice * (count / 100) + averagePrice);
                            }

                            if ((compare == 1 && presentPrice >= averagePrice) || (compare == 2 && presentPrice <= averagePrice))
                            {
                                bool IsSuccess = true;
                                if (day2 >= 2)//计算每日均线
                                {
                                    long lastAveragePrice = 0;
                                    for (int i = 0; i < day2; i++)
                                    {
                                        long tempAveragePrice;
                                        var list2 = quotes.Skip(i).Take(day1).ToList();
                                        if (list2.Count() <= 0)
                                        {
                                            tempAveragePrice = 0;
                                        }
                                        else
                                        {
                                            tempAveragePrice = (long)list2.Average(e => e.PresentPrice);
                                        }
                                        if (upOrDown == 1)
                                        {
                                            //向上，则当前必须<=前一个数据
                                            if (tempAveragePrice > lastAveragePrice && lastAveragePrice != 0)
                                            {
                                                IsSuccess = false;
                                                break;
                                            }
                                            lastAveragePrice = tempAveragePrice;
                                            continue;
                                        }
                                        else if (upOrDown == 2)
                                        {
                                            //向下，则当前必须>=前一个数据
                                            if (tempAveragePrice < lastAveragePrice && lastAveragePrice != 0)
                                            {
                                                IsSuccess = false;
                                                break;
                                            }
                                            lastAveragePrice = tempAveragePrice;
                                            continue;
                                        }
                                        else
                                        {
                                            IsSuccess = false;
                                            break;
                                        }
                                    }
                                }
                                if (IsSuccess)
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                        } while (true);
                    }, TaskCreationOptions.LongRunning);
                    taskArr[index].Start();
                }
                Task.WaitAll(taskArr);
                data.Release();
                return result;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_ReferAverage_New_Batch出错了", ex);
                return new List<SharesBase_Session>();
            }
        }

        //分析买卖变化速度-批量
        public static List<SharesBase_Session> Analysis_QuotesChangeRate_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                int compare = 0;
                int type = 0;
                double count = 0;
                int othercompare = 0;

                compare = Convert.ToInt32(temp.Compare);
                type = Convert.ToInt32(temp.Type);
                count = Convert.ToDouble(temp.Count);
                try
                {
                    othercompare = Convert.ToInt32(temp.Compare);
                }
                catch (Exception)
                {
                }
                string sql = @"select Market,SharesCode,DataType,BuyCount1,BuyCount2,BuyCount3,BuyCount4,BuyCount5,BuyPrice1,BuyPrice2,BuyPrice3,BuyPrice4,BuyPrice5,
SellCount1,SellCount2,SellCount3,SellCount4,SellCount5,SellPrice1,SellPrice2,SellPrice3,SellPrice4,SellPrice5
from t_shares_quotes with(nolock)";
                List<QuotesDateInfo> quotes_list = new List<QuotesDateInfo>();
                using (var db = new meal_ticketEntities())
                {
                    quotes_list = db.Database.SqlQuery<QuotesDateInfo>(sql).ToList();
                }

                ThreadMsgTemplate<SharesBase_Session> data = new ThreadMsgTemplate<SharesBase_Session>();
                data.Init();
                foreach (var item in sharesList)
                {
                    data.AddMessage(item);
                }

                List<SharesBase_Session> result = new List<SharesBase_Session>();
                object dataLock = new object();

                int defaultCount = Singleton.Instance.MaxTrendCheckTaskCount;
                int taskCount = data.GetCount();
                if (taskCount > defaultCount)
                {
                    taskCount = defaultCount;
                }

                Task[] taskArr = new Task[taskCount];
                for (int index = 0; index < taskCount; index++)
                {
                    taskArr[index] = new Task(() =>
                    {
                        do
                        {
                            SharesBase_Session tempData = new SharesBase_Session();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }
                            var quotes = quotes_list.Where(e => e.Market == tempData.Market && e.SharesCode == tempData.SharesCode).ToList();
                            var quotesLast = quotes.Where(e => e.DataType == 0).FirstOrDefault();
                            var quotesPre = quotes.Where(e => e.DataType == 1).FirstOrDefault();
                            if (quotesLast == null || quotesPre == null)
                            {
                                continue;
                            }

                            int disCount = 0;
                            long disPrice = 0;
                            int lastCount = 0;
                            long lastPrice = 0;
                            int tradetype = 0;

                            switch (type)
                            {
                                case 1:
                                    disCount = quotesLast.BuyCount1;
                                    disPrice = quotesLast.BuyPrice1;
                                    lastCount = quotesPre.BuyCount1;
                                    lastPrice = quotesPre.BuyPrice1;
                                    tradetype = 1;
                                    break;
                                case 2:
                                    disCount = quotesLast.BuyCount2;
                                    disPrice = quotesLast.BuyPrice2;
                                    lastCount = quotesPre.BuyCount2;
                                    lastPrice = quotesPre.BuyPrice2;
                                    tradetype = 1;
                                    break;
                                case 3:
                                    disCount = quotesLast.BuyCount3;
                                    disPrice = quotesLast.BuyPrice3;
                                    lastCount = quotesPre.BuyCount3;
                                    lastPrice = quotesPre.BuyPrice3;
                                    tradetype = 1;
                                    break;
                                case 4:
                                    disCount = quotesLast.BuyCount4;
                                    disPrice = quotesLast.BuyPrice4;
                                    lastCount = quotesPre.BuyCount4;
                                    lastPrice = quotesPre.BuyPrice4;
                                    tradetype = 1;
                                    break;
                                case 5:
                                    disCount = quotesLast.BuyCount5;
                                    disPrice = quotesLast.BuyPrice5;
                                    lastCount = quotesPre.BuyCount5;
                                    lastPrice = quotesPre.BuyPrice5;
                                    tradetype = 1;
                                    break;
                                case 6:
                                    disCount = quotesLast.SellCount1;
                                    disPrice = quotesLast.SellPrice1;
                                    lastCount = quotesPre.SellCount1;
                                    lastPrice = quotesPre.SellPrice1;
                                    tradetype = 2;
                                    break;
                                case 7:
                                    disCount = quotesLast.SellCount2;
                                    disPrice = quotesLast.SellPrice2;
                                    lastCount = quotesPre.SellCount2;
                                    lastPrice = quotesPre.SellPrice2;
                                    tradetype = 2;
                                    break;
                                case 8:
                                    disCount = quotesLast.SellCount3;
                                    disPrice = quotesLast.SellPrice3;
                                    lastCount = quotesPre.SellCount3;
                                    lastPrice = quotesPre.SellPrice3;
                                    tradetype = 2;
                                    break;
                                case 9:
                                    disCount = quotesLast.SellCount4;
                                    disPrice = quotesLast.SellPrice4;
                                    lastCount = quotesPre.SellCount4;
                                    lastPrice = quotesPre.SellPrice4;
                                    tradetype = 2;
                                    break;
                                case 10:
                                    disCount = quotesLast.SellCount5;
                                    disPrice = quotesLast.SellPrice5;
                                    lastCount = quotesPre.SellCount5;
                                    lastPrice = quotesPre.SellPrice5;
                                    tradetype = 2;
                                    break;
                                default:
                                    continue;
                            }

                            long currPrice = quotesLast.PresentPrice;
                            if ((othercompare == 1 && disPrice < currPrice) || (othercompare == 2 && disPrice > currPrice))
                            {
                                continue;
                            }
                            if (lastCount <= 0 || lastPrice <= 0)
                            {
                                continue;
                            }

                            double rate = 0;
                            //当前价格等于0，速率为-100%
                            if (disPrice <= 0)
                            {
                                rate = -100;
                            }
                            //价格相等，计算速率
                            else if (lastPrice == disPrice)
                            {
                                rate = (disCount - lastCount) * 1.0 / lastCount * 100;
                            }
                            else if (lastPrice > disPrice)
                            {
                                //卖单则为无限大（999999999）
                                if (tradetype == 2)
                                {
                                    rate = 999999999;
                                }
                                else
                                {
                                    rate = -100;
                                }
                            }
                            else
                            {
                                //买单则为无限大（999999999）
                                if (tradetype == 1)
                                {
                                    rate = 999999999;
                                }
                                else
                                {
                                    rate = -100;
                                }
                            }

                            if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                            {
                                lock (dataLock)
                                {
                                    result.Add(new SharesBase_Session
                                    {
                                        Market = tempData.Market,
                                        SharesCode = tempData.SharesCode
                                    });
                                }
                            }
                        } while (true);
                    }, TaskCreationOptions.LongRunning);
                    taskArr[index].Start();
                }
                Task.WaitAll(taskArr);
                data.Release();
                return result;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_QuotesChangeRate_New_Batch出错了", ex);
                return new List<SharesBase_Session>();
            }
        }

        //分析五档变化速度-批量
        public static List<SharesBase_Session> Analysis_QuotesTypeChangeRate_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                int compare = 0;
                int type = 0;
                double count = 0;
                int priceType = 1;

                type = Convert.ToInt32(temp.Type);

                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                    count = Convert.ToDouble(temp.Count);
                    priceType = Convert.ToInt32(temp.PriceType);
                }
                catch (Exception)
                {
                }

                string sql = @"select Market,SharesCode,DataType,BuyCount1,BuyCount2,BuyCount3,BuyCount4,BuyCount5,BuyPrice1,BuyPrice2,BuyPrice3,BuyPrice4,BuyPrice5,
SellCount1,SellCount2,SellCount3,SellCount4,SellCount5,SellPrice1,SellPrice2,SellPrice3,SellPrice4,SellPrice5,PresentPrice,LimitUpPrice,LimitDownPrice
from t_shares_quotes with(nolock)";
                List<QuotesDateInfo> quotes_list = new List<QuotesDateInfo>();
                using (var db = new meal_ticketEntities())
                {
                    quotes_list = db.Database.SqlQuery<QuotesDateInfo>(sql).ToList();
                }

                ThreadMsgTemplate<SharesBase_Session> data = new ThreadMsgTemplate<SharesBase_Session>();
                data.Init();
                foreach (var item in sharesList)
                {
                    data.AddMessage(item);
                }

                List<SharesBase_Session> result = new List<SharesBase_Session>();
                object dataLock = new object();

                int defaultCount = Singleton.Instance.MaxTrendCheckTaskCount;
                int taskCount = data.GetCount();
                if (taskCount > defaultCount)
                {
                    taskCount = defaultCount;
                }

                Task[] taskArr = new Task[taskCount];
                for (int index = 0; index < taskCount; index++)
                {
                    taskArr[index] = new Task(() =>
                    {
                        do
                        {
                            SharesBase_Session tempData = new SharesBase_Session();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }
                            var quotes = quotes_list.Where(e => e.Market == tempData.Market && e.SharesCode == tempData.SharesCode).ToList();
                            var quotesLast = quotes.Where(e => e.DataType == 0).FirstOrDefault();
                            var quotesPre = quotes.Where(e => e.DataType == 1).FirstOrDefault();
                            if (quotesLast == null || quotesPre == null)
                            {
                                continue;
                            }
                            long currPrice = quotesLast.PresentPrice;
                            if (priceType == 2)
                            {
                                currPrice = quotesLast.LimitUpPrice;
                            }
                            else if (priceType == 3)
                            {
                                currPrice = quotesLast.LimitDownPrice;
                            }

                            int disCount = 0;
                            int lastCount = 0;

                            double rate = 0;
                            switch (type)
                            {
                                case 1:
                                    if (quotesPre.BuyPrice1 == currPrice)
                                    {
                                        lastCount = quotesPre.BuyCount1;
                                    }
                                    else if (quotesPre.BuyPrice2 == currPrice)
                                    {
                                        lastCount = quotesPre.BuyCount2;
                                    }
                                    else if (quotesPre.BuyPrice3 == currPrice)
                                    {
                                        lastCount = quotesPre.BuyCount3;
                                    }
                                    else if (quotesPre.BuyPrice4 == currPrice)
                                    {
                                        lastCount = quotesPre.BuyCount4;
                                    }
                                    else if (quotesPre.BuyPrice5 == currPrice)
                                    {
                                        lastCount = quotesPre.BuyCount5;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (quotesLast.BuyPrice1 == currPrice)
                                    {
                                        disCount = quotesLast.BuyCount1;
                                    }
                                    else if (quotesLast.BuyPrice2 == currPrice)
                                    {
                                        disCount = quotesLast.BuyCount2;
                                    }
                                    else if (quotesLast.BuyPrice3 == currPrice)
                                    {
                                        disCount = quotesLast.BuyCount3;
                                    }
                                    else if (quotesLast.BuyPrice4 == currPrice)
                                    {
                                        disCount = quotesLast.BuyCount4;
                                    }
                                    else if (quotesLast.BuyPrice5 == currPrice)
                                    {
                                        disCount = quotesLast.BuyCount5;
                                    }
                                    else
                                    {
                                        if (quotesLast.BuyPrice1 > currPrice)
                                        {
                                            rate = 0x0FFFFFFF;
                                        }
                                        else
                                        {
                                            rate = -100;
                                        }
                                    }
                                    break;
                                case 2:
                                    if (quotesPre.SellPrice1 == currPrice)
                                    {
                                        lastCount = quotesPre.SellCount1;
                                    }
                                    else if (quotesPre.SellPrice2 == currPrice)
                                    {
                                        lastCount = quotesPre.SellCount2;
                                    }
                                    else if (quotesPre.SellPrice3 == currPrice)
                                    {
                                        lastCount = quotesPre.SellCount3;
                                    }
                                    else if (quotesPre.SellPrice4 == currPrice)
                                    {
                                        lastCount = quotesPre.SellCount4;
                                    }
                                    else if (quotesPre.SellPrice5 == currPrice)
                                    {
                                        lastCount = quotesPre.SellCount5;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (quotesLast.SellPrice1 == currPrice)
                                    {
                                        disCount = quotesLast.SellCount1;
                                    }
                                    else if (quotesLast.SellPrice2 == currPrice)
                                    {
                                        disCount = quotesLast.SellCount2;
                                    }
                                    else if (quotesLast.SellPrice3 == currPrice)
                                    {
                                        disCount = quotesLast.SellCount3;
                                    }
                                    else if (quotesLast.SellPrice4 == currPrice)
                                    {
                                        disCount = quotesLast.SellCount4;
                                    }
                                    else if (quotesLast.SellPrice5 == currPrice)
                                    {
                                        disCount = quotesLast.SellCount5;
                                    }
                                    else
                                    {
                                        if (quotesLast.SellPrice1 > currPrice)
                                        {
                                            rate = -100;
                                        }
                                        else
                                        {
                                            rate = 0x0FFFFFFF;
                                        }
                                    }
                                    break;
                                default:
                                    continue;
                            }

                            if (rate == 0)
                            {
                                if (lastCount == 0)
                                {
                                    continue;
                                }
                                rate = (disCount - lastCount) * 1.0 / lastCount * 100;
                            }


                            if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                            {
                                lock (dataLock)
                                {
                                    result.Add(new SharesBase_Session
                                    {
                                        Market = tempData.Market,
                                        SharesCode = tempData.SharesCode
                                    });
                                }
                            }
                        } while (true);
                    }, TaskCreationOptions.LongRunning);
                    taskArr[index].Start();
                }
                Task.WaitAll(taskArr);
                data.Release();
                return result;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析五档变化速度出错了", ex);
                return new List<SharesBase_Session>();
            }
        }

        //分析按当前价格-批量
        public static List<SharesBase_Session> Analysis_CurrentPrice_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            try
            {
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                int riseType = 0;
                int compare = 0;
                long count = 0;

                riseType = Convert.ToInt32(temp.RiseType);
              
                try
                {
                    compare = Convert.ToInt32(temp.Compare);
                }
                catch (Exception)
                {
                }
                try
                {
                    count = (long)(Convert.ToDouble(temp.Count) * 10000);
                }
                catch (Exception)
                {
                }

                List<QuotesDateInfo> quotes_list = (from item in Singleton.Instance._SharesQuotesSession.GetSessionData()
                                                    join item2 in sharesList on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                                    select new QuotesDateInfo
                                                    {
                                                        Market = item.Market,
                                                        SharesCode = item.SharesCode,
                                                        PresentPrice = item.PresentPrice,
                                                        TriPriceType = item.TriPriceType,
                                                        PriceType = item.PriceType
                                                    }).ToList();

                ThreadMsgTemplate<SharesBase_Session> data = new ThreadMsgTemplate<SharesBase_Session>();
                data.Init();
                foreach (var item in sharesList)
                {
                    data.AddMessage(item);
                }

                List<SharesBase_Session> result = new List<SharesBase_Session>();
                object dataLock = new object();

                int defaultCount = Singleton.Instance.MaxTrendCheckTaskCount;
                int taskCount = data.GetCount();
                if (taskCount > defaultCount)
                {
                    taskCount = defaultCount;
                }

                Task[] taskArr = new Task[taskCount];
                for (int index = 0; index < taskCount; index++)
                {
                    taskArr[index] = new Task(() =>
                    {
                        do
                        {
                            SharesBase_Session tempData = new SharesBase_Session();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }
                            //当前价格
                            var quotes = quotes_list.Where(e=>e.Market==tempData.Market && e.SharesCode==tempData.SharesCode).FirstOrDefault();
                            if (quotes == null)
                            {
                                continue;
                            }

                            if (riseType == 1 && quotes.TriPriceType == 1)
                            {
                                lock (dataLock)
                                {
                                    result.Add(new SharesBase_Session
                                    {
                                        Market = tempData.Market,
                                        SharesCode = tempData.SharesCode
                                    });
                                }
                            }

                            else if (riseType == 2 && quotes.TriPriceType == 2)
                            {
                                lock (dataLock)
                                {
                                    result.Add(new SharesBase_Session
                                    {
                                        Market = tempData.Market,
                                        SharesCode = tempData.SharesCode
                                    });
                                }
                            }

                            else if (riseType == 3 && quotes.PriceType == 1)
                            {
                                lock (dataLock)
                                {
                                    result.Add(new SharesBase_Session
                                    {
                                        Market = tempData.Market,
                                        SharesCode = tempData.SharesCode
                                    });
                                }
                                continue;
                            }

                            else if (riseType == 4 && quotes.PriceType == 2)
                            {
                                lock (dataLock)
                                {
                                    result.Add(new SharesBase_Session
                                    {
                                        Market = tempData.Market,
                                        SharesCode = tempData.SharesCode
                                    });
                                }
                                continue;
                            }
                            else if (riseType == 5)
                            {
                                if ((compare == 1 && quotes.PresentPrice >= count) || (compare == 2 && quotes.PresentPrice <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }

                            }
                            else if (riseType == 6)
                            {
                                //查询流通股
                                var sharesInfo = (from item in Singleton.Instance._SharesBaseSession.GetSessionData()
                                                  where item.Market == tempData.Market && item.SharesCode == tempData.SharesCode
                                                  select item).FirstOrDefault();
                                if (sharesInfo == null)
                                {
                                    continue;
                                }
                                long circulatingCapital = sharesInfo.CirculatingCapital;
                                if ((compare == 1 && quotes.PresentPrice * circulatingCapital >= count) || (compare == 2 && quotes.PresentPrice * circulatingCapital <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                            else if (riseType == 7)
                            {
                                //查询总股本
                                var sharesInfo = (from item in Singleton.Instance._SharesBaseSession.GetSessionData()
                                                  where item.Market == tempData.Market && item.SharesCode == tempData.SharesCode
                                                  select item).FirstOrDefault();
                                if (sharesInfo == null)
                                {
                                    continue;
                                }
                                long totalCapital = sharesInfo.TotalCapital;
                                if ((compare == 1 && quotes.PresentPrice * totalCapital >= count) || (compare == 2 && quotes.PresentPrice * totalCapital <= count))
                                {
                                    lock (dataLock)
                                    {
                                        result.Add(new SharesBase_Session
                                        {
                                            Market = tempData.Market,
                                            SharesCode = tempData.SharesCode
                                        });
                                    }
                                }
                            }
                        } while (true);
                    }, TaskCreationOptions.LongRunning);
                    taskArr[index].Start();
                }
                Task.WaitAll(taskArr);
                data.Release();
                return result;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按当前价格出错了", ex);
                return new List<SharesBase_Session>();
            }
        }

        //分心股票市场-批量
        public static List<SharesBase_Session> Analysis_SharesMarket_New_Batch(List<SharesBase_Session> sharesList, string par)
        {
            try
            {
                List<SharesBase_Session> result = new List<SharesBase_Session>();
                var temp = JsonConvert.DeserializeObject<dynamic>(par);
                string sharesKey = string.Empty;
                int sharesType = 0;
                int sharesMarket = 0;
                bool sharesMarket0 = false;
                bool sharesMarket1 = false;

                sharesKey = Convert.ToString(temp.SharesKey);
                sharesType = Convert.ToInt32(temp.SharesType);

                try
                {
                    sharesMarket0 = Convert.ToBoolean(temp.SharesMarket0);
                }
                catch (Exception ex)
                { }
                try
                {
                    sharesMarket1 = Convert.ToBoolean(temp.SharesMarket1);
                }
                catch (Exception ex)
                { }

                if (string.IsNullOrEmpty(sharesKey))
                {
                    throw new Exception("参数错误");
                }
                if (sharesType==0)
                {
                    throw new Exception("参数错误");
                }
                if (!sharesMarket0 && !sharesMarket1)
                {
                    throw new Exception("参数错误");
                }
                if (sharesMarket0 && sharesMarket1)
                {
                    sharesMarket = -1;
                }
                else if (sharesMarket0)
                {
                    sharesMarket = 0;
                }
                else 
                {
                    sharesMarket = 1;
                }

                foreach (var item in sharesList)
                {
                    string sharesName = (from x in Singleton.Instance._SharesBaseSession.GetSessionData()
                                         where x.Market == item.Market && x.SharesCode == item.SharesCode
                                         select x.SharesName).FirstOrDefault();
                    if (string.IsNullOrEmpty(sharesName))
                    {
                        continue;
                    }
                    if ((sharesMarket == -1 || sharesMarket == item.Market) && ((sharesType == 1 && item.SharesCode.StartsWith(sharesKey)) || (sharesType == 2 && sharesName.StartsWith(sharesKey))))
                    {
                        result.Add(item);
                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_SharesMarket_New_Batch出错", ex);
                return new List<SharesBase_Session>();
            }
        }
    }

    public class QuotesDate
    {
        public int LimitUpCount { get; set; }
        public int TriLimitUpCount { get; set; }
        public int LimitDownCount { get; set; }
        public int TriLimitDownCount { get; set; }
        public int LimitUpBombCount { get; set; }
        public long OpenedPrice { get; set; }
        public long ClosedPrice { get; set; }
        public long PresentPrice { get; set; }
        public long MaxPrice { get; set; }
        public long MinPrice { get; set; }
        public long TotalAmount { get; set; }
        public int TotalCount { get; set; }
    }

    public class QuotesDateInfo
    {
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public int PriceType { get; set; }
        public int TriPriceType { get; set; }
        public int LimitUpCount { get; set; }
        public int TriLimitUpCount { get; set; }
        public int LimitDownCount { get; set; }
        public int TriLimitDownCount { get; set; }
        public int LimitUpBombCount { get; set; }
        public long LimitUpPrice { get; set; }
        public long LimitDownPrice { get; set; }
        public long OpenedPrice { get; set; }
        public long PresentPrice { get; set; }
        public long ClosedPrice { get; set; }
        public long MaxPrice { get; set; }
        public long MinPrice { get; set; }
        public int RiseRate { get; set; }
        public long TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public string Date { get; set; }
        public int DataType { get; set; }
        public int BuyCount1 { get; set; }
        public int BuyCount2 { get; set; }
        public int BuyCount3 { get; set; }
        public int BuyCount4 { get; set; }
        public int BuyCount5 { get; set; }
        public long BuyPrice1 { get; set; }
        public long BuyPrice2 { get; set; }
        public long BuyPrice3 { get; set; }
        public long BuyPrice4 { get; set; }
        public long BuyPrice5 { get; set; }
        public int SellCount1 { get; set; }
        public int SellCount2 { get; set; }
        public int SellCount3 { get; set; }
        public int SellCount4 { get; set; }
        public int SellCount5 { get; set; }
        public long SellPrice1 { get; set; }
        public long SellPrice2 { get; set; }
        public long SellPrice3 { get; set; }
        public long SellPrice4 { get; set; }
        public long SellPrice5 { get; set; }
        public DateTime LastModified { get; set; }
    }
}
