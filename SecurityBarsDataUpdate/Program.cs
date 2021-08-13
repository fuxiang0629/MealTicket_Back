using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityBarsDataUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            var session = Singleton.Instance;

            while (true)
            {
                Console.Write("是否启动K线更新服务（Y/N）:");
                var serviceRun = Console.ReadLine();
                if (serviceRun.ToUpper() == "Y" || serviceRun.ToUpper() == "YES")
                {
                    break;
                }
            }

            var mqHandler = session.StartMqHandler("SecurityBars_1min");
            mqHandler.StartListen();
            Console.WriteLine("程序已启动,按任意键退出");
            Console.ReadLine();
        }
    }
}
