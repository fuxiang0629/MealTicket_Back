using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using 股票数据处理;
using System.Configuration;
using stock_db_core;

namespace ShareData_Service
{
    class Program
    {
        static Timer Timer;

        static void TimerCallBack(object state)
        {
            if (Timer != null)
            {
                Timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("执行一次,下次执行时间");
            Thread.Sleep(10000);
            Console.WriteLine("执行一次,下次执行时间2");
            if (Timer != null)
            {
                Timer.Change(2000, 0);
            }
        }

        static void tr1Fun() 
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try 
                {
                    var temp = (from item in db.t_account_shares_entrust_manager
                                where item.Id == 1
                                select item).FirstOrDefault();
                    temp.CreateTime = temp.CreateTime.AddDays(2);
                    db.SaveChanges();
                    Thread.Sleep(20000);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                }
            }
        }
        static void tr2Fun()
        {
            Thread.Sleep(10000);
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var temp = (from item in db.t_account_shares_entrust_manager
                                where item.Id == 1
                                select item).FirstOrDefault();
                    temp.CreateTime = temp.CreateTime.AddDays(-1);
                    db.SaveChanges();
                    Console.WriteLine(temp.CreateTime);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                }
            }
        }

        static void Main(string[] args)
        {
            HqAnalyse hqAnalyse = new HqAnalyse();
            hqAnalyse.Connect(ConfigurationManager.ConnectionStrings["ConnectionString1"].ConnectionString+"|"+ ConfigurationManager.ConnectionStrings["ConnectionString2"].ConnectionString);

            List<DB_STOCK_INFO> ref_lstStockInfo=new List<DB_STOCK_INFO>();
            var result=hqAnalyse.GetStockInfo(0, ref ref_lstStockInfo);

            Dictionary<string, DB_TRADE_PRICE_INFO> ref_priceInfoDic=new Dictionary<string, DB_TRADE_PRICE_INFO>();
            Dictionary<string, List<DB_TIME_OF_USE_PRICE>> ref_lstTimeOfUsePriceDic=new Dictionary<string, List<DB_TIME_OF_USE_PRICE>>();
            Dictionary<string, int> error=new Dictionary<string, int>();

            List<string> strStock = new List<string>();
            strStock.Add("002920,0");
            result = hqAnalyse.GetTradePriceInfoBatch(strStock, DateTime.Parse(DB_Model.MAX_DATE_TIME),true,ref ref_priceInfoDic, ref ref_lstTimeOfUsePriceDic,ref error);
            List<int> list = new List<int>();
            //list.Add(2);
            //while (true)
            //{
            //    list.Add(1);

            //    var test = list[0];
            //    list.RemoveAt(0);
            //}


            //Timer = new Timer(TimerCallBack, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            //string input=Console.ReadLine();
            //if (input == "stop")
            //{
            //    Timer.Dispose();
            //    Timer = null;
            //}
            //Console.ReadLine();
            //var temp = Singleton.Instance;
            //Console.WriteLine("深圳行情如下：");
            //var result = ShareHelper.TdxHq_GetSecurityList(0);
            //DataBaseHelper.UpdateAllShares(result,0);
            //Console.WriteLine("深圳数据导入完成");
            //Console.WriteLine("\n\n\n\n");
            //Console.WriteLine("上海行情如下：");
            //result = ShareHelper.TdxHq_GetSecurityList(1);
            //DataBaseHelper.UpdateAllShares(result, 1);
            //Console.WriteLine("上海数据导入完成");
            //Console.WriteLine("\n\n\n\n");

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //Console.WriteLine("五档报价数据如下：");
            //var result = ShareHelper.TdxHq_GetSecurityQuotes();
            //Console.WriteLine("开始更新");
            //string timeNow = DateTime.Now.ToString("yyyyMMddHHmmss");
            //DataBaseHelper.UpdateSharesQuotes(result, timeNow);
            //Console.WriteLine("更新完成");
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);
            //Console.ReadLine();

            //for (int i = 0; i < 10; i++)
            //{
            //    byte nCategory=0;
            //    string nCategoryDes = "5分钟K线";
            //    if (i == 1)
            //    {
            //        nCategory = 1; 
            //        nCategoryDes = "15分钟K线";
            //    }
            //    if (i == 2)
            //    {
            //        nCategory = 2;
            //        nCategoryDes = "30分钟K线";
            //    }
            //    if (i == 3)
            //    {
            //        nCategory = 3;
            //        nCategoryDes = "1小时K线";
            //    }
            //    if (i == 4)
            //    {
            //        nCategory = 4;
            //        nCategoryDes = "日K线";
            //    }
            //    if (i == 5)
            //    {
            //        nCategory = 5;
            //        nCategoryDes = "周K线";
            //    }
            //    if (i == 6)
            //    {
            //        nCategory = 6;
            //        nCategoryDes = "月K线";
            //    }
            //    if (i == 7)
            //    {
            //        nCategory = 8;
            //        nCategoryDes = "1分钟K线";
            //    }
            //    if (i == 8)
            //    {
            //        nCategory = 10;
            //        nCategoryDes = "季K线";
            //    }
            //    if (i == 9)
            //    {
            //        nCategory = 11;
            //        nCategoryDes = "年K线";
            //    }
            //    Console.WriteLine(nCategoryDes+ "如下：");
            //    ShareHelper.TdxHq_GetSecurityBars(nCategory);
            //}

            //for (int i = 0; i < 10; i++)
            //{
            //    byte nCategory = 0;
            //    string nCategoryDes = "5分钟K线";
            //    if (i == 1)
            //    {
            //        nCategory = 1;
            //        nCategoryDes = "15分钟K线";
            //    }
            //    if (i == 2)
            //    {
            //        nCategory = 2;
            //        nCategoryDes = "30分钟K线";
            //    }
            //    if (i == 3)
            //    {
            //        nCategory = 3;
            //        nCategoryDes = "1小时K线";
            //    }
            //    if (i == 4)
            //    {
            //        nCategory = 4;
            //        nCategoryDes = "日K线";
            //    }
            //    if (i == 5)
            //    {
            //        nCategory = 5;
            //        nCategoryDes = "周K线";
            //    }
            //    if (i == 6)
            //    {
            //        nCategory = 6;
            //        nCategoryDes = "月K线";
            //    }
            //    if (i == 7)
            //    {
            //        nCategory = 8;
            //        nCategoryDes = "1分钟K线";
            //    }
            //    if (i == 8)
            //    {
            //        nCategory = 10;
            //        nCategoryDes = "季K线";
            //    }
            //    if (i == 9)
            //    {
            //        nCategory = 11;
            //        nCategoryDes = "年K线";
            //    }
            //    Console.WriteLine(nCategoryDes + "如下：");
            //    ShareHelper.TdxHq_GetIndexBars(nCategory);
            //}

            //Console.WriteLine("分时行情数据如下：");
            //ShareHelper.TdxHq_GetMinuteTimeData();

            //Console.WriteLine("分笔成交数据如下：");
            //ShareHelper.TdxHq_GetTransactionData();

            //while (true)
            //{
            //    Console.ReadLine();
            //    Console.WriteLine(Singleton.Instance.hqClientList.Count());
            //}
        }
    }
}