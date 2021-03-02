using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionDataUpdate_NO3
{
    class Program
    {
        static void Main(string[] args)
        {
            var SingletonInstance = Singleton.Instance;

            while (true)
            {
                Console.Write("是否启动新分笔数据处理（Y/N）:");
                var serviceRun = Console.ReadLine();
                if (serviceRun.ToUpper() == "Y" || serviceRun.ToUpper() == "YES")
                {
                    var MQHandlerInstance = MQHandler.instance;
                    MQHandlerInstance.Reconnect();
                    break;
                }
            }
            Console.WriteLine("程序已启动");
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
