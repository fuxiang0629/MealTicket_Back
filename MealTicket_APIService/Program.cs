﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_APIService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            StartDebug();
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new APIService()
            };
            ServiceBase.Run(ServicesToRun);
        }

        [Conditional("DEBUG")]
        private static void StartDebug()
        {
            // 启动 OWIN host
            var _manager = new WebApiManager();
            _manager.Start<Startup>("http://localhost:8800/");
            Console.WriteLine("程序已启动,按任意键退出");
            Console.ReadLine();
        }
    }
}
