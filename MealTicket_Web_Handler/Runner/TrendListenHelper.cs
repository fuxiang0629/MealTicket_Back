using FXCommon.Common;
using Newtonsoft.Json;
using stock_db_core;
using StockTrendMonitor.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockTrendMonitor.Define.StockMonitorDefine;

namespace MealTicket_Web_Handler.Runner
{
    public class TrendListenHelper
    {
        public static void HandlerAccountTrendListen(object obj)
        {
            long accountId = Convert.ToInt64(obj);
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var optionalTrend = (from item in db.t_account_shares_optional
                                     join item2 in db.t_account_shares_optional_seat_rel on item.Id equals item2.OptionalId
                                     join item3 in db.t_account_shares_seat on item2.SeatId equals item3.Id
                                     join item4 in db.t_account_shares_optional_trend_rel on item.Id equals item4.OptionalId
                                     join item5 in db.t_shares_monitor_trend on item4.TrendId equals item5.Id
                                     join item6 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item6.Market, item6.SharesCode }
                                     join item7 in db.t_account_shares_optional_trend_rel_par on item4.Id equals item7.RelId
                                     where item.AccountId == accountId && item3.AccountId == accountId && item3.Status == 1 && item3.ValidEndTime > timeNow && item4.Status == 1 && item5.Status == 1
                                     group new { item, item4, item5, item6, item7 } by new { item, item4, item5, item6 } into g
                                     select new OptionalTrend
                                     {
                                         Market = g.Key.item.Market,
                                         SharesCode = g.Key.item.SharesCode,
                                         SharesName = g.Key.item6.SharesName,
                                         TrendId = g.Key.item4.TrendId,
                                         TrendName = g.Key.item5.Name,
                                         TrendDescription = g.Key.item5.Description,
                                         RelId = g.Key.item4.Id,
                                         OptionalId = g.Key.item.Id,
                                         ParList = g.Select(e => e.item7.ParamsInfo).ToList()
                                     }).ToList();

                List<STOCK_CODE_PARAM> resetList = new List<STOCK_CODE_PARAM>();
                var totalShares=optionalTrend.Select(e => (e.SharesCode + "," + e.Market)).Distinct().ToList();
                resetList = (from sharesCode in totalShares
                             select new STOCK_CODE_PARAM
                             {
                                 strStockCode = sharesCode
                             }).ToList();
                int error=Singleton.Instance.m_stockMonitor.RefreshTradePriceInfoBat(DateTime.Parse(DB_Model.MAX_DATE_TIME), ref resetList);
                if (error != 0)
                {
                    return;
                }
                resetList=resetList.Where(e => e.iErrorCode == 0).ToList();

                optionalTrend = (from item in optionalTrend
                                 join item2 in resetList on new { sharesCode = (item.SharesCode + "," + item.Market) } equals new { sharesCode = item2.strStockCode }
                                 select item).ToList();

                var List_Trend1 = optionalTrend.Where(e => e.TrendId == 1).ToList();
                var List_Trend2 = optionalTrend.Where(e => e.TrendId == 2).ToList();
                var List_Trend3 = optionalTrend.Where(e => e.TrendId == 3).ToList();

                List<TREND_RESULT_RAPID_UP> resultInfo_Trend1 = new List<TREND_RESULT_RAPID_UP>();
                int errorCode_Trend1 = Analysis_Trend1(List_Trend1, ref resultInfo_Trend1);
                if (errorCode_Trend1 == 0)
                {
                    foreach (var item in List_Trend1)
                    {
                        var temp = resultInfo_Trend1.Where(e => e.strStockCode == (item.SharesCode + "," + item.Market)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(temp.strStockCode) && temp.dicUpOrDownInfo.Count() > 0)
                        {
                            var pushTime=temp.dicUpOrDownInfo[0].lastestInfo.dtTradeTime;
                            TrendTri(item.RelId, item.Market, item.SharesCode, accountId, item.TrendId, item.SharesName, item.TrendName, item.TrendDescription, item.OptionalId, pushTime);
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
                            TrendTri(item.RelId, item.Market, item.SharesCode, accountId, item.TrendId, item.SharesName, item.TrendName, item.TrendDescription, item.OptionalId, pushTime);
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
                            TrendTri(item.RelId, item.Market, item.SharesCode, accountId, item.TrendId, item.SharesName, item.TrendName, item.TrendDescription, item.OptionalId, pushTime);
                        }
                    }
                }
            }
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
        private static void TrendTri(long relId,int market,string sharesCode,long accountId,long trendId,string sharesName,string trendName,string trendDescription,long optionalId,DateTime pushTime) 
        {
            bool IsPush = false;
            bool IsFirst = false;

            DateTime timeNow = DateTime.Now;
            long currPrice=0;
            int riseRate = 0;
            using (var db = new meal_ticketEntities())
            {
                var quotes = (from item in db.t_shares_quotes
                              where item.Market == market && item.SharesCode == sharesCode
                              select item).FirstOrDefault();
                if (quotes == null)
                {
                    return;
                }
                if (quotes.PresentPrice <= 0)
                {
                    return;
                }
                if (quotes.ClosedPrice <= 0)
                {
                    return;
                }
                currPrice = quotes.PresentPrice;
                riseRate = (int)((currPrice - quotes.ClosedPrice) * 1.0 / quotes.ClosedPrice * 10000);
            }

            using (SqlConnection conn = new SqlConnection(Singleton.Instance.ConnectionString_meal_ticket))
            {
                conn.Open();
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
                                else if (Convert.ToDateTime(reader["LastPushTime"]) < pushTime)
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
                                    sql = string.Format("update t_account_shares_optional_trend_rel_tri set LastPushTime='{0}',LastPushRiseRate={1},LastPushPrice={2},TriCountToday=TriCountToday+1 where RelId={3}", pushTime.ToString("yyyy-MM-dd HH:mm:ss"), riseRate, currPrice, relId);
                                    comm.CommandText = sql;
                                    comm.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                IsPush = true;
                                IsFirst = true;
                                reader.Close();
                                sql = string.Format("insert into t_account_shares_optional_trend_rel_tri(RelId,LastPushTime,LastPushRiseRate,LastPushPrice,TriCountToday,MinPushTimeInterval,MinPushRateInterval,MinTodayPrice,CreateTime,LastModified) values({0},'{1}',{2},{3},{4},{5},{6},{7},'{1}','{1}')", relId, pushTime.ToString("yyyy-MM-dd HH:mm:ss"), riseRate, currPrice, 1, 0, 0, -1);
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


                            sql = string.Format("insert into t_account_shares_optional_trend_rel_tri_record(RelId,AccountId,Market,SharesCode,SharesName,TrendId,TrendName,TrendDescription,TriPrice,IsPush,CreateTime,OptionalId) values({0},{1},{2},'{3}','{4}',{5},'{6}','{7}',{8},{9},'{10}',{11})", relId, accountId, market, sharesCode, sharesName, trendId, trendName, trendDescription, currPrice, IsPush ? 1 : 0, timeNow.ToString("yyyy-MM-dd HH:mm:ss"), optionalId);
                            comm.CommandText = sql;
                            comm.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
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

    public class Trend1 
    {
        public int ZHFZS { get; set; }

        public List<Trend1Par> ParList { get; set; }
    }

    public class Trend1Par
    {
        public int SZFD { get; set; }

        public int YXFWFD { get; set; }
    }

    public class Trend2 
    {
        public int ZDFSSL { get; set; }

        public int ZXFSSL { get; set; }

        public int YXFWFD { get; set; }

        public int YXJJBL { get; set; }

        public int TPFD { get; set; }

        public bool CCJRZGD { get; set; }

        public DateTime CCSDSJ { get; set; }
    }

    public class Trend3 
    {
        public int ZXFSSL { get; set; }

        public int TPSFZXBL { get; set; }

        public int YXJJBL { get; set; }

        public bool CCJRZGD { get; set; }

        public int YXJXWC { get; set; }

        public int TPHSJ { get; set; }
    }

    public class OptionalTrend 
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public string SharesName { get; set; }

        public long TrendId { get; set; }

        public string TrendName { get; set; }

        public string TrendDescription { get; set; }

        public long RelId { get; set; }

        public long OptionalId { get; set; }

        public List<string> ParList { get; set; }
    }
}
