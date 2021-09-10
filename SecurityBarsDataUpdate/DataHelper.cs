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

namespace SecurityBarsDataUpdate
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
                sm_td_string[idx].sResult = new StringBuilder(512 * 800);
            }
        }

        /// <summary>
        /// 获取K线数据
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int,SecurityBarsDataRes> TdxHq_GetSecurityBarsData(List<SecurityBarsDataParList> sharesList)
        {
            ThreadMsgTemplate<SecurityBarsDataPar> data = new ThreadMsgTemplate<SecurityBarsDataPar>();
            data.Init();
            foreach (var item in sharesList)
            {
                foreach (var item2 in item.DataList)
                {
                    data.AddMessage(new SecurityBarsDataPar
                    {
                        SecurityBarsGetCount= item.SecurityBarsGetCount,
                        DataType = item.DataType,
                        SharesCode =item2.SharesCode,
                        StartTimeKey=item2.StartTimeKey,
                        EndTimeKey=item2.EndTimeKey,
                        Market=item2.Market,
                        PreClosePrice=item2.PreClosePrice,
                        YestodayClosedPrice=item2.YestodayClosedPrice,
                        LastTradeStock=item2.LastTradeStock,
                        LastTradeAmount=item2.LastTradeAmount
                    });
                }
            }

            Dictionary<int, SecurityBarsDataRes> resultList = new Dictionary<int, SecurityBarsDataRes>();
            object resultLock = new object();

            Task[] tArr = new Task[Singleton.Instance.hqClientCount];
            for (int i = 0; i < Singleton.Instance.hqClientCount; i++)
            {
                tArr[i] = Task.Factory.StartNew((ref_string) =>
                {
                    int hqClient = Singleton.Instance.GetHqClient();
                    try
                    {
                        do
                        {
                            SecurityBarsDataPar tempData = new SecurityBarsDataPar();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }

                            bool isReconnectClient = false;
                            var tempList = TdxHq_GetSecurityBarsData_byShares(hqClient, tempData,(TRANSACTION_DATA_STRING)ref_string, ref isReconnectClient);

                            if (isReconnectClient)
                            {
                                Singleton.Instance.AddRetryClient(hqClient);
                                hqClient = Singleton.Instance.GetHqClient();
                            }

                            lock (resultLock)
                            {
                                if (resultList.ContainsKey(tempData.DataType)) 
                                {
                                    resultList[tempData.DataType].DataList.AddRange(tempList);
                                }
                                else 
                                {
                                    resultList.Add(tempData.DataType, new SecurityBarsDataRes 
                                    {
                                        DataType= tempData.DataType,
                                        DataList= tempList
                                    });
                                }
                            }
                        } while (true);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("更新K线数据出错", ex);
                    }
                    finally
                    {
                        Singleton.Instance.AddHqClient(hqClient);
                    }
                }, sm_td_string[i]);
            }
            Task.WaitAll(tArr);
            data.Release();

            return resultList;
        }

        /// <summary>
        /// 获取某只股票K线数据
        /// </summary>
        /// <returns></returns>
        private static List<SecurityBarsDataInfo> TdxHq_GetSecurityBarsData_byShares(int hqClient, SecurityBarsDataPar sharesData, TRANSACTION_DATA_STRING result_info, ref bool isReconnectClient)
        {
            int datatype = sharesData.DataType;
            if (!CheckDataType(datatype))
            {
                return new List<SecurityBarsDataInfo>();
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
                StringBuilder sErrInfo = result_info.sErrInfo;
                StringBuilder sResult = result_info.sResult;
                sErrInfo.Clear();
                sResult.Clear();
                nCount = (short)defaultGetCount;

                bool bRet = TradeX_M.TdxHq_GetSecurityBars(hqClient, category, (byte)sharesData.Market, sharesData.SharesCode, nStart, ref nCount, sResult, sErrInfo);
                if (!bRet)
                {
                    isReconnectClient = true;
                    Logger.WriteFileLog(string.Format("获取K线数据出错，原因：{0}", sErrInfo.ToString()),null);
                    return new List<SecurityBarsDataInfo>();
                }
                else
                {
                    string result = sResult.ToString();
                    string[] rows = result.Split('\n');
                    Array.Reverse(rows);

                    for (int i = 0; i < rows.Length-1; i++)
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

                            DateTime Time=DateTime.Parse("1991-01-01 00:00:00");
                            long timeKey = 0;
                            if (!ParseTime(datatype, TimeStr, ref Time,ref timeKey))
                            {
                                return new List<SecurityBarsDataInfo>();
                            }

                            if (timeKey<sharesData.StartTimeKey && sharesData.StartTimeKey!=-1)
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
                                DataType= datatype,
                                GroupTimeKey = timeKey,
                                TradeStock = Volume,
                                TradeAmount = Turnover,
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
            } while (nCount >=defaultGetCount && isContinue);

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
            foreach (var item in resultlist)
            {
                j++;
                item.PreClosePrice = preClosePrice;
                preClosePrice = item.ClosedPrice;
                if (j == totalCount)
                {
                    item.IsLast = true;
                }
                else
                {
                    item.IsLast = false;
                }
                item.YestodayClosedPrice = sharesData.YestodayClosedPrice;
                item.LastTradeStock = lastTradeStock;
                item.LastTradeAmount = lastTradeAmount;
                lastTradeStock = lastTradeStock+item.TradeStock;
                lastTradeAmount = lastTradeAmount+item.TradeAmount;
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
    }
}
