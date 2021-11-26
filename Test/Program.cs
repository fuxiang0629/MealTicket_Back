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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class A
    {
        public int Id { get; set; }
        public int Id2 { get; set; }

        public string Name { get; set; }
    }

    class B 
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<B> b = new List<B>();
            b.Add(new B 
            {
                Market=0,
                SharesCode="000001"
            });
            b.Add(new B
            {
                Market = 1,
                SharesCode = "000002"
            });
            b.Add(new B
            {
                Market = 2,
                SharesCode = "000001"
            });
            b.Add(new B
            {
                Market = 3,
                SharesCode = "000003"
            });
            Dictionary<int, B> dic = new Dictionary<int, B>();
            dic = b.ToDictionary(k=>k.Market,v=>v);
            b = new List<B>();

            var a = b.OrderBy(e=>e.Market).FirstOrDefault();
            var c = b.Where(e=>e.SharesCode != a.SharesCode).ToList();
            var d = c;

            //Dictionary<int, A> dic1 = new Dictionary<int, A>();
            //dic1.Add(1,new A 
            //{
            //    Id=1,
            //    Name="1"
            //});
            //dic1.Add(2, new A
            //{
            //    Id = 2,
            //    Name = "2"
            //});

            //Dictionary<string, Dictionary<int, A>> dic2 = new Dictionary<string, Dictionary<int, A>>();
            //Dictionary<int, A> temp = new Dictionary<int, A>();
            //temp.Add(1, dic1[1]);
            //dic2.Add("a", temp);

            //dic1[1].Name = "5";

            //Console.Write("url:");
            //string url = Console.ReadLine();
            //Console.Write("userTokrn:");
            //string userToken = Console.ReadLine();
            //try
            //{
            //    url = string.Format("{0}/tradecenter/shares/kline/reset", url);
            //    using (var db = new meal_ticketEntities())
            //    {
            //        var sharesList = (from item in db.v_shares_baseinfo
            //                          select item).ToList();
            //        string isContinue = "";
            //        do
            //        {
            //            Console.WriteLine("即将开始导入年K数据，确定输入'Y'，跳过输入'J'");
            //            isContinue = Console.ReadLine();
            //            if (isContinue.ToUpper() == "Y")
            //            {
            //                Console.WriteLine("年K开始导入");
            //                foreach (var item in sharesList)
            //                {
            //                    var content10 = new
            //                    {
            //                        Market = item.Market,
            //                        SharesCode = item.SharesCode,
            //                        DataType = 11,
            //                        StartGroupTimeKey = 2020,
            //                        EndGroupTimeKey = 2021
            //                    };
            //                    string res10 = HtmlSender.Request(JsonConvert.SerializeObject(content10), url, "application/json", Encoding.UTF8, "POST", userToken);
            //                }
            //                Console.WriteLine("年K结束导入");
            //                break;
            //            }
            //            else if (isContinue.ToUpper() == "J")
            //            {
            //                break;
            //            }

            //        } while (true);

            //        do
            //        {
            //            Console.WriteLine("即将开始导入季K数据，确定输入'Y'，跳过输入'J'");
            //            isContinue = Console.ReadLine();
            //            if (isContinue.ToUpper() == "Y")
            //            {
            //                Console.WriteLine("季K开始导入");
            //                foreach (var item in sharesList)
            //                {
            //                    var content9 = new
            //                    {
            //                        Market = item.Market,
            //                        SharesCode = item.SharesCode,
            //                        DataType = 10,
            //                        StartGroupTimeKey = 202002,
            //                        EndGroupTimeKey = 202103
            //                    };
            //                    string res9 = HtmlSender.Request(JsonConvert.SerializeObject(content9), url, "application/json", Encoding.UTF8, "POST", userToken);
            //                }
            //                Console.WriteLine("季K结束导入");
            //                break;
            //            }
            //            else if (isContinue.ToUpper() == "J")
            //            {
            //                break;
            //            }
            //        } while (true);

            //        do
            //        {
            //            Console.WriteLine("即将开始导入月K数据，确定输入'Y'，跳过输入'J'");
            //            isContinue = Console.ReadLine();
            //            if (isContinue.ToUpper() == "Y")
            //            {
            //                Console.WriteLine("月K开始导入");
            //                foreach (var item in sharesList)
            //                {
            //                    var content8 = new
            //                    {
            //                        Market = item.Market,
            //                        SharesCode = item.SharesCode,
            //                        DataType = 9,
            //                        StartGroupTimeKey = 202001,
            //                        EndGroupTimeKey = 202109
            //                    };
            //                    string res8 = HtmlSender.Request(JsonConvert.SerializeObject(content8), url, "application/json", Encoding.UTF8, "POST", userToken);
            //                }
            //                Console.WriteLine("月K结束导入");
            //                break;
            //            }
            //            else if (isContinue.ToUpper() == "J")
            //            {
            //                break;
            //            }

            //        } while (true);

            //        do
            //        {
            //            Console.WriteLine("即将开始导入周K数据，确定输入'Y'，跳过输入'J'");
            //            isContinue = Console.ReadLine();
            //            if (isContinue.ToUpper() == "Y")
            //            {
            //                Console.WriteLine("周K开始导入");
            //                foreach (var item in sharesList)
            //                {
            //                    var content7 = new
            //                    {
            //                        Market = item.Market,
            //                        SharesCode = item.SharesCode,
            //                        DataType = 8,
            //                        StartGroupTimeKey = 202002,
            //                        EndGroupTimeKey = 202137
            //                    };
            //                    string res7 = HtmlSender.Request(JsonConvert.SerializeObject(content7), url, "application/json", Encoding.UTF8, "POST", userToken);
            //                }
            //                Console.WriteLine("周K结束导入");
            //                break;
            //            }
            //            else if (isContinue.ToUpper() == "J")
            //            {
            //                break;
            //            }

            //        } while (true);

            //        do
            //        {
            //            Console.WriteLine("即将开始导入日K数据，确定输入'Y'，跳过输入'J'");
            //            isContinue = Console.ReadLine();
            //            if (isContinue.ToUpper() == "Y")
            //            {
            //                Console.WriteLine("日K开始导入");
            //                foreach (var item in sharesList)
            //                {
            //                    var content6 = new
            //                    {
            //                        Market = item.Market,
            //                        SharesCode = item.SharesCode,
            //                        DataType = 7,
            //                        StartGroupTimeKey = 20210101,
            //                        EndGroupTimeKey = 20210908
            //                    };
            //                    string res6 = HtmlSender.Request(JsonConvert.SerializeObject(content6), url, "application/json", Encoding.UTF8, "POST", userToken);
            //                }
            //                Console.WriteLine("日K结束导入");
            //                break;
            //            }
            //            else if (isContinue.ToUpper() == "J")
            //            {
            //                break;
            //            }

            //        } while (true);

            //        do
            //        {
            //            Console.WriteLine("即将开始导入60分K数据，确定输入'Y'，跳过输入'J'");
            //            isContinue = Console.ReadLine();
            //            if (isContinue.ToUpper() == "Y")
            //            {
            //                Console.WriteLine("60分K开始导入");
            //                foreach (var item in sharesList)
            //                {
            //                    var content5 = new
            //                    {
            //                        Market = item.Market,
            //                        SharesCode = item.SharesCode,
            //                        DataType = 6,
            //                        StartGroupTimeKey = 202108010930,
            //                        EndGroupTimeKey = 202109081500
            //                    };
            //                    string res5 = HtmlSender.Request(JsonConvert.SerializeObject(content5), url, "application/json", Encoding.UTF8, "POST", userToken);
            //                }
            //                Console.WriteLine("60分K结束导入");
            //                break;
            //            }
            //            else if (isContinue.ToUpper() == "J")
            //            {
            //                break;
            //            }

            //        } while (true);

            //        do
            //        {
            //            Console.WriteLine("即将开始导入30分K数据，确定输入'Y'，跳过输入'J'");
            //            isContinue = Console.ReadLine();
            //            if (isContinue.ToUpper() == "Y")
            //            {
            //                Console.WriteLine("30分K开始导入");
            //                foreach (var item in sharesList)
            //                {
            //                    var content4 = new
            //                    {
            //                        Market = item.Market,
            //                        SharesCode = item.SharesCode,
            //                        DataType = 5,
            //                        StartGroupTimeKey = 202108010930,
            //                        EndGroupTimeKey = 202109081500
            //                    };
            //                    string res4 = HtmlSender.Request(JsonConvert.SerializeObject(content4), url, "application/json", Encoding.UTF8, "POST", userToken);
            //                }
            //                Console.WriteLine("30分K结束导入");
            //                break;
            //            }
            //            else if (isContinue.ToUpper() == "J")
            //            {
            //                break;
            //            }

            //        } while (true);

            //        do
            //        {
            //            Console.WriteLine("即将开始导入15分K数据，确定输入'Y'，跳过输入'J'");
            //            isContinue = Console.ReadLine();
            //            if (isContinue.ToUpper() == "Y")
            //            {
            //                Console.WriteLine("开始导入");
            //                foreach (var item in sharesList)
            //                {
            //                    var content3 = new
            //                    {
            //                        Market = item.Market,
            //                        SharesCode = item.SharesCode,
            //                        DataType = 4,
            //                        StartGroupTimeKey = 202108010930,
            //                        EndGroupTimeKey = 202109081500
            //                    };
            //                    string res3 = HtmlSender.Request(JsonConvert.SerializeObject(content3), url, "application/json", Encoding.UTF8, "POST", userToken);
            //                }
            //                Console.WriteLine("15分K结束导入");
            //                break;
            //            }
            //            else if (isContinue.ToUpper() == "J")
            //            {
            //                break;
            //            }

            //        } while (true);

            //        do
            //        {
            //            Console.WriteLine("即将开始导入5分K数据，确定输入'Y'，跳过输入'J'");
            //            isContinue = Console.ReadLine();
            //            if (isContinue.ToUpper() == "Y")
            //            {
            //                Console.WriteLine("5分K开始导入");
            //                foreach (var item in sharesList)
            //                {
            //                    var content2 = new
            //                    {
            //                        Market = item.Market,
            //                        SharesCode = item.SharesCode,
            //                        DataType = 3,
            //                        StartGroupTimeKey = 202108010930,
            //                        EndGroupTimeKey = 202109081500
            //                    };
            //                    string res2 = HtmlSender.Request(JsonConvert.SerializeObject(content2), url, "application/json", Encoding.UTF8, "POST", userToken);
            //                }
            //                Console.WriteLine("5分K结束导入");
            //                break;
            //            }
            //            else if (isContinue.ToUpper() == "J")
            //            {
            //                break;
            //            }

            //        } while (true);

            //        do
            //        {
            //            Console.WriteLine("即将开始导入1分K数据，确定输入'Y'，跳过输入'J'");
            //            isContinue = Console.ReadLine();
            //            if (isContinue.ToUpper() == "Y")
            //            {
            //                Console.WriteLine("1分K开始导入");
            //                foreach (var item in sharesList)
            //                {
            //                    var content1 = new
            //                    {
            //                        Market = item.Market,
            //                        SharesCode = item.SharesCode,
            //                        DataType = 2,
            //                        StartGroupTimeKey = 202108010930,
            //                        EndGroupTimeKey = 202109081500
            //                    };
            //                    string res1 = HtmlSender.Request(JsonConvert.SerializeObject(content1), url, "application/json", Encoding.UTF8, "POST", userToken);
            //                }
            //                Console.WriteLine("1分K结束导入");
            //                break;
            //            }
            //            else if (isContinue.ToUpper() == "J")
            //            {
            //                break;
            //            }

            //        } while (true);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            //Console.WriteLine("执行完成,按1结束");
            //Console.ReadLine();
        }
    }
    public class SharesInfo
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }
    }
}
