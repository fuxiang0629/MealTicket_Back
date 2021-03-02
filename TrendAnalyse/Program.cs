using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrendAnalyse
{
    class Program
    {
        static void Main(string[] args)
        {
            var SingletonInstance = Singleton.Instance;
            SingletonInstance.mqHandler.Reconnect();//启动队列
            Console.WriteLine("程序已启动");
            Console.ReadLine();
        }
    }
}
