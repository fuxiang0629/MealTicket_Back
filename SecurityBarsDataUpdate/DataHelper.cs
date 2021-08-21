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
                sm_td_string[idx].sResult = new StringBuilder(512 * Singleton.Instance.SecurityBarsGetCount);
            }
        }

        /// <summary>
        /// 获取K线数据
        /// </summary>
        /// <returns></returns>
        public static List<SecurityBarsDataInfo> TdxHq_GetSecurityBarsData(List<SecurityBarsDataInfo> sharesList)
        {
            ThreadMsgTemplate<SecurityBarsDataInfo> data = new ThreadMsgTemplate<SecurityBarsDataInfo>();
            data.Init();
            foreach (var item in sharesList)
            {
                data.AddMessage(item);
            }

            List<SecurityBarsDataInfo> resultList = new List<SecurityBarsDataInfo>();
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
                            SecurityBarsDataInfo tempData = new SecurityBarsDataInfo();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }

                            bool isReconnectClient = false;

                            var tempList = TdxHq_GetSecurityBarsData_byShares(hqClient, tempData, (TRANSACTION_DATA_STRING)ref_string, ref isReconnectClient);
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
                        Logger.WriteFileLog("更新K线数据出错", ex);
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
        /// 获取某只股票K线数据
        /// </summary>
        /// <returns></returns>
        public static List<SecurityBarsDataInfo> TdxHq_GetSecurityBarsData_byShares(int hqClient, SecurityBarsDataInfo sharesData,TRANSACTION_DATA_STRING result_info, ref bool isReconnectClient)
        {
            List<SecurityBarsDataInfo> resultlist = new List<SecurityBarsDataInfo>();
            if (sharesData.Time == null)
            {
                sharesData.Time = DateTime.Now.Date;
            }
  
            short nStart = 0; 
            short nCount = 0;
            short defaultGetCount= (short)Singleton.Instance.SecurityBarsGetCount;
            bool isContinue = true;
            do
            {
                StringBuilder sErrInfo = result_info.sErrInfo;
                StringBuilder sResult = result_info.sResult;
                sErrInfo.Clear();
                sResult.Clear();
                nCount = defaultGetCount;

                bool bRet = TradeX_M.TdxHq_GetSecurityBars(hqClient, 7, (byte)sharesData.Market, sharesData.SharesCode, nStart, ref nCount, sResult, sErrInfo);
                if (!bRet)
                {
                    isReconnectClient = true;
                    string errDes = string.Format("获取K线数据出错，原因：{0}", sErrInfo.ToString());
                    Console.WriteLine(errDes);
                    return new List<SecurityBarsDataInfo>();
                }
                else
                {
                    string result = sResult.ToString();
                    string[] rows = result.Split('\n');
                    Array.Reverse(rows);

                    List<SecurityBarsDataInfo> tempResultlist = new List<SecurityBarsDataInfo>();
                    for (int i = 0; i < rows.Length-1; i++)
                    {
                        string[] column = rows[i].Split('\t');
                        if (column.Length < 7)
                        {
                            string errDes = "获取K线数据结构有误";
                            Console.WriteLine(errDes);
                            return new List<SecurityBarsDataInfo>();
                        }

                        try
                        {
                            int Market = sharesData.Market;//市场代码
                            string SharesCode = sharesData.SharesCode;//股票代码
                            string TimeStr = column[0];
                            DateTime Time;
                            if (TimeStr.Length != 18)
                            {
                                throw new Exception("时间格式有误，时间:"+ TimeStr+",股票:"+ SharesCode+"，市场"+ Market);
                            }

                            //1989--15--20
                            Int32 lYear = 4011 - Int32.Parse(TimeStr.Substring(0, 4));
                            Int32 lMonth = 20 - Int32.Parse(TimeStr.Substring(6, 2));
                            Int32 lDay = 48 - Int32.Parse(TimeStr.Substring(10, 2));
                            Int32 lHour = Int32.Parse(TimeStr.Substring(13, 2));
                            Int32 lMinute = Int32.Parse(TimeStr.Substring(16, 2));
                            if (lHour == 13 && lMinute == 0)
                            {
                                lHour = 11;
                                lMinute = 30;
                            }
                            Time = new DateTime(lYear, lMonth, lDay, lHour, lMinute, 0);

                            long ClosedPrice = (long)(Math.Round(float.Parse(column[2]) * 10000, 0));//收盘
                            long OpenedPrice = (long)(Math.Round(float.Parse(column[1]) * 10000, 0));//开盘
                            long MaxPrice = (long)(Math.Round(float.Parse(column[3]) * 10000, 0));//最高
                            long MinPrice = (long)(Math.Round(float.Parse(column[4]) * 10000, 0));//最低
                            int Volume = int.Parse(column[5]);//成交量
                            long Turnover = (long)(Math.Round(float.Parse(column[6]) * 10000, 0));//成交额

                            if (sharesData.Time.Value > Time)
                            {
                                isContinue = false;
                                break;
                            }

                            tempResultlist.Add(new SecurityBarsDataInfo
                            {
                                SharesCode = SharesCode,
                                TimeStr = TimeStr,
                                ClosedPrice = ClosedPrice,
                                Market = Market,
                                MaxPrice = MaxPrice,
                                MinPrice = MinPrice,
                                OpenedPrice = OpenedPrice,
                                Time = Time,
                                Date = Time.Date,
                                TradeStock = Volume,
                                TradeAmount = Turnover
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("获取K线数据出错", ex);
                            return new List<SecurityBarsDataInfo>();
                        }
                    }
                    resultlist= resultlist.Concat(tempResultlist).ToList();
                    nStart = (short)(nStart + nCount);
                }
            } while (nCount >=defaultGetCount && isContinue);

            resultlist = resultlist.OrderBy(e => e.Time).ToList();
            long preClosePrice = sharesData.PreClosePrice;
            foreach (var item in resultlist)
            {
                item.PreClosePrice = preClosePrice;
                preClosePrice = item.ClosedPrice;
            }    
            return resultlist;
        }
    }
}
