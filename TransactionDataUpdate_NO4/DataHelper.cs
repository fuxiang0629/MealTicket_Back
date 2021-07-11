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
        public static List<TransactiondataAnalyseInfo> TdxHq_GetTransactionData(List<SharesInfo> sharesList)
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
                return new List<TransactiondataAnalyseInfo>();
            }
            SharesInfoNumArr = SharesInfoNumArr.TrimEnd(',');

            List<LastData> lastList = new List<LastData>();
            #region=================查询股票最后index数据=======================
            string sqlquery = string.Format(@"select SharesInfoNum,Max([Time]) [Time]
from t_shares_transactiondata_time_analyse with(nolock)
where SharesInfoNum in ({0}) and [Date]='{1}'
group by SharesInfoNum", SharesInfoNumArr, dateNow.ToString("yyyy-MM-dd"));
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

            List<TransactiondataAnalyseInfo> resultList = new List<TransactiondataAnalyseInfo>();
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
                                        lastTime = lastData.Time;
                                    }
                                    var tempList = TdxHq_GetTransactionData_byShares(hqClient, tempData.Market, tempData.SharesCode, lastTime, (TRANSACTION_DATA_STRING)ref_string, ref isReconnectClient);
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
        public static List<TransactiondataAnalyseInfo> TdxHq_GetTransactionData_byShares(int hqClient, int market, string sharesCode, DateTime? lastTime, TRANSACTION_DATA_STRING result_info, ref bool isReconnectClient)
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
                    resultlist=tempResultlist.Concat(resultlist).ToList();
                    nStart = (short)(nStart + nCount);
                    totalCount += nCount;
                }
            } while (nCount >= Singleton.Instance.NewTransactionDataCount && iscontinue);


            List<TransactiondataAnalyseInfo> analyseList = new List<TransactiondataAnalyseInfo>();
            DateTime _date;
            DateTime _time;
            int _iTradeStock=0;
            long _lTradeAmount = 0;
            long _iOpeningPrice = 0;
            long _iLastestPrice = 0;
            long _iMaxPrice = 0;
            long _iMinPrice = 0;
            DateTime? FirstTime=null;
            for (int i=0;i<resultlist.Count();i++)
            {
                resultlist[i].IsTimeLast = false;
                if (FirstTime == null || resultlist[i].Time > FirstTime.Value)
                {
                    resultlist[i].IsTimeFirst = true;
                    if (i > 0)
                    {
                        resultlist[i - 1].IsTimeLast = true;
                    }
                }
                else
                {
                    resultlist[i].IsTimeFirst = false;
                }
                if (i == resultlist.Count() - 1)
                {
                    resultlist[i].IsTimeLast = true;
                }
                FirstTime = resultlist[i].Time;


                if (resultlist[i].IsTimeFirst)//当前为该分钟第一笔,结束上一分钟统计
                {
                    if (i > 0)
                    {
                        _date = resultlist[i - 1].Time.Date;
                        _time = resultlist[i - 1].Time;
                        _iLastestPrice = resultlist[i - 1].Price;
                        analyseList.Add(new TransactiondataAnalyseInfo
                        {
                            SharesCode = sharesCode,
                            SharesInfoNum = int.Parse(sharesCode) * 10 + market,
                            Market = market,
                            Date = _date,
                            Time = _time,
                            iTradeStock = _iTradeStock,
                            lTradeAmount = _lTradeAmount,
                            iLastestPrice = _iLastestPrice,
                            iOpeningPrice = _iOpeningPrice,
                            iMaxPrice = _iMaxPrice,
                            iMinPrice = _iMinPrice
                        });
                        _iTradeStock = 0;
                        _lTradeAmount = 0;
                        _iOpeningPrice = 0;
                        _iLastestPrice = 0;
                        _iMaxPrice = 0;
                        _iMinPrice = 0;
                    }
                    _iOpeningPrice = resultlist[i].Price;
                }

                _iTradeStock = _iTradeStock + resultlist[i].Stock;
                _lTradeAmount= _lTradeAmount+ resultlist[i].Stock* resultlist[i].Price;
                if (_iMaxPrice < resultlist[i].Price || _iMaxPrice == 0)
                {
                    _iMaxPrice = resultlist[i].Price;
                }
                if (_iMinPrice > resultlist[i].Price || _iMinPrice == 0)
                {
                    _iMinPrice = resultlist[i].Price;
                }
                if (resultlist[i].IsTimeLast)//当前为最后一分钟，结束当前分钟数据
                {
                    _date = resultlist[i].Time.Date;
                    _time = resultlist[i].Time;
                    _iLastestPrice = resultlist[i].Price;
                    analyseList.Add(new TransactiondataAnalyseInfo
                    {
                        SharesCode = sharesCode,
                        SharesInfoNum = int.Parse(sharesCode) * 10 + market,
                        Market = market,
                        Date = _date,
                        Time = _time,
                        iTradeStock = _iTradeStock,
                        lTradeAmount = _lTradeAmount,
                        iLastestPrice = _iLastestPrice,
                        iOpeningPrice = _iOpeningPrice,
                        iMaxPrice = _iMaxPrice,
                        iMinPrice = _iMinPrice
                    });
                    _iTradeStock = 0;
                    _lTradeAmount = 0;
                    _iOpeningPrice = 0;
                    _iLastestPrice = 0;
                    _iMaxPrice = 0;
                    _iMinPrice = 0;
                }
            }
            return analyseList;
        }
    }
}
