using FXCommon.Common;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionDataUpdate_App
{
    class Program
    {
        static void Main(string[] args)
        {
            StartDebug();
        }

        private static void StartDebug()
        {
            // 启动 OWIN host
            var Instance = Singleton.Instance;
            var _kernel = new StandardKernel(new ServiceModule());

            while (true)
            {
                Console.Write("是否启动行情服务（Y/N）:");
                var serviceRun = Console.ReadLine();
                if (serviceRun.ToUpper() == "Y" || serviceRun.ToUpper() == "YES")
                {
                    var runners = _kernel.GetAll<Runner>().ToList();
                    runners.ForEach((e) => e.Run());
                    break;
                }
            }
            Console.WriteLine("程序已启动");

            while (true)
            {
                Console.ReadLine();
                Console.WriteLine("=====================");
                Console.WriteLine("行情链接数：" + Singleton.Instance.cltData.GetCount());
                Console.WriteLine("重连链接数：" + Singleton.Instance.retryData.GetCount());
                Console.WriteLine("=====================");
            }
        }
    }
}
