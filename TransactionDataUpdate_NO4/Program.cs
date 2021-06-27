using FXCommon.Common;
using Ninject;
using System;
using System.Collections.Generic;
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

            var mq=MQHandler.Instance;
            mq.Reconnect();
            var Kernel = new StandardKernel(new ServiceModel());
            //加载循环任务
            var runners = Kernel.GetAll<Runner>().ToList();
            runners.ForEach(e => e.Run());
            Console.WriteLine("程序已启动,按任意键退出");
            Console.ReadLine();
        }
    }
}
