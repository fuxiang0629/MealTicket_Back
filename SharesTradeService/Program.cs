using FXCommon.Common;
using Ninject;
using SharesTradeService.Handler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharesTradeService
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
            //    new TradeService()
            //};
            //ServiceBase.Run(ServicesToRun);
        }

        //[Conditional("DEBUG")]
        private static void StartDebug()
        {
            // 启动 OWIN host
            Singleton Instance = Singleton.instance;
            Singleton.instance.InitTradeClient();
            Singleton.instance.mqHandler = new MQHandler();

            var _kernel = new StandardKernel(new ServiceModule());
            var runners = _kernel.GetAll<Runner>().ToList();
            runners.ForEach((e) => e.Run());
            Console.WriteLine("程序已启动,按任意键退出");
            Console.ReadLine();
        }
    }
}
