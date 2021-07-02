﻿using FXCommon.Common;
using MealTicket_Web_Handler.Model;
using Newtonsoft.Json;
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
            var dataList = GetTrendPar();//查询需要分析的股票及参数


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

        public static int Analysis_HisRiseRate(string sharesCode, int market, List<string> par)
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
                        if (compare == 1 && tempCount >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && tempCount <= count)
                        {
                            return 0;
                        }
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

                        if (compare == 1 && tempCount >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && tempCount <= count)
                        {
                            return 0;
                        }
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
                        if (compare == 1 && tempCount >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && tempCount <= count)
                        {
                            return 0;
                        }
                        return -1;
                    }
                    //炸板次数
                    else if (type == 4)
                    {
                        int tempCount = quotes_date.Where(e => e.PriceType != 1 && e.LimitUpCount > 0).Count();
                        if (compare == 1 && tempCount >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && tempCount <= count)
                        {
                            return 0;
                        }
                        return -1;
                    }
                    //每日涨幅
                    else if (type == 5)
                    {
                        int i = 0;
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PresentPrice <= 0 || quote.ClosedPrice <= 0)
                            {
                                continue;
                            }
                            int rate = (int)((quote.PresentPrice - quote.ClosedPrice) * 1.0 / quote.ClosedPrice * 10000);
                            if (rate < 0)
                            {
                                continue;
                            }
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
                        if (compare == 1 && i >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && i <= count)
                        {
                            return 0;
                        }
                        return -1;
                    }
                    //每日跌幅
                    else if (type == 6)
                    {
                        int i = 0;
                        foreach (var quote in quotes_date)
                        {
                            if (quote.PresentPrice <= 0 || quote.ClosedPrice <= 0)
                            {
                                continue;
                            }
                            int rate = (int)((quote.PresentPrice - quote.ClosedPrice) * 1.0 / quote.ClosedPrice * 10000);
                            if (rate > 0)
                            {
                                continue;
                            }
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
                        if (compare == 1 && i >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && i <= count)
                        {
                            return 0;
                        }
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
                        if (compare == 1 && i >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && i <= count)
                        {
                            return 0;
                        }
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
                                return 0;
                            }
                        }
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
                                return -1;
                            }
                        }
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
                                return 0;
                            }
                        }
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
                                return -1;
                            }
                        }
                        return 0;
                    }
                    //总涨幅
                    else if (type == 11)
                    {
                        long closePrice = quotes_date.OrderBy(e => e.Date).Select(e => e.ClosedPrice).FirstOrDefault();
                        long presentPrice = quotes_date.OrderByDescending(e => e.Date).Select(e => e.PresentPrice).FirstOrDefault();
                        if (closePrice <= 0 || presentPrice <= 0)
                        {
                            return -1;
                        }
                        int rate = (int)((presentPrice - closePrice) * 1.0 / closePrice * 10000);
                        if (rate < 0)
                        {
                            return -1;
                        }
                        if (!flatRise && rate == 0)
                        {
                            return -1;
                        }
                        if (compare == 1)
                        {
                            if (rate >= count)
                            {
                                return 0;
                            }
                            return -1;
                        }
                        else if (compare == 2)
                        {
                            if (rate <= count)
                            {
                                return 0;
                            }
                            return -1;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    //总跌幅
                    else if (type == 12)
                    {
                        long closePrice = quotes_date.OrderBy(e => e.Date).Select(e => e.ClosedPrice).FirstOrDefault();
                        long presentPrice = quotes_date.OrderByDescending(e => e.Date).Select(e => e.PresentPrice).FirstOrDefault();
                        if (closePrice <= 0 || presentPrice <= 0)
                        {
                            return -1;
                        }
                        int rate = (int)((presentPrice - closePrice) * 1.0 / closePrice * 10000);
                        if (rate > 0)
                        {
                            return -1;
                        }
                        if (!flatRise && rate == 0)
                        {
                            return -1;
                        }
                        if (compare == 1)
                        {
                            if (rate >= count)
                            {
                                return 0;
                            }
                            return -1;
                        }
                        else if (compare == 2)
                        {
                            if (rate <= count)
                            {
                                return 0;
                            }
                            return -1;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    //总涨跌幅
                    else if (type == 14)
                    {
                        long closePrice = quotes_date.OrderBy(e => e.Date).Select(e => e.ClosedPrice).FirstOrDefault();
                        long presentPrice = quotes_date.OrderByDescending(e => e.Date).Select(e => e.PresentPrice).FirstOrDefault();
                        if (closePrice <= 0 || presentPrice <= 0)
                        {
                            return -1;
                        }
                        int rate = (int)((presentPrice - closePrice) * 1.0 / closePrice * 10000);
                        if (!flatRise && rate == 0)
                        {
                            return -1;
                        }
                        if (compare == 1)
                        {
                            if (rate >= count)
                            {
                                return 0;
                            }
                            return -1;
                        }
                        else if (compare == 2)
                        {
                            if (rate <= count)
                            {
                                return 0;
                            }
                            return -1;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析历史涨跌幅数据出错了", ex);
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
        public static int Analysis_TodayRiseRate(string sharesCode, int market, List<string> par)
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

                DateTime dateNow = DateTime.Now.Date;
                using (var db = new meal_ticketEntities())
                {
                    comeTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string sqlQuery = string.Format("select top 1 LimitUpCount,TriLimitUpCount,LimitDownCount,TriLimitDownCount,LimitUpBombCount,OpenedPrice,ClosedPrice,MaxPrice,MinPrice,TotalAmount,TotalCount from t_shares_quotes_date with(nolock) where SharesCode='{0}' and Market={1} and LastModified>'{2}'", sharesCode, market, dateNow.ToString("yyyy-MM-dd"));
                    var quotes_date=db.Database.SqlQuery<QuotesDate>(sqlQuery).FirstOrDefault();
                   
                    outTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    if (quotes_date == null)
                    {
                        return -1;
                    }
                    //涨停次数
                    if (type == 1)
                    {
                        if (compare == 1 && !triPrice && quotes_date.LimitUpCount >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && !triPrice && quotes_date.LimitUpCount <= count)
                        {
                            return 0;
                        }
                        if (compare == 1 && triPrice && quotes_date.TriLimitUpCount >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && triPrice && quotes_date.TriLimitUpCount <= count)
                        {
                            return 0;
                        }
                        return -1;
                    }
                    //跌停次数
                    if (type == 2)
                    {
                        if (compare == 1 && !triPrice && quotes_date.LimitDownCount >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && !triPrice && quotes_date.LimitDownCount <= count)
                        {
                            return 0;
                        }
                        if (compare == 1 && triPrice && quotes_date.TriLimitDownCount >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && triPrice && quotes_date.TriLimitDownCount <= count)
                        {
                            return 0;
                        }
                        return -1;
                    }
                    //炸板次数
                    else if (type == 3)
                    {
                        if (compare == 1 && quotes_date.LimitUpBombCount >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && quotes_date.LimitUpBombCount <= count)
                        {
                            return 0;
                        }
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
                    //成交额
                    else if (type == 7)
                    {
                        var quotesCount = (from item in db.t_shares_quotes_date
                                           where item.SharesCode == sharesCode && item.Market == market && item.LastModified < dateNow
                                           select item).Count();
                        if (dayShortageType == 2 && quotesCount < firstDay)
                        {
                            return 0;
                        }
                        if (dayShortageType == 3 && quotesCount < firstDay)
                        {
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
                                return 0;
                            }
                        }
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
                            return 0;
                        }
                        if (dayShortageType == 3 && quotesCount < firstDay)
                        {
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
                                return 0;
                            }
                        }
                        return -1;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog(string.Format("分析今日涨跌幅出错了-{0}-{1}-{2}", sharesCode, comeTime, outTime), ex);
                return -1;
            }
        }

        //分析板块涨跌幅
        public static int Analysis_PlateRiseRate(string sharesCode, int market, List<string> par)
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
                        return 0;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析板块涨跌幅出错了", ex);
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
        public static int Analysis_BuyOrSellCount(string sharesCode, int market, List<string> par)
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
                    switch (type)
                    {
                        case 1:
                            realCount = quotes.BuyCount1;
                            break;
                        case 2:
                            realCount = quotes.BuyCount2;
                            break;
                        case 3:
                            realCount = quotes.BuyCount3;
                            break;
                        case 4:
                            realCount = quotes.BuyCount4;
                            break;
                        case 5:
                            realCount = quotes.BuyCount5;
                            break;
                        case 6:
                            realCount = quotes.SellCount1;
                            break;
                        case 7:
                            realCount = quotes.SellCount2;
                            break;
                        case 8:
                            realCount = quotes.SellCount3;
                            break;
                        case 9:
                            realCount = quotes.SellCount4;
                            break;
                        case 10:
                            realCount = quotes.SellCount5;
                            break;
                        default:
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
                            return -1;
                        }
                        long circulatingCapital = sharesInfo.CirculatingCapital;
                        long setCount = (long)(circulatingCapital * (count / 100));
                        if (compare == 1 && realCount >= setCount)
                        {
                            return 0;
                        }
                        if (compare == 2 && realCount <= setCount)
                        {
                            return 0;
                        }

                    }
                    else if (countType == 2)//股数
                    {
                        if (compare == 1 && realCount >= count)
                        {
                            return 0;
                        }
                        if (compare == 2 && realCount <= count)
                        {
                            return 0;
                        }
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
        public static int Analysis_ReferPrice(string sharesCode, int market, List<string> par)
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
                    return -1;
                }
                DateTime timeNow = DateTime.Now;
                using (var db =new meal_ticketEntities())
                {
                    var quotes = (from item in db.t_shares_quotes_date
                                  where item.Market == market && item.SharesCode == sharesCode
                                  orderby item.Date descending
                                  select item).Take(day + 1).ToList();
                    if (quotes.Count() <= 0)
                    {
                        return -1;
                    }
                    var presentInfo = quotes.OrderByDescending(e => e.Date).FirstOrDefault();
                    if (presentInfo.LastModified < timeNow.AddMinutes(-1))
                    {
                        return -1;
                    }
                    long presentPrice = presentInfo.PresentPrice;
                    if (presentPrice <= 0)
                    {
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
                            return -1;
                    }

                    if (count != 0)//计算偏差
                    {
                        realPrice = (long)(realPrice * (count / 100) + realPrice);
                    }

                    if (compare == 1 && presentPrice >= realPrice)
                    {
                        return 0;
                    }
                    if (compare == 2 && presentPrice <= realPrice)
                    {
                        return 0;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按参照价格出错了", ex);
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
        public static int Analysis_ReferAverage(string sharesCode, int market, List<string> par)
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
                        return -1;
                    }
                    long presentPrice = presentInfo.PresentPrice;
                    if (presentPrice <= 0)
                    {
                        return -1;
                    }
                    //计算均线价格
                    var list1 = quotes.Take(day1).ToList();
                    if (list1.Count() <= 0)
                    {
                        return -1;
                    }
                    if (list1.Count() < day1)
                    {
                        if (dayShortageType == 2)
                        {
                            return 0;
                        }
                        if (dayShortageType == 3)
                        {
                            return -1;
                        }
                    }
                    long averagePrice = (long)list1.Average(e => e.PresentPrice);
                    if (count != 0)//计算偏差
                    {
                        averagePrice = (long)(averagePrice * (count / 100) + averagePrice);
                    }

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
                                        return -1;
                                    }
                                    lastAveragePrice = tempAveragePrice;
                                    continue;
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                        }
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按均线价格出错了", ex);
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

        //分析五档变化速度
        public static int Analysis_QuotesChangeRate(string sharesCode, int market,long currPrice ,List<string> par)
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

        //分析按当前价格
        public static int Analysis_CurrentPrice(string sharesCode, int market, List<string> par)
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
                    return -1;
                }

                if (riseType == 1 && quotes.TriPriceType == 1)
                {
                    return 0;
                }

                if (riseType == 2 && quotes.TriPriceType == 2)
                {
                    return 0;
                }

                if (riseType == 3 && quotes.PriceType == 1)
                {
                    return 0;
                }

                if (riseType == 4 && quotes.PriceType == 2)
                {
                    return 0;
                }
                if (riseType == 5)
                {
                    if ((compare == 1 && quotes.PresentPrice >= count) || (compare == 2 && quotes.PresentPrice <= count))
                    {
                        return 0;
                    }
                }
                if (riseType == 6)
                {
                    //查询流通股
                    var sharesInfo = (from item in db.t_shares_markettime
                                      where item.Market == market && item.SharesCode == sharesCode
                                      select item).FirstOrDefault();
                    if (sharesInfo == null)
                    {
                        return -1;
                    }
                    long circulatingCapital = sharesInfo.CirculatingCapital;
                    if ((compare == 1 && quotes.PresentPrice* circulatingCapital >= count) || (compare == 2 && quotes.PresentPrice * circulatingCapital <= count))
                    {
                        return 0;
                    }
                }
                if (riseType == 7)
                {
                    //查询总股本
                    var sharesInfo = (from item in db.t_shares_markettime
                                      where item.Market == market && item.SharesCode == sharesCode
                                      select item).FirstOrDefault();
                    if (sharesInfo == null)
                    {
                        return -1;
                    }
                    long totalCapital = sharesInfo.TotalCapital;
                    if ((compare == 1 && quotes.PresentPrice * totalCapital >= count) || (compare == 2 && quotes.PresentPrice * totalCapital <= count))
                    {
                        return 0;
                    }
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

        /// <summary>
        /// 触发
        /// </summary>
        private static void TrendTri(List<TrendAnalyseResult> trendResult)
        {
            using (SqlConnection conn = new SqlConnection(Singleton.Instance.connString_meal_ticket))
            {
                conn.Open();
                try
                {
                    using (SqlCommand comm = new SqlCommand("exec P_TradeAnalyseResult_Update @analyseResult", conn))
                    {
                        using (var table = new DataTable())
                        {
                            table.Columns.Add("Market", typeof(int));
                            table.Columns.Add("SharesCode", typeof(string));
                            table.Columns.Add("PushTime", typeof(DateTime));
                            table.Columns.Add("PushDesc", typeof(string));
                            table.Columns.Add("TrendId", typeof(long));

                            foreach (var item in trendResult)
                            {
                                DataRow row = table.NewRow();
                                row["Market"] = item.Market;
                                row["SharesCode"] = item.SharesCode;
                                row["PushTime"] = item.PushTime;
                                row["PushDesc"] = item.PushDesc;
                                row["TrendId"] = item.TrendId;
                                table.Rows.Add(row);
                            }

                            var pList = new SqlParameter("@analyseResult", SqlDbType.Structured);
                            pList.TypeName = "dbo.TrendAnalyseResult";
                            pList.Value = table;

                            comm.Parameters.Add(pList);
                            comm.ExecuteNonQuery();
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

    public class QuotesDate
    {
        public int LimitUpCount { get; set; }
        public int TriLimitUpCount { get; set; }
        public int LimitDownCount { get; set; }
        public int TriLimitDownCount { get; set; }
        public int LimitUpBombCount { get; set; }
        public long OpenedPrice { get; set; }
        public long ClosedPrice { get; set; }
        public long MaxPrice { get; set; }
        public long MinPrice { get; set; }
        public long TotalAmount { get; set; }
        public int TotalCount { get; set; }
    }
}
