using FXCommon.Common;
using Ninject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SharesHqService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            StartDebug();
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new HqService()
            //};
            //ServiceBase.Run(ServicesToRun);
        }


        //[Conditional("DEBUG")]
        private static void StartDebug()
        {
            // 启动 OWIN host
            var Instance = Singleton.Instance;
            var _kernel = new StandardKernel(new ServiceModule());

            while (true)
            {
                Console.Write("是否启动行情服务（Y/N）:");
                var serviceRun=Console.ReadLine();
                if (serviceRun.ToUpper() == "Y" || serviceRun.ToUpper() == "YES")
                {
                    Instance.GetLastSharesQuotesList();
                    Instance.StartSysPar();
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
                Console.WriteLine("行情链接数："+Singleton.Instance.cltData.GetCount());
                Console.WriteLine("重连链接数：" + Singleton.Instance.retryData.GetCount());
                Console.WriteLine("=====================");
            }
        }
    }
}
