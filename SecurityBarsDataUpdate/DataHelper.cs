using FXCommon.Common;
using FXCommon.Database;
using MealTicket_DBCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradeAPI;

namespace SecurityBarsDataUpdate
{
    public class DataHelper
    {
        /// <summary>
        /// 获取K线数据
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, SecurityBarsDataRes> TdxHq_GetSecurityBarsData(int handlerType, List<SecurityBarsDataParList> sharesList, ref List<SecurityBarsDataParList> failPackage, ref List<SecurityBarsDataParList> successPackage,ref int MaxElapsedTime)
        {
            var parList = _buildRequestDataQueue(sharesList);
            int parListCount = parList.Count();
            if (parListCount == 0)
            {
                return new Dictionary<int, SecurityBarsDataRes>();
            }
            Semaphore semap = new Semaphore(0, 0x7FFFFFFF);
            ManualResetEvent watiHandle = new ManualResetEvent(false);

            //结果数据字典
            Dictionary<int, SecurityBarsDataRes> resultList = new Dictionary<int, SecurityBarsDataRes>();
            Dictionary<int, SecurityBarsDataParList> successPackageDic = sharesList.ToDictionary(k => k.DataType, v => new SecurityBarsDataParList
            {
                DataType = v.DataType,
                SecurityBarsGetCount = v.SecurityBarsGetCount,
                DataList = new List<SecurityBarsDataPar>()
            });
            Dictionary<int, SecurityBarsDataParList> failPackageDic = sharesList.ToDictionary(k => k.DataType, v => new SecurityBarsDataParList
            {
                DataType = v.DataType,
                SecurityBarsGetCount = v.SecurityBarsGetCount,
                DataList = new List<SecurityBarsDataPar>()
            });

            object successLock = new object();
            object failLock = new object();
            object downloadLock = new object();
            Dictionary<int, int> downCounter = new Dictionary<int, int>();
            foreach (var item in parList)
            {
                Singleton.Instance.threadTask.AddParQueue(new TaskDataInfo
                {
                    semap = semap,
                    FailPackage = failPackageDic,
                    SuccessPackage = successPackageDic,
                    WatiHandle = watiHandle,
                    HandlerType = handlerType,
                    ResultList = resultList,
                    TotalCount = parListCount,
                    Data = item,
                    SuccessLock = successLock,
                    FailLock = failLock,
                    DownLoadLock = downloadLock,
                    downCounter = downCounter
                });
            } 
            watiHandle.WaitOne();
            semap.Close();
            watiHandle.Close();

            MaxElapsedTime = 0;
            foreach(var item in downCounter)
            {
                if (item.Value > MaxElapsedTime)
                {
                    MaxElapsedTime = item.Value;
                }
            }
           


            failPackage = new List<SecurityBarsDataParList>();
            successPackage = new List<SecurityBarsDataParList>();
            foreach (var item in failPackageDic)
            {
                if (item.Value.DataList.Count() <= 0)
                {
                    continue;
                }
                failPackage.Add(item.Value);
            }
            foreach (var item in successPackageDic)
            {
                if (item.Value.DataList.Count() <= 0)
                {
                    continue;
                }
                successPackage.Add(item.Value);
            }
            
            return resultList;
        }

        private static List<SecurityBarsDataPar> _buildRequestDataQueue(List<SecurityBarsDataParList> sharesList)
        {
            List<SecurityBarsDataPar> list = new List<SecurityBarsDataPar>();
            foreach (var item in sharesList)
            {
                foreach (var item2 in item.DataList)
                {
                    list.Add(new SecurityBarsDataPar
                    {
                        SecurityBarsGetCount = item.SecurityBarsGetCount,
                        DataType = item.DataType,
                        PlateId = item2.PlateId,
                        WeightType = item2.WeightType,
                        SharesCode = item2.SharesCode,
                        StartTimeKey = item2.StartTimeKey,
                        EndTimeKey = item2.EndTimeKey,
                        Market = item2.Market,
                        PreClosePrice = item2.PreClosePrice,
                        YestodayClosedPrice = item2.YestodayClosedPrice,
                        LastTradeStock = item2.LastTradeStock,
                        LastTradeAmount = item2.LastTradeAmount
                    });
                }
            }
            return list;
        }

        /// <summary>
        /// 获取某只股票K线数据
        /// </summary>
        /// <returns></returns>
        public static List<SecurityBarsDataInfo> TdxHq_GetSecurityBarsData_byShares(int hqClient, int handlerType, SecurityBarsDataPar sharesData, ref bool isReconnectClient, ref StringBuilder sResult, ref StringBuilder sErrInfo,ref int download_second)
        {
            int datatype = sharesData.DataType;
            if (!CheckDataType(datatype))
            {
                return new List<SecurityBarsDataInfo>();
            }
            if ((handlerType == 1|| handlerType==3) && (datatype == 2 || datatype == 7))
            {
                DateTime timeNow = DateTime.Now;
                TimeSpan spanNow = TimeSpan.Parse(timeNow.ToString("HH:mm:ss"));
                long groupTimeKey = 0;
                if (datatype == 2)
                {
                    groupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd0930"));
                }
                else if (datatype == 7)
                {
                    groupTimeKey = long.Parse(timeNow.ToString("yyyyMMdd"));
                }
                else
                {
                    return new List<SecurityBarsDataInfo>();
                }
                if (spanNow > TimeSpan.Parse(Singleton.Instance.TradeTime1) && spanNow < TimeSpan.Parse(Singleton.Instance.TradeTime2) && sharesData.YestodayClosedPrice>0)
                {
                    return new List<SecurityBarsDataInfo>
                    {
                        new SecurityBarsDataInfo
                        {
                            SharesCode=sharesData.SharesCode,
                            Market=sharesData.Market,
                            LastTradeStock=0,
                            TimeStr=timeNow.ToString("yyyy-MM-dd 09:30:00"),
                            TradeStock=0,
                            ClosedPrice=sharesData.YestodayClosedPrice,
                            DataType=datatype,
                            GroupTimeKey=groupTimeKey,
                            IsLast=true,
                            IsVaild=true,
                            LastTradeAmount=0,
                            MaxPrice=sharesData.YestodayClosedPrice,
                            MinPrice=sharesData.YestodayClosedPrice,
                            OpenedPrice=sharesData.YestodayClosedPrice,
                            PreClosePrice=sharesData.YestodayClosedPrice,
                            Time=DateTime.Parse(timeNow.ToString("yyyy-MM-dd 09:30:00")),
                            TradeAmount=0,
                            YestodayClosedPrice=sharesData.YestodayClosedPrice
                        }
                    };
                }

                if (spanNow > TimeSpan.Parse(Singleton.Instance.TradeTime2) && spanNow < TimeSpan.Parse(Singleton.Instance.TradeTime3))
                {
                    var quotes = GetSharesQuotes(sharesData.Market, sharesData.SharesCode, timeNow.Date);
                    if (quotes == null)
                    {
                        return new List<SecurityBarsDataInfo>();
                    }
                    if (quotes.ClosedPrice == 0)
                    {
                        return new List<SecurityBarsDataInfo>();
                    }
                    return new List<SecurityBarsDataInfo>
                    {
                        new SecurityBarsDataInfo
                        {
                            SharesCode=sharesData.SharesCode,
                            Market=sharesData.Market,
                            LastTradeStock=0,
                            TimeStr=timeNow.ToString("yyyy-MM-dd 09:30:00"),
                            TradeStock=quotes.TotalCount*100,
                            ClosedPrice=quotes.PresentPrice==0?quotes.ClosedPrice:quotes.PresentPrice,
                            DataType=datatype,
                            GroupTimeKey=groupTimeKey,
                            IsLast=true,
                            IsVaild=true,
                            LastTradeAmount=0,
                            MaxPrice=quotes.MaxPrice==0?quotes.ClosedPrice:quotes.MaxPrice,
                            MinPrice=quotes.MinPrice==0?quotes.ClosedPrice:quotes.MinPrice,
                            OpenedPrice=quotes.OpenedPrice==0?quotes.ClosedPrice:quotes.OpenedPrice,
                            PreClosePrice=quotes.ClosedPrice,
                            Time=DateTime.Parse(timeNow.ToString("yyyy-MM-dd 09:30:00")),
                            TradeAmount=quotes.TotalAmount,
                            YestodayClosedPrice=quotes.ClosedPrice
                        }
                    };
                }
            }

            int defaultGetCount = sharesData.SecurityBarsGetCount;
            if (!CheckDefaultGetCount(ref defaultGetCount))
            {
                return new List<SecurityBarsDataInfo>();
            }

            byte category = 0;
            if (!CheckCategory(datatype, ref category))
            {
                return new List<SecurityBarsDataInfo>();
            }

            Dictionary<long, SecurityBarsDataInfo> tempResultDic = new Dictionary<long, SecurityBarsDataInfo>();

            short nStart = 0;
            short nCount = 0;
            bool isContinue = true;
            do
            {
                nCount = (short)defaultGetCount;

                bool bRet = false;
                if (handlerType == 1 || handlerType == 2 || handlerType == 21)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    bRet = TradeX_M.TdxHq_GetSecurityBars(hqClient, category, (byte)sharesData.Market, sharesData.SharesCode, nStart, ref nCount, sResult, sErrInfo);
                    stopwatch.Stop();
                    download_second = download_second + (int)stopwatch.ElapsedMilliseconds;
                }
                else
                {
                    bRet = TradeX_M.TdxHq_GetIndexBars(hqClient, category, (byte)sharesData.Market, sharesData.SharesCode, nStart, ref nCount, sResult, sErrInfo);
                }

                if (!bRet)
                {
                    isReconnectClient = true;
                    Logger.WriteFileLog(string.Format("获取K线数据出错，原因：{0}", sErrInfo.ToString()), null);
                    return new List<SecurityBarsDataInfo>();
                }
                else
                {
                    string result = sResult.ToString();
                    string[] rows = result.Split('\n');
                    Array.Reverse(rows);

                    for (int i = 0; i < rows.Length - 1; i++)
                    {
                        string[] column = rows[i].Split('\t');
                        if (column.Length < 7)
                        {
                            Logger.WriteFileLog("获取K线数据结构有误", null);
                            return new List<SecurityBarsDataInfo>();
                        }

                        try
                        {
                            int Market = sharesData.Market;//市场代码
                            string SharesCode = sharesData.SharesCode;//股票代码
                            string TimeStr = column[0];

                            DateTime Time = DateTime.Parse("1991-01-01 00:00:00");
                            long timeKey = 0;
                            if (!ParseTime(datatype, TimeStr, ref Time, ref timeKey))
                            {
                                return new List<SecurityBarsDataInfo>();
                            }

                            if (timeKey < sharesData.StartTimeKey && sharesData.StartTimeKey != -1)
                            {
                                isContinue = false;
                                break;
                            }
                            if (timeKey > sharesData.EndTimeKey && sharesData.EndTimeKey != -1)
                            {
                                continue;
                            }

                            long ClosedPrice = (long)(Math.Round(float.Parse(column[2]) * 10000, 0));//收盘
                            long OpenedPrice = (long)(Math.Round(float.Parse(column[1]) * 10000, 0));//开盘
                            long MaxPrice = (long)(Math.Round(float.Parse(column[3]) * 10000, 0));//最高
                            long MinPrice = (long)(Math.Round(float.Parse(column[4]) * 10000, 0));//最低
                            int Volume = int.Parse(column[5]);//成交量
                            long Turnover = (long)(Math.Round(float.Parse(column[6]) * 10000, 0));//成交额

                            tempResultDic[timeKey] = new SecurityBarsDataInfo
                            {
                                SharesCode = SharesCode,
                                TimeStr = TimeStr,
                                ClosedPrice = ClosedPrice,
                                Market = Market,
                                MaxPrice = MaxPrice,
                                MinPrice = MinPrice,
                                OpenedPrice = OpenedPrice,
                                Time = Time,
                                DataType = datatype,
                                GroupTimeKey = timeKey,
                                TradeStock = Volume,
                                TradeAmount = Turnover,
                                PlateId = sharesData.PlateId,
                                WeightType = sharesData.WeightType
                            };
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("获取K线数据出错", ex);
                            return new List<SecurityBarsDataInfo>();
                        }
                    }
                    nStart = (short)(nStart + nCount);
                }
            } while (nCount >= defaultGetCount && isContinue);

            if (tempResultDic.Count() <= 0)
            {
                return new List<SecurityBarsDataInfo>();
            }

            var resultlist = (from item in tempResultDic
                              orderby item.Key
                              select item.Value).ToList();
            long preClosePrice = sharesData.PreClosePrice;
            int totalCount = resultlist.Count();
            int j = 0;
            long lastTradeStock = sharesData.LastTradeStock;
            long lastTradeAmount = sharesData.LastTradeAmount;
            long yestodayClosedPrice = sharesData.YestodayClosedPrice;
            DateTime? lastDate = null;
            bool isVaild = true;

            var range = (from x in Singleton.Instance._FundmultipleSession.GetSessionData()
                         where (x.LimitMarket == -1 || x.LimitMarket == sharesData.Market) && sharesData.SharesCode.StartsWith(x.LimitKey)
                         orderby x.Priority
                         select x.Range).FirstOrDefault();
            if (range == 0)
            {
                range = 2000;
            }

            long limitUpPrice = 0;
            long limitDownPrice = 0;

            bool isNew = false;
            var sharesInfo = (from item in Singleton.Instance._SharesBaseSession.GetSessionData()
                              where item.Market == sharesData.Market && item.SharesCode == sharesData.SharesCode && (item.SharesName.StartsWith(Singleton.Instance.New_N) || item.SharesName.StartsWith(Singleton.Instance.New_C))
                              select item).FirstOrDefault();
            if (sharesInfo != null)
            {
                isNew = true;
            }
            foreach (var item in resultlist)
            {
                if (lastDate != null && lastDate.Value != item.Time.Value.Date)
                {
                    yestodayClosedPrice = preClosePrice;
                }

                if (yestodayClosedPrice > 0 && handlerType == 1 && !isNew)
                {
                    if (lastDate == null || lastDate.Value != item.Time.Value.Date)
                    {
                        //计算涨停价
                        limitUpPrice = ((long)(yestodayClosedPrice * (1 + range * 1.0 / 10000) / 100 + 0.5)) * 100;
                        //计算跌停价
                        limitDownPrice = ((long)(yestodayClosedPrice * (1 - range * 1.0 / 10000) / 100 + 0.5)) * 100;
                    }
                    //去除涨跌幅不正常的
                    if (item.ClosedPrice > limitUpPrice || item.ClosedPrice < limitDownPrice)
                    {
                        item.OpenedPrice = preClosePrice;
                        item.ClosedPrice = preClosePrice;
                        item.MaxPrice = preClosePrice;
                        item.MinPrice = preClosePrice;
                        item.TradeStock = 0;
                        item.TradeAmount = 0;
                    }
                }

                if (handlerType == 1 || handlerType == 2 || handlerType == 21)
                {
                    if (lastDate == null || lastDate.Value != item.Time.Value.Date)
                    {
                        if (CheckQuotes(item.Market, item.SharesCode, item.Time.Value.Date))
                        {
                            isVaild = true;
                        }
                        else
                        {
                            isVaild = false;
                        }
                    }
                    item.IsVaild = isVaild;
                    if (!isVaild)
                    {
                        item.OpenedPrice = preClosePrice;
                        item.ClosedPrice = preClosePrice;
                        item.MaxPrice = preClosePrice;
                        item.MinPrice = preClosePrice;
                        item.TradeStock = 0;
                        item.TradeAmount = 0;
                    }
                }

                item.YestodayClosedPrice = yestodayClosedPrice;

                item.PreClosePrice = preClosePrice;
                preClosePrice = item.ClosedPrice;

                j++;
                if (j == totalCount)
                {
                    item.IsLast = true;
                }
                else
                {
                    item.IsLast = false;
                }
                if (datatype == 2)
                {
                    if (lastDate != null && lastDate.Value != item.Time.Value.Date)
                    {
                        lastTradeStock = 0;
                        lastTradeAmount = 0;
                    }
                    item.LastTradeAmount = lastTradeAmount;
                    item.LastTradeStock = lastTradeStock;
                    lastTradeStock = lastTradeStock + item.TradeStock;
                    lastTradeAmount = lastTradeAmount + item.TradeAmount;
                }
                else
                {
                    item.LastTradeAmount = 0;
                    item.LastTradeStock = 0;
                }

                lastDate = item.Time.Value.Date;
            }
            return resultlist;
        }

        /// <summary>
        /// K线时间解析
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="timeStr"></param>
        /// <returns></returns>
        private static bool ParseTime(int dataType,string timeStr,ref DateTime time,ref long groupTimeKey)
        {
            int lYear = 0, lMonth = 0, lDay = 0, lHour = 0, lMinute = 0;
            try
            {
                //1分钟/5分钟/15分钟/30分钟/60分钟K线
                if (dataType == 2 || dataType == 3 || dataType == 4 || dataType == 5 || dataType == 6)
                {
                    if (timeStr.Length != 18)
                    {
                        lYear = int.Parse(timeStr.Substring(0, 4));
                        lMonth = int.Parse(timeStr.Substring(4, 2));
                        lDay = int.Parse(timeStr.Substring(6, 2));
                        lHour = int.Parse(timeStr.Substring(8, 2));
                        lMinute = int.Parse(timeStr.Substring(10, 2));
                    }
                    else
                    {
                        lYear = 31 + int.Parse(timeStr.Substring(0, 4));
                        lMonth = 20 - int.Parse(timeStr.Substring(6, 2));
                        lDay = 48 - int.Parse(timeStr.Substring(10, 2));
                        lHour = int.Parse(timeStr.Substring(13, 2));
                        lMinute = int.Parse(timeStr.Substring(16, 2));
                    }
                    if (lHour == 13 && lMinute == 0)
                    {
                        lHour = 11;
                        lMinute = 30;
                    }
                    time = new DateTime(lYear, lMonth, lDay, lHour, lMinute, 0);
                    groupTimeKey = long.Parse(time.ToString("yyyyMMddHHmm"));
                    return true;
                }
                //日
                else if (dataType == 7)
                {
                    int tempTime = int.Parse(timeStr);
                    lYear = tempTime / 10000;
                    lMonth = (tempTime / 100) % 100;
                    lDay = tempTime % 100;
                    time = new DateTime(lYear, lMonth, lDay, 15, 0, 0);
                    groupTimeKey = long.Parse(time.ToString("yyyyMMdd"));
                    return true;
                }
                //周
                else if (dataType == 8)
                {
                    int tempTime = int.Parse(timeStr);
                    lYear = tempTime / 10000;
                    lMonth = (tempTime / 100) % 100;
                    lDay = tempTime % 100;
                    time = new DateTime(lYear, lMonth, lDay, 15, 0, 0);
                    int timeInt = int.Parse(time.ToString("yyyyMMdd"));
                    groupTimeKey = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                    where item.the_date == timeInt
                                    select item.the_week).FirstOrDefault() ?? 0;
                    if (groupTimeKey == 0)
                    {
                        throw new Exception("获取周数据有误");
                    }
                    return true;
                }
                //月/季度/年
                else if (dataType == 9)
                {
                    int tempTime = int.Parse(timeStr);
                    lYear = tempTime / 10000;
                    lMonth = (tempTime / 100) % 100;
                    lDay = tempTime % 100;
                    time = new DateTime(lYear, lMonth, lDay, 15, 0, 0);
                    groupTimeKey = long.Parse(time.ToString("yyyyMM"));
                    return true;
                }
                //季度
                else if (dataType == 10)
                {
                    int tempTime = int.Parse(timeStr);
                    lYear = tempTime / 10000;
                    lMonth = (tempTime / 100) % 100;
                    lDay = tempTime % 100;
                    time = new DateTime(lYear, lMonth, lDay, 15, 0, 0);
                    int timeInt = int.Parse(time.ToString("yyyyMMdd"));
                    groupTimeKey = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                    where item.the_date == timeInt
                                    select item.the_quarter).FirstOrDefault() ?? 0;
                    if (groupTimeKey == 0)
                    {
                        throw new Exception("获取季度据有误");
                    }
                    return true;
                }
                //年
                else if (dataType == 11)
                {
                    int tempTime = int.Parse(timeStr);
                    lYear = tempTime / 10000;
                    lMonth = (tempTime / 100) % 100;
                    lDay = tempTime % 100;
                    time = new DateTime(lYear, lMonth, lDay, 15, 0, 0);
                    groupTimeKey = lYear;
                    return true;
                }
                else
                {
                    throw new Exception("参数dataType有误");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("时间解析有误,错误时间:"+ timeStr,ex);
                return false;
            }
        }

        private static bool CheckDataType(int dataType)
        {
            if (dataType < 2 || dataType > 11)
            {
                return false;
            }
            return true;
        }

        private static bool CheckDefaultGetCount(ref int defaultGetCount)
        {
            if (defaultGetCount <= 0)
            {
                defaultGetCount=Singleton.Instance.SecurityBarsGetCount;
            }
            return true;
        }

        private static bool CheckCategory(int dataType,ref byte category)
        {
            switch (dataType)
            {
                case 2:
                    category = 7;
                    break;
                case 3:
                    category = 0;
                    break;
                case 4:
                    category = 1;
                    break;
                case 5:
                    category = 2;
                    break;
                case 6:
                    category = 3;
                    break;
                case 7:
                    category = 4;
                    break;
                case 8:
                    category = 5;
                    break;
                case 9:
                    category = 6;
                    break;
                case 10:
                    category = 10;
                    break;
                case 11:
                    category = 11;
                    break;
                default:
                    return false;
            }
            return true;
        }

        private static t_shares_quotes_date GetSharesQuotes(int market, string sharesCode, DateTime date) 
        {
            int dateInt = int.Parse(date.ToString("yyyyMMdd"));
            string dateStr = date.ToString("yyyy-MM-dd");
            var quotesSession = Singleton.Instance._SharesQuotesSession.GetSessionData();
            if (dateInt == quotesSession.Date)
            {
                int key = int.Parse(sharesCode) * 10 + market;
                if (!quotesSession.QuotesDic.ContainsKey(key))
                {
                    return null;
                }
                return quotesSession.QuotesDic[key];
            }
            else
            {
                //Process processes = Process.GetCurrentProcess();

                Console.WriteLine(string.Format("获取K线未匹配到缓存，日期:{0},缓存日期：{3},股票:{1}-{2},时间:{4}", dateInt, market, sharesCode, quotesSession.Date, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                using (var db = new meal_ticketEntities())
                {
                    var quotes = (from item in db.t_shares_quotes_date
                                  where item.Date == dateStr && item.Market == market && item.SharesCode == sharesCode
                                  select item).FirstOrDefault();
                    return quotes;
                }
            }
        }

        private static bool CheckQuotes(int market,string sharesCode,DateTime date) 
        {
            var result=GetSharesQuotes(market, sharesCode, date);
            if (result == null)
            {
                return false;
            }
            if (result.PresentPrice > 0)
            {
                return true;
            }
            return false;
        }
    }
}
