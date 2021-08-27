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
                string startday = "2021-05-01 00:00:00";
                string endday = "2021-08-27 00:00:00";
                using (var db = new meal_ticketEntities())
                {
                    var sharesList = (from item in db.v_shares_baseinfo
                                      select item).ToList();
                    foreach (var item in sharesList)
                    {
                        var content = new
                        {
                            Market = item.Market,
                            SharesCode = item.SharesCode,
                            StartDate = startday,
                            EndDate = endday
                        };
                        string res = HtmlSender.Request(JsonConvert.SerializeObject(content), url, "application/json", Encoding.UTF8, "POST", userToken);
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
}
