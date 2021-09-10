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
                    throw new Exception(errDes);
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
                        throw new Exception("数据结构有误");
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
            List<SharesBaseInfo> tempList = Singleton.Instance._getSharesBaseInfoList();
            Regex regex0 = new Regex(Singleton.Instance.SharesCodeMatch0);
            Regex regex1 = new Regex(Singleton.Instance.SharesCodeMatch1);
            tempList = (from item in tempList
                        where regex0.IsMatch(item.ShareCode) || regex1.IsMatch(item.ShareCode)
                        select item).ToList();
            list.AddRange(tempList);

            object resultLock = new object();
            List<SharesQuotesInfo> resultlist = new List<SharesQuotesInfo>();
          
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

            ThreadMsgTemplate<List<SharesBaseInfo>> data = new ThreadMsgTemplate<List<SharesBaseInfo>>();
            data.Init();
            for (int size = 0; size < pageSize; size++)
            {
                var batchList = list.Skip(size * QuotesCount).Take(QuotesCount).ToList();
                data.AddMessage(batchList);
            }
            int taskCount = Singleton.Instance.hqClientCount<= pageSize? Singleton.Instance.hqClientCount: pageSize;
            Task[] tArr = new Task[taskCount];

            for(int i=0;i< taskCount;i++)
            {
                tArr[i] = new Task((tdx_result) =>
                {
                    int hqClient = Singleton.Instance.GetHqClient();
                    try
                    {
                        do
                        {
                            List<SharesBaseInfo> tempData = new List<SharesBaseInfo>();
                            if (!data.GetMessage(ref tempData, true))
                            {
                                break;
                            }
                            var temp_tdx_result = tdx_result as TdxHq_Result;
                            StringBuilder sErrInfo = temp_tdx_result.sErrInfo;
                            StringBuilder sResult = temp_tdx_result.sResult;
                            sErrInfo.Clear();
                            sResult.Clear();

                            short nCount = (short)tempData.Count();
                            byte[] nMarketArr = new byte[nCount];
                            string[] pszZqdmArr = new string[nCount];
                            for (int j = 0; j < nCount; j++)
                            {
                                nMarketArr[j] = (byte)tempData[j].Market;
                                pszZqdmArr[j] = tempData[j].ShareCode;
                            }
                            bool bRet = TradeX_M.TdxHq_GetSecurityQuotes(hqClient, nMarketArr, pszZqdmArr, ref nCount, sResult, sErrInfo);
                            if (!bRet)
                            {
                                Singleton.Instance.AddRetryClient(hqClient);
                                hqClient = Singleton.Instance.GetHqClient();
                                string errDes = string.Format("获取所有股票的五档报价数据出错，原因：{0}", sErrInfo.ToString());
                                Console.WriteLine(errDes);
                                continue;
                            }
                            string result = sResult.ToString();
                            string[] rows = result.Split('\n');
                            for (int j = 0; j < rows.Length; j++)
                            {
                                if (j == 0)
                                {
                                    continue;
                                }
                                string[] column = rows[j].Split('\t');
                                if (column.Length < 43)
                                {
                                    break;
                                }

                                string SharesCode = tempData[j - 1].ShareCode;//原股票代码
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
                                lock (resultLock)
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
                                        LastModified = DateTime.Now
                                    });
                                }
                            }
                        } while (true);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("获取行情数据业务出错", ex);
                    }
                    finally 
                    {
                        Singleton.Instance.AddHqClient(hqClient);
                    }
                },Singleton.Instance.tdxHq_Result[i],TaskCreationOptions.LongRunning);
                tArr[i].Start();
            }

            Task.WaitAll(tArr);
            data.Release();
            return resultlist;
        }
    }
}
