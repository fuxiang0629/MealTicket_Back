using FXCommon.Common;
using Ninject;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionDataUpdate_NO4
{
    class Program
    {
        static void Main(string[] args)
        {
            var session = Singleton.Instance;

            while (true)
            {
                Console.Write("是否启动分笔更新服务（Y/N）:");
                var serviceRun = Console.ReadLine();
                if (serviceRun.ToUpper() == "Y" || serviceRun.ToUpper() == "YES")
                {
                    break;
                }
            }
            var mqHandler = session.StartMqHandler(ConfigurationManager.AppSettings["listenQueueName"]);
            mqHandler.StartListen();
           
            Console.WriteLine("程序已启动,按任意键退出");
            Console.ReadLine();
        }
    }
}
