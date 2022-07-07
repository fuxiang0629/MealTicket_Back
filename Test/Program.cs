using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Handler.SecurityBarsData;
using MealTicket_Web_Handler;
using MealTicket_Web_Handler.Runner;
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
    public class LimitInfo
    {
        public long SharesKey { get; set; }

        public int Type { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime Time 
        {
            get
            {
                if (CreateTime.Hour == 9 && CreateTime.Minute < 30)
                {
                    return DateTime.Parse(CreateTime.ToString("yyyy-MM-dd 09:30:00"));
                }
                return DateTime.Parse(CreateTime.ToString("yyyy-MM-dd HH:mm:00"));
            }
        }
    }

    public class LimitResult
    {
        public long SharesKey { get; set; }

        public DateTime Time { get; set; }

        public int PriceType { get; set; }

        public bool IsLimitUpBomb { get; set; }

        public bool IsLimitDownBomb { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始更新"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            DateTime date = DateTime.Parse("2022-06-15 00:00:00");
            DateTime startDate = DateTime.Parse("2022-06-15 00:00:00");
            int idx = 0;
            for (DateTime tempDate = date; tempDate >= startDate; tempDate = tempDate.AddDays(-1))
            {
                idx++;
                if (!DbHelper.CheckTradeDate(tempDate))
                {
                    continue;
                }
                Execute(tempDate);

                if (idx % 10 == 0)
                {
                    Console.WriteLine("===已完成" + idx + "天===");
                }
            }
            Console.WriteLine("结束更新" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            Console.ReadLine();
        }
        public static void Execute(DateTime date)
        {
            var limitDic = GetLimitList(date);
            List<LimitResult> result = new List<LimitResult>();
            foreach (var item in limitDic)
            {
                int lastPriceType = 0;
                bool lastIsLimitDownBomb = false;
                bool lastIsLimitUpBomb = false;
                List<LimitInfo> tempList = item.Value;
                for (TimeSpan spanT = TimeSpan.Parse("09:30:00"); spanT <= TimeSpan.Parse("15:00:00");)
                {
                    LimitInfo tempInfo = null;
                    if (tempList.Count() > 0)
                    {
                        tempInfo = tempList[0];
                    }
                    if (tempInfo != null && tempInfo.Time <= date.AddHours(spanT.Hours).AddMinutes(spanT.Minutes))
                    {
                        if (tempInfo.Type == 1)
                        {
                            lastPriceType = 1;
                            lastIsLimitUpBomb = false;
                        }
                        if (tempInfo.Type == 2)
                        {
                            lastPriceType = 2;
                            lastIsLimitDownBomb = false;
                        }
                        if (tempInfo.Type == 3)
                        {
                            lastIsLimitUpBomb = true;
                            lastPriceType = 0;
                        }
                        if (tempInfo.Type == 4)
                        {
                            lastIsLimitDownBomb = true;
                            lastPriceType = 0;
                        }
                        tempList.RemoveAt(0);
                    }

                    int PriceType = lastPriceType;
                    bool IsLimitDownBomb = lastIsLimitDownBomb;
                    bool IsLimitUpBomb = lastIsLimitUpBomb;
                    result.Add(new LimitResult
                    {
                        SharesKey = item.Key,
                        Time = date.AddHours(spanT.Hours).AddMinutes(spanT.Minutes),
                        PriceType = PriceType,
                        IsLimitDownBomb = IsLimitDownBomb,
                        IsLimitUpBomb = IsLimitUpBomb
                    });
                    if (spanT == TimeSpan.Parse("11:30:00"))
                    {
                        spanT = TimeSpan.Parse("13:00:00");
                    }
                    else
                    {
                        spanT = spanT.Add(TimeSpan.Parse("00:01:00"));
                    }
                }
            }
            WriteToDataBase(result);
        }

        public static Dictionary<long, List<LimitInfo>> GetLimitList(DateTime date)
        {
            string sql = string.Format(@"SELECT convert(bigint,SharesCode)*10+Market SharesKey,[Type],[CreateTime]
FROM[meal_ticket].[dbo].[t_shares_quotes_history]
where Date = '{0}' and[Type] <> 5 and[Type] <> 6 and BusinessType = 1
order by SharesCode,CreateTime",date.ToString("yyyy-MM-dd"));
            using (var db = new meal_ticketEntities())
            {
                var list=db.Database.SqlQuery<LimitInfo>(sql).ToList();
                return list.GroupBy(e => e.SharesKey).ToDictionary(k => k.Key, v => v.ToList());
            }
        }

        public static void WriteToDataBase(List<LimitResult> data) 
        {
            DataTable table = new DataTable();
            table.Columns.Add("SharesKey", typeof(long));
            table.Columns.Add("Time", typeof(DateTime));
            table.Columns.Add("PriceType", typeof(int));
            table.Columns.Add("IsLimitUpBomb", typeof(bool));
            table.Columns.Add("IsLimitDownBomb", typeof(bool));

            foreach (var item in data)
            {
                DataRow row = table.NewRow();
                row["SharesKey"] = item.SharesKey;
                row["Time"] = item.Time;
                row["PriceType"] = item.PriceType;
                row["IsLimitUpBomb"] = item.IsLimitUpBomb;
                row["IsLimitDownBomb"] = item.IsLimitDownBomb;
                table.Rows.Add(row);
            }

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@limitInfo", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.LimitInfo";
                    //赋值
                    parameter.Value = table;

                    db.Database.ExecuteSqlCommand("exec P_LimitInfo_Update @limitInfo", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("入库失败", ex);
                    tran.Rollback();
                }
            }
        }
    }
}
