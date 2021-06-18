using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TradeAPI;

namespace SharesHqService
{
    public class ShareHelper
    {
        /// <summary>
        /// 获取指定市场内的证券数目
        /// </summary>
        /// <param name="nMarket">市场代码0.深圳 1.上海</param>
        /// <returns></returns>
        public static short TdxHq_GetSecurityCount(byte nMarket)
        {
            int hqClient = Singleton.Instance.GetHqClient();
            short nCount = 0;
            StringBuilder sErrInfo = new StringBuilder(256);
            bool bRet = TradeX_M.TdxHq_GetSecurityCount(hqClient, nMarket, ref nCount, sErrInfo);
            if (!bRet)
            {
                Singleton.Instance.AddRetryClient(hqClient);
                string errDes = string.Format("获取指定市场（{0}）的证券数目出错，原因：{1}", nMarket == 0 ? "深圳" : "上海", sErrInfo.ToString());
                Console.WriteLine(errDes);
            }
            else
            {
                //归还行情链接
                Singleton.Instance.AddHqClient(hqClient);
            }
            return nCount;
        }

        /// <summary>
        /// 获取指定市场内所有股票代码
        /// </summary>
        /// <param name="nMarket">市场代码0.深圳 1.上海</param>
        /// <returns></returns>
        public static List<SharesBaseInfo> TdxHq_GetSecurityList(byte nMarket)
        {
            var pingyinList = Helper.GetSharesPingyin();

            //某个市场股票总数量
            short totalCount = TdxHq_GetSecurityCount(nMarket);

            short realCount = 0;
            short nCount = 0;

            List<SharesBaseInfo> list = new List<SharesBaseInfo>();
            int hqClient = Singleton.Instance.GetHqClient();
            StringBuilder sErrInfo = new StringBuilder(256);
            StringBuilder sResult = new StringBuilder(1024 * 1024);
            int failCount = 0;//失败次数
            while (totalCount > realCount)
            {
                bool bRet = TradeX_M.TdxHq_GetSecurityList(hqClient, nMarket, realCount, ref nCount, sResult, sErrInfo);
                if (!bRet)
                {
                    failCount++;
                    Singleton.Instance.AddRetryClient(hqClient);
                    string errDes = string.Format("指定市场（{0}）所有股票代码出错，原因：{1}", nMarket == 0 ? "深圳" : "上海", sErrInfo.ToString());
                    Console.WriteLine(errDes);
                    if (failCount <= 3)
                    {
                        hqClient = Singleton.Instance.GetHqClient();
                        continue;
                    }
                    return null;
                }
                realCount += nCount;
                string result = sResult.ToString();

                string[] rows = result.Split('\n');
                for (int i = 0; i < rows.Length; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    string[] column = rows[i].Split('\t');
                    if (column.Length < 6)
                    {
                        //归还行情链接
                        Singleton.Instance.AddHqClient(hqClient);
                        return null;
                    }
                    string ShareCode;//股票代码
                    int ShareHandCount;//股票一手数量
                    string ShareName;//股票名称
                    long ShareClosedPrice;//股票昨日收盘价格
                    try
                    {
                        ShareCode = column[0];//股票代码
                        ShareHandCount = int.Parse(column[1]);//股票一手数量
                        ShareName = column[2];//股票名称
                        ShareClosedPrice = (long)(Math.Round(float.Parse(column[5]) * Singleton.Instance.PriceFormat, 0));//股票昨日收盘价格

                        string Pyjc = string.Empty;
                        Pyjc=pingyinList.Where(e => e.ShareCode == ShareCode && e.Market == nMarket).Select(e => e.Pyjc).FirstOrDefault();
                        if (string.IsNullOrEmpty(Pyjc))
                        {
                            Pyjc = Helper.GetFirstSpell(ShareName);
                        }

                        list.Add(new SharesBaseInfo
                        {
                            ShareCode = ShareCode,
                            ShareHandCount = ShareHandCount,
                            ShareName = ShareName,
                            Pyjc= Pyjc,
                            ShareClosedPrice = ShareClosedPrice,
                            Market = nMarket
                        });
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            //归还行情链接
            Singleton.Instance.AddHqClient(hqClient);
            return list;
        }

        /// <summary>
        /// 批量获取所有股票的五档报价数据
        /// </summary>
        /// <returns></returns>
        public static List<SharesQuotesInfo> TdxHq_GetSecurityQuotes()
        {
            List<SharesBaseInfo> list = new List<SharesBaseInfo>();
            //拼接所有股票
            List<SharesBaseInfo> tempList = new List<SharesBaseInfo>(); 
            List<SharesQuotesInfo> resultlist = new List<SharesQuotesInfo>();
            if (Singleton.Instance.SharesBaseInfoList.TryGetValue(0, out tempList))
            {
                Regex regex = new Regex(Singleton.Instance.SharesCodeMatch0);
                tempList = (from item in tempList
                            where regex.IsMatch(item.ShareCode)
                            select item).ToList();
                list.AddRange(tempList);
            }
            if (Singleton.Instance.SharesBaseInfoList.TryGetValue(1, out tempList))
            {
                Regex regex = new Regex(Singleton.Instance.SharesCodeMatch1);
                tempList = (from item in tempList
                            where regex.IsMatch(item.ShareCode)
                            select item).ToList();
                list.AddRange(tempList);
            }
            //计算分页数量
            int QuotesCount = Singleton.Instance.QuotesCount;
            int totalCount = list.Count();
            int pageSize;
            if (totalCount % QuotesCount == 0)
            {
                pageSize = totalCount / QuotesCount;
            }
            else
            {
                pageSize = totalCount / QuotesCount + 1;
            }
            list = list.OrderBy(e => e.ShareCode).ToList();

            Thread[] arrayThread = new Thread[pageSize];
            AutoResetEvent eventObj = new AutoResetEvent(true);

            for (int size = 0; size < pageSize; size++)
            {
                var batchList = list.Skip(size * QuotesCount).Take(QuotesCount).ToList();
                arrayThread[size] = new Thread(() =>
                {
                    try
                    {
                        var temp = batchList;
                        short nCount = (short)temp.Count();
                        byte[] nMarketArr = new byte[nCount];
                        string[] pszZqdmArr = new string[nCount];
                        for (int j = 0; j < nCount; j++)
                        {
                            nMarketArr[j] = (byte)temp[j].Market;
                            pszZqdmArr[j] = temp[j].ShareCode;
                        }
                        int hqClient = Singleton.Instance.GetHqClient();
                        try
                        {
                            StringBuilder sErrInfo = new StringBuilder(256);
                            StringBuilder sResult = new StringBuilder(1024 * 2048);
                            int failCount = 0;//失败次数
                            do
                            {
                                int tempClient = hqClient;
                                bool bRet = TradeX_M.TdxHq_GetSecurityQuotes(tempClient, nMarketArr, pszZqdmArr, ref nCount, sResult, sErrInfo);
                                if (!bRet)
                                {
                                    failCount++;
                                    Singleton.Instance.AddRetryClient(hqClient);
                                    hqClient = -1;
                                    string errDes = string.Format("获取所有股票的五档报价数据出错，原因：{0}", sErrInfo.ToString());
                                    Console.WriteLine(errDes);
                                    if (failCount > 3)
                                    {
                                        return;
                                    }

                                    hqClient = Singleton.Instance.GetHqClient();
                                }
                                else
                                {
                                    failCount = 999;
                                }
                            } while (failCount <= 3);
                            string result = sResult.ToString();
                            string[] rows = result.Split('\n');
                            for (int i = 0; i < rows.Length; i++)
                            {
                                if (i == 0)
                                {
                                    continue;
                                }
                                string[] column = rows[i].Split('\t');
                                if (column.Length < 43)
                                {
                                    string errDes = "获取所有股票的五档报价数据结构有误";
                                    break;
                                }

                                string SharesCode = temp[i - 1].ShareCode;//原股票代码
                                int Market = int.Parse(column[0]);//市场代码
                                string BackSharesCode = column[1];//返回股票代码
                                long PresentPrice = (long)(Math.Round(float.Parse(column[3]) * Singleton.Instance.PriceFormat, 0));//现价
                                long ClosedPrice = (long)(Math.Round(float.Parse(column[4]) * Singleton.Instance.PriceFormat, 0));//昨收
                                long OpenedPrice = (long)(Math.Round(float.Parse(column[5]) * Singleton.Instance.PriceFormat, 0));//开盘
                                long MaxPrice = (long)(Math.Round(float.Parse(column[6]) * Singleton.Instance.PriceFormat, 0));//最高
                                long MinPrice = (long)(Math.Round(float.Parse(column[7]) * Singleton.Instance.PriceFormat, 0));//最低
                                int TotalCount = int.Parse(column[10]);//总量
                                int PresentCount = int.Parse(column[11]);//现量
                                long TotalAmount = (long)(Math.Round(float.Parse(column[12]) * Singleton.Instance.PriceFormat, 0));//总金额
                                int InvolCount = int.Parse(column[13]);//内盘
                                int OuterCount = int.Parse(column[14]);//外盘
                                long BuyPrice1 = (long)(Math.Round(float.Parse(column[17]) * Singleton.Instance.PriceFormat, 0));//买一价
                                long SellPrice1 = (long)(Math.Round(float.Parse(column[18]) * Singleton.Instance.PriceFormat, 0));//卖一价
                                int BuyCount1 = int.Parse(column[19]);//买一量
                                int SellCount1 = int.Parse(column[20]);//卖一量
                                long BuyPrice2 = (long)(Math.Round(float.Parse(column[21]) * Singleton.Instance.PriceFormat, 0));//买二价
                                long SellPrice2 = (long)(Math.Round(float.Parse(column[22]) * Singleton.Instance.PriceFormat, 0));//卖二价
                                int BuyCount2 = int.Parse(column[23]);//买二量
                                int SellCount2 = int.Parse(column[24]);//卖二量
                                long BuyPrice3 = (long)(Math.Round(float.Parse(column[25]) * Singleton.Instance.PriceFormat, 0));//买三价
                                long SellPrice3 = (long)(Math.Round(float.Parse(column[26]) * Singleton.Instance.PriceFormat, 0));//卖三价
                                int BuyCount3 = int.Parse(column[27]);//买三量
                                int SellCount3 = int.Parse(column[28]);//卖三量
                                long BuyPrice4 = (long)(Math.Round(float.Parse(column[29]) * Singleton.Instance.PriceFormat, 0));//买四价
                                long SellPrice4 = (long)(Math.Round(float.Parse(column[30]) * Singleton.Instance.PriceFormat, 0));//卖四价
                                int BuyCount4 = int.Parse(column[31]);//买四量
                                int SellCount4 = int.Parse(column[32]);//卖四量
                                long BuyPrice5 = (long)(Math.Round(float.Parse(column[33]) * Singleton.Instance.PriceFormat, 0));//买五价
                                long SellPrice5 = (long)(Math.Round(float.Parse(column[34]) * Singleton.Instance.PriceFormat, 0));//卖五价
                                int BuyCount5 = int.Parse(column[35]);//买五量
                                int SellCount5 = int.Parse(column[36]);//卖五量
                                string SpeedUp = column[42];//股票代码
                                string Activity = column[2];//股票代码

                                if (SharesCode != BackSharesCode)
                                {
                                    continue;
                                }
                                //var tempClosedPrice = Singleton.Instance.SharesList.Where(e => e.ShareCode == SharesCode && e.Market == Market).Select(e => e.ShareClosedPrice).FirstOrDefault();
                                //if (ClosedPrice <= 0 || (tempClosedPrice != ClosedPrice && tempClosedPrice>0))
                                //{
                                //    continue;
                                //}

                                eventObj.WaitOne();
                                try
                                {
                                    resultlist.Add(new SharesQuotesInfo
                                    {
                                        SellCount1 = SellCount1,
                                        SellCount2 = SellCount2,
                                        SellCount3 = SellCount3,
                                        SellCount4 = SellCount4,
                                        SellCount5 = SellCount5,
                                        SellPrice1 = SellPrice1,
                                        SellPrice2 = SellPrice2,
                                        SellPrice3 = SellPrice3,
                                        SellPrice4 = SellPrice4,
                                        SellPrice5 = SellPrice5,
                                        SharesCode = SharesCode,
                                        SpeedUp = SpeedUp,
                                        Activity = Activity,
                                        BuyCount1 = BuyCount1,
                                        BuyCount2 = BuyCount2,
                                        BuyCount3 = BuyCount3,
                                        BuyCount4 = BuyCount4,
                                        BuyCount5 = BuyCount5,
                                        BuyPrice1 = BuyPrice1,
                                        BuyPrice2 = BuyPrice2,
                                        BuyPrice3 = BuyPrice3,
                                        BuyPrice4 = BuyPrice4,
                                        BuyPrice5 = BuyPrice5,
                                        ClosedPrice = ClosedPrice,
                                        InvolCount = InvolCount,
                                        Market = Market,
                                        MaxPrice = MaxPrice,
                                        MinPrice = MinPrice,
                                        OpenedPrice = OpenedPrice,
                                        OuterCount = OuterCount,
                                        PresentCount = PresentCount,
                                        PresentPrice = PresentPrice,
                                        TotalAmount = TotalAmount,
                                        TotalCount = TotalCount,
                                        BackSharesCode = BackSharesCode,
                                        LastModified=DateTime.Now
                                    });
                                }
                                catch (Exception)
                                { }

                                eventObj.Set();
                            }
                        }
                        catch (Exception ex)
                        { }

                        if (hqClient != -1)
                        {
                            //归还行情链接
                            Singleton.Instance.AddHqClient(hqClient);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                });
                arrayThread[size].Start();
            }

            for (int size = 0; size < pageSize; size++)
            {
                arrayThread[size].Join();
                arrayThread[size] = null;
            }

            eventObj.Close();
            return resultlist;
        }

        /// <summary>
        /// 获取证券 K 线数据
        /// </summary>
        /// <param name="nCategory"></param>
        public static void TdxHq_GetSecurityBars(byte nCategory)
        {
            List<SharesBaseInfo> list = new List<SharesBaseInfo>();
            //拼接所有股票
            List<SharesBaseInfo> tempList = new List<SharesBaseInfo>();
            if (Singleton.Instance.SharesBaseInfoList.TryGetValue(0, out tempList))
            {
                Regex regex = new Regex(Singleton.Instance.SharesCodeMatch0);
                tempList = (from item in tempList
                            where regex.IsMatch(item.ShareCode)
                            select item).ToList();
                list.AddRange(tempList);
            }
            if (Singleton.Instance.SharesBaseInfoList.TryGetValue(1, out tempList))
            {
                Regex regex = new Regex(Singleton.Instance.SharesCodeMatch1);
                tempList = (from item in tempList
                            where regex.IsMatch(item.ShareCode)
                            select item).ToList();
                list.AddRange(tempList);
            }
            //list = list.Take(200).ToList();
            //计算分页数量
            int BarsCount = Singleton.Instance.BarsCount;
            int totalCount = list.Count();
            int pageSize;
            if (totalCount % BarsCount == 0)
            {
                pageSize = totalCount / BarsCount;
            }
            else
            {
                pageSize = totalCount / BarsCount + 1;
            }
            list = list.OrderBy(e => e.ShareCode).ToList();

            for (int size = 0; size < pageSize; size++)
            {
                int temSize = size;
                var temp = list.Skip(size * BarsCount).Take(BarsCount).ToList();
                WaitCallback method = (t) => GetSecurityBars(nCategory, temp, temSize);
                ThreadPool.QueueUserWorkItem(method);
            }
        }

        /// <summary>
        /// 获取证券 K 线数据执行线程
        /// </summary>
        private static void GetSecurityBars(byte nCategory, List<SharesBaseInfo> list, int size)
        {
            short nCount = 10;
            StringBuilder sErrInfo = new StringBuilder(256);
            StringBuilder sResult = new StringBuilder(1024 * 1024);
            List<SharesBarsInfo> resultList = new List<SharesBarsInfo>();
            int hqClient = Singleton.Instance.GetHqClient();
            foreach (var item in list)
            {
                bool bRet = TradeX_M.TdxHq_GetSecurityBars(hqClient, nCategory, (byte)item.Market, item.ShareCode, 0, ref nCount, sResult, sErrInfo);
                if (!bRet)
                {
                    Singleton.Instance.AddRetryClient(hqClient);
                    hqClient = Singleton.Instance.GetHqClient();
                    string errDes = string.Format("获取股票的证券K线数据出错，原因：{0}", sErrInfo.ToString());
                    Console.WriteLine(errDes);
                    Console.WriteLine(errDes);
                    continue;
                }
                if (nCount <= 0)
                {
                    continue;
                }
                string result = sResult.ToString();
                string[] rows = result.Split('\n');
                for (int i = 0; i < rows.Length; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    string[] column = rows[i].Split('\t');
                    if (column.Length != 7)
                    {
                        string errDes = "获取股票的证券K线数据结构有误";
                        Console.WriteLine(errDes);
                        break;
                    }

                    try
                    {
                        int Market = item.Market;//市场代码
                        string SharesCode = item.ShareCode;//股票代码
                        string TimeStr = column[0];
                        DateTime Time;
                        if (TimeStr.Length == 18)
                        {
                            //1989--15--20
                            Int32 lYear = 4009 - Int32.Parse(TimeStr.Substring(0, 4));
                            Int32 lMonth = 20 - Int32.Parse(TimeStr.Substring(6, 2));
                            Int32 lDay = 48 - Int32.Parse(TimeStr.Substring(10, 2));
                            Int32 lHour = Int32.Parse(TimeStr.Substring(13, 2));
                            Int32 lMinute = Int32.Parse(TimeStr.Substring(16, 2));
                            Time = new DateTime(lYear, lMonth, lDay, lHour, lMinute, 0);
                        }
                        else
                        {
                            Time = DateTime.ParseExact(TimeStr, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        }
                        int Type = nCategory;
                        long ClosedPrice = (long)(Math.Round(float.Parse(column[2]) * Singleton.Instance.PriceFormat, 0));//收盘
                        long OpenedPrice = (long)(Math.Round(float.Parse(column[1]) * Singleton.Instance.PriceFormat, 0));//开盘
                        long MaxPrice = (long)(Math.Round(float.Parse(column[3]) * Singleton.Instance.PriceFormat, 0));//最高
                        long MinPrice = (long)(Math.Round(float.Parse(column[4]) * Singleton.Instance.PriceFormat, 0));//最低
                        int Volume = int.Parse(column[5]);//成交量
                        long Turnover = (long)(Math.Round(float.Parse(column[6]) * Singleton.Instance.PriceFormat, 0));//成交额

                        resultList.Add(new SharesBarsInfo
                        {
                            SharesCode = SharesCode,
                            TimeStr = TimeStr,
                            ClosedPrice = ClosedPrice,
                            Market = Market,
                            MaxPrice = MaxPrice,
                            MinPrice = MinPrice,
                            OpenedPrice = OpenedPrice,
                            Time = Time,
                            Turnover = Turnover,
                            Type = Type,
                            Volume = Volume
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            //归还行情链接
            Singleton.Instance.AddHqClient(hqClient);

            string timeNow = DateTime.Now.ToString("yyyyMMddHHmmss") + nCategory + size;
            lock (Singleton.Instance.SecurityBarsLocked)
            {
                DataBaseHelper.UpdateSharesBars(resultList, timeNow);
            }
        }

        /// <summary>
        /// 获取指数k线数据
        /// </summary>
        /// <param name="nCategory"></param>
        public static void TdxHq_GetIndexBars(byte nCategory)
        {
            List<SharesBaseInfo> list = new List<SharesBaseInfo>();
            //拼接所有股票
            List<SharesBaseInfo> tempList = new List<SharesBaseInfo>();
            if (Singleton.Instance.SharesBaseInfoList.TryGetValue(0, out tempList))
            {
                Regex regex = new Regex(Singleton.Instance.SharesCodeMatch0);
                tempList = (from item in tempList
                            where regex.IsMatch(item.ShareCode)
                            select item).ToList();
                list.AddRange(tempList);
            }
            if (Singleton.Instance.SharesBaseInfoList.TryGetValue(1, out tempList))
            {
                Regex regex = new Regex(Singleton.Instance.SharesCodeMatch1);
                tempList = (from item in tempList
                            where regex.IsMatch(item.ShareCode)
                            select item).ToList();
                list.AddRange(tempList);
            }
            //list = list.Take(200).ToList();
            //计算分页数量
            int IndexBarsCount = Singleton.Instance.IndexBarsCount;
            int totalCount = list.Count();
            int pageSize;
            if (totalCount % IndexBarsCount == 0)
            {
                pageSize = totalCount / IndexBarsCount;
            }
            else
            {
                pageSize = totalCount / IndexBarsCount + 1;
            }
            list = list.OrderBy(e => e.ShareCode).ToList();

            for (int size = 0; size < pageSize; size++)
            {
                int temSize = size;
                var temp = list.Skip(size * IndexBarsCount).Take(IndexBarsCount).ToList();
                WaitCallback method = (t) => GetSecurityIndexBars(nCategory, temp, temSize);
                ThreadPool.QueueUserWorkItem(method);
            }
        }

        /// <summary>
        /// 获取指数 K 线数据执行线程
        /// </summary>
        private static void GetSecurityIndexBars(byte nCategory, List<SharesBaseInfo> list, int size)
        {
            short nCount = 10;
            StringBuilder sErrInfo = new StringBuilder(256);
            StringBuilder sResult = new StringBuilder(1024 * 1024);
            List<SharesIndexBarsInfo> resultList = new List<SharesIndexBarsInfo>();
            int hqClient = Singleton.Instance.GetHqClient();
            foreach (var item in list)
            {
                bool bRet = TradeX_M.TdxHq_GetIndexBars(hqClient, nCategory, (byte)item.Market, item.ShareCode, 0, ref nCount, sResult, sErrInfo);
                if (!bRet)
                {
                    Singleton.Instance.AddRetryClient(hqClient);
                    hqClient = Singleton.Instance.GetHqClient();
                    string errDes = string.Format("获取股票的指数K线数据出错，原因：{0}", sErrInfo.ToString());
                    Console.WriteLine(errDes);
                    Console.WriteLine(errDes);
                    continue;
                }
                if (nCount <= 0)
                {
                    continue;
                }
                string result = sResult.ToString();
                string[] rows = result.Split('\n');
                for (int i = 0; i < rows.Length; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    string[] column = rows[i].Split('\t');
                    if (column.Length != 9)
                    {
                        string errDes = "获取股票的证券K线数据结构有误";
                        Console.WriteLine(errDes);
                        break;
                    }

                    try
                    {
                        int Market = item.Market;//市场代码
                        string SharesCode = item.ShareCode;//股票代码
                        string TimeStr = column[0];
                        DateTime Time;
                        if (TimeStr.Length == 18)
                        {
                            //1989--15--20
                            Int32 lYear = 4009 - Int32.Parse(TimeStr.Substring(0, 4));
                            Int32 lMonth = 20 - Int32.Parse(TimeStr.Substring(6, 2));
                            Int32 lDay = 48 - Int32.Parse(TimeStr.Substring(10, 2));
                            Int32 lHour = Int32.Parse(TimeStr.Substring(13, 2));
                            Int32 lMinute = Int32.Parse(TimeStr.Substring(16, 2));
                            Time = new DateTime(lYear, lMonth, lDay, lHour, lMinute, 0);
                        }
                        else
                        {
                            Time = DateTime.ParseExact(TimeStr, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        }
                        int Type = nCategory;
                        long ClosedPrice = (long)(Math.Round(float.Parse(column[2]) * Singleton.Instance.PriceFormat, 0));//收盘
                        long OpenedPrice = (long)(Math.Round(float.Parse(column[1]) * Singleton.Instance.PriceFormat, 0));//开盘
                        long MaxPrice = (long)(Math.Round(float.Parse(column[3]) * Singleton.Instance.PriceFormat, 0));//最高
                        long MinPrice = (long)(Math.Round(float.Parse(column[4]) * Singleton.Instance.PriceFormat, 0));//最低
                        int Volume = int.Parse(column[5]);//成交量
                        long Turnover = (long)(Math.Round(float.Parse(column[6]) * Singleton.Instance.PriceFormat, 0));//成交额
                        int RiseCount = int.Parse(column[7]);//涨家数
                        int FallCount = int.Parse(column[8]);//跌家数

                        resultList.Add(new SharesIndexBarsInfo
                        {
                            SharesCode = SharesCode,
                            TimeStr = TimeStr,
                            ClosedPrice = ClosedPrice,
                            Market = Market,
                            MaxPrice = MaxPrice,
                            MinPrice = MinPrice,
                            OpenedPrice = OpenedPrice,
                            Time = Time,
                            Turnover = Turnover,
                            Type = Type,
                            Volume = Volume,
                            FallCount = FallCount,
                            RiseCount = RiseCount
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            //归还行情链接
            Singleton.Instance.AddHqClient(hqClient);

            string timeNow = DateTime.Now.ToString("yyyyMMddHHmmss") + nCategory + size;
            lock (Singleton.Instance.SecurityIndexBarsLocked)
            {
                DataBaseHelper.UpdateSharesIndexBars(resultList, timeNow);
            }
        }

        /// <summary>
        /// 获取分时行情数据
        /// </summary>
        public static void TdxHq_GetMinuteTimeData()
        {
            List<SharesBaseInfo> list = new List<SharesBaseInfo>();
            //拼接所有股票
            List<SharesBaseInfo> tempList = new List<SharesBaseInfo>();
            if (Singleton.Instance.SharesBaseInfoList.TryGetValue(0, out tempList))
            {
                Regex regex = new Regex(Singleton.Instance.SharesCodeMatch0);
                tempList = (from item in tempList
                            where regex.IsMatch(item.ShareCode)
                            select item).ToList();
                list.AddRange(tempList);
            }
            if (Singleton.Instance.SharesBaseInfoList.TryGetValue(1, out tempList))
            {
                Regex regex = new Regex(Singleton.Instance.SharesCodeMatch1);
                tempList = (from item in tempList
                            where regex.IsMatch(item.ShareCode)
                            select item).ToList();
                list.AddRange(tempList);
            }
            //list = list.Take(200).ToList();
            //计算分页数量
            int MinuteTimeDataCount = Singleton.Instance.MinuteTimeDataCount;
            int totalCount = list.Count();
            int pageSize;
            if (totalCount % MinuteTimeDataCount == 0)
            {
                pageSize = totalCount / MinuteTimeDataCount;
            }
            else
            {
                pageSize = totalCount / MinuteTimeDataCount + 1;
            }
            list = list.OrderBy(e => e.ShareCode).ToList();

            for (int size = 0; size < pageSize; size++)
            {
                int temSize = size;
                var temp = list.Skip(size * MinuteTimeDataCount).Take(MinuteTimeDataCount).ToList();
                WaitCallback method = (t) => GetMinuteTimeData(temp, temSize);
                ThreadPool.QueueUserWorkItem(method);
            }
        }

        /// <summary>
        /// 获取分时行情数据执行线程
        /// </summary>
        private static void GetMinuteTimeData(List<SharesBaseInfo> list, int size)
        {
            StringBuilder sErrInfo = new StringBuilder(256);
            StringBuilder sResult = new StringBuilder(1024 * 1024);
            List<SharesMinuteTimeData> resultList = new List<SharesMinuteTimeData>();
            int hqClient = Singleton.Instance.GetHqClient();
            foreach (var item in list)
            {
                bool bRet = TradeX_M.TdxHq_GetMinuteTimeData(hqClient, (byte)item.Market, item.ShareCode, sResult, sErrInfo);
                if (!bRet)
                {
                    Singleton.Instance.AddRetryClient(hqClient);
                    hqClient = Singleton.Instance.GetHqClient();
                    string errDes = string.Format("获取分时行情数据出错，原因：{0}", sErrInfo.ToString());
                    Console.WriteLine(errDes);
                    continue;
                }
                string result = sResult.ToString();
                string[] rows = result.Split('\n');
                DateTime tempTime = DateTime.Now.Date.AddHours(9).AddMinutes(30);
                for (int i = 0; i < rows.Length; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    string[] column = rows[i].Split('\t');
                    if (column.Length != 3)
                    {
                        string errDes = "获取分时行情数据结构有误";
                        Console.WriteLine(errDes);
                        break;
                    }

                    try
                    {
                        int Market = item.Market;//市场代码
                        string SharesCode = item.ShareCode;//股票代码
                        int Volume = int.Parse(column[1]);
                        long Price = (long)(Math.Round(float.Parse(column[0]) * Singleton.Instance.PriceFormat, 0));//现价
                        if (i == 120)
                        {
                            tempTime = tempTime.AddHours(1).AddMinutes(30);
                        }
                        DateTime Time = tempTime.AddMinutes(i);

                        resultList.Add(new SharesMinuteTimeData
                        {
                            SharesCode = SharesCode,
                            Market = Market,
                            Price = Price,
                            Volume = Volume,
                            Time = Time
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            //归还行情链接
            Singleton.Instance.AddHqClient(hqClient);

            string timeNow = DateTime.Now.ToString("yyyyMMddHHmmss") + size;
            lock (Singleton.Instance.MinuteTimeDataLocked)
            {
                DataBaseHelper.UpdateMinuteTimeData(resultList, timeNow);
            }
        }

        /// <summary>
        /// 获取分笔成交数据
        /// </summary>
        public static List<SharesTransactionDataInfo> TdxHq_GetTransactionData()
        {
            List<SharesBaseInfo> list = new List<SharesBaseInfo>();
            //拼接所有股票
            List<SharesBaseInfo> tempList = new List<SharesBaseInfo>();
            if (Singleton.Instance.SharesBaseInfoList.TryGetValue(0, out tempList))
            {
                Regex regex = new Regex(Singleton.Instance.SharesCodeMatch0);
                tempList = (from item in tempList
                            where regex.IsMatch(item.ShareCode)
                            select item).ToList();
                list.AddRange(tempList);
            }
            if (Singleton.Instance.SharesBaseInfoList.TryGetValue(1, out tempList))
            {
                Regex regex = new Regex(Singleton.Instance.SharesCodeMatch1);
                tempList = (from item in tempList
                            where regex.IsMatch(item.ShareCode)
                            select item).ToList();
                list.AddRange(tempList);
            }
            //计算分页数量
            int TransactionDataCount = Singleton.Instance.TransactionDataCount;
            int totalCount = list.Count();
            int pageSize;
            if (totalCount % TransactionDataCount == 0)
            {
                pageSize = totalCount / TransactionDataCount;
            }
            else
            {
                pageSize = totalCount / TransactionDataCount + 1;
            }
            list = list.OrderBy(e => e.ShareCode).ToList();

            List<SharesTransactionDataInfo> resultlist = new List<SharesTransactionDataInfo>();
            Thread[] arrayThread = new Thread[pageSize];
            AutoResetEvent eventObj = new AutoResetEvent(true);

            string date = DateTime.Now.ToString("yyyy-MM-dd");
            for (int size = 0; size < pageSize; size++)
            {
                var batchList = list.Skip(size * TransactionDataCount).Take(TransactionDataCount).ToList();
                arrayThread[size] = new Thread(() =>
                {
                    int hqClient = Singleton.Instance.GetHqClient();
                    try
                    {
                        var temp = batchList;//股票列表
                        foreach (var share in temp)
                        {
                            StringBuilder sErrInfo = new StringBuilder(256);
                            StringBuilder sResult = new StringBuilder(1024 * 2048);
                            byte nMarket = (byte)share.Market;//市场代码
                            string shareCode = share.ShareCode;//股票代码
                            short nStart = 0;
                            short nCount;
                            do
                            {
                                nCount = 50;
                                int tempClient = hqClient;
                                bool bRet = TradeX_M.TdxHq_GetTransactionData(tempClient, nMarket, shareCode, nStart, ref nCount, sResult, sErrInfo);
                                if (!bRet)
                                {
                                    Singleton.Instance.AddRetryClient(hqClient);
                                    hqClient = -1;
                                    string errDes = string.Format("获取分笔成交数据出错，原因：{0}", sErrInfo.ToString());
                                    Console.WriteLine(errDes);
                                    hqClient = Singleton.Instance.GetHqClient();
                                    break;
                                }
                                else
                                {
                                    string result = sResult.ToString();
                                    string[] rows = result.Split('\n');
                                    for (int i = 0; i < rows.Length; i++)
                                    {
                                        if (i == 0)
                                        {
                                            continue;
                                        }
                                        string[] column = rows[i].Split('\t');
                                        if (column.Length != 6)
                                        {
                                            string errDes = "获取分笔成交数据结构有误";
                                            Console.WriteLine(errDes);
                                            break;
                                        }
                                        eventObj.WaitOne();
                                        try
                                        {
                                            DateTime Time = DateTime.Parse(date + " " + column[0] + ":00");
                                            string TimeStr = column[0];
                                            long Price = (long)(Math.Round(float.Parse(column[1]) * Singleton.Instance.PriceFormat, 0));//现价
                                            int Volume = int.Parse(column[2]);
                                            int Stock = int.Parse(column[3]);
                                            int Type = int.Parse(column[4]);
                                            resultlist.Add(new SharesTransactionDataInfo
                                            {
                                                SharesCode = shareCode,
                                                Market = nMarket,
                                                Price = Price,
                                                Volume = Volume,
                                                Time = Time,
                                                TimeStr = TimeStr,
                                                Stock = Stock,
                                                Type = Type
                                            });
                                        }
                                        finally
                                        {
                                            eventObj.Set();
                                        }
                                    }
                                    nStart = (short)(nStart + nCount);
                                }
                            } while (nCount >= 1000);
                        }
                    }
                    finally
                    {
                        if (hqClient != -1)
                        {
                            //归还行情链接
                            Singleton.Instance.AddHqClient(hqClient);
                        }
                    }
                });
                arrayThread[size].Start();
            }

            for (int size = 0; size < pageSize; size++)
            {
                arrayThread[size].Join();
                arrayThread[size] = null;
            }

            eventObj.Close();
            return resultlist;
        }
    }
}
