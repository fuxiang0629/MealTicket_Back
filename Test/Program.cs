﻿using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Handler.SecurityBarsData;
using MealTicket_Web_Handler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Test
{
    public class A : IComparable<A>
    {
        public int Id { get; set; }
        public int Id2 { get; set; }

        public string Name { get; set; }

        public int CompareTo(A other)
        {
            return (other.Id.CompareTo(this.Id) > 0 ? 1 : -1);
        }
    }

    class B 
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public int TrendId { get; set; }

        public string Time { get; set; }

        public string Des { get; set; }
    }

    public static class TransExpV2<TIn, TOut>
    {

        private static readonly Func<TIn, TOut> cache = GetFunc();
        private static Func<TIn, TOut> GetFunc()
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TIn), "p");
            List<MemberBinding> memberBindingList = new List<MemberBinding>();

            foreach (var item in typeof(TOut).GetProperties())
            {
                if (!item.CanWrite)
                    continue;

                MemberExpression property = Expression.Property(parameterExpression, typeof(TIn).GetProperty(item.Name));
                MemberBinding memberBinding = Expression.Bind(item, property);
                memberBindingList.Add(memberBinding);
            }

            MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
            Expression<Func<TIn, TOut>> lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, new ParameterExpression[] { parameterExpression });

            return lambda.Compile();
        }

        public static TOut Trans(TIn tIn)
        {
            return cache(tIn);
        }

    }


    class Program
    {
        static Dictionary<int,Dictionary<int,A>> aDic = new Dictionary<int, Dictionary<int, A>>();


        static List<A> aList = new List<A>();
        static void Main(string[] args)
        {
            Dictionary<long, Dictionary<long,PlateImportData>> a = new Dictionary<long, Dictionary<long, PlateImportData>>();
            Dictionary<long, Dictionary<long, PlateImportData>> b = new Dictionary<long, Dictionary<long, PlateImportData>>();
            do
            {
                for (long i = 1000000; i < 1001000; i++)
                {
                    for (long j = 100000000000; j < 100000000480; j++)
                    {
                        if (!b.ContainsKey(i))
                        {
                            b.Add(i,new Dictionary<long, PlateImportData>());
                        }
                        if (!b[i].ContainsKey(j))
                        {
                            b[i].Add(j, new PlateImportData());
                        }
                        b[i][j] = new PlateImportData
                        {
                            SharesCode = "123456",
                            LastTradeStock = 1000000,
                            ClosedPrice = 1000000,
                            DataType = 2,
                            GroupTimeKey = 202020000000,
                            LastTradeAmount = 2020202020,
                            Market = 1,
                            MaxPrice = 2929292,
                            MinPrice = 29292929,
                            OpenedPrice = 29828282,
                            PlateId = 22323,
                            PreClosePrice = 39232323,
                            Time = DateTime.Now,
                            TotalCapital = 2342424234234,
                            Tradable = 12341132123,
                            TradeAmount = 12313123123123,
                            TradeStock = 12312313,
                            WeightType = 1,
                            YestodayClosedPrice = 12312313123132
                        };
                    }
                }
                a = b;
                a.Clear();
                Thread.Sleep(500);
            } while (true);


            //var tt = a;
            //var a = new SortedDictionary<int, A>();
            //a.Add(2,new A 
            //{
            //    Id=2
            //});
            //a.Add(1, new A
            //{
            //    Id = 1
            //});
            //a.Add(3, new A
            //{
            //    Id = 3
            //});
            //Dictionary<int, A> b=new Dictionary<int, A>();
            //var c = b;
            //c = a.ToDictionary(k => k.Key, v => v.Value);
            //c.Add(4,new A 
            //{
            //    Id=4
            //});

            // var rr = a;
            //List<A> alist = new List<A>
            //{
            //    new A
            //    {
            //        Id=1,
            //        Name="1"
            //    }
            //};
            //List<A> blist = new List<A>(alist);
            //alist.Clear();
            //var c = blist;

            //TaskThread.SetTaskThreads();
            //int i = 0;
            //int idx = 0;
            //while (true)
            //{
            //    i++;
            //    if (i % 2 == 0)
            //    {
            //        RunOneThread(idx);
            //        idx += 400000;
            //        RunOneThreadOne(idx);
            //        idx += 100000;
            //        RunMoreThread(idx);
            //        idx += 400000;
            //    }
            //    else
            //    {
            //        RunMoreThread(idx);
            //        idx += 400000;
            //        RunOneThreadOne(idx);
            //        idx += 100000;
            //        RunOneThread(idx);
            //        idx += 400000;
            //    }
            //    Console.WriteLine("=============================================");
            //    Thread.Sleep(5000);
            //}
        }

        static void RunOneThread(int idx)
        {
            var list1 = BuildInsertData(idx);
            var list2 = BuildInsertData(idx+100000);
            var list3 = BuildInsertData(idx + 200000);
            var list4 = BuildInsertData(idx + 300000);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            InsertToDataBaseTest(list1);
            InsertToDataBaseTest(list2);
            InsertToDataBaseTest(list3);
            InsertToDataBaseTest(list4);
            stopwatch.Stop();
            Console.WriteLine("单线程（多个）用时："+ stopwatch.ElapsedMilliseconds+"毫秒");
        }
        static void RunOneThreadOne(int idx)
        {
            var list1 = BuildInsertData(idx);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            InsertToDataBaseTest(list1);
            stopwatch.Stop();
            Console.WriteLine("单线程(单个)用时：" + stopwatch.ElapsedMilliseconds + "毫秒");
        }
        static void RunMoreThread(int idx)
        {
            var list1 = BuildInsertData(idx);
            var list2 = BuildInsertData(idx + 100000);
            var list3 = BuildInsertData(idx + 200000);
            var list4 = BuildInsertData(idx + 300000);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            WaitHandle[] taskarr = new WaitHandle[4];
            for (int i = 0; i < 4; i++)
            {
                var temp = i == 0 ? list1 : i == 1 ? list2 : i == 2 ? list3 : list4;
                taskarr[i] = TaskThread.CreateTask(obj=> 
                {
                    InsertToDataBaseTest(obj as List<t_shares_securitybarsdata_1min>);
                }, temp);
            }
            TaskThread.WaitAll(taskarr,Timeout.Infinite);
            TaskThread.CloseAllTasks(taskarr);
            stopwatch.Stop();
            Console.WriteLine("多线程用时：" + stopwatch.ElapsedMilliseconds + "毫秒");
        }

        static void InsertToDataBaseTest(List<t_shares_securitybarsdata_1min> list,int idx=0) 
        {
            var newList = list.Skip(idx).Take(20000).ToList();

            DataTable table = new DataTable();
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("GroupTimeKey", typeof(long));
            table.Columns.Add("Time", typeof(DateTime));
            table.Columns.Add("TimeStr", typeof(string));
            table.Columns.Add("OpenedPrice", typeof(long));
            table.Columns.Add("ClosedPrice", typeof(long));
            table.Columns.Add("PreClosePrice", typeof(long));
            table.Columns.Add("MinPrice", typeof(long));
            table.Columns.Add("MaxPrice", typeof(long));
            table.Columns.Add("TradeStock", typeof(long));
            table.Columns.Add("TradeAmount", typeof(long));
            table.Columns.Add("LastTradeStock", typeof(long));
            table.Columns.Add("LastTradeAmount", typeof(long));
            table.Columns.Add("Tradable", typeof(long));
            table.Columns.Add("TotalCapital", typeof(long));
            table.Columns.Add("HandCount", typeof(int));
            table.Columns.Add("LastModified", typeof(DateTime));
            table.Columns.Add("IsLast", typeof(bool));
            table.Columns.Add("YestodayClosedPrice", typeof(long));
            table.Columns.Add("IsVaild", typeof(bool));
            foreach (var item in newList)
            {
                DataRow row = table.NewRow();
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["GroupTimeKey"] = item.GroupTimeKey;
                row["Time"] = item.Time;
                row["TimeStr"] = item.TimeStr;
                row["OpenedPrice"] = item.OpenedPrice;
                row["ClosedPrice"] = item.ClosedPrice;
                row["PreClosePrice"] = item.PreClosePrice;
                row["MinPrice"] = item.MinPrice;
                row["MaxPrice"] = item.MaxPrice;
                row["TradeStock"] = item.TradeStock;
                row["TradeAmount"] = item.TradeAmount;
                row["LastTradeStock"] = item.LastTradeStock;
                row["LastTradeAmount"] = item.LastTradeAmount;
                row["Tradable"] = item.Tradable;
                row["TotalCapital"] = item.TotalCapital;
                row["HandCount"] = item.HandCount;
                row["LastModified"] = DateTime.Now;
                row["IsLast"] = item.IsLast;
                row["YestodayClosedPrice"] = item.YestodayClosedPrice;
                row["IsVaild"] = item.IsVaild;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@sharesSecurityBarsData", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SharesSecurityBarsData";
                    //赋值
                    parameter.Value = table;

                    SqlParameter dataType_parameter = new SqlParameter("@dataType", SqlDbType.Int);
                    db.Database.ExecuteSqlCommand("exec P_securityBarsData_Update @sharesSecurityBarsData", parameter);
                    tran.Commit();
                    idx = idx + newList.Count();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("更新K线数据出错", ex);
                    tran.Rollback();
                }
            }
            if (idx < list.Count())
            {
                InsertToDataBaseTest(list, idx);
            }
        }

        static List<t_shares_securitybarsdata_1min> BuildInsertData(int i) 
        {
            List<t_shares_securitybarsdata_1min> result = new List<t_shares_securitybarsdata_1min>();
            for (int idx= i; idx < (i + 100000); idx++)
            {
                result.Add(new t_shares_securitybarsdata_1min 
                {
                    SharesCode= idx.ToString(),
                    Market= idx,
                    GroupTimeKey= idx,
                    LastTradeStock=0,
                    TimeStr="0",
                    TradeStock=0,
                    IsLast=false,
                    IsVaild=false,
                    ClosedPrice=0,
                    HandCount=0,
                    LastModified=DateTime.Now,
                    LastTradeAmount=0,
                    MaxPrice=0,
                    MinPrice=0,
                    OpenedPrice=0,
                    PreClosePrice=0,
                    TotalCapital=0,
                    Tradable=0,
                    YestodayClosedPrice=0,
                    TradeAmount=0,
                    Time=DateTime.Now
                });
            }
            return result;
        }
    }
}
