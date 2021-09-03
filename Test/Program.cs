using FXCommon.Common;
using MealTicket_DBCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("url:");
            string url = Console.ReadLine();
            Console.Write("userTokrn:");
            string userToken = Console.ReadLine();
            try
            {
                url = string.Format("{0}/tradecenter/shares/kline/reset", url);
                using (var db = new meal_ticketEntities())
                {
                    var sharesList = (from item in db.v_shares_baseinfo
                                      select item).ToList();
                    //Console.WriteLine("年K开始导入");
                    //foreach (var item in sharesList)
                    //{
                    //    var content10 = new
                    //    {
                    //        Market = item.Market,
                    //        SharesCode = item.SharesCode,
                    //        DataType = 11,
                    //        StartGroupTimeKey = 2020,
                    //        EndGroupTimeKey = 2021
                    //    };
                    //    string res10 = HtmlSender.Request(JsonConvert.SerializeObject(content10), url, "application/json", Encoding.UTF8, "POST", userToken);
                    //}
                    //Console.WriteLine("季K开始导入");
                    //foreach (var item in sharesList)
                    //{
                    //    var content9 = new
                    //    {
                    //        Market = item.Market,
                    //        SharesCode = item.SharesCode,
                    //        DataType = 10,
                    //        StartGroupTimeKey = 202001,
                    //        EndGroupTimeKey = 202104
                    //    };
                    //    string res9 = HtmlSender.Request(JsonConvert.SerializeObject(content9), url, "application/json", Encoding.UTF8, "POST", userToken);
                    //}
                    //Console.WriteLine("月K开始导入");
                    //foreach (var item in sharesList)
                    //{
                    //    var content8 = new
                    //    {
                    //        Market = item.Market,
                    //        SharesCode = item.SharesCode,
                    //        DataType = 9,
                    //        StartGroupTimeKey = 202001,
                    //        EndGroupTimeKey = 202108
                    //    };
                    //    string res8 = HtmlSender.Request(JsonConvert.SerializeObject(content8), url, "application/json", Encoding.UTF8, "POST", userToken);
                    //}
                    //Console.WriteLine("周K开始导入");
                    //foreach (var item in sharesList)
                    //{
                    //    var content7 = new
                    //    {
                    //        Market = item.Market,
                    //        SharesCode = item.SharesCode,
                    //        DataType = 8,
                    //        StartGroupTimeKey = 202001,
                    //        EndGroupTimeKey = 202153
                    //    };
                    //    string res7 = HtmlSender.Request(JsonConvert.SerializeObject(content7), url, "application/json", Encoding.UTF8, "POST", userToken);
                    //}
                    //Console.WriteLine("日K开始导入");
                    //foreach (var item in sharesList)
                    //{
                    //    var content6 = new
                    //    {
                    //        Market = item.Market,
                    //        SharesCode = item.SharesCode,
                    //        DataType = 7,
                    //        StartGroupTimeKey = 20210101,
                    //        EndGroupTimeKey = 20210831
                    //    };
                    //    string res6 = HtmlSender.Request(JsonConvert.SerializeObject(content6), url, "application/json", Encoding.UTF8, "POST", userToken);
                    //}
                    //Console.WriteLine("60分K开始导入");
                    //foreach (var item in sharesList)
                    //{
                    //    var content5 = new
                    //    {
                    //        Market = item.Market,
                    //        SharesCode = item.SharesCode,
                    //        DataType = 6,
                    //        StartGroupTimeKey = 202108010930,
                    //        EndGroupTimeKey = 202108311500
                    //    };
                    //    string res5 = HtmlSender.Request(JsonConvert.SerializeObject(content5), url, "application/json", Encoding.UTF8, "POST", userToken);
                    //}
                    //Console.WriteLine("30分K开始导入");
                    //foreach (var item in sharesList)
                    //{
                    //    var content4 = new
                    //    {
                    //        Market = item.Market,
                    //        SharesCode = item.SharesCode,
                    //        DataType = 5,
                    //        StartGroupTimeKey = 202108010930,
                    //        EndGroupTimeKey = 202108311500
                    //    };
                    //    string res4 = HtmlSender.Request(JsonConvert.SerializeObject(content4), url, "application/json", Encoding.UTF8, "POST", userToken);
                    //}
                    //Console.WriteLine("15分K开始导入");
                    //foreach (var item in sharesList)
                    //{
                    //    var content3 = new
                    //    {
                    //        Market = item.Market,
                    //        SharesCode = item.SharesCode,
                    //        DataType = 4,
                    //        StartGroupTimeKey = 202108010930,
                    //        EndGroupTimeKey = 202108311500
                    //    };
                    //    string res3 = HtmlSender.Request(JsonConvert.SerializeObject(content3), url, "application/json", Encoding.UTF8, "POST", userToken);
                    //}
                    //Console.WriteLine("5分K开始导入");
                    //foreach (var item in sharesList)
                    //{
                    //    var content2 = new
                    //    {
                    //        Market = item.Market,
                    //        SharesCode = item.SharesCode,
                    //        DataType = 3,
                    //        StartGroupTimeKey = 202108010930,
                    //        EndGroupTimeKey = 202108311500
                    //    };
                    //    string res2 = HtmlSender.Request(JsonConvert.SerializeObject(content2), url, "application/json", Encoding.UTF8, "POST", userToken);
                    //}

                    string sql = @"select Market,SharesCode
  from t_shares_securitybarsdata_1min with(nolock)
  group by Market,SharesCode
  having count(*)=5280";

                    var tempList=db.Database.SqlQuery<SharesInfo>(sql).ToList();
                    sharesList = (from item in sharesList
                                  join item2 in tempList on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                                  from ai in a.DefaultIfEmpty()
                                  where ai == null
                                  select item).ToList();
                    Console.WriteLine("1分K开始导入");
                    int i = 0;
                    foreach (var item in sharesList)
                    {
                        i++;
                        var content1 = new
                        {
                            Market = item.Market,
                            SharesCode = item.SharesCode,
                            DataType = 2,
                            StartGroupTimeKey = 202108010930,
                            EndGroupTimeKey = 202108311500
                        };
                        string res1 = HtmlSender.Request(JsonConvert.SerializeObject(content1), url, "application/json", Encoding.UTF8, "POST", userToken);
                        if (i % 50 == 0)
                        {
                            Console.WriteLine("已完成" + i);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("执行完成,按1结束");
            Console.ReadLine();
        }
    }
    public class SharesInfo
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }
    }
}
